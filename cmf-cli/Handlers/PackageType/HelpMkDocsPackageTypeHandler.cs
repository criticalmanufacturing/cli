using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Handler for MkDocs-based Help packages.
    /// </summary>
    public class HelpMkDocsPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpMkDocsPackageTypeHandler" /> class.
        /// </summary>
        public HelpMkDocsPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                targetDirectory:
                    "UI/Reference",
                targetLayer:
                    "reference",
                steps:
                    new List<Step>
                    {
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "site/**",
                            TargetDirectory = "/"
                        }
                    }
            );

            cmfPackage.DFPackageType = PackageType.Presentation;

            var packageDirectory = CmfPackage.GetFileInfo().DirectoryName;

            // MkDocs build must run where requirements.txt is located.
            var requirementsFile = fileSystem.Directory.GetFiles(packageDirectory, "requirements.txt", SearchOption.AllDirectories).FirstOrDefault();

            if (requirementsFile == null)
            {
                throw new CliException($"Could not find requirements.txt under {packageDirectory}. Cannot build Help package.");
            }

            var workingDirectory = fileSystem.DirectoryInfo.New(fileSystem.Path.GetDirectoryName(requirementsFile));

            BuildSteps =
            [
                new ExecuteCommand<RestoreCommand>()
                {
                    Command = new RestoreCommand(fileSystem),
                    DisplayName = "cmf restore",
                    Execute = command =>
                    {
                        command.Execute(CmfPackage.GetFileInfo().Directory, null);
                    }
                },
                new PythonCommand
                {
                    DisplayName = "Create Python virtual environment",
                    Module = "venv",
                    Args = [".venv"],
                    WorkingDirectory = workingDirectory
                },
                new PythonCommand
                {
                    DisplayName = "Install MkDocs",
                    Module = "pip",
                    Args = ["install", "mkdocs"],
                    VirtualEnvironment = ".venv",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["REQUESTS_CA_BUNDLE"] = null
                    },
                    WorkingDirectory = workingDirectory
                },
                new PythonCommand
                {
                    DisplayName = "Install Python requirements",
                    Module = "pip",
                    Args = ["install", "-r", "requirements.txt"],
                    VirtualEnvironment = ".venv",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["REQUESTS_CA_BUNDLE"] = null
                    },
                    WorkingDirectory = workingDirectory
                },
                new MkDocsCommand
                {
                    DisplayName = "MkDocs build",
                    Command = "build",
                    VirtualEnvironment = ".venv",
                    WorkingDirectory = workingDirectory
                }
            ];
        }

        /// <summary>
        /// Packs the generated MkDocs site.
        /// </summary>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false)
        {
            var packageDirectory = CmfPackage.GetFileInfo().Directory.FullName;
            var siteDirectory = fileSystem.DirectoryInfo.New(fileSystem.Path.Join(packageDirectory, "site"));
            var docsDirectory = fileSystem.DirectoryInfo.New(fileSystem.Path.Join(packageDirectory, "docs"));

            if (!siteDirectory.Exists || !docsDirectory.Exists)
            {
                throw new CliException($"Could not find MkDocs site or documentation source at {packageDirectory}. Run 'cmf build' before packing.");
            }

            base.Pack(packageOutputDir, outputDir, dryRun);
        }

        /// <summary>
        /// This restore function takes everything from the package zip and copies into the local docs folder also adding a gitignore.
        /// </summary>
        public override void RestoreDependencies(Uri[] repoUris)
        {
            if (CmfPackage.Dependencies == null || CmfPackage.Dependencies.Count == 0)
            {
                Log.Information("No dependencies to restore.");
                return; // No dependencies to restore.
            }

            var packageDirectory = CmfPackage.GetFileInfo().DirectoryName;
            var docsFolder = fileSystem.DirectoryInfo.New(fileSystem.Path.Join(packageDirectory, "docs"));
            var temporaryFolderForUnziping = fileSystem.DirectoryInfo.New(fileSystem.Path.Join(fileSystem.Path.GetTempPath(), $"cmf-mkdocs-restore-{Guid.NewGuid()}"));
            var originalDependenciesFolder = DependenciesFolder;

            try
            {
                if (!docsFolder.Exists)
                {
                    docsFolder.Create();
                }

                // Extract dependency zips to a temporary folder, never directly into the docs/ folder.
                DependenciesFolder = temporaryFolderForUnziping;
                base.RestoreDependencies(repoUris);

                if (temporaryFolderForUnziping.Exists)
                {
                    // Each restored mkdocs zip unpacks site, docs and manifest.
                    // We only care about the docs folder
                    var dependencyDocsFolders = fileSystem.Directory.GetDirectories(temporaryFolderForUnziping.FullName, "docs", SearchOption.AllDirectories);

                    foreach (var dependencyDocsFolder in dependencyDocsFolders)
                    {
                        var depDocsDirInfo = fileSystem.DirectoryInfo.New(dependencyDocsFolder);

                        foreach (var entry in depDocsDirInfo.GetDirectories())
                        {
                            if (entry.Name.IgnoreCaseEquals("assets"))
                            {
                                continue; // Skip the assets folder
                            }

                            var targetFolder = fileSystem.Path.Join(docsFolder.FullName, entry.Name);

                            // replace with what's actually in the zip, nothing more
                            if (fileSystem.Directory.Exists(targetFolder))
                            {
                                fileSystem.Directory.Delete(targetFolder, true);
                            }

                            FileSystemUtilities.CopyDirectory(
                                entry.FullName,
                                targetFolder,
                                fileSystem,
                                copySubDirs: true,
                                isCopyDependencies: true);

                            fileSystem.File.WriteAllText(fileSystem.Path.Join(targetFolder, ".gitignore"),"**" + Environment.NewLine);
                        }
                    }
                }
            }
            finally
            {
                DependenciesFolder = originalDependenciesFolder;
                if (temporaryFolderForUnziping.Exists)
                {
                    temporaryFolderForUnziping.Delete(true); // Delete created temporary folder
                }
            }
        }
    }
}

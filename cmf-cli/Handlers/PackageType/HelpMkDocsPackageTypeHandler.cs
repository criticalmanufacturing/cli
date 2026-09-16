using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Builders;
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
            var requirementsFile = this.fileSystem.Directory
                .GetFiles(packageDirectory, "requirements.txt", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (requirementsFile == null)
            {
                throw new CliException($"Could not find requirements.txt under {packageDirectory}. Cannot build Help package.");
            }

            var workingDirectory = this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.GetDirectoryName(requirementsFile));

            BuildSteps = new IBuildCommand[]
            {
                new PythonCommand
                {
                    DisplayName = "Create Python virtual environment",
                    Module = "venv",
                    Args = new[] { ".venv" },
                    WorkingDirectory = workingDirectory
                },
                new PythonCommand
                {
                    DisplayName = "Install MkDocs",
                    Module = "pip",
                    Args = new[] { "install", "mkdocs" },
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
                    Args = new[] { "install", "-r", "requirements.txt" },
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
            };
        }

        /// <summary>
        /// Packs the generated MkDocs site.
        /// </summary>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false)
        {
            var packageDirectory = CmfPackage.GetFileInfo().Directory.FullName;
            var siteDirectory = this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.Join(packageDirectory, "site"));
            var docsDirectory = this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.Join(packageDirectory, "docs"));

            if (!siteDirectory.Exists || !docsDirectory.Exists)
            {
                throw new CliException($"Could not find MkDocs site or documentation source at {packageDirectory}. Run 'cmf build' before packing.");
            }

            base.Pack(packageOutputDir, outputDir, dryRun);
        }

        /// <summary>
        /// Restores dependency documentation into the local docs folder.
        /// </summary>
        public override void RestoreDependencies(Uri[] repoUris)
        {
            var packageDirectory = CmfPackage.GetFileInfo().DirectoryName;
            var docsFolder = this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.Join(packageDirectory, "docs"));
            var stagingFolder = this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.Join(
                this.fileSystem.Path.GetTempPath(), $"cmf-mkdocs-restore-{Guid.NewGuid()}"));
            var originalDependenciesFolder = DependenciesFolder;

            try
            {
                DependenciesFolder = stagingFolder;
                base.RestoreDependencies(repoUris);

                if (!docsFolder.Exists)
                {
                    docsFolder.Create();
                }

                foreach (var dependencyFolder in stagingFolder.GetDirectories())
                {
                    var dependencyDocsFolder = this.fileSystem.Path.Join(dependencyFolder.FullName, "docs");
                    if (this.fileSystem.Directory.Exists(dependencyDocsFolder))
                    {
                        FileSystemUtilities.CopyDirectory(
                            dependencyDocsFolder,
                            docsFolder.FullName,
                            this.fileSystem,
                            copySubDirs: true,
                            isCopyDependencies: true);
                    }
                }

                // Create gitignore
                this.fileSystem.File.WriteAllText(this.fileSystem.Path.Join(docsFolder.FullName, ".gitignore"),"**" + Environment.NewLine);
            }
            finally
            {
                DependenciesFolder = originalDependenciesFolder;
                if (stagingFolder.Exists)
                {
                    stagingFolder.Delete(true);
                }
            }
        }
    }
}

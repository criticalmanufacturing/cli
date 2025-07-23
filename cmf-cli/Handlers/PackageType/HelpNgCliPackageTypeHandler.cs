﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cmf.CLI.Builders;
using Cmf.CLI.Commands;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Handler for packages managed with @angular/cli
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class HelpNgCliPackageTypeHandler : PackageTypeHandler
    {
        protected AngularWorkspace Workspace;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpNgCliPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        public HelpNgCliPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
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
                            ContentPath = "**" // TODO: remove assets/config.json from the deployed files, should only apply transform
                        },
                        new Step(StepType.TaggedFile)
                        {
                            ContentPath = "index.html",
                            TagFile = true
                        },
                        new Step(StepType.TaggedFile)
                        {
                            ContentPath = "ngsw.json",
                            TagFile = true
                        },
                        new Step(StepType.TaggedFile)
                        {
                            ContentPath = "assets/config.json",
                            TagFile = true
                        }
                    }
            );

            BuildSteps = new IBuildCommand[]
            {
                new ExecuteCommand<RestoreCommand>()
                {
                    Command = new RestoreCommand(),
                    DisplayName = "cmf restore",
                    Execute = command =>
                    {
                        command.Execute(cmfPackage.GetFileInfo().Directory, null);
                    }
                },
                new NPMCommand()
                {
                    DisplayName = "NPM Install",
                    Command  = "install",
                    Args = new []{ "--force" },
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                // generate based on templates
                new ExecuteCommand<GenerateBasedOnTemplatesCommand>()
                {
                    DisplayName = "Generate help pages based on templates",
                    Command = new GenerateBasedOnTemplatesCommand(),
                    Execute = command =>
                    {
                        command.Execute(cmfPackage.GetFileInfo().Directory);
                    }
                },
                // generate menu items
                new ExecuteCommand<GenerateMenuItemsCommand>()
                {
                    DisplayName = "Generate menu items",
                    Command = new GenerateMenuItemsCommand(),
                    Execute = command =>
                    {
                        command.Execute(cmfPackage.GetFileInfo().Directory);
                    }
                },
                // generate documents database
                new NPMCommand()
                {
                    DisplayName = "Generate documents database",
                    Command = "run",
                    Args = new[] { "build-db" },
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                // generate markdown links
                new NPMCommand()
                {
                    DisplayName = "Generate markdown links",
                    Command = "run",
                    Args = new[] { "check-md-links" },
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                }
            };

            cmfPackage.DFPackageType = PackageType.Presentation;

            // Projects BuildSteps
            Workspace = new AngularWorkspace(cmfPackage.GetFileInfo().Directory);
            var apps = Workspace.Projects.Where(lib => lib.Type == "application").Select(p => (p.PackageJson.Content.name as JValue)?.Value as string ?? p.Name).ToList();
            foreach (var package in Workspace.PackagesToBuild)
            {
                BuildSteps = BuildSteps.Concat(new IBuildCommand[]
                {
                    new NgCommand()
                    {
                        DisplayName = $"ng build {package.Key}",
                        Command = "build",
                        Args = apps.Contains(package.Key.ToKebabCase()) ? new []{ "--base-href", "$(APPLICATION_BASE_HREF)" } : Array.Empty<string>(),
                        WorkingDirectory = package.Value
                    }
                }).ToArray();
            }
            
            BuildSteps = BuildSteps.Append(new NgBuildFileTokenReplacerCommand(this.fileSystem, apps, cmfPackage)).ToArray();
        }

        /// <summary>
        /// Bumps package.json and package-lock.json
        /// of each project to the specified version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="bumpInformation">The bump information.</param>
        public override void Bump(string version, string buildNr, Dictionary<string, object> bumpInformation = null)
        {
            base.Bump(version, buildNr, bumpInformation);

            foreach (var project in Workspace.Projects)
            {
                if (project.PackageJson.Content.version == null)
                {
                    throw new CliException(string.Format(CoreMessages.MissingMandatoryPropertyInFile, "version", project.PackageJson.File.FullName));
                }
                project.PackageJson.Content.version = GenericUtilities.RetrieveNewPresentationVersion(project.PackageJson.Content.version.ToString(), version, buildNr);

                // write package.json
                fileSystem.File.WriteAllText(project.PackageJson.File.FullName, JsonConvert.SerializeObject(project.PackageJson.Content, Formatting.Indented));

                // In some cases we don't have the package-lock
                if (project.PackageLock != null)
                {
                    if (project.PackageLock.Content.version == null)
                    {
                        throw new CliException(string.Format(CoreMessages.MissingMandatoryPropertyInFile, "version", project.PackageLock.File.FullName));
                    }

                    project.PackageLock.Content.version = project.PackageJson.Content.version;

                    // if package-lock also refer the package in the packages list
                    Dictionary<string, dynamic> packages = project.PackageLock.Content.packages.ToObject<Dictionary<string, dynamic>>();
                    if (packages.ContainsKey("") && packages[""].name == project.Name)
                    {
                        packages[""].version = project.PackageJson.Content.version;
                        project.PackageLock.Content.packages = JObject.FromObject(packages);
                    }


                    // write package-lock.json
                    fileSystem.File.WriteAllText(project.PackageLock.File.FullName, JsonConvert.SerializeObject(project.PackageLock.Content, Formatting.Indented));
                }

            }
        }

        /// <summary>
        /// Bumps the MES version of the package
        /// </summary>
        /// <param name="version">The new MES version.</param>
        public override void MESBump(string version)
        {
            base.MESBump(version);

            // package.json files
            string[] filesToUpdate = this.fileSystem.Directory.GetFiles(this.CmfPackage.GetFileInfo().DirectoryName, "package.json", SearchOption.AllDirectories);
            string pattern = @"release-\d+";

            foreach (string filePath in filesToUpdate.Where(path => !path.Contains("node_modules") && !path.Contains("dist")))
            {
                string text = this.fileSystem.File.ReadAllText(filePath);
                text = Regex.Replace(text, pattern, $"release-{version.Replace(".", "")}", RegexOptions.IgnoreCase);

                this.fileSystem.File.WriteAllText(filePath, text);
            }

            // package-lock.json files
            string[] filesToDelete = this.fileSystem.Directory.GetFiles(this.CmfPackage.GetFileInfo().DirectoryName, "package-lock.json", SearchOption.AllDirectories);
            foreach (string filePath in filesToDelete.Where(path => !path.Contains("node_modules") && !path.Contains("dist")))
            {
                Log.Warning($"Package lock {filePath} has been deleted. Please build the {this.CmfPackage.PackageId} package to regenerate this file");
                this.fileSystem.File.Delete(filePath);
            }
        }
    }
}
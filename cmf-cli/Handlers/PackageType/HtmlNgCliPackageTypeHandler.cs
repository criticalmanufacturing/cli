using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Cmf.CLI.Builders;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Handler for packages managed with @angular/cli
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class HtmlNgCliPackageTypeHandler : PackageTypeHandler
    {
        protected AngularWorkspace Workspace;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlNgCliPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        public HtmlNgCliPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                targetDirectory:
                    "UI/Html",
                targetLayer:
                    "ui",
                steps:
                    new List<Step>
                    {
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "**"
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
                }
};

            cmfPackage.DFPackageType = PackageType.Presentation;

            // Projects BuildSteps
            Workspace = new AngularWorkspace(cmfPackage.GetFileInfo().Directory);
            foreach (var package in Workspace.PackagesToBuild)
            {
                BuildSteps = BuildSteps.Concat(new IBuildCommand[]
                {
                    new NgCommand()
                    {
                        DisplayName = $"ng build {package.Key}",
                        Command = "build",
                        WorkingDirectory = package.Value
                    }
                }).ToArray();
            }
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
                    if(packages.ContainsKey("") && packages[""].name == project.Name)
                    {
                        packages[""].version = project.PackageJson.Content.version;
                        project.PackageLock.Content.packages = JObject.FromObject(packages);
                    }


                    // write package-lock.json
                    fileSystem.File.WriteAllText(project.PackageLock.File.FullName, JsonConvert.SerializeObject(project.PackageLock.Content, Formatting.Indented));
                }

            }
        }

    }
}
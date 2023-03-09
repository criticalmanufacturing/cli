using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Handler for packages managed with @angular/cli
    /// </summary>
    /// <seealso cref="PresentationPackageTypeHandler" />
    public class HtmlNgCliPackageTypeHandler : PresentationPackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGulpPackageTypeHandler" /> class.
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
            var workspace = new AngularWorkspace(cmfPackage.GetFileInfo().Directory);
            foreach (var package in workspace.PackagesToBuild)
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
    }
}
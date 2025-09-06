
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Cmf.CLI.Builders;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.TemplateEngine.Abstractions;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PresentationPackageTypeHandler" />
    public class HtmlGulpPackageTypeHandler : PresentationPackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGulpPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        public HtmlGulpPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
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
                            ContentPath = "bundles/**"
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
                new GulpCommand()
                {
                    GulpFile = "gulpfile.js",
                    Task = "install",
                    DisplayName = "Gulp Install",
                    GulpJS = "node_modules/gulp/bin/gulp.js",
                    Args = new [] { "--update" },
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                new GulpCommand()
                {
                    GulpFile = "gulpfile.js",
                    Task = "build",
                    DisplayName = "Gulp Build",
                    GulpJS = "node_modules/gulp/bin/gulp.js",
                    Args = new [] { "--production" , "--dist", "--brotli"},
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
            };

            cmfPackage.DFPackageType = PackageType.Presentation;
        }

        /// <summary>
        /// Bumps the MES version of the package
        /// </summary>
        /// <param name="version">The new MES version.</param>
        public override void MESBump(string version, string iotVersion, List<string> iotPackagesToIgnore)
        {
            base.MESBump(version, iotVersion, iotPackagesToIgnore);
            MESBumpUtilities.UpdateNPMProject(this.fileSystem, this.CmfPackage, version);

            IFileInfo projectConfig = this.fileSystem.FileInfo.New($"{this.CmfPackage.GetFileInfo().DirectoryName}/apps/customization.web/config.json");
            if (projectConfig.Exists)
            {
                Log.Information($"Updating apps/customization.web/config.json file");

                string text = fileSystem.File.ReadAllText(projectConfig.FullName);
                JObject configObject = JsonConvert.DeserializeObject<JObject>(text);

                if (configObject.ContainsKey("version"))
                {
                    string configVersion = (string)configObject["version"];
                    configObject["version"] = Regex.Replace(configVersion, @"\d+\.\d+\.\d+", version);
                }

                fileSystem.File.WriteAllText(projectConfig.FullName, JsonConvert.SerializeObject(configObject, Formatting.Indented));
            }
        }
    }
}
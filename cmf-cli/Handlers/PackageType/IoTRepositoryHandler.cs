using Cmf.Common.Cli.Builders;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Cmf.Common.Cli.Commands.restore;
using Newtonsoft.Json.Serialization;

namespace Cmf.Common.Cli.Handlers
{
    /// <summary>
    ///
    /// </summary>
    public class IoTRepositoryHandler : PackageTypeHandler
    {
        #region Private Methods

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="IoTRepositoryHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public IoTRepositoryHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                steps:
                    new List<Step>()
                    {
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath= "*.tgz" ,
                            TargetDirectory= "/opt/connectiot/"
                        },
                        new Step(StepType.GenerateRepositoryIndex)
                        {
                            ContentPath= "/opt/connectiot/", 
                            TargetFile= "/opt/connectiot/.repositoryContent.json"
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
                        command.Execute(cmfPackage.GetFileInfo().Directory.Parent, null);
                    }
                },
                new NPMCommand()
                {
                    DisplayName = "NPM Install",
                    Command = "install",
                    Args = new[] {"--force"},
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory.Parent
                },
                new GulpCommand()
                {
                    GulpFile = "gulpfile.js",
                    Task = "install",
                    DisplayName = "Gulp Install",
                    GulpJS = "node_modules/gulp/bin/gulp.js",
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory.Parent
                },
                new GulpCommand()
                {
                    GulpFile = "gulpfile.js",
                    Task = "build",
                    DisplayName = "Gulp Build",
                    GulpJS = "node_modules/gulp/bin/gulp.js",
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory.Parent
                },
                new ConsistencyCheckCommand()
                {
                    DisplayName = "Consistency Check Validator Command",
                    FileSystem = this.fileSystem,
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                }
            };

            cmfPackage.DFPackageType = PackageType.Generic;
        }

        /// <summary>
        /// Packs the specified package output dir.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="outputDir">The output dir.</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            IoTPackageTypeHandler.GenerateIoTTGZPackages(CmfPackage, packageOutputDir, "");

            base.Pack(packageOutputDir, outputDir);
        }
    }
}
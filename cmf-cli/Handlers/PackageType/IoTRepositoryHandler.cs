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
                //targetDirectory:
                //    "UI/Html",
                //targetLayer:
                //    "ui",
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
        /// Copies the install dependencies.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        protected override void CopyInstallDependencies(IDirectoryInfo packageOutputDir)
        {
            FileSystemUtilities.CopyInstallDependenciesFiles(packageOutputDir, PackageType.IoT, this.fileSystem);
        }

        /// <summary>
        /// Packs the specified package output dir.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="outputDir">The output dir.</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            foreach (ContentToPack contentToPack in CmfPackage.ContentToPack)
            {
                if (contentToPack.Action == null || contentToPack.Action == PackAction.Pack)
                {
                    IDirectoryInfo[] packDirectories = CmfPackage.GetFileInfo().Directory.GetDirectories(contentToPack.Source);

                    foreach (IDirectoryInfo packDirectory in packDirectories)
                    {
                        string inputDirPath = packDirectory.FullName;
                        IFileInfo packConfig = this.fileSystem.FileInfo.FromFileName($"{inputDirPath}/packconfig.json");
                        if (!packConfig.Exists)
                        {
                            Log.Warning("packconfig.json doesn't exist! packagePacker will not run.");
                            continue;
                        }
                        Log.Debug("Running Package Packer");

                        NPMCommand npmCommand = new NPMCommand()
                        {
                            DisplayName = "npm shrinkwrap",
                            Args = new string[] { "shrinkwrap" },
                            WorkingDirectory = packDirectory
                        };

                        npmCommand.Exec();

                        CmdCommand cmdCommand = new CmdCommand()
                        {
                            DisplayName = "npx yo @criticalmanufacturing/iot:packagePacker",
                            Command = "npx",
                            Args = new string[] { "yo @criticalmanufacturing/iot:packagePacker", $"-i \"{inputDirPath}\"", $"-o \"{packageOutputDir}\"" },
                            WorkingDirectory = packDirectory
                        };

                        cmdCommand.Exec();
                    }
                }
                else if (contentToPack.Action == PackAction.Untar)
                {
                    IFileInfo tgzFile = this.fileSystem.FileInfo.FromFileName($"{CmfPackage.GetFileInfo().Directory.FullName}/{contentToPack.Source}");
                    CmdCommand cmdCommand = new CmdCommand()
                    {
                        DisplayName = "tar -xzvf",
                        Command = "tar",
                        Args = new string[] { $"-xzvf {tgzFile.FullName}" },
                        WorkingDirectory = packageOutputDir
                    };

                    cmdCommand.Exec();

                    dynamic packageJson = tgzFile.Directory.GetPackageJsonFile();

                    string packDirectoryName = packageJson == null ? tgzFile.Directory.Name : packageJson.name;

                    IDirectoryInfo packageDirectory = this.fileSystem.DirectoryInfo.FromDirectoryName($"{packageOutputDir}/package");
                    IDirectoryInfo destinationDirectory = this.fileSystem.DirectoryInfo.FromDirectoryName($"{packageOutputDir}/{contentToPack.Target}/{packDirectoryName}");
                    destinationDirectory.Parent.Create();
                    packageDirectory.MoveTo(destinationDirectory.FullName);
                }
            }

            base.Pack(packageOutputDir, outputDir);
        }
    }
}
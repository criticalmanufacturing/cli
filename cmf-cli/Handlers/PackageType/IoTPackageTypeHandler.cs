using Cmf.CLI.Builders;
using Cmf.CLI.Commands.New;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PresentationPackageTypeHandler" />
    public class IoTPackageTypeHandler : PresentationPackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IoTPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public IoTPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            var minimumVersion = new Version("8.3.5");
            var targetVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            IBuildCommand[] buildCommands = Array.Empty<IBuildCommand>();

            if (targetVersion.CompareTo(minimumVersion) < 0)
            {
                Log.Debug(
                    $"MES version lower than {minimumVersion}, skipping DeployRepositoryFiles and GenerateRepositoryIndex steps. Make sure you have alternative steps in your manifest.");
            }

            if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major < 10)
            {
                buildCommands = new IBuildCommand[]
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
                       Command = "install",
                       Args = new[] {"--force"},
                       WorkingDirectory = cmfPackage.GetFileInfo().Directory
                    },
                    new GulpCommand()
                    {
                       GulpFile = "gulpfile.js",
                       Task = "install",
                       DisplayName = "Gulp Install",
                       GulpJS = "node_modules/gulp/bin/gulp.js",
                       WorkingDirectory = cmfPackage.GetFileInfo().Directory
                    },
                    new GulpCommand()
                    {
                       GulpFile = "gulpfile.js",
                       Task = "build",
                       DisplayName = "Gulp Build",
                       GulpJS = "node_modules/gulp/bin/gulp.js",
                       WorkingDirectory = cmfPackage.GetFileInfo().Directory
                    },
                    new GulpCommand()
                    {
                        GulpFile = "gulpfile.js",
                        Task = "cliTest",
                        DisplayName = "Gulp Test",
                        GulpJS = "node_modules/gulp/bin/gulp.js",
                        WorkingDirectory = cmfPackage.GetFileInfo().Directory,
                        Test = true
                    }
                };
            }
            else
            {
                buildCommands = new IBuildCommand[]
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
                       Command = "install",
                       Args = new[] {"--force"},
                       WorkingDirectory = cmfPackage.GetFileInfo().Directory
                    },
                    new NPMCommand()
                    {
                       DisplayName = "NPM Build",
                       Command = "run build -ws",
                       Args = new[] {"--force"},
                       WorkingDirectory = cmfPackage.GetFileInfo().Directory
                    },
                    new NPMCommand()
                    {
                       DisplayName = "NPM Test",
                       Command = "run test -ws",
                       Args = new[] {"--force"},
                       WorkingDirectory = cmfPackage.GetFileInfo().Directory
                    },
                   new ExecuteCommand<IoTLibCommand>()
                    {
                        Command = new IoTLibCommand(),
                        DisplayName = "cmf iot lib command",
                        Execute = command =>
                        {
                            command.Execute(cmfPackage.GetFileInfo().Directory);
                        }
                    },
                };
            }

            cmfPackage.SetDefaultValues
            (
                targetDirectory:
                    "UI/Html",
                targetLayer:
                    "ui",
                steps:
                    new List<Step>()
                    {
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "runtimePackages/**"
                        },
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "*.ps1"
                        },
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "*.bat"
                        },
                        new Step(StepType.Generic)
                        {
                            OnExecute = $"$(Package[{cmfPackage.PackageId}].TargetDirectory)/runtimePackages/ValidateIoTInstall.ps1"
                        },
                        new Step(StepType.Generic)
                        {
                            OnExecute = $"$(Package[{cmfPackage.PackageId}].TargetDirectory)/runtimePackages/PublishToRepository.ps1"
                        },
                        new Step(StepType.Generic)
                        {
                            OnExecute = $"$(Package[{cmfPackage.PackageId}].TargetDirectory)/runtimePackages/PublishToDirectory.ps1"
                        },

                        new Step(StepType.DeployRepositoryFiles)
                        {
                            ContentPath = "runtimePackages/**"
                        },
                        new Step(StepType.GenerateRepositoryIndex)
                    }.Where(step =>
                            // if MES version is inferior to 8.3.5, the DeployRepositoryFiles and GenerateRepositoryIndex steps are not supported
                            targetVersion.CompareTo(minimumVersion) >= 0 || step.Type != StepType.DeployRepositoryFiles && step.Type != StepType.GenerateRepositoryIndex
                        ).ToList()

            );

            DefaultContentToIgnore.AddRange(new List<string>()
            {
                "gulpfile.js",
                "package-lock.json",
                "package.json",
                "packConfig.json",
                "README.md"
            });

            BuildSteps = buildCommands;

            cmfPackage.DFPackageType = PackageType.Presentation;
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
        /// Bumps the specified CMF package.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="bumpInformation">The bump information.</param>
        /// <exception cref="CliException"></exception>
        public override void Bump(string version, string buildNr, Dictionary<string, object> bumpInformation = null)
        {
            base.Bump(version, buildNr, bumpInformation);

            #region GetCustomPackages

            // Get Dev Tasks
            string parentDirectory = CmfPackage.GetFileInfo().DirectoryName;
            string devTasksFile = this.fileSystem.Directory.GetFiles(parentDirectory, ".dev-tasks.json")[0];

            string devTasksJson = this.fileSystem.File.ReadAllText(devTasksFile);
            dynamic devTasksJsonObject = JsonConvert.DeserializeObject(devTasksJson);

            string packageNames = devTasksJsonObject["packagesBuildBump"]?.ToString();

            if (string.IsNullOrEmpty(packageNames))
            {
                packageNames = devTasksJsonObject["packages"]?.ToString();
            }

            if (string.IsNullOrEmpty(packageNames))
            {
                throw new CliException(string.Format(CliMessages.MissingMandatoryProperty, packageNames));
            }

            #endregion GetCustomPackages

            // IoT -> src -> Package XPTO
            IoTUtilities.BumpIoTCustomPackages(CmfPackage.GetFileInfo().DirectoryName, version, buildNr, packageNames, this.fileSystem);
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
                        IFileInfo packConfig = this.fileSystem.FileInfo.New($"{inputDirPath}/packconfig.json");
                        if (!packConfig.Exists)
                        {
                            Log.Warning("packconfig.json doesn't exist! packagePacker will not run.");
                            continue;
                        }
                        Log.Debug("Running Package Packer");

                        string outputDirPath = $"{packageOutputDir}/runtimePackages";

                        // Is not Supported in workspaces
                        if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major < 10)
                        {
                            NPMCommand npmCommand = new NPMCommand()
                            {
                                DisplayName = "npm shrinkwrap",
                                Args = new string[] { "shrinkwrap" },
                                WorkingDirectory = packDirectory
                            };

                            npmCommand.Exec();
                        }

                        CmdCommand cmdCommand = new CmdCommand()
                        {
                            DisplayName = "npx yo @criticalmanufacturing/iot:packagePacker",
                            Command = "npx",
                            Args = new string[] { "yo @criticalmanufacturing/iot:packagePacker", $"-i \"{inputDirPath}\"", $"-o \"{outputDirPath}\"" },
                            WorkingDirectory = packDirectory
                        };

                        cmdCommand.Exec();
                    }
                }
                else if (contentToPack.Action == PackAction.Untar)
                {
                    IFileInfo tgzFile = this.fileSystem.FileInfo.New($"{CmfPackage.GetFileInfo().Directory.FullName}/{contentToPack.Source}");
                    CmdCommand cmdCommand = new CmdCommand()
                    {
                        DisplayName = "tar -xzvf",
                        Command = "tar",
                        Args = new string[] { $"-xzvf {tgzFile.FullName}" },
                        WorkingDirectory = packageOutputDir
                    };

                    cmdCommand.Exec();

                    dynamic packageJson = tgzFile.Directory.GetFile(CoreConstants.PackageJson);

                    string packDirectoryName = packageJson == null ? tgzFile.Directory.Name : packageJson.name;

                    IDirectoryInfo packageDirectory = this.fileSystem.DirectoryInfo.New($"{packageOutputDir}/package");
                    IDirectoryInfo destinationDirectory = this.fileSystem.DirectoryInfo.New($"{packageOutputDir}/{contentToPack.Target}/{packDirectoryName}");
                    destinationDirectory.Parent.Create();
                    packageDirectory.MoveTo(destinationDirectory.FullName);
                }
            }

            base.Pack(packageOutputDir, outputDir);
        }
    }
}
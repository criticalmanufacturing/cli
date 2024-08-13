 using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Generates IoT package structure
    /// </summary>
    [CmfCommand("iot", ParentId = "new", Id = "new_iot")]
    public class IoTCommand : LayerTemplateCommand
    {
        private string baseWebPackage = null;

        /// <summary>
        /// constructor
        /// </summary>
        public IoTCommand() : base("iot", PackageType.IoT)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem">the filesystem implementation</param>
        public IoTCommand(IFileSystem fileSystem) : base("iot", PackageType.IoT, fileSystem)
        {
        }

        public override void Configure(Command cmd)
        {
            base.GetBaseCommandConfig(cmd);

            cmd.AddOption(new Option<string>(
                aliases: new[] { "--htmlPackageLocation" },
                description: "Location of the HTML Package"
            ));

            cmd.AddOption(new Option<string>(
                aliases: new[] { "--isAngularPackage" },
                description: "Customization package with angular"
            ));
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, string, bool>(this.Execute);
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args)
        {
            var npmRegistry = ExecutionContext.Instance.ProjectConfig.NPMRegistry;
            var devTasksVersion = ExecutionContext.Instance.ProjectConfig.DevTasksVersion;
            var repoType = ExecutionContext.Instance.ProjectConfig.RepositoryType ?? CliConstants.DefaultRepositoryType;
            Log.Debug($"Creating IoT Package at {workingDir} for repo type {repoType} using registry {npmRegistry}");

            // calculate relative path to local environment and create a new symbol for it
            var relativePathToRoot =
                this.fileSystem.Path.Join("..", "..", //always two levels deeper, because we are targeting the inner cmfpackage.json, which is one level down from the IoT root
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        projectRoot.FullName)
                ).Replace("\\", "/");

            var packageName = base.GeneratePackageName(workingDir)!.Value.Item1;

            if (!string.IsNullOrEmpty(devTasksVersion?.ToString()))
            {
                args.AddRange(new[] { "--DevTasksVersion", devTasksVersion.ToString() });
            }

            args.AddRange(new[]
            {
                "--iotdata", $"{packageName}.Data",
                "--iotpackages", $"{packageName}.Packages",
                "--rootInnerRelativePath", relativePathToRoot,
                "--npmRegistry", npmRegistry.OriginalString,
                "--repositoryType", repoType.ToString()
            });

            return args;
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir">nearest root package</param>
        /// <param name="version">package version</param>
        /// <param name="htmlPackageLocation">location of html package</param>
        public void Execute(IDirectoryInfo workingDir, string version, string htmlPackageLocation, bool isAngularPackage)
        {
            if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major > 9)
            {
                // only introduced in 10.2.7
                if (!isAngularPackage && ExecutionContext.Instance.ProjectConfig.MESVersion.Major > 10 || (ExecutionContext.Instance.ProjectConfig.MESVersion.Major == 10 && ExecutionContext.Instance.ProjectConfig.MESVersion.Minor >= 2 &&
                        ExecutionContext.Instance.ProjectConfig.MESVersion.Build >= 7))
                {
                    // Automation Task Library Package
                    this.ExecuteV10ATL(workingDir, version);
                }
                else
                {
                    this.ExecuteV10AngularPackage(workingDir, version, htmlPackageLocation);
                }
            }
            else
            {
                this.CommandName = "iot-upto9x";
                base.Execute(workingDir, version);
            }
        }

        public void ExecuteV10ATL(IDirectoryInfo workingDir, string version)
        {
            this.CommandName = "iot-from1000-atl";
            base.Execute(workingDir, version); // create package base - generate cmfpackage.json

            var packageName = base.GeneratePackageName(workingDir)!.Value.Item1;

            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{workingDir}/{packageName}/{CliConstants.CmfPackageFileName}");
            var cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true, this.fileSystem);
            cmfPackage.LoadDependencies(null, null, true);

            var iotCustomPackage = cmfPackage.Dependencies.FirstOrDefault(package => package.CmfPackage?.PackageType == PackageType.IoT).CmfPackage;

            var iotRoot = cmfPackage.GetFileInfo().Directory;
            var iotCustomPackageWorkDir = iotCustomPackage.GetFileInfo().Directory;
            var iotCustomPackageName = base.GeneratePackageName(iotCustomPackageWorkDir)!.Value.Item1;

            var mesVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;

            // Install IoT Yeoman
            Log.Debug($"Installing Yeoman");

            new NPMCommand()
            {
                DisplayName = "npm yeoman",
                Args = new string[] { "install", "yo@4.3.1", "--save-dev" },
                WorkingDirectory = iotCustomPackageWorkDir
            }.Exec();

            Log.Debug($"Installing Yeoman IoT Generator");

            new NPMCommand()
            {
                DisplayName = "npm yeoman generator-iot",
                Args = new string[] { "install", $"@criticalmanufacturing/generator-iot@{mesVersion.Major}{mesVersion.Minor}x", "--save-dev" },
                WorkingDirectory = iotCustomPackageWorkDir
            }.Exec();

            Log.Information($"Feel free to create your task libraries by running npm run generateTaskLibrary");
        }

        public void ExecuteV10AngularPackage(IDirectoryInfo workingDir, string version, string htmlPackageLocation)
        {
            if (string.IsNullOrEmpty(htmlPackageLocation))
            {
                throw new CliException(CliMessages.IoTV10HTMLPackageMustBeProvided);
            }

            var ngxSchematicsVersion = ExecutionContext.Instance.ProjectConfig.NGXSchematicsVersion;

            if (ngxSchematicsVersion == null)
            {
                throw new CliException("Seems like the repository scaffolding was run on a previous version of MES. Please re-init for versions 10+.");
            }

            IDirectoryInfo htmlPackageDir = fileSystem.DirectoryInfo.New(htmlPackageLocation);

            if (!htmlPackageDir.Exists) throw new CliException(string.Format(CliMessages.SomePackagesNotFound, string.Join(", ", htmlPackageLocation)));

            var baseLayer = ExecutionContext.Instance.ProjectConfig.BaseLayer ?? CliConstants.DefaultBaseLayer;
            this.baseWebPackage = baseLayer == BaseLayer.MES
                ? "@criticalmanufacturing/mes-ui-web"
                : "@criticalmanufacturing/core-ui-web";
            Log.Debug($"Project is targeting base layer {baseLayer}, so scaffolding with base web package {baseWebPackage}");

            this.CommandName = "iot-from1000-angular";
            base.Execute(workingDir, version); // create package base - generate cmfpackage.json

            // this won't return null because it has to success on the base.Execute call
            var ngCliVersion = ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().AngularCLI(ExecutionContext.Instance.ProjectConfig.MESVersion);

            var packageName = base.GeneratePackageName(workingDir)!.Value.Item1;

            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{workingDir}/{packageName}/{CliConstants.CmfPackageFileName}");
            var cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true, this.fileSystem);
            cmfPackage.LoadDependencies(null, null, true);

            var iotCustomPackage = cmfPackage.Dependencies.FirstOrDefault(package => package.CmfPackage?.PackageType == PackageType.IoT).CmfPackage;

            var iotRoot = cmfPackage.GetFileInfo().Directory;
            var iotCustomPackageWorkDir = iotCustomPackage.GetFileInfo().Directory;
            var iotCustomPackageName = base.GeneratePackageName(iotCustomPackageWorkDir)!.Value.Item1;

            var mesVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;

            var schematicsVersion = ngxSchematicsVersion.ToString() ?? $"@release-{mesVersion.Major}{mesVersion.Minor}{mesVersion.Build}";

            Log.Debug($"Creating new IoT Workspace {packageName}");
            // ng new <packageName> --create-application false
            new NPXCommand()
            {
                Command = $"@angular/cli@{ngCliVersion}",
                Args = new[] { "new", iotCustomPackage.PackageId, "--create-application", "false" },
                WorkingDirectory = iotRoot,
                ForceColorOutput = false
            }.Exec();

            Log.Debug($"Adding @criticalmanufacturing/ngx-iot-schematics@{schematicsVersion} to the package, which can be used to scaffold new components and libraries");
            // cd <packageName>
            // ng add --skip-confirmation @criticalmanufacturing/ngx-schematics [--npmRegistry http://npm.example/] --lint --base-app <Core|MES>
            new NPXCommand()
            {
                Command = $"@angular/cli@{ngCliVersion}",
                Args = new[] { "add", "--registry", ExecutionContext.Instance.ProjectConfig.NPMRegistry.OriginalString, "--skip-confirmation", $"@criticalmanufacturing/ngx-iot-schematics@{schematicsVersion}", "--lint", "--base-app", baseLayer.ToString(), "--version", $"release-{mesVersion.Major}{mesVersion.Minor}" },
                WorkingDirectory = iotCustomPackageWorkDir,
                ForceColorOutput = false
            }.Exec();

            // Install IoT Yeoman
            Log.Debug($"Installing Yeoman");

            new NPMCommand()
            {
                DisplayName = "npm yeoman",
                Args = new string[] { "install", "yo@4.3.1", "--save-dev" },
                WorkingDirectory = iotCustomPackageWorkDir
            }.Exec();

            Log.Debug($"Installing Yeoman IoT Generator");

            new NPMCommand()
            {
                DisplayName = "npm yeoman generator-iot",
                Args = new string[] { "install", $"@criticalmanufacturing/generator-iot@{mesVersion.Major}{mesVersion.Minor}x", "--save-dev" },
                WorkingDirectory = iotCustomPackageWorkDir
            }.Exec();

            #region Link To HTML Package

            // After building the IoT Package we must ensure to also build the HTML Package
            iotCustomPackage.RelatedPackages = new()
            {
                new RelatedPackage() { Path = fileSystem.Path.GetRelativePath(iotCustomPackageWorkDir.FullName, htmlPackageDir.FullName), PostBuild = true, PostPack = true }
            };

            iotCustomPackage.SaveCmfPackage();

            #endregion Link To HTML Package
        }
    }
}
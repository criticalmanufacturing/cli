using Cmf.Common.Cli.Builders;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Cmf.Common.Cli.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Handlers.PackageTypeHandler" />
    public class DataPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public DataPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            // TargetDirectory with DateTimeStamp to avoid wrong files installation
            cmfPackage.SetDefaultValues
            (
                isUniqueInstall:
                    true,
                targetDirectory:
                    $"installPackages/{cmfPackage.PackageId}.{cmfPackage.Version}-{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                steps:
                    new List<Step>()
                    {
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "**/**"
                        },
                        new Step(StepType.Generic)
                        {
                            OnExecute = $"$(Package[{cmfPackage.PackageId}].TargetDirectory)/RunCustomizationInstallDF.ps1"
                        }
                    }
            );

            BuildSteps = new IBuildCommand[]
            {
                new JSONValidatorCommand()
                {
                    DisplayName = "JSON Validator Command",
                    FilesToValidate = GetContentToPack(this.fileSystem.DirectoryInfo.FromDirectoryName("."))
                }
            };

            DFPackageType = PackageType.Business;
        }

        /// <summary>
        /// Copies the install dependencies.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        protected override void CopyInstallDependencies(IDirectoryInfo packageOutputDir)
        {
            FileSystemUtilities.CopyInstallDependenciesFiles(packageOutputDir, PackageType.Data, this.fileSystem);

            string globalVariablesPath = this.fileSystem.Path.Join(packageOutputDir.FullName, "EnvironmentConfigs", "GlobalVariables.yml");

            string globalVariablesFile = this.fileSystem.File.ReadAllText(globalVariablesPath);
            globalVariablesFile = globalVariablesFile.Replace(CliConstants.TokenVersion, CmfPackage.Version);
            this.fileSystem.File.WriteAllText(globalVariablesPath, globalVariablesFile);

            IFileInfo runCustomizationInstallDF = this.fileSystem.FileInfo.FromFileName(this.fileSystem.Path.Join(packageOutputDir.FullName, "RunCustomizationInstallDF.ps1"));

            string fileContent = runCustomizationInstallDF.ReadToString();

            fileContent = fileContent.Replace(CliConstants.TokenPackageId, CmfPackage.PackageId);

            this.fileSystem.File.WriteAllText(runCustomizationInstallDF.FullName, fileContent);
        }
    }
}
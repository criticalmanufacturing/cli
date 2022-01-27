using Cmf.Common.Cli.Builders;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using System.Collections.Generic;
using Cmf.Common.Cli.Commands.restore;
using System.IO.Abstractions;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Handlers
{

    /// <summary>
    /// 
    /// </summary>
    public class SecurityPortalPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPortalPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        public SecurityPortalPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                targetDirectory:
                    "/app",
                targetLayer:
                    "securityportal",
                steps:
                    new List<Step>
                    {
                        new Step(StepType.TransformFile)
                        {
                            File = "config.json",
                            TagFile = true
                        }
                    }
            );

            BuildSteps = new IBuildCommand[]
            {
            };

            DFPackageType = PackageType.Generic;
        }


        /// <summary>
        /// Packs the specified package output dir.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="outputDir">The output dir.</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            Log.Debug("Generating SecurityPortal config.json");
            string path = $"{packageOutputDir.FullName}/{CliConstants.CmfPackagePresentationConfig}";

            IDirectoryInfo cmfPackageDirectory = CmfPackage.GetFileInfo().Directory;

            dynamic configJson = cmfPackageDirectory.GetFile(CliConstants.CmfPackagePresentationConfig);
            if (configJson != null)
            {
                string packageName = configJson.id;
                string type = configJson.type;
                string metadataUrl = configJson.config.metadataUrl;
                string redirectUrl = configJson.config.redirectUrl;
                string clientId = configJson.config.clientId;

                // Get Template
                string fileContent = GenericUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CmfPackage.PackageType}/{CliConstants.CmfPackagePresentationConfig}");

                fileContent = fileContent.Replace(CliConstants.TokenPackageId, packageName);
                fileContent = fileContent.Replace(CliConstants.StrategyPath, CliConstants.DefaultStrategyPath).Replace(CliConstants.Tenant, FileSystemUtilities.ReadProjectConfig(this.fileSystem).RootElement.GetProperty("Tenant").GetString());
                fileContent = fileContent.Replace(CliConstants.Strategy, type);
                fileContent = fileContent.Replace(CliConstants.MetadataUrl, metadataUrl);
                fileContent = fileContent.Replace(CliConstants.RedirectUrl, redirectUrl);
                fileContent = fileContent.Replace(CliConstants.ClientId, clientId);

                this.fileSystem.File.WriteAllText(path, fileContent);

                GenerateDeploymentFrameworkManifest(packageOutputDir);

                FinalArchive(packageOutputDir, outputDir);

                Log.Information($"{outputDir.FullName}/{CmfPackage.ZipPackageName} created");

            }
            else
            {
                throw new CliException("No config.json was provided");
            }
        }
    }
}
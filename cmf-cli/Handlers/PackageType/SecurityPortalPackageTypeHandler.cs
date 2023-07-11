using System.Collections.Generic;
using System.IO.Abstractions;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Handlers;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Handlers
{

    /// <summary>
    /// Handler for SecurityPortal packages
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
                    "SecurityPortal",
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

            cmfPackage.DFPackageType = PackageType.Business;
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
                string fileContent = ResourceUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CmfPackage.PackageType}/{CliConstants.CmfPackagePresentationConfig}");

                fileContent = fileContent.Replace(CliConstants.TokenPackageId, packageName);
                fileContent = fileContent.Replace(CliConstants.StrategyPath, CliConstants.DefaultStrategyPath).Replace(CliConstants.Tenant, ExecutionContext.Instance.ProjectConfig.Tenant);
                fileContent = fileContent.Replace(CliConstants.Strategy, type);
                fileContent = fileContent.Replace(CliConstants.MetadataUrl, metadataUrl);
                fileContent = fileContent.Replace(CliConstants.RedirectUrl, redirectUrl);
                fileContent = fileContent.Replace(CliConstants.ClientId, clientId);

                this.fileSystem.File.WriteAllText(path, fileContent);

                GenerateDeploymentFrameworkManifest(packageOutputDir);

                FinalArchive(packageOutputDir, outputDir);

                Log.Debug($"{outputDir.FullName}/{CmfPackage.ZipPackageName} created");
                Log.Information($"{CmfPackage.PackageName} packed");

            }
            else
            {
                throw new CliException("No config.json was provided");
            }
        }
    }
}
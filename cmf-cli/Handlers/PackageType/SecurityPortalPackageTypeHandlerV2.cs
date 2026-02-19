using Cmf.CLI.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core;
using Cmf.CLI.Utilities;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;

namespace Cmf.CLI.Handlers
{
    public class SecurityPortalPackageTypeHandlerV2 : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPortalPackageTypeHandlerV2" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        public SecurityPortalPackageTypeHandlerV2(CmfPackage cmfPackage) : base(cmfPackage)
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
                        new(StepType.TransformFile)
                        {
                            File = "config.json",
                            TagFile = true,
                            RelativePath = "./src/"
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
        /// <param name="dryRun">if set to <c>true</c> list the package structure without creating files.</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false)
        {
            Log.Debug("Generating SecurityPortal config.json");
            string path = $"{packageOutputDir.FullName}{Path.DirectorySeparatorChar}{CliConstants.CmfPackageSecurityPortalConfig}";

            IDirectoryInfo cmfPackageDirectory = CmfPackage.GetFileInfo().Directory;

            dynamic configJson = cmfPackageDirectory.GetFile(CliConstants.CmfPackageSecurityPortalConfig);
            if (configJson != null)
            {
                string packageName = configJson.id;
                string type = configJson.type;
                string metadataUrl = configJson.config.metadataUrl;
                string redirectUrl = configJson.config.redirectUrl;
                string clientId = configJson.config.clientId;

                // Get Template
                string fileContent = ResourceUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CmfPackage.PackageType}/{CliConstants.CmfPackageSecurityPortalConfig}");

                fileContent = fileContent.Replace(CliConstants.TokenPackageId, packageName);
                fileContent = fileContent.Replace(CliConstants.StrategyPath, CliConstants.DefaultStrategyPath).Replace(CliConstants.Tenant, ExecutionContext.Instance.ProjectConfig.Tenant);
                fileContent = fileContent.Replace(CliConstants.Strategy, type);
                fileContent = fileContent.Replace(CliConstants.MetadataUrl, metadataUrl);
                fileContent = fileContent.Replace(CliConstants.RedirectUrl, redirectUrl);
                fileContent = fileContent.Replace(CliConstants.ClientId, clientId);

                this.fileSystem.File.WriteAllText(path, fileContent);

                GenerateDeploymentFrameworkManifest(packageOutputDir);

                FinalArchive(packageOutputDir, outputDir);

                Log.Debug($"{outputDir.FullName}{Path.DirectorySeparatorChar}{CmfPackage.ZipPackageName} created");
                Log.Information($"{CmfPackage.PackageName} packed");

            }
            else
            {
                throw new CliException("No config.json was provided");
            }
        }
    }
}

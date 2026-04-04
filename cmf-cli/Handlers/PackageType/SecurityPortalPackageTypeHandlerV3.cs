using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Cmf.CLI.Handlers
{
    public class SecurityPortalPackageTypeHandlerV3 : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPortalPackageTypeHandlerV3" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        public SecurityPortalPackageTypeHandlerV3(CmfPackage cmfPackage) : base(cmfPackage)
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
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            Log.Debug("Generating SecurityPortal package");
            string path = $"{packageOutputDir.FullName}{Path.DirectorySeparatorChar}{CliConstants.CmfPackageSecurityPortalConfig}";

            IDirectoryInfo cmfPackageDirectory = CmfPackage.GetFileInfo().Directory;

            dynamic configJson = cmfPackageDirectory.GetFile(CliConstants.CmfPackageSecurityPortalConfig);

            if (configJson != null)
            {
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
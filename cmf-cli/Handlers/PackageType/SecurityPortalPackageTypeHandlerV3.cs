using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.Collections.Generic;
using System.IO.Abstractions;
using NuGet.Versioning;

namespace Cmf.CLI.Handlers
{
    /// <summary>Preserves Security Portal configuration tokens during packing.</summary>
    public class SecurityPortalPackageTypeHandlerV3 : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPortalPackageTypeHandlerV3" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public SecurityPortalPackageTypeHandlerV3(CmfPackage cmfPackage) : base(cmfPackage)
        {
            var mesVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            var configDirectory = mesVersion >= NuGetVersion.Parse("11.2.5") ? "./dist/" : "./src/";

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
                            RelativePath = configDirectory
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
        /// <param name="dryRun">Whether to list the package without creating files.</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false)
        {
            Log.Debug("Generating SecurityPortal package");

            IDirectoryInfo cmfPackageDirectory = CmfPackage.GetFileInfo().Directory;

            var configJson = cmfPackageDirectory.GetFile(CliConstants.CmfPackageSecurityPortalConfig);

            if (configJson != null)
            {
                foreach (var content in CmfPackage.ContentToPack)
                {
                    content.Target ??= string.Empty;
                }

                base.Pack(packageOutputDir, outputDir, dryRun);
            }
            else
            {
                throw new CliException("No config.json was provided");
            }
        }
    }
}

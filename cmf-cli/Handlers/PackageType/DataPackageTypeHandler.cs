using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

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
                        }
                    }
            );

            DFPackageType = PackageType.Business;
        }

        /// <summary>
        /// Generates the deployment framework manifest.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <exception cref="Cmf.Common.Cli.Utilities.CliException"></exception>
        internal override void GenerateDeploymentFrameworkManifest(IDirectoryInfo packageOutputDir)
        {
            if (this.FilesToPack?.HasAny() ?? false)
            {
                // TOKEN replacement
                foreach (ContentToPack contentToPack in this.CmfPackage.ContentToPack)
                {
                    contentToPack.Target = contentToPack.Target.Contains(CliConstants.TokenVersion) ? contentToPack.Target.Replace(CliConstants.TokenVersion, CmfPackage.GetPropertyValueFromTokenName(CliConstants.TokenVersion).ToString()) : contentToPack.Target;
                }

                var steps = this.FilesToPack.Select(ftp =>
                {
                    var target = this.fileSystem.Path.GetRelativePath(packageOutputDir.FullName, ftp.Target.FullName).Replace('\\', '/');
                    return ftp.ContentToPack.ContentType switch
                    {
                        ContentType.MasterData => new Step(StepType.MasterData)
                        {
                            Order = 30,
                            FilePath = $"../{CmfPackage.TargetDirectory}/{target}",
                            CreateInCollection = false,
                            DeeBasePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.DEE)?.ContentToPack.Target,
                            ChecklistImagePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.ChecklistImages)?.ContentToPack.Target,
                            DocumentFileBasePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.Documents)?.ContentToPack.Target,
                            AutomationWorkflowFileBasePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.AutomationWorkFlows)?.ContentToPack.Target,
                            MappingFileBasePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.Maps)?.ContentToPack.Target,
                            ImportXMLObjectPath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.ExportedObjects)?.ContentToPack.Target,
                            Title = "Master Data"
                        },
                        ContentType.EntityTypes => new Step(StepType.ProcessRules) { Order = 10, ContentPath = target, Title = "Process Rules - Entity Types" },
                        ContentType.ProcessRulesPre => new Step(StepType.ProcessRules) { Order = 20, ContentPath = target, Title = "Process Rules - Before" },
                        ContentType.ProcessRulesPost => new Step(StepType.ProcessRules) { Order = 40, ContentPath = target, Title = "Process Rules - After" },
                        _ => null,
                    };
                }).Where(step => step != null).OrderBy(step => step.Order);

                if (this.CmfPackage.Steps != null)
                {
                    this.CmfPackage.Steps.AddRange(steps);
                }
                else
                {
                    this.CmfPackage.Steps = steps.ToList();
                }
            }
            base.GenerateDeploymentFrameworkManifest(packageOutputDir);
        }
    }
}
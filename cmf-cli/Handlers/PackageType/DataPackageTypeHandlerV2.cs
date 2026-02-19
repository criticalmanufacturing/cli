using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class DataPackageTypeHandlerV2 : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public DataPackageTypeHandlerV2(CmfPackage cmfPackage) : base(cmfPackage)
        {
            Log.Debug("Using Data Handler v2");
            // TargetDirectory with DateTimeStamp to avoid wrong files installation
            cmfPackage.SetDefaultValues
            (
                isUniqueInstall:
                    true,
                waitForIntegrationEntries:
                    true,
                targetDirectory:
                    "BusinessTier",
                targetLayer:
                    "host",
                steps:
                    new List<Step>()
                    {
                        new Step(StepType.Generic)
                        {
                            OnExecute = "$(Agent.Root)/agent/scripts/stop_host.ps1"
                        },
                        new Step(StepType.TransformFile)
                        {
                            File = "Cmf.Foundation.Services.HostService.dll.config",
                            TagFile = true
                        },
                        new Step(StepType.Generic)
                        {
                            OnExecute = "$(Agent.Root)/agent/scripts/start_host.ps1"
                        }
                     });

            BuildSteps = new IBuildCommand[]
            {
                new JSONValidatorCommand()
                {
                    DisplayName = "JSON Validator Command",
                    FilesToValidate = GetContentToPack(fileSystem.DirectoryInfo.New("."))
                },
                new DEEValidatorCommand()
                {
                    DisplayName = "DEE Validator Command",
                    FilesToValidate = GetContentToPack(this.fileSystem.DirectoryInfo.New("."))
                }
            };

            cmfPackage.DFPackageType = PackageType.Business; // necessary because we restart the host during installation

        }

        public override void Build(bool test)
        {
            base.Build(test);
        }

        /// <summary>
        /// Pack a Data package
        /// </summary>
        /// <param name="packageOutputDir">source directory</param>
        /// <param name="outputDir">output directory</param>
        /// <param name="dryRun">if set to <c>true</c> list the package structure without creating files.</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false)
        {
            GenerateHostConfigFile(packageOutputDir);

            base.Pack(packageOutputDir, outputDir, dryRun);
        }

        /// <summary>
        /// Bumps the Base version of the package
        /// </summary>
        /// <param name="version">The new Base version.</param>
        public override void UpgradeBase(string version, string iotVersion, List<string> iotPackagesToIgnore)
        {
            base.UpgradeBase(version, iotVersion, iotPackagesToIgnore);
            UpgradeBaseUtilities.UpdateCSharpProject(this.fileSystem, this.CmfPackage, version, true);

            if (iotVersion == null)
            {
                return;
            }

            UpgradeBaseUtilities.UpdateIoTMasterdatasAndWorkflows(this.fileSystem, this.CmfPackage, iotVersion, iotPackagesToIgnore);
        }

        /// <summary>
        /// Generates the deployment framework manifest.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <exception cref="CliException"></exception>
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
                            FilePath = target,
                            CreateInCollection = false,
                            DeeBasePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.DEE)?.ContentToPack.Target,
                            ChecklistImagePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.ChecklistImages)?.ContentToPack.Target,
                            DocumentFileBasePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.Documents)?.ContentToPack.Target,
                            AutomationWorkflowFileBasePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.AutomationWorkFlows)?.ContentToPack.Target,
                            MappingFileBasePath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.Maps)?.ContentToPack.Target,
                            ImportXMLObjectPath = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.ExportedObjects)?.ContentToPack.Target,
                            TargetPlatform = this.FilesToPack.Find(f => f.ContentToPack.ContentType == ContentType.MasterData && f.ContentToPack.TargetPlatform != null && f.ContentToPack.TargetPlatform == ftp.ContentToPack.TargetPlatform)?.ContentToPack.TargetPlatform
                                ?? MasterDataTargetPlatformType.Self,
                            Title = "Master Data",
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

            this.CmfPackage.Steps = this.CmfPackage.Steps.OrderBy(step => step.FilePath).OrderBy(step => step.Order).ToList();

            base.GenerateDeploymentFrameworkManifest(packageOutputDir);
        }

        /// <summary>
        /// Generates the host configuration file.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        private void GenerateHostConfigFile(IDirectoryInfo packageOutputDir)
        {
            Log.Debug("Generating host Cmf.Foundation.Services.HostService.dll.config");
            string path = $"{packageOutputDir.FullName}/{CliConstants.CmfPackageHostConfig}";

            // Get Template
            string fileContent = ResourceUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/Data/{CliConstants.CmfPackageHostConfig}");
            this.fileSystem.File.WriteAllText(path, fileContent);
        }
    }
}
using Cmf.Common.Cli.Builders;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;

namespace Cmf.Common.Cli.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Handlers.PackageTypeHandler" />
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
                    "host"
            );

            BuildSteps = new IBuildCommand[]
            {
                new JSONValidatorCommand()
                {
                    DisplayName = "JSON Validator Command",
                    FilesToValidate = GetContentToPack(this.fileSystem.DirectoryInfo.FromDirectoryName("."))
                }
            };

            cmfPackage.DFPackageType = PackageType.Business; // necessary because we restart the host during installation
        }

        /// <summary>
        /// Pack a Data package
        /// </summary>
        /// <param name="packageOutputDir">source directory</param>
        /// <param name="outputDir">output directory</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            GenerateHostConfigFile(packageOutputDir);

            base.Pack(packageOutputDir, outputDir);
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
                            FilePath = target,
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
            string fileContent = GenericUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/Data/{CliConstants.CmfPackageHostConfig}");
            this.fileSystem.File.WriteAllText(path, fileContent);
        }

        /// <summary>
        /// Copies the install dependencies.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        protected override void CopyInstallDependencies(IDirectoryInfo packageOutputDir)
        {
            IDirectoryInfo dir = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.Join(AppDomain.CurrentDomain.BaseDirectory, CliConstants.FolderInstallDependencies, "Data"));
            IFileInfo generateLBOsFile = dir.GetFiles("GenerateLBOs.ps1")[0];
            string tempPath = fileSystem.Path.Combine(packageOutputDir.FullName, generateLBOsFile.Name);
            generateLBOsFile.CopyTo(tempPath, true);
        }
    }
}

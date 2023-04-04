using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.Json;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Generates the Business layer structure
    /// </summary>
    [CmfCommand("business", ParentId = "new")]
    public class BusinessCommand : LayerTemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public BusinessCommand() : base("business", PackageType.Business)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public BusinessCommand(IFileSystem fileSystem) : base("business", PackageType.Business, fileSystem)
        {
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig)
        {
            var mesVersion = projectConfig.RootElement.GetProperty("MESVersion").GetString();
            var includeMESNugets = true;
            
            var version = Version.Parse(mesVersion);
            if (version.Major > 8)
            {
                this.CommandName = "business9";
                var bl = projectConfig.RootElement.GetProperty("BaseLayer").GetString();
                var couldParse = Enum.TryParse<BaseLayer>(bl, out var baseLayerValue);
                var baseLayer = couldParse ? baseLayerValue : CliConstants.DefaultBaseLayer;
                includeMESNugets = baseLayer == BaseLayer.MES;
                Log.Debug($"Project is targeting base layer {baseLayer} ({bl} {couldParse} {baseLayerValue}), so scaffolding {(includeMESNugets ? "with" : "without")} MES nugets.");
            }

            // calculate relative path to local environment and create a new symbol for it
            var relativePathToLocalEnv =
                this.fileSystem.Path.Join("..", "..", //always two levels deep, this is the depth of the business solution projects
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        this.fileSystem.Path.Join(projectRoot.FullName, "LocalEnvironment"))
                );
            var relativePathToDeploymentMetadata =
                this.fileSystem.Path.Join("..", //always one levels deep, this is the depth of the business solution cmfpackage.json
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        this.fileSystem.Path.Join(projectRoot.FullName, "DeploymentMetadata"))
                ).Replace("\\", "/");

            args.AddRange(new []
            {
                "--MESVersion", mesVersion,
                "--localEnvRelativePath", relativePathToLocalEnv,
                "--deploymentMetadataRelativePath", relativePathToDeploymentMetadata,
                "--includeMESNugets", includeMESNugets.ToString()
            });
            return args;
        }
    }
}
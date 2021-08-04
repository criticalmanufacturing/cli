using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// Generates the Business layer structure
    /// </summary>
    [CmfCommand("business", Parent = "new")]
    public class BusinessCommand : LayerTemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public BusinessCommand() : base("business", "Cmf.Custom.Business")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public BusinessCommand(IFileSystem fileSystem) : base("business", "Cmf.Custom.Business", fileSystem)
        {
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig)
        {
            var mesVersion = projectConfig.RootElement.GetProperty("MESVersion").GetString();
            
            // calculate relative path to local environment and create a new symbol for it
            var relativePathToLocalEnv =
                this.fileSystem.Path.Join("..", "..", //always two levels deep, this is the depth of the business solution projects
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        this.fileSystem.Path.Join(projectRoot.FullName, "LocalEnviroment"))
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
                "--deploymentMetadataRelativePath", relativePathToDeploymentMetadata
            });
            return args;
        }
    }
}
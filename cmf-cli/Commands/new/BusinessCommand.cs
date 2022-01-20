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

namespace Cmf.Common.Cli.Commands.New
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
        public BusinessCommand() : base("business", Enums.PackageType.Business)
        {
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig)
        {
            var mesVersion = projectConfig.RootElement.GetProperty("MESVersion").GetString();
            
            // calculate relative path to local environment and create a new symbol for it
            var relativePathToLocalEnv =
                ExecutionContext.Instance.FileSystem.Path.Join("..", "..", //always two levels deep, this is the depth of the business solution projects
                    ExecutionContext.Instance.FileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        ExecutionContext.Instance.FileSystem.Path.Join(projectRoot.FullName, "LocalEnvironment"))
                );
            var relativePathToDeploymentMetadata =
                ExecutionContext.Instance.FileSystem.Path.Join("..", //always one levels deep, this is the depth of the business solution cmfpackage.json
                    ExecutionContext.Instance.FileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        ExecutionContext.Instance.FileSystem.Path.Join(projectRoot.FullName, "DeploymentMetadata"))
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
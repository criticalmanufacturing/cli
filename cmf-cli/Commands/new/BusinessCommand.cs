using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Generates the Business layer structure
    /// </summary>
    [CmfCommand("business", ParentId = "new")]
    public class BusinessCommand : LayerTemplateCommand
    {
        public bool AddApplicationVersionAssembly { get; set; } = false;

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

        /// <summary>
        /// Configure the business command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddOption(new Option<bool>(
                aliases: new[] { "--addApplicationVersionAssembly", "-av" },
                description: "Will add application version project to the final solution if project is an app"
            ));
            base.GetBaseCommandConfig(cmd);

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, bool>(this.Execute);
        }

        /// <summary>
        /// Executes business command
        /// </summary>
        /// <param name="workingDir">Working directory</param>
        /// <param name="version">Version</param>
        /// <param name="addApplicationVersionAssembly">Indicates if project creation should include application version assembly</param>
        /// <exception cref="CliException"></exception>
        public void Execute(IDirectoryInfo workingDir, string version, bool addApplicationVersionAssembly)
        {
            if (ExecutionContext.Instance.ProjectConfig.RepositoryType != RepositoryType.App && addApplicationVersionAssembly)
            {
                throw new CliException("Application version assembly should only be included in app projects.");
            }

            AddApplicationVersionAssembly = addApplicationVersionAssembly;
            base.Execute(workingDir, version);
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args)
        {
            var mesVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            var includeMESNugets = true;
            
            if (mesVersion.Major > 8)
            {
                this.CommandName = "business9";
                var baseLayer = ExecutionContext.Instance.ProjectConfig.BaseLayer ?? CliConstants.DefaultBaseLayer;
                includeMESNugets = baseLayer == BaseLayer.MES;
                Log.Debug($"Project is targeting base layer {baseLayer}, so scaffolding {(includeMESNugets ? "with" : "without")} MES nugets.");

                bool isProjectApp = ExecutionContext.Instance.ProjectConfig.RepositoryType == RepositoryType.App;

                if (isProjectApp)
                {
                    args.AddRange(new[]
                    {
                        "--app", isProjectApp.ToString(),
                        "--fileVersion", $"{mesVersion}.0",
                        "--assemblyVersion", $"{mesVersion.Major}.{mesVersion.Minor}.0.0",
                        "--addApplicationVersionAssembly", AddApplicationVersionAssembly.ToString()
                    });
                }
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
                "--MESVersion", mesVersion.ToString(),
                "--localEnvRelativePath", relativePathToLocalEnv,
                "--deploymentMetadataRelativePath", relativePathToDeploymentMetadata,
                "--includeMESNugets", includeMESNugets.ToString()
            });
            return args;
        }
    }
}
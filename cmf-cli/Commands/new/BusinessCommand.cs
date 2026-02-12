using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Threading.Tasks;
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
            var addApplicationVersionAssemblyOption = new Option<bool>("--addApplicationVersionAssembly", "-av")
            {
                Description = "Will add application version project to the final solution if project is an app"
            };
            cmd.Options.Add(addApplicationVersionAssemblyOption);

            var (workingDirArg, versionOpt) = base.GetBaseCommandConfig(cmd);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var workingDir = parseResult.GetValue(workingDirArg);
                var version = parseResult.GetValue(versionOpt);
                var addApplicationVersionAssembly = parseResult.GetValue(addApplicationVersionAssemblyOption);

                Execute(workingDir, version, addApplicationVersionAssembly);
                return Task.FromResult(0);
            });
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
            if (Core.Objects.ExecutionContext.Instance.ProjectConfig.RepositoryType != RepositoryType.App && addApplicationVersionAssembly)
            {
                throw new CliException("Application version assembly should only be included in app projects.");
            }

            AddApplicationVersionAssembly = addApplicationVersionAssembly;
            base.Execute(workingDir, version);
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args)
        {
            var mesVersion = Core.Objects.ExecutionContext.Instance.ProjectConfig.MESVersion;
            var includeMESNugets = true;
            
            if (mesVersion.Major > 8)
            {
                this.CommandName = "business9";
                var baseLayer = Core.Objects.ExecutionContext.Instance.ProjectConfig.BaseLayer ?? CliConstants.DefaultBaseLayer;
                includeMESNugets = baseLayer == BaseLayer.MES;
                Log.Debug($"Project is targeting base layer {baseLayer}, so scaffolding {(includeMESNugets ? "with" : "without")} MES nugets.");

                args.AddRange(new []{ "--targetFramework",  mesVersion.Major >= 11 ? "net8.0" : "net6.0" });

                if (Core.Objects.ExecutionContext.Instance.ProjectConfig.RepositoryType == RepositoryType.App)
                {
                    var appData = Core.Objects.ExecutionContext.Instance.AppData ??
                        throw new CliException("Could not retrieve repository AppData.");
                    args.AddRange(new[]
                    {
                        "--app", "true",
                        "--licensedAppName", appData.licensedApplication,
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
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Text.Json;
using System.Linq;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// Layer Template Abstract Command
    /// provides arguments and execution flow common to all layer templates
    /// </summary>
    public abstract class LayerTemplateCommand : TemplateCommand
    {
        // private string packagePrefix;
        private PackageType packageType;

        /// <summary>
        /// should register the newly created package in its parent as dependency
        /// </summary>
        protected bool registerInParent = true;
        /// <summary>
        /// Arguments used to execute the template. Available after Execute runs.
        /// </summary>
        protected string[] executedArgs;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="commandName">the name of the command</param>
        /// <param name="packageType">the package type</param>
        protected LayerTemplateCommand(string commandName, PackageType packageType) : base(commandName)
        {
            this.packageType = packageType;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="commandName">the name of the command</param>
        /// <param name="packageType">the package type</param>
        /// <param name="fileSystem">the filesystem implementation</param>
        protected LayerTemplateCommand(string commandName, PackageType packageType, IFileSystem fileSystem) : base(commandName, fileSystem)
        {
            this.packageType = packageType;
        }

        /// <summary>
        /// configure the command
        /// </summary>
        /// <param name="cmd">base command</param>
        public override void Configure(Command cmd)
        {
            GetBaseCommandConfig(cmd);
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string>(Execute);
        }

        /// <summary>
        /// Injects the base arguments and options into the command, required for layer commands
        /// </summary>
        /// <param name="cmd">base command</param>
        protected void GetBaseCommandConfig(Command cmd)
        {
            var nearestRootPackage = FileSystemUtilities.GetPackageRootByType(
                this.fileSystem.Directory.GetCurrentDirectory(),
                PackageType.Root,
                this.fileSystem
            );

            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, nearestRootPackage?.FullName),
                isDefault: true
            )
            {
                Description = "Working Directory"
            });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--version" },
                description: "Package Version",
                getDefaultValue: () => "1.0.0"
            ));
        }

        protected (string, string)? GeneratePackageName(IDirectoryInfo workingDir)
        {

            string featureName = null;
            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem, throwException: true);

            //load .project-config
            var projectConfig = FileSystemUtilities.ReadProjectConfig(this.fileSystem);
            var organization = Constants.CliConstants.DefaultOrganization;
            var product = Constants.CliConstants.DefaultProduct;
            if (projectConfig.RootElement.TryGetProperty("Organization", out JsonElement element))
            {
                organization = element.GetString();
            }
            if (projectConfig.RootElement.TryGetProperty("Product", out JsonElement element2))
            {
                product = element2.GetString();
            }
            var packageName = $"{organization}.{product}.{this.packageType}";

            if (string.Equals(projectRoot.FullName, workingDir.FullName))
            {
                // is a root-level package.
                var featuresPath = this.fileSystem.Path.Join(projectRoot.FullName, "Features");
                if (this.fileSystem.Directory.Exists(featuresPath))
                {
                    throw new CliException($"Cannot create a root-level layer package when features already exist.");
                }
            }
            else
            {
                // is a feature-level package
                var package = base.GetPackageInFolder(workingDir.FullName);
                if (package.PackageId.Count(e => e == '.') < 2)
                {
                    featureName = package.PackageId;
                }
                else
                {
                    featureName = string.Join('.', package.PackageId.Split('.').Skip(2));
                }
                packageName = $"{organization}.{product}.{featureName}.{this.packageType}";
            }

            return (packageName, featureName);
        }


        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir">the nearest root package</param>
        /// <param name="version">the package version</param>
        public void Execute(IDirectoryInfo workingDir, string version)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            if (workingDir == null)
            {
                throw new CliException("This command needs to run inside a project. Run `cmf init` to create a new project.");
            }

            var names = this.GeneratePackageName(workingDir);
            if (names == null)
            {
                return;
            }
            var (packageName, featureName) = names.Value;

            //load .project-config
            var projectConfig = FileSystemUtilities.ReadProjectConfig(this.fileSystem);
            var tenant = projectConfig.RootElement.GetProperty("Tenant").GetString();
            var args = new List<string>()
            {
                // engine options
                "--output", workingDir.FullName,
                
                // template symbols
                "--name", packageName,
                "--packageVersion", version,
                "--idSegment", featureName != null ? $"{tenant}.{featureName}" : tenant,
                "--Tenant", tenant
            };

            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            args = this.GenerateArgs(projectRoot, workingDir, args, projectConfig);
            this.executedArgs = args.ToArray();
            base.RunCommand(args);
            if (registerInParent)
            {
                base.RegisterAsDependencyInParent(packageName, version, workingDir.FullName);
            }
        }

        /// <summary>
        /// generates additional arguments for the templating engine
        /// </summary>
        /// <param name="projectRoot">the project root</param>
        /// <param name="workingDir">the current feature root (project root if no features exist)</param>
        /// <param name="args">the base arguments: output, package name, version, id segment and tenant</param>
        /// <param name="projectConfig">a JsonDocument with the .project-config.json content</param>
        /// <returns>the complete list of arguments</returns>
        protected abstract List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig);
    }
}
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

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
            var (workingDirArg, versionOpt) = GetBaseCommandConfig(cmd);
            
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var workingDir = parseResult.GetValue(workingDirArg);
                var version = parseResult.GetValue(versionOpt);
                
                Execute(workingDir, version, null);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Injects the base arguments and options into the command, required for layer commands
        /// </summary>
        /// <param name="cmd">base command</param>
        /// <returns>Tuple with the workingDir argument and version option for use in SetAction</returns>
        protected (Argument<IDirectoryInfo>, Option<string>) GetBaseCommandConfig(Command cmd)
        {
            var nearestRootPackage = FileSystemUtilities.GetPackageRootByType(
                this.fileSystem.Directory.GetCurrentDirectory(),
                PackageType.Root,
                this.fileSystem
            );

            var workingDirArgument = new Argument<IDirectoryInfo>("workingDir")
            {
                Description = "Working Directory"
            };
            workingDirArgument.CustomParser = argResult => Parse<IDirectoryInfo>(argResult, nearestRootPackage?.FullName);
            workingDirArgument.DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, nearestRootPackage?.FullName);
            cmd.Arguments.Add(workingDirArgument);

            var versionOption = new Option<string>("--version")
            {
                Description = "Package Version",
                DefaultValueFactory = _ => "1.0.0"
            };
            cmd.Options.Add(versionOption);

            return (workingDirArgument, versionOption);
        }

        protected (string, string) GetOrganizationAndProductFromProjectConfig()
        {
            //load .project-config
            var projectConfig = Core.Objects.ExecutionContext.Instance.ProjectConfig;
            var organization = Constants.CliConstants.DefaultOrganization;
            var product = Constants.CliConstants.DefaultProduct;
            if (!string.IsNullOrWhiteSpace(projectConfig.Organization))
            {
                organization = projectConfig.Organization;
            }
            if (!string.IsNullOrWhiteSpace(projectConfig.Product))
            {
                product = projectConfig.Product;
            }

            return (organization, product);
        }

        protected (string, string)? GeneratePackageName(IDirectoryInfo workingDir)
        {
            var (organization, product) = GetOrganizationAndProductFromProjectConfig();

            var packageName = $"{organization}.{product}.{this.packageType}";

            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem, throwException: true);

            string featureName = null;
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
        public void Execute(IDirectoryInfo workingDir, string version, List<string> args = null)
        {
            using var activity = Core.Objects.ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
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
            var projectConfig = Core.Objects.ExecutionContext.Instance.ProjectConfig;
            var tenant = projectConfig.Tenant;

            var (organization, product) = this.GetOrganizationAndProductFromProjectConfig();

            args =
            [
                .. (new List<string>()
                            {
                                // engine options
                                "--output", workingDir.FullName,
                
                                // template symbols
                                "--name", packageName,
                                "--packageVersion", version,
                                "--idSegment", featureName != null ? $"{tenant}.{featureName}" : tenant,
                                "--Tenant", tenant,
                                "--Product", product,
                                "--Organization", organization
                            }),
                .. (args ?? []),
            ];

            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            args = this.GenerateArgs(projectRoot, workingDir, args);
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
        protected abstract List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args);
    }
}
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Text.Json;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// Layer Template Abstract Command
    /// provides arguments and execution flow common to all layer templates
    /// </summary>
    public abstract class LayerTemplateCommand : TemplateCommand
    {
        private string packagePrefix;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="commandName">the name of the command</param>
        /// <param name="packagePrefix">the package prefix. used as full name if not inside a feature.</param>
        protected LayerTemplateCommand(string commandName, string packagePrefix) : base(commandName)
        {
            this.packagePrefix = packagePrefix;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="commandName">the name of the command</param>
        /// <param name="packagePrefix">the package prefix. used as full name if not inside a feature.</param>
        /// <param name="fileSystem">the filesystem implementation</param>
        protected LayerTemplateCommand(string commandName, string packagePrefix, IFileSystem fileSystem) : base(commandName, fileSystem)
        {
            this.packagePrefix = packagePrefix;
        }
        
        /// <summary>
        /// configure the command
        /// </summary>
        /// <param name="cmd">base command</param>
        public override void Configure(Command cmd)
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
                getDefaultValue: () => "1.1.0"
            ));
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string>(Execute);
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir">the nearest root package</param>
        /// <param name="version">the package version</param>
        public void Execute(IDirectoryInfo workingDir, string version)
        {
            if (workingDir == null)
            {
                Log.Error("This command needs to run inside a project. Run `cmf init` to create a new project.");
                return;
            }

            var packageName = this.packagePrefix;
            string featureName = null;
            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            if (string.Equals(projectRoot.FullName, workingDir.FullName))
            {
                // is a root-level package.
                var featuresPath = this.fileSystem.Path.Join(projectRoot.FullName, "Features");
                if (this.fileSystem.Directory.Exists(featuresPath))
                {
                    Log.Error($"Cannot create a root-level layer package when features already exist.");
                    return;
                }
            }
            else
            {
                // is a feature-level package
                var package = base.GetPackageInFolder(workingDir.FullName);
                featureName = package.PackageId.Replace("Cmf.Custom.", "");
                packageName = $"{this.packagePrefix}.{featureName}";
            }
            
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
            
            args = this.GenerateArgs(projectRoot, workingDir, args, projectConfig);
            
            base.RunCommand(args);
            base.RegisterAsDependencyInParent(packageName, version, workingDir.FullName);
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
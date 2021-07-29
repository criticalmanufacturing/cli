using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// 
    /// </summary>
    [CmfCommand("business", Parent = "new")]
    public class BusinessCommand : TemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public BusinessCommand() : base("business")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public BusinessCommand(IFileSystem fileSystem) : base("business", fileSystem)
        {
        }

        /// <summary>
        /// configure the command
        /// </summary>
        /// <param name="cmd"></param>
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
        /// <param name="workingDir"></param>
        /// <param name="version"></param>
        public void Execute(IDirectoryInfo workingDir, string version)
        {
            if (workingDir == null)
            {
                Log.Error("This command needs to run inside a project. Run `cmf init` to create a new project.");
                return;
            }

            var packageName = "Cmf.Custom.Business";
            string featureName = null;
            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            if (string.Equals(projectRoot.FullName, workingDir.FullName))
            {
                // is a root-level package.
                var featuresPath = this.fileSystem.Path.Join(projectRoot.FullName, "Features");
                if (this.fileSystem.Directory.Exists(featuresPath))
                {
                    Log.Error($"Cannot create a root-level business package when features already exist.");
                    return;
                }
            }
            else
            {
                // is a feature-level package
                var package = base.GetPackageInFolder(workingDir.FullName);
                featureName = package.PackageId.Replace("Cmf.Custom.", "");
                packageName = $"Cmf.Custom.Business.{featureName}";
            }
            
            //load .project-config
            var projectConfig = FileSystemUtilities.ReadProjectConfig(this.fileSystem);
            var tenant = projectConfig.RootElement.GetProperty("Tenant").GetString();
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
            
            var args = new List<string>()
            {
                // engine options
                "--output", workingDir.FullName,
                
                // template symbols
                "--name", packageName,
                "--packageVersion", version,
                "--idSegment", featureName != null ? $"{tenant}.{featureName}" : tenant,
                "--Tenant", tenant,
                "--MESVersion", mesVersion,
                "--localEnvRelativePath", relativePathToLocalEnv,
                "--deploymentMetadataRelativePath", relativePathToDeploymentMetadata
            };
            
            base.RunCommand(args);
            base.RegisterAsDependencyInParent(packageName, version, workingDir.FullName);
        }
    }
}
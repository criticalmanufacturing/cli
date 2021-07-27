using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// new feature command
    /// </summary>
    [CmfCommand("feature", Parent = "new")]
    public class FeatureCommand : TemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public FeatureCommand() : base("feature")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public FeatureCommand(IFileSystem fileSystem) : base("feature", fileSystem)
        {
        }

        /// <summary>
        /// configure the command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var root = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            cmd.AddArgument(new Argument<string>(
                name: "packageName",
                description: "The Feature package name"
            ));
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, root.FullName ?? "."),
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
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, string>(Execute);
        }

        /// <summary>
        /// execute the command
        /// </summary>
        /// <param name="workingDir"></param>
        /// <param name="packageName"></param>
        /// <param name="version"></param>
        public void Execute(IDirectoryInfo workingDir, string packageName, string version)
        {
            var featurePath = this.fileSystem.Path.Join(workingDir.FullName, "Features");
            var args = new List<string>()
            {
                // engine options
                "--output", featurePath,
                
                // template symbols
                "--name", packageName,
                "--packageVersion", version
            };
            
            this.RunCommand(args);
            
            // add to upper level package
            // {
            //     "id": "Cmf.Custom.Business",
            //     "version": "4.33.0"
            // }
            var parentPackageDir = FileSystemUtilities.GetPackageRoot(this.fileSystem, featurePath);
            var package = CmfPackage.Load(
                this.fileSystem.FileInfo.FromFileName(
                    this.fileSystem.Path.Join(parentPackageDir.FullName, CliConstants.CmfPackageFileName)),
                fileSystem: this.fileSystem);
            package.Dependencies.Add(new Dependency(packageName, version));
            package.SaveCmfPackage();
        }
    }
}
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Commands.New
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
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, root?.FullName),
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
            if (workingDir == null)
            {
                Log.Error("This command needs to run inside a project. Run `cmf init` to create a new project.");
                return;
            }
            var featurePath = this.fileSystem.Path.Join(workingDir.FullName, "Features");
            var args = new List<string>()
            {
                // engine options
                "--output", featurePath,
                
                // template symbols
                "--name", packageName,
                "--packageVersion", version
            };
            
            base.RunCommand(args);
            base.RegisterAsDependencyInParent(packageName, version, featurePath);
        }
    }
}
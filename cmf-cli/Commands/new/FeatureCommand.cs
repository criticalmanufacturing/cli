using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;


namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// new feature command
    /// </summary>
    [CmfCommand("feature", ParentId = "new")]
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

            var packageNameArgument = new Argument<string>("packageName")
            {
                Description = "The Feature package name"
            };
            cmd.Add(packageNameArgument);

            var workingDirArgument = new Argument<IDirectoryInfo>("workingDir")
            {
                Description = "Working Directory",
                CustomParser = argResult => Parse<IDirectoryInfo>(argResult, root?.FullName),
                DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, root?.FullName)
            };
            cmd.Add(workingDirArgument);

            var versionOption = new Option<string>("--version")
            {
                Description = "Package Version",
                DefaultValueFactory = _ => "1.0.0"
            };
            cmd.Add(versionOption);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var workingDir = parseResult.GetValue(workingDirArgument);
                var packageName = parseResult.GetValue(packageNameArgument);
                var version = parseResult.GetValue(versionOption);

                Execute(workingDir, packageName, version);
                return Task.FromResult(0);
            });
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
                throw new CliException("This command needs to run inside a project. Run `cmf init` to create a new project.");
            }
            var featurePath = this.fileSystem.Path.Join(workingDir.FullName, "Features");
            var args = new List<string>()
            {
                // engine options
                "--output", featurePath,
                
                // template symbols
                "--name", packageName,
                "--packageVersion", version,
                "--MESVersion", ExecutionContext.Instance.ProjectConfig.MESVersion.ToString()
            };
            
            base.RunCommand(args);
            base.RegisterAsDependencyInParent(packageName, version, featurePath);
        }
    }
}
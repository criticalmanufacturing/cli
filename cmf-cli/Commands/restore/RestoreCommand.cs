using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Factories;
using Cmf.Common.Cli.Interfaces;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Commands.restore
{
    /// <summary>
    /// Restore package dependencies (declared cmfpackage.json) from repository packages
    /// </summary>
    [CmfCommand("restore")]
    public class RestoreCommand : BaseCommand
    {
        /// <summary>
        /// Configure the Restore command options and arguments 
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddOption(new Option<Uri[]>(
                aliases: new string[] { "-r", "--repos", "--repo" },
                description: "Repositories where dependencies are located (folder)"));
            
            var packageRoot = FileSystemUtilities.GetPackageRoot(ExecutionContext.Instance.FileSystem);
            var arg = new Argument<IDirectoryInfo>(
                name: "packagePath",
                description: "Package path");
            cmd.AddArgument(arg);

            if (packageRoot != null)
            {
                var packagePath = ExecutionContext.Instance.FileSystem.Path.GetRelativePath(ExecutionContext.Instance.FileSystem.Directory.GetCurrentDirectory(), packageRoot.FullName);
                arg.SetDefaultValue(ExecutionContext.Instance.FileSystem.DirectoryInfo.FromDirectoryName(packagePath));
            }
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, Uri[]>(Execute);
        }

        /// <summary>
        /// Execute the restore command
        /// </summary>
        /// <param name="packagePath">The path of the current package folder</param>
        /// <param name="repos">The package repositories URI/path</param>
        public void Execute(IDirectoryInfo packagePath, Uri[] repos)
        {
            IFileInfo cmfpackageFile = ExecutionContext.Instance.FileSystem.FileInfo.FromFileName($"{packagePath}/{CliConstants.CmfPackageFileName}");

            if (repos != null)
            {
                ExecutionContext.Instance.RepositoriesConfig.Repositories.InsertRange(0, repos);
            }

            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);
            packageTypeHandler.RestoreDependencies(ExecutionContext.Instance.RepositoriesConfig.Repositories.ToArray());
        }
    }
}
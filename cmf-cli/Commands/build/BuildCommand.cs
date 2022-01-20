using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Factories;
using Cmf.Common.Cli.Interfaces;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
    [CmfCommand("build")]
    public class BuildCommand : BaseCommand
    {
        /// <summary>
        /// Build command Constructor
        /// </summary>
        public BuildCommand()
        {
        }

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var packageRoot = FileSystemUtilities.GetPackageRoot(ExecutionContext.Instance.FileSystem);
            var packagePath = ".";
            if (packageRoot != null)
            {
                packagePath = ExecutionContext.Instance.FileSystem.Path.GetRelativePath(ExecutionContext.Instance.FileSystem.Directory.GetCurrentDirectory(), packageRoot.FullName);
            }
            var arg = new Argument<IDirectoryInfo>(
                name: "packagePath",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, packagePath),
                isDefault: true)
            {
                Description = "Package Path"
            };

            cmd.AddArgument(arg);
            
            cmd.Handler = CommandHandler.Create<IDirectoryInfo>(Execute);
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        public void Execute(IDirectoryInfo packagePath)
        {
            IFileInfo cmfpackageFile = ExecutionContext.Instance.FileSystem.FileInfo.FromFileName($"{packagePath}/{CliConstants.CmfPackageFileName}");

            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);

            packageTypeHandler.Build();
        }
    }
}
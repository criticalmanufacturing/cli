using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Factories;
using Cmf.Common.Cli.Interfaces;
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
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var packageRoot = FileSystemUtilities.GetPackageRoot(this.fileSystem);
            var arg = new Argument<DirectoryInfo>(
                name: "packagePath",
                description: "Package path");
            cmd.AddArgument(arg);
            if (packageRoot != null)
            {
                var packagePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), packageRoot.FullName);
                arg.SetDefaultValue(new DirectoryInfo(packagePath));
            }
            cmd.Handler = CommandHandler.Create<DirectoryInfo>(Execute);
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        public void Execute(DirectoryInfo packagePath)
        {
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{packagePath}/{CliConstants.CmfPackageFileName}");

            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);

            packageTypeHandler.Build();
        }
    }
}
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Interfaces;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="BaseCommand" />
    [CmfCommand("build", Id = "build")]
    public class BuildCommand : BaseCommand
    {
        /// <summary>
        /// Build command Constructor
        /// </summary>
        public BuildCommand()
        {
        }
        /// <summary>
        /// Build Command Constructor specify fileSystem
        /// Must have this for tests
        /// </summary>
        /// <param name="fileSystem"></param>
        public BuildCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var packageRoot = FileSystemUtilities.GetPackageRoot(this.fileSystem);
            var packagePath = ".";
            if (packageRoot != null)
            {
                packagePath = this.fileSystem.Path.GetRelativePath(this.fileSystem.Directory.GetCurrentDirectory(), packageRoot.FullName);
            }
            var arg = new Argument<IDirectoryInfo>(
                name: "packagePath",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, packagePath),
                isDefault: true)
            {
                Description = "Package Path"
            };

            cmd.AddArgument(arg);

            cmd.AddOption(new Option<bool>(
                aliases: new[] { "--test" },
                description: "Build and Run Unit Tests",
                getDefaultValue: () => false
            ));

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, bool>(Execute);
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        public void Execute(IDirectoryInfo packagePath, bool test = false)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{packagePath}/{CliConstants.CmfPackageFileName}");

            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);

            packageTypeHandler.Build(test);
        }
    }
}
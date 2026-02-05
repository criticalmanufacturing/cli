using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.IO.Abstractions;
using System.Threading.Tasks;

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

            var packagePathArgument = new Argument<IDirectoryInfo>("packagePath")
            {
                Description = "Package Path"
            };
            packagePathArgument.CustomParser = argResult => Parse<IDirectoryInfo>(argResult, packagePath);
            packagePathArgument.DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, packagePath);
            cmd.Arguments.Add(packagePathArgument);

            var testOption = new Option<bool>("--test")
            {
                Description = "Build and Run Unit Tests",
                DefaultValueFactory = _ => false
            };
            cmd.Options.Add(testOption);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var pkgPath = parseResult.GetValue(packagePathArgument);
                var test = parseResult.GetValue(testOption);

                Execute(pkgPath, test);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        public void Execute(IDirectoryInfo packagePath, bool test = false)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{packagePath}/{CliConstants.CmfPackageFileName}");

            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);

            packageTypeHandler.Build(test);
        }
    }
}
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Factories;
using System.CommandLine;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// Performs an upgrade of the current package
    /// </summary>
    /// <seealso cref="BaseCommand" />
    [CmfCommand("upgrade", Id = "upgrade", Description = "Project upgrade utilities")]
    public class UpgradeCommand : BaseCommand
    {
        
        /// <summary>
        /// constructor for System.IO filesystem
        /// </summary>
        public UpgradeCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public UpgradeCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var packagePathArgument = new Argument<IDirectoryInfo>("packagePath")
            {
                Description = "Package path",
                CustomParser = argResult => Parse<IDirectoryInfo>(argResult, ".")
            };
            cmd.Add(packagePathArgument);

            var baseVersionArgument = new Argument<string>("BaseVersion")
            {
                Description = "New framework/MES Version"
            };
            cmd.Add(baseVersionArgument);

            var manifestOption = new Option<string>("--manifest", "-m")
            {
                Description = "The manifest file to use for the upgrade."
            };
            cmd.Add(manifestOption);

            cmd.SetAction((parseResult) =>
            {
                var packagePath = parseResult.GetValue(packagePathArgument);
                var baseVersion = parseResult.GetValue(baseVersionArgument);
                var manifest = parseResult.GetValue(manifestOption);

                Execute(packagePath, baseVersion, manifest);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="version">The new framework/MES version.</param>
        /// <param name="manifest">The manifest file to use for the upgrade.</param>
        public void Execute(IDirectoryInfo packagePath, string version, string manifest = null)
        {
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New(this.fileSystem.Path.Combine(packagePath.FullName, CliConstants.CmfPackageFileName));

            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile);
            packageTypeHandler.Upgrade(version, manifest);
        }
    }
}
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands.restore
{
    /// <summary>
    /// Restore package dependencies (declared cmfpackage.json) from repository packages
    /// </summary>
    [CmfCommand("restore", Id = "restore")]
    public class RestoreCommand : BaseCommand
    {
        #region Constructors

        /// <summary>
        /// Restore Command
        /// </summary>
        public RestoreCommand() : base() { }

        /// <summary>
        /// Restore Command
        /// </summary>
        /// <param name="fileSystem"></param>
        public RestoreCommand(IFileSystem fileSystem) : base(fileSystem) { }

        #endregion Constructors

        /// <summary>
        /// Configure the Restore command options and arguments
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var reposOption = new Option<Uri[]>("--repos", "-r", "--repo")
            {
                Description = "Repositories where dependencies are located (folder)"
            };
            cmd.Options.Add(reposOption);

            var packageRoot = FileSystemUtilities.GetPackageRoot(this.fileSystem);
            var packagePath = ".";
            if (packageRoot != null)
            {
                packagePath = this.fileSystem.Path.GetRelativePath(this.fileSystem.Directory.GetCurrentDirectory(), packageRoot.FullName);
            }

            var packagePathArgument = new Argument<IDirectoryInfo>("packagePath")
            {
                Description = "Package path"
            };
            packagePathArgument.CustomParser = argResult => Parse<IDirectoryInfo>(argResult, packagePath);
            packagePathArgument.DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, packagePath);
            cmd.Arguments.Add(packagePathArgument);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var pkgPath = parseResult.GetValue(packagePathArgument);
                var repos = parseResult.GetValue(reposOption);

                Execute(pkgPath, repos);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Execute the restore command
        /// </summary>
        /// <param name="packagePath">The path of the current package folder</param>
        /// <param name="repos">The package repositories URI/path</param>
        public void Execute(IDirectoryInfo packagePath, Uri[] repos)
        {
            using var activity = Core.Objects.ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{packagePath}/{CliConstants.CmfPackageFileName}");
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);
            if (repos != null)
            {
                Core.Objects.ExecutionContext.Instance.RepositoriesConfig.Repositories.InsertRange(0, repos);
            }
           
            packageTypeHandler.RestoreDependencies(Core.Objects.ExecutionContext.Instance.RepositoriesConfig.Repositories.ToArray());
        }
    }
}
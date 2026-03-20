using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="BaseCommand" />
    [CmfCommand("pack", Id = "pack")]
    public class PackCommand : BaseCommand
    {
        #region Constructors

        /// <summary>
        /// Assemble Command
        /// </summary>
        public PackCommand() : base() { }

        /// <summary>
        /// Assemble Command
        /// </summary>
        /// <param name="fileSystem"></param>
        public PackCommand(IFileSystem fileSystem) : base(fileSystem) { }

        #endregion

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var packageRoot = FileSystemUtilities.GetPackageRoot(this.fileSystem);
            var workingDir = ".";
            if (packageRoot != null)
            {
                workingDir = this.fileSystem.Path.GetRelativePath(this.fileSystem.Directory.GetCurrentDirectory(), packageRoot.FullName);
            }

            var workingDirArgument = new Argument<IDirectoryInfo>("workingDir")
            {
                Description = "Working Directory",
                DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, workingDir),
                CustomParser = argResult => Parse<IDirectoryInfo>(argResult, workingDir)
            };
            cmd.Add(workingDirArgument);

            var outputDirOption = new Option<IDirectoryInfo>("--outputDir", "-o")
            {
                Description = "Output directory for created package",
                DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, $"{workingDir}/Package"),
                CustomParser = argResult => Parse<IDirectoryInfo>(argResult, $"{workingDir}/Package")
            };
            cmd.Add(outputDirOption);

            var forceOption = new Option<bool>("--force", "-f")
            {
                Description = "Overwrite all packages even if they already exists"
            };
            cmd.Add(forceOption);

            var dryRunOption = new Option<bool>("--dry-run")
            {
                Description = "List the final structure of the package without creating files"
            };
            cmd.Add(dryRunOption);

            // Add the handler
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var workDir = parseResult.GetValue(workingDirArgument);
                var outputDir = parseResult.GetValue(outputDirOption);
                var force = parseResult.GetValue(forceOption);
                var dryRun = parseResult.GetValue(dryRunOption);

                Execute(workDir, outputDir, force);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Executes the specified working dir.
        /// </summary>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <param name="dryRun">if set to <c>true</c> list the package structure without creating files.</param>
        /// <returns></returns>
        public void Execute(IDirectoryInfo workingDir, IDirectoryInfo outputDir, bool force, bool dryRun = false)
        {
            using var activity = ExecutionContext.ServiceProvider.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{workingDir}/{CliConstants.CmfPackageFileName}");

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);
            cmfPackage.ValidatePackage();

            Execute(cmfPackage, outputDir, force, dryRun);
        }

        /// <summary>
        /// Executes the specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <param name="dryRun">if set to <c>true</c> list the package structure without creating files.</param>
        /// <returns></returns>
        /// <exception cref="CmfPackageCollection">
        /// </exception>
        public void Execute(CmfPackage cmfPackage, IDirectoryInfo outputDir, bool force, bool dryRun)
        {
            // TODO: Need to review file patterns in contentToPack and contentToIgnore
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);

            IDirectoryInfo packageDirectory = cmfPackage.GetDirectoryInfo();

            #region Output Directories Handling

            if (!dryRun)
            {
                outputDir = FileSystemUtilities.GetOutputDir(cmfPackage, outputDir, force);
                if (outputDir == null)
                {
                    return;
                }
            }

            IDirectoryInfo packageOutputDir = dryRun
                ? this.fileSystem.DirectoryInfo.New($"{packageDirectory}/{cmfPackage.PackageName}")
                : FileSystemUtilities.GetPackageOutputDir(cmfPackage, packageDirectory, this.fileSystem);

            #endregion

            try
            {
                packageTypeHandler.Pack(packageOutputDir, outputDir, dryRun);
            }
            catch (Exception e)
            {
                throw new CliException(e.Message, e);
            }
            finally
            {
                // Clean-Up - only delete temp directory if not in dry-run mode
                if (!dryRun && packageOutputDir.Exists)
                {
                    packageOutputDir.Delete(true);
                }
            }
        }
    }
}
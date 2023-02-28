
using System;
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
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, "."),
                isDefault: true
            )
            {
                Description = "Working Directory"
            });

            cmd.AddOption(new Option<IDirectoryInfo>(
                aliases: new string[] { "-o", "--outputDir" },
                parseArgument: argResult => Parse<IDirectoryInfo>(argResult, "Package"),
                isDefault: true,
                description: "Output directory for created package"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "-f", "--force" },
                description: "Overwrite all packages even if they already exists"));

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, bool>(Execute);
        }

        /// <summary>
        /// Executes the specified working dir.
        /// </summary>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <returns></returns>
        public void Execute(IDirectoryInfo workingDir, IDirectoryInfo outputDir, bool force)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{workingDir}/{CliConstants.CmfPackageFileName}");

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);
            cmfPackage.ValidatePackage();

            Execute(cmfPackage, outputDir, force);
        }

        /// <summary>
        /// Executes the specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <returns></returns>
        /// <exception cref="CmfPackageCollection">
        /// </exception>
        public void Execute(CmfPackage cmfPackage, IDirectoryInfo outputDir, bool force)
        {
            // TODO: Need to review file patterns in contentToPack and contentToIgnore
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);

            IDirectoryInfo packageDirectory = cmfPackage.GetFileInfo().Directory;

            #region Output Directories Handling

            outputDir = FileSystemUtilities.GetOutputDir(cmfPackage, outputDir, force);
            if (outputDir == null)
            {
                return;
            }

            IDirectoryInfo packageOutputDir = FileSystemUtilities.GetPackageOutputDir(cmfPackage, packageDirectory, this.fileSystem);

            #endregion

            try
            {
                packageTypeHandler.Pack(packageOutputDir, outputDir);
            }
            catch (Exception e)
            {
                throw new CliException(e.Message, e);
            }
            finally
            {
                // Clean-Up
                packageOutputDir.Delete(true);
            }
        }
    }
}

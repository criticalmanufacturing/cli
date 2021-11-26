using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Factories;
using Cmf.Common.Cli.Interfaces;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
    [CmfCommand("pack")]
    public class PackCommand : BaseCommand
    {
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
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{workingDir}/{CliConstants.CmfPackageFileName}");

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);

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
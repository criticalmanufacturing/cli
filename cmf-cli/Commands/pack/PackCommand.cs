using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Factories;
using Cmf.Common.Cli.Interfaces;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
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
                getDefaultValue: () => { return this.fileSystem.DirectoryInfo.FromDirectoryName("."); },
                description: "Working Directory"));

            cmd.AddOption(new Option<IDirectoryInfo>(
                aliases: new string[] { "-o", "--outputDir" },
                getDefaultValue: () => { return this.fileSystem.DirectoryInfo.FromDirectoryName("Package"); },
                description: "Output directory for created package"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-r", "--repo" },
                description: "Repository where dependencies are located (url or folder)"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "-f", "--force" },
                description: "Overwrite all packages even if they already exists"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "--skipDependencies" },
                description: "Do not get all dependencies recursively"));

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, string, bool, bool>(Execute);
        }

        /// <summary>
        /// Executes the specified working dir.
        /// </summary>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="repo">The repo.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <param name="skipDependencies"></param>
        /// <returns></returns>
        public void Execute(IDirectoryInfo workingDir, IDirectoryInfo outputDir, string repo, bool force, bool skipDependencies)
        {
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{workingDir}/{CliConstants.CmfPackageFileName}");

            Uri repoUri = repo != null ? new Uri(repo) : null;

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);

            Execute(cmfPackage, outputDir, repoUri, null, force, skipDependencies);
        }

        /// <summary>
        /// Executes the specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="repoUri">The repo URI.</param>
        /// <param name="loadedPackages">The loaded packages.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <param name="skipDependencies"></param>
        /// <returns></returns>
        /// <exception cref="CmfPackageCollection">
        /// </exception>
        public void Execute(CmfPackage cmfPackage, IDirectoryInfo outputDir, Uri repoUri, CmfPackageCollection loadedPackages, bool force, bool skipDependencies)
        {
            // TODO: Need to review file patterns in contentToPack and contentToIgnore
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);

            loadedPackages ??= new CmfPackageCollection();

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

                #region Get Dependencies

                if (!skipDependencies
                    && cmfPackage.Dependencies.HasAny())
                {
                    // Read all local manifests
                    CmfPackageCollection _loadedPackages = packageDirectory.LoadCmfPackagesFromSubDirectories(setDefaultValues: true);
                    _loadedPackages.AddRange(loadedPackages);

                    // TODO: Bulk Copy
                    foreach (var dependency in cmfPackage.Dependencies)
                    {
                        string _dependencyFileName = $"{dependency.Id}.{dependency.Version}.zip";
                        bool dependencyFound = false;

                        // Check if dependency is already in the Output Directory
                        if (outputDir.GetFiles(_dependencyFileName).HasAny())
                        {
                            if (force)
                            {
                                outputDir.GetFiles(_dependencyFileName)[0].Delete();
                            }
                            else
                            {
                                // TODO: Validate if all child Dependencies are also in the OutputDir
                                Log.Information($"Skipping {_dependencyFileName}. Already packed in Output Directory.");
                                continue;
                            }
                        }

                        // Some logic removed is trying to load CmfPackage.json on output repository and generate package again
                        dependencyFound = GenericUtilities.GetPackageFromRepository(outputDir, repoUri, force, dependency.Id, dependency.Version, this.fileSystem);

                        #region Get Loaded Dependencies

                        if (!dependencyFound)
                        {
                            // Search by Packages to Pack
                            CmfPackage dependencyPackage = _loadedPackages.GetDependency(dependency);

                            // cmfpackage.json found, need to pack
                            if (dependencyPackage != null)
                            {
                                dependencyFound = true;
                                Execute(dependencyPackage, outputDir, repoUri, _loadedPackages, force, skipDependencies);
                            }
                        }

                        #endregion

                        if (!dependencyFound)
                        {
                            if (dependency.Mandatory)
                            {
                                throw new CliException(string.Format(CliMessages.MissingMandatoryDependency, dependency));
                            }
                            else
                            {
                                Log.Warning(string.Format(CliMessages.MissingMandatoryDependency, dependency));
                            }
                        }
                    }
                }

                #endregion
            }
            catch (Exception e)
            {
                throw new CliException(e.Message);
            }
            finally
            {
                // Clean-Up
                packageOutputDir.Delete(true);
            }
        }
    }
}
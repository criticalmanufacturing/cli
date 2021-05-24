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

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
    [CmfCommand("pack")]
    public class PackCommand : BaseCommand
    {
        #region Private Methods

        /// <summary>
        /// Gets the output dir.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <returns></returns>
        private static DirectoryInfo GetOutputDir(CmfPackage cmfPackage, DirectoryInfo outputDir, bool force)
        {
            // Create OutputDir
            if (!outputDir.Exists)
            {
                Log.Information($"Creating {outputDir.Name} folder");
                outputDir.Create();
            }
            else
            {
                if (outputDir.GetFiles(cmfPackage.ZipPackageName).HasAny())
                {
                    if (force)
                    {
                        outputDir.GetFiles(cmfPackage.ZipPackageName)[0].Delete();
                    }
                    else
                    {
                        Log.Information($"Skipping {cmfPackage.ZipPackageName}. Already packed in Output Directory.");
                        return null;
                    }
                }
            }

            return outputDir;
        }

        /// <summary>
        /// Gets the package output dir.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="packageDirectory">The package directory.</param>
        /// <returns></returns>
        private static DirectoryInfo GetPackageOutputDir(CmfPackage cmfPackage, DirectoryInfo packageDirectory)
        {
            // Clear and Create packageOutputDir
            DirectoryInfo packageOutputDir = new($"{packageDirectory}/{cmfPackage.PackageName}");
            if (packageOutputDir.Exists)
            {
                packageOutputDir.Delete(true);
                packageOutputDir.Refresh();
            }

            Log.Information($"Generating {cmfPackage.PackageName}");
            packageOutputDir.Create();

            return packageOutputDir;
        }

        #endregion

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddArgument(new Argument<DirectoryInfo>(
                name: "workingDir",
                getDefaultValue: () => { return new("."); },
                description: "Working Directory"));

            cmd.AddOption(new Option<DirectoryInfo>(
                aliases: new string[] { "-o", "--outputDir" },
                getDefaultValue: () => { return new("Package"); },
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
            cmd.Handler = CommandHandler.Create<DirectoryInfo, DirectoryInfo, string, bool, bool>(Execute);
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
        public void Execute(DirectoryInfo workingDir, DirectoryInfo outputDir, string repo, bool force, bool skipDependencies)
        {
            FileInfo cmfpackageFile = new($"{workingDir}/{CliConstants.CmfPackageFileName}");

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
        public void Execute(CmfPackage cmfPackage, DirectoryInfo outputDir, Uri repoUri, CmfPackageCollection loadedPackages, bool force, bool skipDependencies)
        {
            // TODO: Need to review file patterns in contentToPack and contentToIgnore
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);

            loadedPackages ??= new CmfPackageCollection();

            DirectoryInfo packageDirectory = cmfPackage.GetFileInfo().Directory;

            #region Output Directories Handling

            outputDir = GetOutputDir(cmfPackage, outputDir, force);
            if (outputDir == null)
            {
                return;
            }

            DirectoryInfo packageOutputDir = GetPackageOutputDir(cmfPackage, packageDirectory);

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

                        #region Get Dependencies from Dependencies Directory

                        // TODO: Support for nexus repository

                        if (repoUri != null)
                        {
                            if (repoUri.IsDirectory())
                            {
                                DirectoryInfo repoDirectory = new(repoUri.OriginalString);

                                if (repoDirectory.Exists)
                                {
                                    // Search by Packages already Packed
                                    FileInfo[] dependencyFiles = repoDirectory.GetFiles(_dependencyFileName);
                                    dependencyFound = dependencyFiles.HasAny();

                                    if (!dependencyFound)
                                    {
                                        // Search by Packages to Pack
                                        CmfPackage dependencyPackage = repoDirectory.LoadCmfPackagesFromSubDirectories(setDefaultValues: true).GetDependency(dependency);

                                        // cmfpackage.json found, need to pack
                                        if (dependencyPackage != null)
                                        {
                                            dependencyFound = true;
                                            Execute(dependencyPackage, outputDir, repoUri, _loadedPackages, force, skipDependencies);
                                        }
                                    }
                                    else
                                    {
                                        foreach (FileInfo dependencyFile in dependencyFiles)
                                        {
                                            string destDependencyFile = $"{outputDir.FullName}/{dependencyFile.Name}";
                                            dependencyFile.CopyTo(destDependencyFile);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new CliException(CliMessages.UrlsNotSupported);
                            }
                        }

                        #endregion

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
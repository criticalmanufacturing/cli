using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Factories;
using Cmf.Common.Cli.Interfaces;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
    [CmfCommand("publish")]
    public class PublishCommand : BaseCommand
    {
        #region Private Methods

        /// <summary>
        /// Publish Dependencies from one package. recursive operation
        /// </summary>
        /// <param name="outputDir">Destination for the dependencies package and also used for the current package</param>
        /// <param name="repoUri">Source Location where the package dependencies should be downloaded</param>
        /// <param name="packageId">Source Package ID</param>
        /// <param name="packageVersion">Source Package Version</param>
        /// <param name="loadedPackages">List of packages already processed.</param>
        private void PublishDependenciesFromPackage(DirectoryInfo outputDir, Uri repoUri, string packageId, string packageVersion, List<string> loadedPackages)
        {
            loadedPackages ??= new List<string>();
            string dependencyFileName = $"{packageId}.{packageVersion}.zip";
            string packageLocation = $"{outputDir.FullName}/{dependencyFileName}";

            XDocument dFManifest = FileSystemUtilities.GetManifestFromPackage(packageLocation);
            XElement rootNode = dFManifest.Descendants("dependencies").FirstOrDefault();
            if (rootNode == null)
            {
                // No dependencies found for package
                Log.Information($"Package {packageId}.{packageVersion} has no dependencies");
                return;
            }

            foreach (XElement element in rootNode.Elements())
            {
                // Get Dependency for package
                string dependencyId = element.Attribute("id").Value;
                string dependencyVersion = element.Attribute("version").Value;
                if (!loadedPackages.Contains($"{dependencyId}.{dependencyVersion}"))
                {
                    Log.Information($"Get Dependency Package {packageId}.{packageVersion}...");
                    loadedPackages.Add($"{dependencyId}.{dependencyVersion}");
                    PublishPackageToOutput(outputDir, repoUri, dependencyId, dependencyVersion);
                    PublishDependenciesFromPackage(outputDir, repoUri, dependencyId, dependencyVersion, loadedPackages);
                }
            }

        }

        /// <summary>
        /// Publish a package to the output directory
        /// </summary>
        /// <param name="outputDir">Destiny for the package</param>
        /// <param name="repoUri">Source Location where the package should be downloaded</param>
        /// <param name="packageId">Package Id to publish</param>
        /// <param name="packageVersion">Package version to publish</param>
        /// <returns>True if package was coppied </returns>
        private void PublishPackageToOutput(DirectoryInfo outputDir, Uri repoUri, string packageId, string packageVersion)
        {
            string _dependencyFileName = $"{packageId}.{packageVersion}.zip";
            string destDependencyFile = $"{outputDir.FullName}/{_dependencyFileName}";

            if (File.Exists(destDependencyFile))
            {
                Log.Information($"Package {packageId}.{packageVersion} already in output directory");
                return;
            }

            bool packageFound = GenericUtilities.GetPackageFromRepository(outputDir, repoUri, true, packageId, packageVersion);

            if (!packageFound)
            {
                throw new CliException(string.Format(CliMessages.MissingMandatoryDependency, packageId, packageVersion));
            }
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

            // Add the handler
            cmd.Handler = CommandHandler.Create<DirectoryInfo, DirectoryInfo, string>(Execute);
        }

        /// <summary>
        /// Executes the specified working dir.
        /// </summary>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="repo">The repo.</param>
        /// <returns></returns>
        public void Execute(DirectoryInfo workingDir, DirectoryInfo outputDir, string repo)
        {
            FileInfo cmfpackageFile = new($"{workingDir}/{CliConstants.CmfPackageFileName}");

            Uri repoUri = repo != null ? new Uri(repo) : null;

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);

            Execute(cmfPackage, outputDir, repoUri);
        }

        /// <summary>
        /// Executes the specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="repoUri">The repo URI.</param>
        /// <returns></returns>
        /// <exception cref="CmfPackageCollection">
        /// </exception>
        public void Execute(CmfPackage cmfPackage, DirectoryInfo outputDir, Uri repoUri)
        {
            #region Output Directories Handling

            DirectoryInfo packageDirectory = cmfPackage.GetFileInfo().Directory;
            outputDir = FileSystemUtilities.GetOutputDir(cmfPackage, outputDir, true);
            if (outputDir == null)
            {
                return;
            }

            DirectoryInfo packageOutputDir = FileSystemUtilities.GetPackageOutputDir(cmfPackage, packageDirectory);

            #endregion

            try
            {
                PublishPackageToOutput(outputDir, repoUri, cmfPackage.PackageId, cmfPackage.Version);

                PublishDependenciesFromPackage(outputDir, repoUri, cmfPackage.PackageId, cmfPackage.Version, null);
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
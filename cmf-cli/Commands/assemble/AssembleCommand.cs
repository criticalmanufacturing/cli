using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Linq;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// This command will be responsible for assembling a package based on a given cmfpackage and respective dependencies
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
    [CmfCommand("assemble")]
    public class AssembleCommand : BaseCommand
    {
        #region Private Properties

        /// <summary>
        /// Packages names and Uri to saved in a file in the end of the command execution
        /// </summary>
        private readonly Dictionary<string, string> packagesLocation = new();

        #endregion

        #region Constructors

        /// <summary>
        /// Assemble Command
        /// </summary>
        public AssembleCommand() : base() { }

        /// <summary>
        /// Assemble Command
        /// </summary>
        /// <param name="fileSystem"></param>
        public AssembleCommand(IFileSystem fileSystem) : base(fileSystem) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, "."),
                isDefault: true)
            {
                Description = "Working Directory"
            });

            cmd.AddOption(new Option<IDirectoryInfo>(
                aliases: new string[] { "-o", "--outputDir" },
                parseArgument: argResult => Parse<IDirectoryInfo>(argResult, "Assemble"),
                isDefault: true,
                description: "Output directory for assembled package"));

            cmd.AddOption(new Option<Uri>(
                aliases: new string[] { "--cirepo" },
                description: "Repository where Continuous Integration packages are located (url or folder)"));

            cmd.AddOption(new Option<Uri[]>(
                aliases: new string[] { "-r", "--repos", "--repo" },
                description: "Repository or repositories where published dependencies are located (url or folder)"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "--includeTestPackages" },
                description: "Include test packages on assemble"));

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, Uri, Uri[], bool>(Execute);
        }

        /// <summary>
        /// Executes the specified working dir.
        /// </summary>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="ciRepo"></param>
        /// <param name="repos">The repo.</param>
        /// <param name="includeTestPackages">True to publish test packages</param>
        /// <returns></returns>
        public void Execute(IDirectoryInfo workingDir, IDirectoryInfo outputDir, Uri ciRepo, Uri[] repos, bool includeTestPackages)
        {
            if (ciRepo == null)
            {
                throw new CliException(CliMessages.MissingMandatoryOption, "cirepo");
            }

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"{workingDir}/{CliConstants.CmfPackageFileName}");

            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: false, fileSystem: fileSystem);

            if (cmfPackage.PackageType != Enums.PackageType.Root)
            {
                throw new CliException(CliMessages.NotARootPackage);
            }

            // The method LoadDependencies will return the dependency from the first repo in the list
            // We need to force the CI Repo to be the last to be checked, to make sure that we first check the "release repositories"
            List<Uri> remoteRepos = new();
            if (repos.HasAny())
            {
                remoteRepos.AddRange(repos);
            }
            remoteRepos.Add(ciRepo);

            cmfPackage.LoadDependencies(remoteRepos, true);

            #region Missing Dependencies Handling

            // If a dependency is not found in any repository an error should be throw
            List<string> missingPackages = new();
            foreach (Dependency dependency in cmfPackage.Dependencies.Where(x => x.IsMissing))
            {
                missingPackages.Add($"{dependency.Id}@{dependency.Version}");
            }

            if (missingPackages.HasAny())
            {
                throw new CliException(CliMessages.SomePackagesNotFound, string.Join(", ", missingPackages));
            }

            #endregion

            #region Output Directories Handling

            outputDir = FileSystemUtilities.GetOutputDir(cmfPackage, outputDir, force: true);

            if (includeTestPackages)
            {
                IDirectoryInfo outputTestDir = this.fileSystem.DirectoryInfo.FromDirectoryName(outputDir + CliConstants.FolderTests);
                if (!outputTestDir.Exists)
                {
                    outputTestDir.Create();
                }
            }

            #endregion

            try
            {
                // Assemble current package
                AssemblePackage(outputDir, remoteRepos, cmfPackage);

                // Assemble Dependencies
                AssembleDependencies(outputDir, ciRepo, remoteRepos, cmfPackage);

                // Save Dependencies File
                // This file will be needed for CMF Internal Releases to know where the external dependencies are located
                string depedenciesFilePath = this.fileSystem.Path.Join(workingDir.FullName, CliConstants.FileDependencies);
                fileSystem.File.WriteAllText(depedenciesFilePath, JsonConvert.SerializeObject(packagesLocation));
            }
            catch (Exception e)
            {
                throw new CliException(e.Message);
            }
        }

        #endregion

        #region Private Methods

        private void AssembleTestPackages(IDirectoryInfo outputDir, IEnumerable<Uri> repos, CmfPackage cmfPackage, DependencyCollection loadedDependencies = null)
        {
            if (!cmfPackage.TestPackages.HasAny())
            {
                // No test packages found for package
                Log.Information(string.Format(CliMessages.PackageHasNoTestPackages, cmfPackage.PackageId, cmfPackage.Version));
            }
            else
            {
                IDirectoryInfo testOutputDir = this.fileSystem.DirectoryInfo.FromDirectoryName(outputDir + "/Tests");

                foreach (Dependency testPackage in cmfPackage.TestPackages)
                {
                    if (!loadedDependencies.Contains(testPackage))
                    {
                        Log.Information(string.Format(CliMessages.GetPackage, testPackage.Id, testPackage.Version));
                        loadedDependencies.Add(testPackage);

                        AssemblePackage(testOutputDir, repos, testPackage.CmfPackage);
                    }
                }
            }
        }

        /// <summary>
        /// Publish Dependencies from one package. recursive operation
        /// </summary>
        /// <param name="outputDir">Destination for the dependencies package and also used for the current package</param>
        /// <param name="ciRepo"></param>
        /// <param name="repos">The repos.</param>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="assembledDependencies">The loaded dependencies.</param>
        private void AssembleDependencies(IDirectoryInfo outputDir, Uri ciRepo, IEnumerable<Uri> repos, CmfPackage cmfPackage, DependencyCollection assembledDependencies = null)
        {
            if (cmfPackage.Dependencies.HasAny())
            {
                assembledDependencies ??= new();

                foreach (Dependency dependency in cmfPackage.Dependencies)
                {
                    string dependencyPath = fileSystem.Path.GetDirectoryName(dependency.CmfPackage.Uri.OriginalString);

                    // To avoid assembling the same dependency twice
                    // Only assemble dependencies from the CI Repository
                    if (!assembledDependencies.Contains(dependency) &&
                        string.Equals(ciRepo.OriginalString, dependencyPath))
                    {
                        Log.Information(string.Format(CliMessages.GetPackage, dependency.Id, dependency.Version));

                        assembledDependencies.Add(dependency);
                        AssemblePackage(outputDir, repos, dependency.CmfPackage);
                    }
                    // Save all external dependencies and locations in a dictionary
                    else
                    {
                        packagesLocation.Add($"{dependency.Id}@{dependency.Version}", dependency.CmfPackage.Uri.OriginalString);
                    }

                    AssembleDependencies(outputDir, ciRepo, repos, dependency.CmfPackage, assembledDependencies);
                }
            }
        }

        /// <summary>
        /// Publish a package to the output directory
        /// </summary>
        /// <param name="outputDir">Destiny for the package</param>
        /// <param name="repos">The repos.</param>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="includeTestPackages"></param>
        /// <exception cref="Cmf.Common.Cli.Utilities.CliException"></exception>
        private void AssemblePackage(IDirectoryInfo outputDir, IEnumerable<Uri> repos, CmfPackage cmfPackage, bool includeTestPackages = false)
        {
            IDirectoryInfo[] repoDirectories = repos?.Select(r => fileSystem.DirectoryInfo.FromDirectoryName(r.OriginalString)).ToArray();

            // Load package from repo if is not loaded yet
            if (cmfPackage == null || (cmfPackage != null && cmfPackage.Uri == null))
            {
                cmfPackage = CmfPackage.LoadFromRepo(repoDirectories, cmfPackage.PackageId, cmfPackage.Version);
            }

            string destinationFile = $"{outputDir.FullName}/{cmfPackage.Uri.Segments.Last()}";
            if (fileSystem.File.Exists(destinationFile))
            {
                fileSystem.File.Delete(destinationFile);
            }

            fileSystem.FileInfo.FromFileName(cmfPackage.Uri.LocalPath).CopyTo(destinationFile);

            // Assemble Tests
            if (includeTestPackages)
            {
                AssembleTestPackages(outputDir, repos, cmfPackage);
            }
        }

        #endregion
    }
}

using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Linq;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// This command will be responsible for assemble a package based on a given cmfpackage and respective dependencies
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
    [CmfCommand("assemble")]
    public class AssembleCommand : BaseCommand
    {
        #region Private Methods

        private void AssembleTestPackages(IDirectoryInfo outputDir, Uri[] repos, CmfPackage cmfPackage, DependencyCollection loadedDependencies = null)
        {
            IDirectoryInfo testOutputDir = this.fileSystem.DirectoryInfo.FromDirectoryName(outputDir + "/Tests");

            if (!cmfPackage.TestPackages.HasAny())
            {
                // TO-DO: Missing resource message
                // No dependencies found for package
                Log.Information($"Package {cmfPackage.PackageId}.{cmfPackage.Version} has no test packages");
            }
            else
            {
                foreach (Dependency testPackage in cmfPackage.TestPackages)
                {
                    if (!loadedDependencies.Contains(testPackage))
                    {
                        // TO-DO: Missing resource message
                        Log.Information($"Get Test Package {testPackage.Id}.{testPackage.Version}...");
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
        /// <param name="repos">The repos.</param>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="loadedDependencies">The loaded dependencies.</param>
        private void AssembleDependencies(IDirectoryInfo outputDir, Uri[] repos, CmfPackage cmfPackage, DependencyCollection loadedDependencies = null)
        {
            if(cmfPackage.Dependencies.HasAny())
            {
                // TO-DO: Missing comments
                loadedDependencies ??= new();
                foreach (Dependency localDependency in cmfPackage.Dependencies)
                {
                    if (!loadedDependencies.Contains(localDependency))
                    {
                        Log.Information($"Get Dependency Package {localDependency.Id}.{localDependency.Version}...");
                        loadedDependencies.Add(localDependency);
                        AssemblePackage(outputDir, repos, localDependency.CmfPackage);
                        if (localDependency.CmfPackage.Dependencies.HasAny())
                        {
                            AssembleDependencies(outputDir, repos, localDependency.CmfPackage, loadedDependencies);
                        }

                    }
                }
            }           
        }

        /// <summary>
        /// Publish a package to the output directory
        /// </summary>
        /// <param name="outputDir">Destiny for the package</param>
        /// <param name="repos">The repos.</param>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <returns>
        /// True if package was coppied
        /// </returns>
        /// <exception cref="Cmf.Common.Cli.Utilities.CliException"></exception>
        private void AssemblePackage(IDirectoryInfo outputDir, Uri[] repos, CmfPackage cmfPackage)
        {
            IDirectoryInfo[] repoDirectories = repos?.Select(r => fileSystem.DirectoryInfo.FromDirectoryName(r.OriginalString)).ToArray();

            if (cmfPackage == null || (cmfPackage!= null && cmfPackage.Uri == null))
            {
                cmfPackage = CmfPackage.LoadFromRepo(repoDirectories, cmfPackage.PackageId, cmfPackage.Version);
            }           

            string destinationFile = $"{outputDir.FullName}/{cmfPackage.Uri.Segments.Last()}";
            if (fileSystem.File.Exists(destinationFile))
            {
                fileSystem.File.Delete(destinationFile);
            }

            fileSystem.FileInfo.FromFileName(cmfPackage.Uri.LocalPath).CopyTo(destinationFile);
        }

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
                isDefault: true)
            {
                Description = "Working Directory"
            });

            cmd.AddOption(new Option<IDirectoryInfo>(
                aliases: new string[] { "-o", "--outputDir" },
                parseArgument: argResult => Parse<IDirectoryInfo>(argResult, "Assemble"),
                isDefault: true,
                description: "Output directory for assembled package"));

            cmd.AddOption(new Option<Uri[]>(
                aliases: new string[] { "-r", "--repos", "--repo" },
                description: "Repository or repositories where dependencies are located (url or folder)"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "--includeTestPackages" },
                description: "Include test packages on assemble"));

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, Uri[], bool>(Execute);
        }

        /// <summary>
        /// Executes the specified working dir.
        /// </summary>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="repos">The repo.</param>
        /// <param name="includeTestPackages">True to publish test packages</param>
        /// <returns></returns>
        public void Execute(IDirectoryInfo workingDir, IDirectoryInfo outputDir, Uri[] repos, bool includeTestPackages)
        {
            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"{workingDir}/{CliConstants.CmfPackageFileName}");

            // To avoid memory references the CmfPackage needs to be loaded twice
            // One - get the dependencies from the same working directory where the package is located
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile);
            cmfPackage.LoadDependencies(null, true);

            if (cmfPackage.PackageType != Enums.PackageType.Root)
            {
                // TO-DO: Throw error?
            }

            // Two - get the dependencies located in the input repositories
            CmfPackage repoCmfPackage = CmfPackage.Load(cmfpackageFile);
            repoCmfPackage.LoadDependencies(repos, true);

            #region Missing Dependencies Handling

            // If a dependency is not found in the working directory or in the repository and error should be throw
            List<string> missingPackages = new();
            foreach (Dependency missingLocalDep in cmfPackage.Dependencies.Where(x => x.IsMissing))
            {
                Dependency dependencyFromRepo = repoCmfPackage.Dependencies.Get(missingLocalDep);
                if (dependencyFromRepo.IsMissing)
                {
                    missingPackages.Add($"{dependencyFromRepo.Id}@{dependencyFromRepo.Version}");
                }
            }

            if (missingPackages.HasAny())
            {
                throw new CliException(string.Format(CliMessages.SomePackagesNotFound, string.Join(",", missingPackages)));
            }

            #endregion

            #region Output Directories Handling

            outputDir = FileSystemUtilities.GetOutputDir(cmfPackage, outputDir, force: true);
            if (outputDir == null)
            {
                return;
            }

            if (includeTestPackages)
            {
                // TO-DO: missing Tests constant
                IDirectoryInfo outputTestDir = this.fileSystem.DirectoryInfo.FromDirectoryName(outputDir + "/Tests");
                if (!outputTestDir.Exists)
                {
                    outputTestDir.Create();
                }
            }

            #endregion

            try
            {
                // Assemble scope package
                AssemblePackage(outputDir, repos, repoCmfPackage);
                // Assemble Dependencies
                AssembleDependencies(outputDir, repos, repoCmfPackage);

                // Assemble Tests
                if(includeTestPackages)
                {
                    AssembleTestPackages(outputDir, repos, repoCmfPackage);
                }
            }
            catch (Exception e)
            {
                throw new CliException(e.Message);
            }
        }
    }
}
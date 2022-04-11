using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cmf.CLI.Core;
using Cmf.Common.Cli.Enums;

namespace Cmf.Common.Cli.Builders
{
    /// <summary>
    /// Checks the consistency of packages under a root
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Builders.ProcessCommand" />
    /// <seealso cref="Cmf.Common.Cli.Builders.IBuildCommand" />
    public class ConsistencyCheckCommand : IBuildCommand
    {
        /// <summary>
        /// Virtual File System
        /// </summary>
        public IFileSystem FileSystem { get; set; }
        
        /// <summary>
        /// Hook to start search root algorithm
        /// </summary>
        public IDirectoryInfo WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Find Root Package, check dependencies, enforce consistency of package version
        /// </summary>
        /// <returns></returns>
        public Task Exec()
        {
            IDirectoryInfo directory = Utilities.FileSystemUtilities.GetPackageRootByType(WorkingDirectory.FullName, Enums.PackageType.Root, FileSystem);
            IFileInfo cmfpackageFile = FileSystem.FileInfo.FromFileName($"{directory.FullName}/{CliConstants.CmfPackageFileName}");

            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);

            Log.Status($"Loading {cmfPackage.PackageId} dependency tree...", ctx => {
                cmfPackage.LoadDependencies(ExecutionContext.Instance.RepositoriesConfig.Repositories.ToArray(), ctx, true);
                ctx.Status("Checking dependency tree consistency...");
                IterateTree(cmfPackage);
                ctx.Status("Done!");
            });

            return null;
        }


        /// <summary>
        /// Iterate through a dependency tree and check the dependencies
        /// </summary>
        /// <param name="pkg"></param>
        /// <exception cref="CliException"></exception>
        private static void IterateTree(CmfPackage pkg)
        {
            if (pkg.Dependencies.HasAny())
            {
                for (int i = 0; i < pkg.Dependencies.Count; i++)
                {
                    Dependency dep = pkg.Dependencies[i];

                    if (!dep.IsIgnorable && 
                        dep.CmfPackage != null &&
                        dep.CmfPackage.Location == PackageLocation.Local &&
                        pkg.Version != dep.Version)
                    {
                        throw new CliException(string.Format(CoreMessages.VersionFailedConsistencyCheck, pkg.Version, dep.Version));
                    }

                    if (!dep.IsMissing)
                    {
                        IterateTree(dep.CmfPackage);
                    }
                }
            }
        }
    }
}
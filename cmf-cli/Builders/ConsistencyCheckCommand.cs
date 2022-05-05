using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System.IO.Abstractions;
using System.Threading.Tasks;

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
        /// Only Executes on Test (--test)
        /// </summary>
        /// <value>
        /// boolean if to execute on Test should be true
        /// </value>
        public bool Test { get; set; } = false;

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

            cmfPackage.LoadDependencies(ExecutionContext.Instance.RepositoriesConfig.Repositories.ToArray(), true);

            GenericUtilities.IterateTree(cmfPackage, isDisplay: false, isConsistencyCheck: true);

            return null;
        }
    }
}
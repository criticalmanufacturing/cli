using System.IO.Abstractions;
using Cmf.Common.Cli.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    /// The current execution context
    /// </summary>
    public class ExecutionContext
    {
        private static ExecutionContext instance;
        private IFileSystem fileSystem;

        /// <summary>
        /// The current ExecutionContext object
        /// </summary>
        public static ExecutionContext Instance => instance;

        /// <summary>
        /// The current FileSystem object
        /// </summary>
        public IFileSystem FileSystem => fileSystem;

        /// <summary>
        /// The current execution RepositoriesConfig object
        /// </summary>
        public RepositoriesConfig RepositoriesConfig { get; set; }

        /// <summary>
        /// Get the current (executing) version of the CLI
        /// </summary>
        public static string CurrentVersion => (ServiceProvider.GetService<IVersionService>()!.CurrentVersion) ?? "dev";
        
        /// <summary>
        /// Get the package id of the current running application
        /// </summary>
        public static string PackageId => (ServiceProvider.GetService<IVersionService>()!.PackageId) ?? "unknown";

        /// <summary>
        /// true if we're running a development/unstable version 
        /// </summary>
        public static bool IsDevVersion => CurrentVersion.Contains("-");
        
        /// <summary>
        /// IoC container for services
        /// NOTE: As we already have this ExecutionContext object, we're not enabling Hosting, but instead we are hosting the container in the execution context
        /// </summary>
        public static ServiceProvider ServiceProvider { get; set; }

        private ExecutionContext(IFileSystem fileSystem)
        {
            // private constructor, can only obtain instance via the Instance property
            this.fileSystem = fileSystem;
            this.RepositoriesConfig = FileSystemUtilities.ReadRepositoriesConfig(fileSystem);
        }

        /// <summary>
        /// Initialize the current ExecutionContext instance
        /// </summary>
        public static ExecutionContext Initialize(IFileSystem fileSystem) => instance = new ExecutionContext(fileSystem);
    }
}

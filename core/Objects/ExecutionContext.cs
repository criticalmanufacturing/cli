using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Utilities;
using Core.Objects;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using Cmf.CLI.Core.Objects.CmfApp;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    /// The current execution context
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// The current ExecutionContext object
        /// </summary>
        public static ExecutionContext? Instance { get; private set; }

        /// <summary>
        /// The current FileSystem object
        /// </summary>
        public IFileSystem FileSystem { get; private set; }

        /// <summary>
        /// The current execution RepositoriesConfig object
        /// </summary>
        public RepositoriesConfig RepositoriesConfig { get; set; } = new();

        /// <summary>
        /// the current repository's project config
        /// </summary>
        public ProjectConfig? ProjectConfig { get; private set; }

        /// <summary>
        /// The current repository app data (only applicable for repositories of type App)
        /// </summary>
        public AppData? AppData { get; }

        public bool RunningOnWindows { get; set; }

        public List<ICIFSClient> CIFSClients { get; set; } = [];

        /// <summary>
        /// Get the current (executing) version of the CLI
        /// </summary>
        public static string CurrentVersion => (ServiceProvider?.GetService<IVersionService>()?.CurrentVersion) ?? "dev";

        /// <summary>
        /// Get or set the latest version of the CLI. Use this if the CLI checks for new versions
        /// </summary>
        public static string? LatestVersion { get; set; }

        /// <summary>
        /// Get the package id of the current running application
        /// </summary>
        public static string PackageId => (ServiceProvider?.GetService<IVersionService>()?.PackageId) ?? "unknown";

        /// <summary>
        /// true if we're running a development/unstable version 
        /// </summary>
        public static bool IsDevVersion => CurrentVersion.Contains("-");

        /// <summary>
        /// IoC container for services
        /// NOTE: As we already have this ExecutionContext object, we're not enabling Hosting, but instead we are hosting the container in the execution context
        /// </summary>
        public static ServiceProvider ServiceProvider
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }
                else
                {
                    throw new CliException("ServiceProvider not initialized, please report this issue.");
                }
            }
            set;
        }

        /// <summary>
        /// Is the current CLI outdated.
        /// True if LatestVersion is not null (meaning a check was performed) and if the LatestVersion does not match the CurrentVersion
        /// </summary>
        public static bool IsOutdated => ExecutionContext.LatestVersion != null && ExecutionContext.CurrentVersion != ExecutionContext.LatestVersion;

        /// <summary>
        /// Prefix that should be used in environment variables defition in the whole aplication
        /// example: telemetry service envars use this prefix
        /// </summary>
        public static string? EnvVarPrefix { get; set; }

        /// <summary>
        /// Cache of the Related Packages
        /// </summary>
        public static RelatedPackageCollection RelatedPackagesCache { get; set; } = new();

        /// <summary>
        /// Verifies that current command is executing inside a project.
        /// </summary>
        /// <returns>The current project's config object.</returns>
        /// <exception cref="CliException">If the current ExecutionContext is not yet initialized</exception>
        /// <exception cref="CliException">If no project config is found</exception>
        public static ProjectConfig VerifyIsInsideProject()
        {
            if (Instance is null)
            {
                throw new CliException("ExecutionContext instance not initialized, please report this issue.");
            }
            if (Instance.ProjectConfig is null)
            {
                throw new CliException("Could not resolve project's config file. Make sure you are running this command inside a project.");
            }

            return Instance.ProjectConfig;
        }

        private ExecutionContext(IFileSystem fileSystem)
        {
            RunningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            // private constructor, can only obtain instance via the Instance property
            this.FileSystem = fileSystem;
            this.RepositoriesConfig = FileSystemUtilities.ReadRepositoriesConfig(fileSystem);

            // Make sure the cached credentials are reset, to recalculate the derived credentials
            ServiceProvider?.GetService<IRepositoryAuthStore>()?.Unload();

            this.AppData = FileSystemUtilities.ReadAppData(fileSystem);

            this.ProjectConfig = ServiceProvider?.GetService<IProjectConfigService>()?.Load(fileSystem);

            // connect and load shares for all UNC repositories
            if (!RunningOnWindows && RepositoriesConfig.Repositories.HasAny())
            {
                CIFSClients = RepositoriesConfig?.Repositories
                    .Where(uri => uri.IsUnc)
                    .Select(uri => new CIFSClient(uri) as ICIFSClient)
                    .ToList() ?? [];
            }

            RelatedPackagesCache = new();
        }

        /// <summary>
        /// Initialize the current ExecutionContext instance
        /// </summary>
        public static ExecutionContext Initialize(IFileSystem fileSystem) => Instance = new ExecutionContext(fileSystem);
    }
}

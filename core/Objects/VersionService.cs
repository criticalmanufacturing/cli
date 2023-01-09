using System.Reflection;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    /// Interface for a service that returns the current (running) CLI version
    /// </summary>
    public interface IVersionService
    {
        public string PackageId { get; }
        string CurrentVersion { get;  }
    }

    /// <summary>
    /// Implementation for a service that return the current (running) application version
    /// </summary>
    public class VersionService : IVersionService
    {
        public string PackageId { get; protected set; }
        public string CurrentVersion { get; protected set; }

        public VersionService(string packageId, string currentVersion)
        {
            PackageId = packageId;
            CurrentVersion = currentVersion ?? Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "dev";
        }

        public VersionService(string packageId) : this(packageId, null) { }
    }
}
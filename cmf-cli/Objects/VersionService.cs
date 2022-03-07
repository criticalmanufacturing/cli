using System.Reflection;

namespace Cmf.Common.Cli.Objects
{
    /// <summary>
    /// Interface for a service that returns the current (running) CLI version
    /// </summary>
    public interface IVersionService
    {
        string CurrentVersion { get;  }
    }

    /// <summary>
    /// Implementation for a service that return the current (running) CLI version
    /// </summary>
    public class VersionService : IVersionService
    {
        public string CurrentVersion => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "dev";
    }
}
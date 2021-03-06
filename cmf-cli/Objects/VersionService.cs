using System.Reflection;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Objects
{
    /// <summary>
    /// Implementation for a service that return the current (running) CLI version
    /// </summary>
    public class VersionService : IVersionService
    {
        public string PackageId => "@criticalmanufacturing/cli";
        public string CurrentVersion => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "dev";
    }
}
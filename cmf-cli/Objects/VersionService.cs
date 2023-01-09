using System.Reflection;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Objects
{
    /// <summary>
    /// Implementation for a service that return the current (running) CLI version
    /// </summary>
    public class VersionServices// : IVersionService
    {
        public string PackagseId => "@criticalmanufacturing/cli";
        public string CurrentVsersion => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "dev";
    }
}
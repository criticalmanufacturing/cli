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
}
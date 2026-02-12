using System;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    /// Exception thrown when MES version validation fails
    /// </summary>
    public class MESVersionValidationException : Exception
    {
        public MESVersionValidationException(string message) : base(message) { }
    }

    /// <summary>
    /// Interface for MES version validation service
    /// </summary>
    public interface IMESVersionValidationService
    {
        /// <summary>
        /// Validates that the current MES version meets the minimum required version
        /// </summary>
        /// <param name="minimumVersion">Minimum required MES version (e.g., "11.0.0")</param>
        /// <exception cref="MESVersionValidationException">Thrown when current MES version is below the minimum required version</exception>
        void ValidateMinimumVersion(string minimumVersion);
        
        /// <summary>
        /// Checks if the current MES version meets the minimum required version
        /// </summary>
        /// <param name="minimumVersion">Minimum required MES version (e.g., "11.0.0")</param>
        /// <returns>True if current version meets minimum requirement, false otherwise</returns>
        bool IsVersionCompatible(string minimumVersion);
    }

    /// <summary>
    /// Implementation of MES version validation service
    /// </summary>
    public class MESVersionValidationService : IMESVersionValidationService
    {
        /// <inheritdoc/>
        public void ValidateMinimumVersion(string minimumVersion)
        {
            if (string.IsNullOrWhiteSpace(minimumVersion))
            {
                return; // No validation needed if no minimum version is specified
            }

            if (!Version.TryParse(minimumVersion, out Version minVersion))
            {
                throw new ArgumentException($"Invalid minimum version format: {minimumVersion}. Expected format: 'Major.Minor.Build' (e.g., '11.0.0')");
            }

            var currentVersion = ExecutionContext.Instance?.ProjectConfig?.MESVersion;
            if (currentVersion == null)
            {
                throw new MESVersionValidationException("MES version information is not available. Please ensure you are running this command in a valid project context.");
            }

            if (currentVersion < minVersion)
            {
                throw new MESVersionValidationException($"This command requires MES version {minimumVersion} or higher. Current version: {currentVersion}");
            }
        }

        /// <inheritdoc/>
        public bool IsVersionCompatible(string minimumVersion)
        {
            if (string.IsNullOrWhiteSpace(minimumVersion))
            {
                return true; // No minimum version requirement
            }

            if (!Version.TryParse(minimumVersion, out Version minVersion))
            {
                return false;
            }

            var currentVersion = ExecutionContext.Instance?.ProjectConfig?.MESVersion;
            if (currentVersion == null)
            {
                return false;
            }

            return currentVersion >= minVersion;
        }
    }
}

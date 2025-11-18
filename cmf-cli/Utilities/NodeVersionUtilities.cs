using Cmf.CLI.Core;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Cmf.CLI.Utilities
{
    /// <summary>
    /// Utilities for Node.js version management and validation
    /// </summary>
    public static class NodeVersionUtilities
    {
        /// <summary>
        /// Gets the installed Node.js version
        /// </summary>
        /// <returns>The installed Node.js version, or null if Node.js is not installed</returns>
        public static Version GetInstalledNodeVersion()
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "node" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ""),
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    return null;
                }

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    return null;
                }

                // Node version output is in format "v18.19.0" or "v20.10.0"
                // We need to parse it to extract the version
                var match = Regex.Match(output.Trim(), @"v?(\d+)\.(\d+)\.(\d+)");
                if (match.Success)
                {
                    int major = int.Parse(match.Groups[1].Value);
                    int minor = int.Parse(match.Groups[2].Value);
                    int patch = int.Parse(match.Groups[3].Value);
                    return new Version(major, minor, patch);
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Debug($"Error getting Node.js version: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Validates that the installed Node.js version is compatible with the target MES version
        /// </summary>
        /// <param name="mesVersion">The target MES version</param>
        /// <param name="requiredNodeMajorVersion">The required Node.js major version</param>
        /// <exception cref="CliException">Thrown when Node.js is not installed or the version is incompatible</exception>
        public static void ValidateNodeVersion(Version mesVersion, string requiredNodeMajorVersion)
        {
            var installedVersion = GetInstalledNodeVersion();

            if (installedVersion == null)
            {
                throw new CliException(CliMessages.NodeNotInstalled);
            }

            // Parse the required major version (could be "12", "18", "20", etc.)
            if (!int.TryParse(requiredNodeMajorVersion, out int requiredMajor))
            {
                Log.Warning($"Could not parse required Node.js version: {requiredNodeMajorVersion}");
                return;
            }

            if (installedVersion.Major != requiredMajor)
            {
                throw new CliException(string.Format(
                    CliMessages.IncompatibleNodeVersion,
                    installedVersion,
                    mesVersion,
                    requiredNodeMajorVersion
                ));
            }

            Log.Debug($"Node.js version {installedVersion} is compatible with MES version {mesVersion} (requires Node.js v{requiredNodeMajorVersion}.x)");
        }
    }
}

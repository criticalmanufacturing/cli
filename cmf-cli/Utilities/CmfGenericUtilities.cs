using Cmf.CLI.Builders;
using Cmf.CLI.Commands;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using System;
using System.IO.Abstractions;

namespace Cmf.CLI.Utilities
{
    /// <summary>
    ///     Utilities Class for CMF
    /// </summary>
    public static class CmfGenericUtilities
    {
        /// <summary>
        ///     Builds all packages of a given type.
        /// </summary>
        /// <param name="packageType">
        ///     Type of package to be built.
        /// </param>
        /// <param name="fileSystem">
        ///     FileSystem
        /// </param>
        public static void BuildAllPackagesOfType(PackageType packageType, IFileSystem fileSystem)
        {
            IDirectoryInfo projectRoot = FileSystemUtilities.GetProjectRoot(fileSystem);

            // Get packages of given type
            CmfPackageCollection packages = projectRoot.LoadCmfPackagesFromSubDirectories(packageType: packageType);

            BuildCommand buildCommand = new BuildCommand(fileSystem);

            // Build All packages
            foreach (CmfPackage package in packages)
            {
                Log.Information($"Build {Enum.GetName(typeof(PackageType), packageType)}: {package.PackageId}");
                buildCommand.Execute(package.GetFileInfo().Directory);
            }
        }

        /// <summary>
        ///     Stop any running project DB containers and start current project DB container.
        /// </summary>
        /// <param name="fileSystem">
        ///     FileSystem`
        /// </param>
        public static void StartProjectDbContainer(IFileSystem fileSystem)
        {
            string powershellExe = GenericUtilities.GetPowerShellExecutable();
            IDirectoryInfo projectRoot = FileSystemUtilities.GetProjectRoot(fileSystem);

            Log.Information("Stopping other project containers if any is running");
            new CmdCommand()
            {
                DisplayName = $"{powershellExe} -C \"if (wsl docker ps -q --filter name=\"-\") {{ wsl docker stop $(wsl docker ps -q --filter name=\"-\") }}\"",
                Command = $"{powershellExe} -C \"if (wsl docker ps -q --filter name=\"-\") {{ wsl docker stop $(wsl docker ps -q --filter name=\"-\") }}\"",
                Args = Array.Empty<string>(),
                WorkingDirectory = projectRoot
            }.Exec();

            string dockerDbContainerName = $"{ExecutionContext.Instance.ProjectConfig.EnvironmentName}-DB";

            Log.Information("Starting project container");
            new CmdCommand()
            {
                DisplayName = $"wsl docker start {dockerDbContainerName}",
                Command = $"wsl docker start {dockerDbContainerName}",
                Args = Array.Empty<string>(),
                WorkingDirectory = projectRoot
            }.Exec();
        }
    }
}
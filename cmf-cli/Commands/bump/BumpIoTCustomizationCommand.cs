using NuGet.Versioning;
using System.CommandLine;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="BaseCommand" />
    [CmfCommand(name: "customization", ParentId = "bump_iot")]
    public class BumpIoTCustomizationCommand : BaseCommand
    {
        /// <summary>Creates the command.</summary>
        public BumpIoTCustomizationCommand() { }

        /// <summary>Creates the command with a filesystem.</summary>
        /// <param name="fileSystem">The project filesystem.</param>
        public BumpIoTCustomizationCommand(IFileSystem fileSystem) : base(fileSystem) { }

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var pathArgument = new Argument<IDirectoryInfo>("path")
            {
                Description = "Working Directory",
                CustomParser = argResult => Parse<IDirectoryInfo>(argResult, "."),
                DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, ".")
            };
            cmd.Add(pathArgument);

            var versionOption = new Option<SemanticVersion>("--version", "-v")
            {
                Description = "Will bump all versions to the version specified",
                CustomParser = result => VersionUtilities.ParseVersion(result, text => VersionUtilities.ParseNpmVersion(text, "--version"))
            };
            var buildNrVersionOption = new Option<string>("--buildNrVersion", "-b")
            {
                Description = "Will add this version next to the version (v-b)",
                CustomParser = result => VersionUtilities.ParseSuffix(result, "--buildNrVersion")
            };
            var preReleaseVersionOption = new Option<string>("--preReleaseVersion", "-p")
            {
                Description = "Will add this version as pre-release version (v-p)",
                CustomParser = result => VersionUtilities.ParseSuffix(result, "--preReleaseVersion")
            };
            cmd.Add(versionOption);
            cmd.Add(buildNrVersionOption);
            cmd.Add(preReleaseVersionOption);
            BumpCommand.ValidateVersionOptions(cmd, versionOption, buildNrVersionOption, preReleaseVersionOption,
                iot: true, requireVersion: true, npm: true);

            var packageNamesOption = new Option<string>("--packageNames", "-pckNames")
            {
                Description = "Packages to be bumped"
            };
            cmd.Add(packageNamesOption);

            var isToTagOption = new Option<bool>("--isToTag", "-isToTag")
            {
                Description = "Instead of replacing the version will add -$version",
                DefaultValueFactory = _ => false
            };
            cmd.Add(isToTagOption);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var path = parseResult.GetValue(pathArgument);
                var version = parseResult.GetValue(versionOption);
                var buildNr = parseResult.GetValue(buildNrVersionOption);
                var preReleaseVersion = parseResult.GetValue(preReleaseVersionOption);
                var packageNames = parseResult.GetValue(packageNamesOption);
                var isToTag = parseResult.GetValue(isToTagOption);

                Execute(path, version, buildNr, preReleaseVersion, packageNames, isToTag);
                return Task.FromResult(0);
            });        
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNrVersion"></param>
        /// <param name="preReleaseVersion"></param>
        /// <param name="packageNames">The package names.</param>
        /// <param name="isToTag">if set to <c>true</c> [is to tag].</param>
        /// <exception cref="CliException"></exception>
        /// <exception cref="CliException"></exception>
        public void Execute(IDirectoryInfo packagePath, SemanticVersion version, string buildNrVersion, string preReleaseVersion, string packageNames, bool isToTag)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{packagePath}/{CliConstants.CmfPackageFileName}");

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile);

            Execute(cmfPackage, version, buildNrVersion, preReleaseVersion, packageNames, isToTag);
        }

        /// <summary>
        /// Executes the BumpIoTCustomPackages for specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNrVersion">The version for build Nr.</param>
        /// <param name="preReleaseVersion">The pre-release version.</param>
        /// <param name="packageNames">The package names.</param>
        /// <param name="isToTag">if set to <c>true</c> [is to tag].</param>
        /// <exception cref="CliException"></exception>
        public void Execute(CmfPackage cmfPackage, SemanticVersion version, string buildNrVersion, string preReleaseVersion, string packageNames, bool isToTag)
        {
            var suffix = !string.IsNullOrEmpty(buildNrVersion) ? buildNrVersion : preReleaseVersion;
            var finalVersion = version == null ? null : GenericUtilities.RetrieveNewVersion(null, VersionUtilities.GetVersionText(version), suffix);
            var iotPackages = cmfPackage.PackageType == PackageType.IoT
                ? new[] { cmfPackage }
                : cmfPackage.GetFileInfo().Directory.LoadCmfPackagesFromSubDirectories(packageType: PackageType.IoT).ToArray();

            // The public package overload historically permits suffix-only updates.
            // Validate every selected npm version before the first package is changed.
            if (version == null)
            {
                foreach (var package in iotPackages)
                {
                    foreach (var directory in fileSystem.Directory.GetDirectories(package.GetFileInfo().DirectoryName + "/src/", "*"))
                    {
                        var name = directory.Split('/').Last();
                        var packageJson = fileSystem.Path.Combine(directory, "package.json");
                        if ((string.IsNullOrEmpty(packageNames) || packageNames.Contains(name)) && fileSystem.File.Exists(packageJson))
                        {
                            var current = JObject.Parse(fileSystem.File.ReadAllText(packageJson))["version"]?.Value<string>();
                            VersionUtilities.ParseNpmVersion(
                                GenericUtilities.RetrieveNewVersion(current, null, suffix), "--version");
                        }
                    }
                }
            }

            foreach (var package in iotPackages)
            {
                IoTUtilities.BumpIoTCustomPackages(package.GetFileInfo().DirectoryName,
                    finalVersion, finalVersion == null ? suffix : null, packageNames, this.fileSystem);
            }
        }
    }
}

using System;
using NuGet.Versioning;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="BaseCommand" />
    [CmfCommand("bump", Id = "bump")]
    public class BumpCommand : BaseCommand
    {
        internal static void ValidateVersionOptions<T>(Command command, Option<T> version,
            Option<string> build, Option<string> prerelease, bool iot = false, bool requireVersion = true, bool npm = false)
            where T : SemanticVersion
        {
            command.Validators.Add(result =>
            {
                if (result.GetResult(version)?.Errors.Any() == true ||
                    result.GetResult(build)?.Errors.Any() == true ||
                    result.GetResult(prerelease)?.Errors.Any() == true) return;
                try
                {
                    ValidateBumpVersion(VersionUtilities.GetOptionalVersion(result, version), result.GetValue(build),
                        result.GetValue(prerelease), iot, requireVersion, npm);
                }
                catch (CliException exception)
                {
                    result.AddError(exception.Message);
                }
                catch (InvalidOperationException)
                {
                    // Failed option parsers already supply their own diagnostics.
                }
            });
        }

        private static void ValidateBumpVersion(SemanticVersion version, string build, string prerelease,
            bool iot = false, bool requireVersion = true, bool npm = false)
        {
            if (requireVersion && version == null)
                throw new CliException(string.Format(CliMessages.MissingMandatoryProperty, "version"));
            var buildField = iot ? "--buildNrVersion" : "--buildNr";
            var prereleaseField = iot ? "--preReleaseVersion" : "--preRelease";
            if (!string.IsNullOrEmpty(build) && !string.IsNullOrEmpty(prerelease))
                throw new CliException(string.Format(CliMessages.MutuallyExclusiveProperties,
                    $"{buildField.TrimStart('-')}, {prereleaseField.TrimStart('-')}"));
            var suffix = !string.IsNullOrEmpty(build) ? build : prerelease;
            if (version != null)
            {
                if (!string.IsNullOrEmpty(suffix))
                {
                    var finalVersion = GenericUtilities.RetrieveNewVersion(null, VersionUtilities.GetVersionText(version), suffix);
                    if (npm) VersionUtilities.ParseNpmVersion(finalVersion, "--version");
                    else VersionUtilities.ParseNuGetVersion(finalVersion, "--version");
                }
            }
        }

        /// <summary>Creates the command.</summary>
        public BumpCommand() { }

        /// <summary>Creates the command with a filesystem.</summary>
        /// <param name="fileSystem">The project filesystem.</param>
        public BumpCommand(IFileSystem fileSystem) : base(fileSystem) { }

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var packagePathArgument = new Argument<DirectoryInfo>("packagePath")
            {
                Description = "Package path",
                DefaultValueFactory = _ => new DirectoryInfo(".")
            };
            cmd.Add(packagePathArgument);

            var versionOption = new Option<NuGetVersion>("--version", "-v")
            {
                Description = "Will bump all versions to the version specified",
                CustomParser = result => VersionUtilities.ParseVersion(result, text => VersionUtilities.ParseNuGetVersion(text, "--version"))
            };
            var buildOption = new Option<string>("--buildNr", "-b")
            {
                Description = "Will add this version next to the version (v-b)",
                CustomParser = result => VersionUtilities.ParseSuffix(result, "--buildNr")
            };
            var prereleaseOption = new Option<string>("--preRelease", "-p")
            {
                Description = "Will add this version as pre-release version (v-p)",
                CustomParser = result => VersionUtilities.ParseSuffix(result, "--preRelease")
            };
            cmd.Add(versionOption);
            cmd.Add(buildOption);
            cmd.Add(prereleaseOption);
            BumpCommand.ValidateVersionOptions(cmd, versionOption, buildOption, prereleaseOption,
                iot: false, requireVersion: true, npm: false);

            var rootOption = new Option<string>("--root", "-r")
            {
                Description = "Will bump only versions under a specific root folder (i.e. 1.0.0)"
            };
            cmd.Add(rootOption);

            // Add the handler
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var packagePath = parseResult.GetValue(packagePathArgument);
                var root = parseResult.GetValue(rootOption);

                Execute(packagePath, parseResult.GetValue(versionOption),
                    parseResult.GetValue(buildOption), parseResult.GetValue(prereleaseOption), root);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="preRelease">The pre-release version.</param>
        /// <param name="root">The root.</param>
        /// <exception cref="CliException"></exception>
        /// <exception cref="CliException"></exception>
        public void Execute(DirectoryInfo packagePath, NuGetVersion version, string buildNr, string preRelease, string root)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{packagePath}/{CliConstants.CmfPackageFileName}");

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile);

            Execute(cmfPackage, version, buildNr, preRelease, root);
        }

        /// <summary>
        /// Executes the specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="root">The root.</param>
        /// <exception cref="CliException"></exception>
        public void Execute(CmfPackage cmfPackage, NuGetVersion version, string buildNr, string preRelease, string root)
        {
            var suffix = !string.IsNullOrEmpty(buildNr) ? buildNr : preRelease;
            IDirectoryInfo packageDirectory = cmfPackage.GetFileInfo().Directory;
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);

            // Will execute specific bump code per Package Type
            Dictionary<string, object> bumpInformation = new()
            {
                { "root", root }
            };

            packageTypeHandler.Bump(VersionUtilities.GetVersionText(version), suffix, bumpInformation);

            // will save with new version
            cmfPackage.SaveCmfPackage();
        }
    }
}
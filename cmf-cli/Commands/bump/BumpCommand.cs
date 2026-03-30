using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
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

            var versionOption = new Option<string>("--version", "-v")
            {
                Description = "Will bump all versions to the version specified"
            };
            cmd.Add(versionOption);

            var buildNrOption = new Option<string>("--buildNr", "-b")
            {
                Description = "Will add this version next to the version (v-b)"
            };
            cmd.Add(buildNrOption);

            var preReleaseOption = new Option<string>("--preRelease", "-p")
            {
                Description = "Will add this version as pre-release version (v-p)"
            };
            cmd.Add(preReleaseOption);

            var rootOption = new Option<string>("--root", "-r")
            {
                Description = "Will bump only versions under a specific root folder (i.e. 1.0.0)"
            };
            cmd.Add(rootOption);

            // Add the handler
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var packagePath = parseResult.GetValue(packagePathArgument);
                var version = parseResult.GetValue(versionOption);
                var buildNr = parseResult.GetValue(buildNrOption);
                var preRelease = parseResult.GetValue(preReleaseOption);
                var root = parseResult.GetValue(rootOption);

                Execute(packagePath, version, buildNr, preRelease, root);
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
        public void Execute(DirectoryInfo packagePath, string version, string buildNr, string preRelease, string root)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{packagePath}/{CliConstants.CmfPackageFileName}");

            if (string.IsNullOrEmpty(version) && (string.IsNullOrEmpty(buildNr) || string.IsNullOrEmpty(preRelease)))
            {
                throw new CliException(string.Format(CliMessages.MissingMandatoryProperties, "version, buildNr, preRelease"));
            }

            if (!string.IsNullOrEmpty(buildNr) && !string.IsNullOrEmpty(preRelease))
            {
                throw new CliException(string.Format(CliMessages.MutuallyExclusiveProperties, "buildNr, preRelease"));
            }

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
        public void Execute(CmfPackage cmfPackage, string version, string buildNr, string preRelease, string root)
        {
            IDirectoryInfo packageDirectory = cmfPackage.GetFileInfo().Directory;
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);

            // Will execute specific bump code per Package Type
            Dictionary<string, object> bumpInformation = new()
            {
                { "root", root }
            };

            string versionSuffix = !string.IsNullOrEmpty(buildNr) ? buildNr : preRelease;

            packageTypeHandler.Bump(version, versionSuffix, bumpInformation);

            // will save with new version
            cmfPackage.SaveCmfPackage();
        }
    }
}
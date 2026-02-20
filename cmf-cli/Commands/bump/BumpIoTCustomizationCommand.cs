using System.CommandLine;
using System.IO.Abstractions;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
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

            var versionOption = new Option<string>("--version", "-v")
            {
                Description = "Will bump all versions to the version specified"
            };
            cmd.Add(versionOption);

            var buildNrVersionOption = new Option<string>("--buildNrVersion", "-b")
            {
                Description = "Will add this version next to the version (v-b)"
            };
            cmd.Add(buildNrVersionOption);

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
                var packageNames = parseResult.GetValue(packageNamesOption);
                var isToTag = parseResult.GetValue(isToTagOption);

                Execute(path, version, buildNr, packageNames, isToTag);
                return Task.FromResult(0);
            });        
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr"></param>
        /// <param name="packageNames">The package names.</param>
        /// <param name="isToTag">if set to <c>true</c> [is to tag].</param>
        /// <exception cref="CliException"></exception>
        /// <exception cref="CliException"></exception>
        public void Execute(IDirectoryInfo packagePath, string version, string buildNr, string packageNames, bool isToTag)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{packagePath}/{CliConstants.CmfPackageFileName}");

            if (string.IsNullOrEmpty(version))
            {
                throw new CliException(string.Format(CliMessages.MissingMandatoryProperty, "version"));
            }

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile);

            Execute(cmfPackage, version, buildNr, packageNames, isToTag);
        }

        /// <summary>
        /// Executes the BumpIoTCustomPackages for specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr"></param>
        /// <param name="packageNames">The package names.</param>
        /// <param name="isToTag">if set to <c>true</c> [is to tag].</param>
        /// <exception cref="CliException"></exception>
        public void Execute(CmfPackage cmfPackage, string version, string buildNr, string packageNames, bool isToTag)
        {
            if (cmfPackage.PackageType != PackageType.IoT)
            {
                IDirectoryInfo packageDirectory = cmfPackage.GetFileInfo().Directory;
                CmfPackageCollection iotPackages = packageDirectory.LoadCmfPackagesFromSubDirectories(packageType: PackageType.IoT);
                foreach (var iotPackage in iotPackages)
                {
                    // IoT -> src -> Package XPTO
                    IoTUtilities.BumpIoTCustomPackages(iotPackage.GetFileInfo().DirectoryName, version, buildNr, packageNames, this.fileSystem);
                }
            }
            else
            {
                IoTUtilities.BumpIoTCustomPackages(cmfPackage.GetFileInfo().DirectoryName, version, buildNr, packageNames, this.fileSystem);
            }
        }
    }
}
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

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
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "packagePath",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, "."),
                isDefault: true
            )
            {
                Description = "Package Path"
            });

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-v", "--version" },
                description: "Will bump all versions to the version specified"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-b", "--buildNrVersion" },
                description: "Will add this version next to the version (v-b)"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-pckNames", "--packageNames" },
                description: "Packages to be bumped"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "-isToTag", "--isToTag" },
                getDefaultValue: () => { return false; },
                description: "Instead of replacing the version will add -$version"));

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, string, string, bool>(Execute);
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
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{packagePath}/{CliConstants.CmfPackageFileName}");

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
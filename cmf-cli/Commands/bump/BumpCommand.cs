using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.IO.Abstractions;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// </summary>
    /// <seealso cref="BaseCommand"/>
    [CmfCommand("bump", Id = "bump")]
    public class BumpCommand : BaseCommand
    {
        /// <summary>
        ///     Configure command
        /// </summary>
        /// <param name="cmd">
        /// </param>
        public override void Configure(Command cmd)
        {
            cmd.AddArgument(new Argument<DirectoryInfo>(
                name: "packagePath",
                getDefaultValue: () => { return new("."); },
                description: "Package path"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-v", "--version" },
                description: "Will bump all versions to the version specified"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-b", "--buildNr" },
                description: "Will add this version next to the version (v-b)"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-r", "--root" },
                description: "Will bump only versions under a specific root folder (i.e. 1.0.0)"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "-p", "--parentDep" },
                getDefaultValue: () => false,
                description: "Will bump the dependency version on parent packages"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "-f", "--renameVersionFolders" },
                getDefaultValue: () => false,
                description: "Will rename Version folders from old to new version"));

            // Add the handler
            cmd.Handler = CommandHandler.Create<DirectoryInfo, string, string, string, bool, bool>(Execute);
        }

        /// <summary>
        ///     Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">
        ///     The package path.
        /// </param>
        /// <param name="version">
        ///     The version.
        /// </param>
        /// <param name="buildNr">
        ///     The version for build Nr.
        /// </param>
        /// <param name="root">
        ///     The root.
        /// </param>
        /// <param name="parentDep">
        ///     Flag indicating if the dependency version in all parent packages will be updated.
        /// </param>
        /// <param name="renameVersionFolders">
        ///     Flag indicating if version folders will be renamed.
        /// </param>
        /// <exception cref="CliException">
        /// </exception>
        public void Execute(DirectoryInfo packagePath, string version, string buildNr, string root, bool parentDep, bool renameVersionFolders)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{packagePath}/{CliConstants.CmfPackageFileName}");

            if (string.IsNullOrEmpty(version) && string.IsNullOrEmpty(buildNr))
            {
                throw new CliException(string.Format(CliMessages.MissingMandatoryProperties, "version, buildNr"));
            }

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile);

            Execute(
                cmfPackage,
                version,
                buildNr,
                root,
                packagesToUpdateDep: parentDep ? FileSystemUtilities.GetAllPackages(fileSystem) : null,
                renameVersionFolders: renameVersionFolders
            );
        }

        /// <summary>
        ///     Executes the specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">
        ///     The CMF package.
        /// </param>
        /// <param name="version">
        ///     The version.
        /// </param>
        /// <param name="buildNr">
        ///     The version for build Nr.
        /// </param>
        /// <param name="root">
        ///     The root.
        /// </param>
        /// <param name="packagesToUpdateDep">
        ///     <para>CmfPackages to check and update dependency version.</para>
        ///     <para>Default = null (means that dependency version wont be updated in any package)</para>
        /// </param>
        /// <param name="renameVersionFolders">
        ///     Flag indicating if version folders will be renamed.
        /// </param>
        /// <exception cref="CliException">
        /// </exception>
        public void Execute(CmfPackage cmfPackage, string version, string buildNr, string root, CmfPackageCollection packagesToUpdateDep = null, bool renameVersionFolders = false)
        {
            string oldVersion = cmfPackage.Version;

            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);

            // Will execute specific bump code per Package Type
            Dictionary<string, object> bumpInformation = new()
            {
                { "root", root }
            };

            packageTypeHandler.Bump(version, buildNr, bumpInformation);

            // will save with new version
            cmfPackage.SaveCmfPackage();

            // Update Version in package dependencies
            if (packagesToUpdateDep.HasAny())
            {
                foreach (CmfPackage packageToUpdate in packagesToUpdateDep)
                {
                    packageToUpdate.UpdateDependency(cmfPackage);
                }
            }

            if (renameVersionFolders)
            {
                cmfPackage.RenameVersionFolders(oldVersion);
            }
        }
    }
}
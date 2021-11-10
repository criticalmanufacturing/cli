using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Factories;
using Cmf.Common.Cli.Interfaces;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
    [CmfCommand("bump")]
    public class BumpCommand : BaseCommand
    {
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
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

            // Add the handler
            cmd.Handler = CommandHandler.Create<DirectoryInfo, string, string, string>(Execute);
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="root">The root.</param>
        /// <param name="all">if set to <c>true</c> [all].</param>
        /// <exception cref="Cmf.Common.Cli.Utilities.CliException"></exception>
        /// <exception cref="CliException"></exception>
        public void Execute(DirectoryInfo packagePath, string version, string buildNr, string root)
        {
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{packagePath}/{CliConstants.CmfPackageFileName}");

            if (string.IsNullOrEmpty(version) && string.IsNullOrEmpty(buildNr))
            {
                throw new CliException(string.Format(CliMessages.MissingMandatoryProperties, "version, buildNr"));
            }

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile);

            Execute(cmfPackage, version, buildNr, root);
        }

        /// <summary>
        /// Executes the specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="root">The root.</param>
        /// <param name="all">if set to <c>true</c> [all].</param>
        /// <exception cref="CliException"></exception>
        public void Execute(CmfPackage cmfPackage, string version, string buildNr, string root)
        {
            IDirectoryInfo packageDirectory = cmfPackage.GetFileInfo().Directory;
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);

            // Will execute specific bump code per Package Type
            Dictionary<string, object> bumpInformation = new()
            {
                { "root", root }
            };

            packageTypeHandler.Bump(version, buildNr, bumpInformation);

            // will save with new version
            cmfPackage.SaveCmfPackage();
        }
    }
}
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;

namespace Cmf.CLI.Commands.Install
{
    /// <summary>
    ///     InstallHelpCommand
    /// </summary>
    [CmfCommand("help", Id = "install_help", ParentId = "install")]
    public class InstallHelpCommand : BaseCommand
    {
        /// <summary>
        ///     Configure command
        /// </summary>
        /// <param name="cmd">
        ///     Command
        /// </param>
        public override void Configure(Command cmd)
        {
            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        public void Execute()
        {
            CmfPackage helpPackage = GenericUtilities.SelectPackage(fileSystem, packageType: PackageType.Help);

            Log.Information($"Install Help: {helpPackage.PackageId}");
            IDirectoryInfo helpPackageDirectory = helpPackage.GetFileInfo().Directory;

            // Set the "Environment.CurrentDirectory" to the Help Package directory because "GenerateBasedOnTemplatesCommand" is using the "Environment.CurrentDirectory"
            Environment.CurrentDirectory = helpPackageDirectory.FullName;

            new BuildCommand(fileSystem).Execute(helpPackageDirectory);
        }
    }
}
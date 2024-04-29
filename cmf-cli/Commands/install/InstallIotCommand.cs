using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cmf.CLI.Commands.Install
{
    /// <summary>
    ///     InstallIotCommand
    /// </summary>
    [CmfCommand("iot", Id = "install_iot", ParentId = "install")]
    public class InstallIotCommand : BaseCommand
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
            CmfPackage iotPackage = GenericUtilities.SelectPackage(fileSystem, packageType: PackageType.IoT);

            Log.Information($"Install IoT: {iotPackage.PackageId}");
            new BuildCommand(fileSystem).Execute(iotPackage.GetFileInfo().Directory);
        }
    }
}
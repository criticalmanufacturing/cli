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
    ///     InstallHtmlCommand
    /// </summary>
    [CmfCommand("html", Id = "install_html", ParentId = "install")]
    public class InstallHtmlCommand : BaseCommand
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
            CmfPackage htmlPackage = GenericUtilities.SelectPackage(fileSystem, packageType: PackageType.HTML);

            Log.Information($"Install HTML: {htmlPackage.PackageId}");
            new BuildCommand(fileSystem).Execute(htmlPackage.GetFileInfo().Directory);
        }
    }
}
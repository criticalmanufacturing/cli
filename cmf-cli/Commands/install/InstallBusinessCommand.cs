using Cmf.CLI.Core.Attributes;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cmf.CLI.Commands.Install
{
    /// <summary>
    ///     InstallBusinessCommand
    /// </summary>
    [CmfCommand("business", Id = "install_business", ParentId = "install")]
    public class InstallBusinessCommand : BaseCommand
    {
        /// <summary>
        ///     Configure command
        /// </summary>
        /// <param name="cmd">
        ///     Command
        /// </param>
        public override void Configure(Command cmd)
        {
            cmd.AddOption(
                new Option<long>(
                    aliases: new[] { "--licenseId" },
                    description: "Project License Id"
                )
                { IsRequired = true }
            );

            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the commands:
        ///     <list type="number">
        ///         <item><see cref="InstallBusinessDatabaseCommand"/></item>
        ///         <item><see cref="InstallBusinessLocalEnvCommand"/></item>
        ///     </list>
        /// </summary>
        /// <param name="licenseId">
        ///     Project License Id
        /// </param>
        public void Execute(long licenseId)
        {
            new InstallBusinessDatabaseCommand().Execute(licenseId);
            new InstallBusinessLocalEnvCommand().Execute();
        }
    }
}
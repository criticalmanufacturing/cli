using Cmf.CLI.Commands.Install;
using Cmf.CLI.Core.Attributes;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///     InstallCommand
    /// </summary>
    /// <seealso cref="BaseCommand"/>
    [CmfCommand("install", Id = "install")]
    public class InstallCommand : BaseCommand
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
        ///     <para>Installs everything needed for the local environment to work</para>
        ///     <para>Executes the commands:</para>
        ///     <list type="number">
        ///         <item><see cref="InstallBusinessCommand"/></item>
        ///         <item><see cref="InstallHtmlCommand"/></item>
        ///         <item><see cref="InstallHelpCommand"/></item>
        ///     </list>
        /// </summary>
        /// <param name="licenseId">
        ///     Project License Id
        /// </param>
        public void Execute(long licenseId)
        {
            new InstallBusinessCommand().Execute(licenseId);
            new InstallHtmlCommand().Execute();
            new InstallHelpCommand().Execute();
        }
    }
}
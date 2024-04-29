using Cmf.CLI.Commands.Run;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///     RunCommand
    /// </summary>
    /// <seealso cref="BaseCommand"/>
    [CmfCommand("run", Id = "run")]
    public class RunCommand : BaseCommand
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
        ///     <para>Runs everything needed to use the Local Environment</para>
        ///     <para>Executes the commands:</para>
        ///     <list type="number">
        ///         <item><see cref="RunMessageBusCommand"/></item>
        ///         <item><see cref="RunHostCommand"/></item>
        ///         <item><see cref="RunHTMLCommand"/></item>
        ///     </list>
        /// </summary>
        public void Execute()
        {
            GenericUtilities.EnsureWindowsTerminalIsInstalled();

            IDirectoryInfo projectRoot = FileSystemUtilities.GetProjectRoot(fileSystem);

            /*
             * Use Windows Terminal to run all
             * This will create 3 new WindowsTerminal tabs, each of them executing a specific command in the CLI.
             */

            // Create Windows Terminal tab for MessageBus
            GenericUtilities.ExecutePowerShellCommand("wt.exe",
                "-w 0",
                "nt",
                "--title MessageBus",
                "--suppressApplicationTitle",
                $"-d {projectRoot}",
                GenericUtilities.GetPowerShellExecutable(),
                @"-c cmf run messagebus"
            );

            // Create Windows Terminal tab for Host
            GenericUtilities.ExecutePowerShellCommand("wt.exe",
                "-w 0",
                "nt",
                "--title Host",
                "--suppressApplicationTitle",
                $"-d {projectRoot}",
                GenericUtilities.GetPowerShellExecutable(),
                @"-c cmf run host"
            );

            // Create Windows Terminal tab for HTML
            GenericUtilities.ExecutePowerShellCommand("wt.exe",
                "-w 0",
                "nt",
                "--title HTML",
                "--suppressApplicationTitle",
                $"-d {projectRoot}",
                GenericUtilities.GetPowerShellExecutable(),
                @"-c cmf run html"
            );
        }
    }
}
using Cmf.CLI.Builders;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;

namespace Cmf.CLI.Commands.Install
{
    /// <summary>
    ///     InstallBusinessLocalEnvCommand
    /// </summary>
    [CmfCommand("localenv", Id = "install_business_localenv", ParentId = "install_business")]
    public class InstallBusinessLocalEnvCommand : BaseCommand
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
        ///     <code>
        ///cmf-dev local env;
        ///     </code>
        /// </summary>
        public void Execute()
        {
            IDirectoryInfo projectRoot = FileSystemUtilities.GetProjectRoot(fileSystem);

            // cmf-dev local env;
            Log.Information("Install Business Local Env");
            new CmdCommand()
            {
                DisplayName = "cmf-dev local env",
                Command = "cmf-dev local env",
                Args = Array.Empty<string>(),
                WorkingDirectory = projectRoot
            }.Exec();
        }
    }
}
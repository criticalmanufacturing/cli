using Cmf.CLI.Builders;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;

namespace Cmf.CLI.Commands.Install
{
    /// <summary>
    ///     InstallBusinessDatabaseCommand
    /// </summary>
    [CmfCommand("database", Id = "install_business_database", ParentId = "install_business")]
    public class InstallBusinessDatabaseCommand : BaseCommand
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
        ///     Executes the command
        ///     <code>
        ///cmf-dev local db --license {licenseId};
        ///     </code>
        /// </summary>
        /// <param name="licenseId">
        ///     Project License Id
        /// </param>
        public void Execute(long licenseId)
        {
            IDirectoryInfo projectRoot = FileSystemUtilities.GetProjectRoot(fileSystem);

            // Set default DB as current DB
            Log.Information($"Setting default DB as {ExecutionContext.Instance.ProjectConfig.EnvironmentName}");
            GenericUtilities.SetCurrentDb(fileSystem, $"{ExecutionContext.Instance.ProjectConfig.EnvironmentName}");

            // cmf-dev local db --license {licenseId};
            Log.Information("Install Business Database");
            new CmdCommand()
            {
                DisplayName = $"cmf-dev local db --license {licenseId}",
                Command = "cmf-dev local db",
                Args = new string[] { $"--license {licenseId}" },
                WorkingDirectory = projectRoot
            }.Exec();
        }
    }
}
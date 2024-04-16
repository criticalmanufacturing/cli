using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cmf.CLI.Commands.DbManager
{
    /// <summary>
    ///     BackupDatabaseCommand
    /// </summary>
    [CmfCommand("backup", Id = "dbmanager_backup", ParentId = "dbmanager")]
    public class BackupDatabaseCommand : BaseCommand
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
                new Option<string>(
                    aliases: new[] { "--backupName" },
                    getDefaultValue: () => null,
                    description: "Backup Name."
                )
            );

            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        public void Execute(string backupName = null)
        {
            string defaultDbName = ExecutionContext.Instance.ProjectConfig.EnvironmentName;
            string currentDb = GenericUtilities.GetCurrentDb(fileSystem);

            if (backupName.IsNullOrEmpty())
            {
                backupName = GenericUtilities.ReadStringValueFromConsole(prompt: $"Backup name (Press ENTER to use the current '{currentDb}'):", allowEmptyValueInput: true);
                if (backupName.IsNullOrEmpty())
                {
                    backupName = currentDb;
                }
            }

            string backupFileName = $"Custom-{backupName}.bak";

            Log.Status("Backing up database", (_) =>
            {
                GenericUtilities.ExecuteSqlCommand(
                    connectionString: $"Data Source=127.0.0.1,1433;Initial Catalog=master;User ID={CoreConstants.LocalDbUser};Password={CoreConstants.LocalDbPassword}",
                    sqlCommand: $"USE[{defaultDbName}]; BACKUP DATABASE [{defaultDbName}] TO DISK = N'/DBDumps/{backupFileName}' WITH NOFORMAT, INIT, NAME = N'{defaultDbName}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"
                );

                Log.Information($"DB backup '{backupFileName}' created.");
            });
        }
    }
}
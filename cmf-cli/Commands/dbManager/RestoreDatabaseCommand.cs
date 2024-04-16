using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;

namespace Cmf.CLI.Commands.DbManager
{
    /// <summary>
    ///     RestoreDatabaseCommand
    /// </summary>
    [CmfCommand("restore", Id = "dbmanager_restore", ParentId = "dbmanager")]
    public class RestoreDatabaseCommand : BaseCommand
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
                    aliases: new[] { "-f", "--backupFileName" },
                    getDefaultValue: () => null,
                    description: "Backup Name (e.g.: Custom-Babkup.bak)."
                )
            );

            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        public void Execute(string backupFileName = null)
        {
            string defaultDbName = ExecutionContext.Instance.ProjectConfig.EnvironmentName;

            Log.Verbose("");
            Log.Verbose($"Current database: {GenericUtilities.GetCurrentDb(fileSystem)}");
            Log.Warning("Close all programs using the DB (e.g.: HostService)");

            string[] dbBackups = GenericUtilities.GetBdBackups(fileSystem);

            if (backupFileName.IsNullOrEmpty())
            {
                Log.Verbose("Select DB backup to restore:");
                for (int i = 0; i < dbBackups.Length; i++)
                {
                    Log.Verbose($"{i + 1} - {Path.GetFileName(dbBackups[i])}");
                }
                int option = GenericUtilities.ReadIntValueFromConsole(
                    prompt: "Option:",
                    minValue: 1,
                    maxValue: dbBackups.Length
                );
                backupFileName = Path.GetFileName(dbBackups[option - 1]);
            }
            else if (!File.Exists(fileSystem.Path.Combine(FileSystemUtilities.GetProjectRoot(fileSystem).FullName, "LocalEnvironment", "DBDumps", backupFileName)))
            {
                throw new FileNotFoundException($"DB Backup '{backupFileName}' doesn't exist.");
            }

            Log.Status("Restoring database", (_) =>
            {
                GenericUtilities.ExecuteSqlCommand(
                    connectionString: $"Data Source=127.0.0.1,1433;Initial Catalog=master;User ID={CoreConstants.LocalDbUser};Password={CoreConstants.LocalDbPassword}",
                    sqlCommand: $"USE[master] RESTORE DATABASE[{defaultDbName}] FROM DISK = N'/DBDumps/{backupFileName}' WITH FILE = 1, MOVE N'{defaultDbName}_Primary' TO N'/var/opt/mssql/data/{defaultDbName}.mdf', MOVE N'{defaultDbName}_FG_MainTableDat_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_MainTableDat_1.ndf', MOVE N'{defaultDbName}_FG_MainTableIdx_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_MainTableIdx_1.ndf', MOVE N'{defaultDbName}_FG_HstTableDat_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_HstTableDat_1.ndf', MOVE N'{defaultDbName}_FG_HstTableIdx_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_HstTableIdx_1.ndf', MOVE N'{defaultDbName}_FG_MappingTableIdx_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_MappingTableIdx_1.ndf', MOVE N'{defaultDbName}_FG_MappingTableDat_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_MappingTableDat_1.ndf', MOVE N'{defaultDbName}_FG_MappingHstIdx_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_MappingHstIdx_1.ndf', MOVE N'{defaultDbName}_FG_MappingHstDat_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_MappingHstDat_1.ndf', MOVE N'{defaultDbName}_FG_IntegrationTableIdx_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_IntegrationTableIdx_1.ndf', MOVE N'{defaultDbName}_FG_IntegrationTableDat_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_IntegrationTableDat_1.ndf', MOVE N'{defaultDbName}_FG_IntegrationHstIdx_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_IntegrationHstIdx_1.ndf', MOVE N'{defaultDbName}_FG_IntegrationHstDat_1' TO N'/var/opt/mssql/data/{defaultDbName}_FG_IntegrationHstDat_1.ndf', MOVE N'{defaultDbName}_log' TO N'/var/opt/mssql/data/{defaultDbName}.ldf', NOUNLOAD, STATS = 5"
                );

                Log.Information($"Restored '{backupFileName}'.");
            });
        }
    }
}
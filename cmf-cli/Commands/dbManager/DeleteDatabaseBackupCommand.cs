using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;

namespace Cmf.CLI.Commands.DbManager
{
    /// <summary>
    ///     DeleteDatabaseBackupCommand
    /// </summary>
    [CmfCommand("delete", Id = "dbmanager_delete", ParentId = "dbmanager")]
    public class DeleteDatabaseBackupCommand : BaseCommand
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
                    description: "Backup file name (e.g.: Custom-Babkup.bak)."
                )
            );

            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        public void Execute(string backupFileName = null)
        {
            Log.Verbose("");

            // Set initial backupFilePath
            string backupFilePath = fileSystem.Path.Combine(FileSystemUtilities.GetProjectRoot(fileSystem).FullName, "LocalEnvironment", "DBDumps", backupFileName.IsNullOrEmpty() ? "" : backupFileName);

            string[] dbBackups = GenericUtilities.GetBdBackups(fileSystem);

            if (backupFileName.IsNullOrEmpty())
            {
                Log.Verbose("Select DB backup file to delete:");
                for (int i = 0; i < dbBackups.Length; i++)
                {
                    Log.Verbose($"{i + 1} - {Path.GetFileName(dbBackups[i])}");
                }
                int option = GenericUtilities.ReadIntValueFromConsole(
                    prompt: "Option:",
                    minValue: 1,
                    maxValue: dbBackups.Length
                );
                backupFilePath = dbBackups[option - 1];
                backupFileName = Path.GetFileName(backupFilePath);
            }
            else if (!File.Exists(backupFilePath))
            {
                throw new FileNotFoundException($"DB Backup '{backupFileName}' doesn't exist.");
            }

            Log.Status("Deleting backup file", (_) =>
            {
                File.Delete(backupFilePath);

                Log.Information($"Deleted '{backupFileName}'.");
            });
        }
    }
}
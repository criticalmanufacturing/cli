using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.Threading;

namespace Cmf.CLI.Commands.Run
{
    /// <summary>
    ///     RunHostCommand
    /// </summary>
    /// <seealso cref="BaseCommand"/>
    [CmfCommand("host", Id = "run_host", ParentId = "run")]
    public class RunHostCommand : BaseCommand
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
        ///     <para>Runs the Host service</para>
        ///     <para>Press 'R' to restart the Host execution</para>
        /// </summary>
        public void Execute()
        {
            string businessTierFolderPath = fileSystem.Path.Combine(FileSystemUtilities.GetProjectRoot(fileSystem).FullName, "LocalEnvironment", "BusinessTier");
            string hostExePath = fileSystem.Path.Combine(businessTierFolderPath, "Cmf.Foundation.Services.HostService.exe");

            Environment.CurrentDirectory = businessTierFolderPath;

            while (true)
            {
                CmfGenericUtilities.StartProjectDbContainer(fileSystem);

                CmfGenericUtilities.BuildAllPackagesOfType(PackageType.Business, fileSystem);

                Environment.CurrentDirectory = businessTierFolderPath;

                // Create HostService process
                using Process process = new Process();
                process.StartInfo.FileName = hostExePath;
                process.Start();
                Log.Information("Host Service process started. Press 'R' to rebuild and restart.");

                while (!process.HasExited)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.R)
                        {
                            process.Kill();
                            GenericUtilities.FullConsoleClear();
                            process.WaitForExit();

                            CmfGenericUtilities.BuildAllPackagesOfType(PackageType.Business, fileSystem);

                            process.Start();
                            Log.Information("Host Service process rebuild and restarted.");
                        }
                    }
                    Thread.Sleep(1000);
                }

                Log.Information("Process Terminated! Restarting in 30 sec.");
                Thread.Sleep(30000);
                GenericUtilities.FullConsoleClear();
            }
        }
    }
}
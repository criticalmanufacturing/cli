using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading;

namespace Cmf.CLI.Commands.Run
{
    /// <summary>
    ///     RunMessageBusCommand
    /// </summary>
    /// <seealso cref="BaseCommand"/>
    [CmfCommand("messagebus", Id = "run_messagebus", ParentId = "run")]
    public class RunMessageBusCommand : BaseCommand
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
        ///     <para>Runs the Message Bus</para>
        ///     <para>Press 'R' to restart the MessageBus execution</para>
        /// </summary>
        public void Execute()
        {
            IDirectoryInfo projectRoot = FileSystemUtilities.GetProjectRoot(fileSystem);

            string messageBusFolderPath = fileSystem.Path.Combine(projectRoot.FullName, "LocalEnvironment", "MessageBusGateway");
            string messageBusExePath = fileSystem.Path.Combine(messageBusFolderPath, "Cmf.MessageBus.Gateway.exe");

            Environment.CurrentDirectory = messageBusFolderPath;

            while (true)
            {
                using Process process = new Process();
                process.StartInfo.FileName = messageBusExePath;

                process.Start();
                Log.Information("Process started. Press 'R' to restart.");

                while (!process.HasExited)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.R)
                        {
                            GenericUtilities.FullConsoleClear();
                            process.Kill();
                            process.WaitForExit();

                            process.Start();
                            Log.Information("Process restarted.");
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
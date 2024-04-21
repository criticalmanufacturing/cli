using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="BaseCommand" />
    public class PluginCommand : BaseCommand
    {
        /// <summary>
        /// The command name
        /// </summary>
        private readonly string commandName;

        /// <summary>
        /// The command path
        /// </summary>
        private readonly string commandPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginCommand"/> class.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="commandPath">The command path.</param>
        public PluginCommand(string commandName, string commandPath)
        {
            this.commandName = commandName;
            this.commandPath = commandPath;
        }

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.TreatUnmatchedTokensAsErrors = false;
            // Add the handler
            cmd.Handler = CommandHandler.Create(
                (ParseResult parseResult, IConsole console) =>
                {
                    // console.Out.WriteLine($"{parseResult}");
                    this.Execute(parseResult.UnparsedTokens);
                });
        }

        /// <summary>
        /// Executes the plugin with the supplied parameters
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Execute(IReadOnlyCollection<string> args)
        {
            ProcessStartInfo ps = new();
            ps.FileName = this.commandPath;
            args.ToList().ForEach(arg => ps.ArgumentList.Add(arg));
            ps.UseShellExecute = false;
            ps.RedirectStandardOutput = true;
            ps.RedirectStandardError = true;
            
            Action<string> outputHandler = Console.WriteLine;
            Action<string> errorHandler = Log.Error;

            using var process = System.Diagnostics.Process.Start(ps);
            if (process == null)
            {
                throw new Exception("Could not spawn child command");
            }
            
            process.ErrorDataReceived += (sender, args) => errorHandler(args.Data);
            process.OutputDataReceived += (sender, args) => outputHandler(args.Data);
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Log.Debug("Caught SIGINT, terminating child process");
                process.Disposed += (sender, args) => Log.Debug("Child process Disposed");
                process.Kill(entireProcessTree: true);
                Environment.Exit(-1);
            };
            process.WaitForExit();
            var exitCode = process.ExitCode;
            Log.Debug($"Child process exited with code {exitCode}");

            if (exitCode != 0)
            {
                Log.Debug($"Plugin did not complete successfully: {exitCode}.");

                throw new CliException($"{commandName} {string.Join(" ", args)} did not finished successfully.", (ErrorCode)exitCode);
            }
        }
    }
}
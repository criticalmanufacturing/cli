using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Threading.Tasks;
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
        /// Whether the plugin writes directly to this process' stdout/stderr, instead of having its output relayed line by line.
        /// When null (default), each stream is inherited when it is not redirected, i.e. when cmf is running in a terminal,
        /// so that plugins can show interactive prompts. Set it to force a mode (used by tests).
        /// </summary>
        internal bool? InheritConsoleOutput { get; init; }

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
            // Add the handler
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                // Get unmatched tokens from parseResult
                this.Execute(parseResult.UnmatchedTokens);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Executes the plugin with the supplied parameters
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Execute(IReadOnlyCollection<string> args)
        {
            // A plugin only sees a terminal (and can prompt the user) if its stdout/stderr are not redirected;
            // stdin is always inherited. When cmf's own output is redirected (CI, pipes, tests), keep relaying it line by line.
            var inheritOutput = InheritConsoleOutput ?? !Console.IsOutputRedirected;
            var inheritError = InheritConsoleOutput ?? !Console.IsErrorRedirected;

            ProcessStartInfo ps = new();
            ps.FileName = this.commandPath;
            args.ToList().ForEach(arg => ps.ArgumentList.Add(arg));
            ps.UseShellExecute = false;
            ps.RedirectStandardOutput = !inheritOutput;
            ps.RedirectStandardError = !inheritError;
            Log.Debug($"Running plugin {commandName} (stdout {(inheritOutput ? "inherited" : "relayed")}, stderr {(inheritError ? "inherited" : "relayed")})");

            Action<string> outputHandler = Console.WriteLine;
            Action<string> errorHandler = Log.Error;

            using var process = System.Diagnostics.Process.Start(ps);
            if (process == null)
            {
                throw new CliException("Could not spawn child command");
            }

            if (ps.RedirectStandardError)
            {
                process.ErrorDataReceived += (sender, args) => errorHandler(args.Data);
                process.BeginErrorReadLine();
            }

            if (ps.RedirectStandardOutput)
            {
                process.OutputDataReceived += (sender, args) => outputHandler(args.Data);
                process.BeginOutputReadLine();
            }

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

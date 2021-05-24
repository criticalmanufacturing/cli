using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
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
            ps.Arguments = String.Join(' ', args);
            ps.UseShellExecute = false;
            ps.RedirectStandardOutput = true;

            using var process = System.Diagnostics.Process.Start(ps);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
        }
    }
}
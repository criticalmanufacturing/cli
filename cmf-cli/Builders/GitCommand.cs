using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cmf.CLI.Builders
{
    /// <summary>
    /// Execute a git command
    /// </summary>
    public class GitCommand : ProcessCommand
    {
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public string Command { get; set; }
        
        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public string[] Args { get; set; } = Array.Empty<string>();

        /// <inheritdoc />
        public override ProcessBuildStep[] GetSteps()
        {
            var args = this.Args.ToList();
            args.Insert(0, this.Command);
            // args.AddRange(new[] { "--color", "always" });
            return new[]
            {
                new ProcessBuildStep()
                {
                    Command = "git",
                    Args = args.ToArray()
                }
            };
        }
    }
}
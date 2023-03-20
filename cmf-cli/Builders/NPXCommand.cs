using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Builders
{
    /// <summary>
    /// run npx command
    /// </summary>
    public class NPXCommand : ProcessCommand, IBuildCommand
    {
        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public string[] Args { get; set; }
        
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public string Command { get; set; }

        /// <summary>
        /// Should for npx output to always be colorized
        /// </summary>
        public bool ForceColorOutput { get; set; } = true;
        
        /// <inheritdoc />
        public override ProcessBuildStep[] GetSteps()
        {
            var args = this.Args?.ToList() ?? new List<string>();
            args.Insert(0, this.Command);
            if (this.ForceColorOutput)
            {
                args.AddRange(new[] { "--color", "always" });
            }
            return new[]
            {
                new ProcessBuildStep()
                {
                    Command = "npx" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".cmd" : ""),
                    Args = args.ToArray()
                }
            };
        }

        /// <inheritdoc />
        public string DisplayName { get; set; }

        /// <summary>
        /// Only Executes on Test (--test)
        /// </summary>
        /// <value>
        /// boolean if to execute on Test should be true
        /// </value>
        public bool Test { get; set; } = false;

    }
}
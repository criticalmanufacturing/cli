using System;
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
            var args = new List<string>
            {
                // Bypass any interactive confirmation prompts that might appear
                "-y"
            };

            // The initial versions of NPX (<= 6) that are used by MES v8 and v9 must be called like this:
            //      npx -y true
            // Later versions of NPX (>= 7) that are used by MES v10 and greater, must be called only like this:
            //      npx -y
            // So we get the MES version of the project, and we assume that the user must be running the supported
            // node version for that MES project
            var mesVersion = ExecutionContext.Instance?.ProjectConfig?.MESVersion;
            if (mesVersion != null && mesVersion < new Version(10, 0)) 
            {
                args.Add("true");
            }
 
            args.Add(this.Command);
            if (this.Args != null) 
            {
                args.AddRange(this.Args);
            }

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
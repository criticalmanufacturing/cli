using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cmf.Common.Cli.Builders
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Builders.ProcessCommand" />
    /// <seealso cref="Cmf.Common.Cli.Builders.IBuildCommand" />
    public class NPMCommand : ProcessCommand, IBuildCommand
    {
        // # npm
        //     - task: Npm@1
        //     displayName: 'npm install'
        //     inputs:
        //     command: custom
        //     workingDir: UI/HTML
        //     verbose: false
        //     customCommand: 'install --force'
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Only Executes on Test (--test)
        /// </summary>
        /// <value>
        /// boolean if to execute on Test should be true
        /// </value>
        public bool Test { get; set; } = false;

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public string[] Args { get; set; }

        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <returns></returns>
        public override ProcessBuildStep[] GetSteps()
        {
            var args = this.Args?.ToList() ?? new List<string>();
            args.Insert(0, this.Command);
            args.AddRange(new[] { "--color", "always" });
            return new[]
            {
                new ProcessBuildStep()
                {
                    Command = "npm" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".cmd" : ""),
                    Args = args.ToArray()
                }
            };
        }
    }
}
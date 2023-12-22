using Cmf.CLI.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cmf.CLI.Builders
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="ProcessCommand" />
    /// <seealso cref="IBuildCommand" />
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
        /// Gets or sets the condition. 
        /// This will impact the Condition(), the Condtion will run the Func to determine if it should reply with true or false
        /// By Default it will return true
        /// </summary>
        /// <value>
        /// A Func that if it returns true it will allow the Execute to run.
        /// </value>
        /// <returns>Func<bool></returns>
        public Func<bool> ConditionForExecute = () => { return true; };

        /// <summary>
        /// This method will be used to do a run check before the Exec() is able to run.
        /// If Condition() is false, the Exec() will not be able to run
        /// If Condition() is true, the Exec() will run
        /// </summary>
        /// <returns></returns>
        public override bool Condition()
        {
            return this.ConditionForExecute();
        }

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
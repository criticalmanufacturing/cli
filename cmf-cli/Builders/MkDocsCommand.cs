using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Builders
{
    /// <summary>
    /// Executes MkDocs commands (e.g. mkdocs build),
    /// optionally targeting a Python virtual environment.
    /// </summary>
    /// <seealso cref="ProcessCommand" />
    /// <seealso cref="IBuildCommand" />
    public class MkDocsCommand : ProcessCommand, IBuildCommand
    {
        /// <summary>
        /// Gets or sets the command (e.g. build, serve).
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
        /// Gets or sets the path of a Python virtual environment (relative to the working directory).
        /// When set, the environment's own MkDocs executable is used, which is equivalent to activating it.
        /// </summary>
        /// <value>
        /// The virtual environment path.
        /// </value>
        public string VirtualEnvironment { get; set; }

        /// <summary>
        /// Gets or sets the list of environment variables
        /// </summary>
        /// <value>
        /// The environment variables.
        /// </value>
        public Dictionary<string, string> EnvironmentVariables { get; set; }

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
        /// Gets the MkDocs executable, resolved inside the virtual environment when one is set.
        /// </summary>
        /// <returns></returns>
        public string GetMkDocsExecutable()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (string.IsNullOrEmpty(this.VirtualEnvironment))
            {
                return isWindows ? "mkdocs.exe" : "mkdocs";
            }

            return this.fileSystem.Path.Join(
                this.VirtualEnvironment,
                isWindows ? "Scripts" : "bin",
                isWindows ? "mkdocs.exe" : "mkdocs");
        }

        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <returns></returns>
        public override ProcessBuildStep[] GetSteps()
        {
            var args = new List<string>();
            if (!string.IsNullOrEmpty(this.Command))
            {
                args.Add(this.Command);
            }
            if (this.Args != null)
            {
                args.AddRange(this.Args);
            }

            return new[]
            {
                new ProcessBuildStep()
                {
                    Command = this.GetMkDocsExecutable(),
                    Args = args.ToArray(),
                    WorkingDirectory = this.WorkingDirectory,
                    EnvironmentVariables = this.EnvironmentVariables
                }
            };
        }
    }
}

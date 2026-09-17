using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Builders
{
    /// <summary>
    /// Executes Python commands (e.g. python3 -m venv, python -m pip),
    /// optionally targeting a virtual environment.
    /// </summary>
    /// <seealso cref="ProcessCommand" />
    /// <seealso cref="IBuildCommand" />
    public class PythonCommand : ProcessCommand, IBuildCommand
    {
        /// <summary>
        /// Gets or sets the module to run with '-m' (e.g. venv, pip).
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public string Module { get; set; }

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
        /// When set, the environment's own Python interpreter is used, which is equivalent to activating it.
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
        /// Gets the Python executable, resolved inside the virtual environment when one is set.
        /// </summary>
        /// <returns></returns>
        public string GetPythonExecutable()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (string.IsNullOrEmpty(this.VirtualEnvironment))
            {
                return isWindows ? "python.exe" : "python3";
            }

            return this.fileSystem.Path.Join(
                this.GetVirtualEnvironmentPath(),
                isWindows ? "Scripts" : "bin",
                isWindows ? "python.exe" : "python");
        }

        /// <summary>
        /// Gets the absolute path of the configured virtual environment.
        /// Relative paths are resolved from the command's working directory.
        /// </summary>
        /// <returns>The absolute virtual environment path.</returns>
        public string GetVirtualEnvironmentPath()
        {
            if (string.IsNullOrEmpty(this.VirtualEnvironment))
            {
                return this.fileSystem.Path.GetFullPath(
                    this.fileSystem.Path.Combine(this.WorkingDirectory.FullName, ".venv"));
            }

            return this.fileSystem.Path.IsPathRooted(this.VirtualEnvironment)
                ? this.VirtualEnvironment
                : this.fileSystem.Path.GetFullPath(
                    this.fileSystem.Path.Combine(this.WorkingDirectory.FullName, this.VirtualEnvironment));
        }

        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <returns></returns>
        public override ProcessBuildStep[] GetSteps()
        {
            var args = new List<string>();
            if (!string.IsNullOrEmpty(this.Module))
            {
                args.Add("-m");
                args.Add(this.Module);
            }
            if (this.Args != null)
            {
                args.AddRange(this.Args);
            }

            var environmentVariables = this.EnvironmentVariables == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(this.EnvironmentVariables);

            if (!string.IsNullOrEmpty(this.VirtualEnvironment))
            {
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                var scriptsDirectory = isWindows ? "Scripts" : "bin";
                var virtualEnvironmentPath = this.GetVirtualEnvironmentPath();
                var virtualEnvironmentBin = this.fileSystem.Path.Join(virtualEnvironmentPath, scriptsDirectory);
                var path = System.Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

                // Match the environment that would be produced by activating the venv.
                // A null value is handled by ProcessCommand as a request to remove the
                // inherited variable from ProcessStartInfo.EnvironmentVariables.
                environmentVariables["PYTHONHOME"] = null;
                environmentVariables["VIRTUAL_ENV"] = virtualEnvironmentPath;
                environmentVariables["VIRTUAL_ENV_PROMPT"] = this.fileSystem.DirectoryInfo.New(virtualEnvironmentPath).Name;
                environmentVariables["PATH"] = string.IsNullOrEmpty(path)
                    ? virtualEnvironmentBin
                    : virtualEnvironmentBin + this.fileSystem.Path.PathSeparator + path;
            }

            return new[]
            {
                new ProcessBuildStep()
                {
                    Command = this.GetPythonExecutable(),
                    Args = args.ToArray(),
                    WorkingDirectory = this.WorkingDirectory,
                    EnvironmentVariables = environmentVariables
                }
            };
        }
    }
}

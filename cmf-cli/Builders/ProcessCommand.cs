using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Builders
{
    /// <summary>
    ///
    /// </summary>
    public class ProcessBuildStep
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
        /// Gets or sets the working directory.
        /// </summary>
        /// <value>
        /// The working directory.
        /// </value>
        public IDirectoryInfo WorkingDirectory { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class ProcessCommand
    {
        /// <summary>
        /// the underlying file system
        /// </summary>
        protected IFileSystem fileSystem = new FileSystem();
        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        /// <value>
        /// The working directory.
        /// </value>
        public IDirectoryInfo WorkingDirectory { get; set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        public Task Exec()
        {
            foreach (var step in this.GetSteps())
            {
                var command = step.Command;
                if (step.Command.IndexOf(this.fileSystem.Path.PathSeparator) < 0)
                {
                    // determine full path
                    var enviromentPath = System.Environment.GetEnvironmentVariable("PATH");

                    var paths = enviromentPath.Split(';');
                    var exePath = paths.Select(x => this.fileSystem.Path.Combine(x, command))
                        .Where(this.fileSystem.File.Exists)
                        .FirstOrDefault();

                    command = exePath ?? command;
                }
                Log.Debug($"Executing '{command} {String.Join(' ', step.Args)}'");
                ProcessStartInfo ps = new();
                ps.FileName = command;
                ps.WorkingDirectory = step.WorkingDirectory != null ? step.WorkingDirectory.FullName : this.WorkingDirectory.FullName;
                ps.Arguments = String.Join(' ', step.Args);
                ps.UseShellExecute = false;
                ps.RedirectStandardOutput = true;
                ps.RedirectStandardError = true;

                using var process = System.Diagnostics.Process.Start(ps);
                process.OutputDataReceived += (sender, args) => Log.Verbose(args.Data);
                process.ErrorDataReceived += (sender, args) => Log.Error(args.Data);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                // Console.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new CliException($"Command '{command} {String.Join(' ', step.Args)}' did not finished successfully: Exit code {process.ExitCode}. Please check the log for more details");
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <returns></returns>
        public abstract ProcessBuildStep[] GetSteps();
    }
}
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Media;
using System.Threading.Tasks;

namespace Cmf.CLI.Builders
{
    /// <summary>
    /// </summary>
    public abstract class ProcessCommand : IProcessCommand
    {
        /// <summary>
        ///     the underlying file system
        /// </summary>
        protected IFileSystem fileSystem = new FileSystem();

        /// <summary>
        ///     Gets or sets the working directory.
        /// </summary>
        /// <value>
        ///     The working directory.
        /// </value>
        public IDirectoryInfo WorkingDirectory { get; set; }

        public virtual bool Condition()
        {
            return true;
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>
        /// </returns>
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

                if (Condition())
                {
                    Log.Debug($"Executing '{command} {String.Join(' ', step.Args ?? Array.Empty<string>())}'");
                    using var ps = ExecutionContext.ServiceProvider.GetService<IProcessStartInfoCLI>();
                    ps.FileName = command;
                    ps.WorkingDirectory = step.WorkingDirectory != null ? step.WorkingDirectory.FullName : this.WorkingDirectory.FullName;
                    ps.Arguments = String.Join(' ', step.Args);
                    ps.UseShellExecute = false;
                    ps.RedirectStandardOutput = true;
                    ps.RedirectStandardError = true;

                    if (step.EnvironmentVariables.HasAny())
                    {
                        foreach (var envVar in step.EnvironmentVariables)
                        {
                            // if the key exists we should remove it to avoid errors
                            if (ps.EnvironmentVariables.ContainsKey(envVar.Key))
                            {
                                ps.EnvironmentVariables.Remove(envVar.Key);
                            }

                            ps.EnvironmentVariables.Add(envVar.Key, envVar.Value);
                        }
                    }

                    ps.Start();
                    ps.AddEventOutDataReceived((sender, args) => Log.Verbose(args.Data));
                    ps.AddEvenErrorDataReceived((sender, args) => Log.Error(args.Data));
                    ps.BeginOutputReadLine();
                    ps.BeginErrorReadLine();
                    // Console.WriteLine(process.StandardOutput.ReadToEnd());
                    ps.WaitForExit();
                    if (ps.ExitCode != 0)
                    {
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            // SoundPlayer is only supported on windows
                            new SoundPlayer(@"C:\Windows\Media\chord.wav").PlaySync();
                        }
                        throw new CliException($"Command '{command} {String.Join(' ', step.Args)}' did not finish successfully: Exit code {ps.ExitCode}. Please check the log for more details");
                    }
                    ps.Dispose();
                }
                else
                {
                    Log.Debug($"Command: '{command} {String.Join(' ', step.Args ?? Array.Empty<string>())}' will not be executed as its condition was not met");
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the steps.
        /// </summary>
        /// <returns>
        /// </returns>
        public abstract ProcessBuildStep[] GetSteps();
    }
}
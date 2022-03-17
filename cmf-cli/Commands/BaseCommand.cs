using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Core;

namespace Cmf.Common.Cli.Commands
{
    public abstract class BaseCommand : Cmf.CLI.Core.Commands.BaseCommand
    {
        /// <summary>
        /// constructor for System.IO filesystem
        /// </summary>
        public BaseCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public BaseCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }
        
        
        /// <summary>
        /// Adds the plugin commands.
        /// </summary>
        /// <param name="command">The command.</param>
        public static void AddPluginCommands(Command command)
        {
            const string pluginsPrefix = "cmf-";
            var UNIX = new string[] { "", ".sh", ".ps1" };
            var WIN = new string[] { ".exe", ".cmd", ".ps1" };
            string[] prio;
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                // in unix, avoid .exe and .cmd. Accept no extension, .sh and .ps1 (assume it's PSCore)
                prio = UNIX;
            }
            else
            {
                // in windows, use .exe, .cmd or .ps1
                prio = WIN;
            }

            var plugins = new Dictionary<string, string>();
            var paths = (Environment.GetEnvironmentVariable("PATH") ?? "")
                .Split(System.IO.Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Prepend(Environment.CurrentDirectory);

            foreach (string test in paths)
            {
                string path = test.Trim();
                DirectoryInfo d = new(path);
                if (d.Exists) // we may have some trash in PATH
                {
                    FileInfo[] commands = null;
                    try
                    {
                        commands = d.GetFiles($"{pluginsPrefix}*");
                    }
                    catch (Exception ex)
                    {
                        /* ignore paths we cannot access (this is very common in WSL) */
                        Log.Debug($"Cannot fetch commands in {d.FullName}: {ex.Message}");
                        continue;
                    }
                    foreach (var file in commands)
                    {
                        var commandName = !string.IsNullOrEmpty(file.Extension) ? file.Name[0..^file.Extension.Length] : file.Name;
                        commandName = commandName[pluginsPrefix.Length..];

                        if (!string.IsNullOrWhiteSpace(commandName))
                        {
                            var pos = Array.IndexOf(prio, file.Extension);
                            if (pos > -1)
                            {
                                if (!plugins.ContainsKey(commandName))
                                {
                                    plugins.Add(commandName, file.FullName);
                                    // Console.WriteLine($"Added command {commandName} as {file.FullName}");
                                }
                                else
                                {
                                    var existingPos = Array.IndexOf(prio, file.Extension);
                                    if (existingPos > pos)
                                    {
                                        plugins[commandName] = file.FullName;
                                        // Console.WriteLine($"Replaced command {commandName} with {file.FullName}");
                                    }
                                }
                            }
                            else
                            {
                                // Console.WriteLine($"Skipping {file.FullName} for command {file.Name} as it's not supported on this platform");
                            }
                        }
                    }
                }
            }

            foreach (var commandPlugin in plugins)
            {
                var cmdInstance = new Command(commandPlugin.Key);
                var commandHandler = new PluginCommand(commandPlugin.Key, commandPlugin.Value);
                commandHandler.Configure(cmdInstance);
                command.AddCommand(cmdInstance);
            }
        }
    }
}
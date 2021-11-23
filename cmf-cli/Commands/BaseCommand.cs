using Cmf.Common.Cli.Attributes;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Cmf.Common.Cli.Objects;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    public abstract class BaseCommand
    {
        /// <summary>
        /// The underlying filesystem
        /// </summary>
        protected IFileSystem fileSystem;

        /// <summary>
        /// constructor for System.IO filesystem
        /// </summary>
        public BaseCommand() : this(new FileSystem())
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public BaseCommand(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            ExecutionContext.Initialize(fileSystem);
        }

        /// <summary>
        /// Configure command
        /// </summary>
        public abstract void Configure(Command cmd);

        /// <summary>
        /// Register all available commands, identified using the CmfCommand attribute.
        /// </summary>
        /// <param name="command">Command to which commands will be added</param>
        public static void AddChildCommands(Command command)
        {
            // Get all types that are marked with CmfCommand attribute
            var commandTypes = new List<Type>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.GetCustomAttributes<CmfCommandAttribute>(false).Any())
                {
                    commandTypes.Add(type);
                }
            }

            // Commands that depend on root (have no defined parent)
            var topmostCommands = commandTypes.Where(
                t => string.IsNullOrWhiteSpace(t.GetCustomAttributes<CmfCommandAttribute>(false)
                    .First().Parent));

            foreach (var cmd in topmostCommands)
            {
                var childCmd = FindChildCommands(cmd, commandTypes);
                command.AddCommand(childCmd);
            }
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
                    foreach (var file in d.GetFiles($"{pluginsPrefix}*"))
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

        /// <summary>
        /// Finds the child commands.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="commandTypes">The command types.</param>
        /// <returns></returns>
        private static Command FindChildCommands(Type cmd, List<Type> commandTypes)
        {
            var cmdName = cmd.GetCustomAttribute<CmfCommandAttribute>().Name;

            // Create command
            var cmdInstance = new Command(cmdName);

            // Call "Configure" method
            BaseCommand cmdHandler = Activator.CreateInstance(cmd) as BaseCommand;
            cmdHandler.Configure(cmdInstance);

            // Add commands that depend on me
            var childCommands = commandTypes.Where(
                t => t.GetCustomAttributes(typeof(CmfCommandAttribute), false)
                    .Cast<CmfCommandAttribute>()
                    .First().Parent == cmdName);

            foreach (var child in childCommands)
            {
                var childCmd = FindChildCommands(child, commandTypes);
                cmdInstance.AddCommand(childCmd);
            }
            return cmdInstance;
        }

        /// <summary>
        /// parse argument/option
        /// </summary>
        /// <typeparam name="T">the (target) type of the argument/parameter</typeparam>
        /// <param name="argResult">the arguments to parse</param>
        /// <param name="default">the default value if no value is passed for the argument</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        protected T Parse<T>(ArgumentResult argResult, string @default = null)
        {
            var path = @default;
            if (argResult.Tokens.Any())
            {
                path = argResult.Tokens.First().Value;
            }

            if (string.IsNullOrEmpty(path))
            {
                return default(T);
            }

            return typeof(T) switch
            {
                {} dirType when dirType == typeof(IDirectoryInfo) => (T)this.fileSystem.DirectoryInfo.FromDirectoryName(path),
                {} fileType when fileType == typeof(IFileInfo) => (T)this.fileSystem.FileInfo.FromFileName(path),
                _ => throw new ArgumentOutOfRangeException("This method only parses directory or file paths")
            };
        }
    }
}
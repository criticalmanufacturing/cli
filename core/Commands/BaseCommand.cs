using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Core.Commands
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
            foreach (Type type in Assembly.GetEntryAssembly().GetTypes())
            {
                if (type.GetCustomAttributes<CmfCommandAttribute>(false).Any())
                {
                    commandTypes.Add(type);
                }
            }

            // Commands that depend on root (have no defined parent)
            var topmostCommands = commandTypes.Where(
                t => string.IsNullOrWhiteSpace(t.GetCustomAttributes<CmfCommandAttribute>(false)
                    .First().Parent) && 
                     string.IsNullOrWhiteSpace(t.GetCustomAttributes<CmfCommandAttribute>(false)
                         .First().ParentId));

            foreach (var cmd in topmostCommands)
            {
                var childCmd = FindChildCommands(cmd, commandTypes);
                command.AddCommand(childCmd);
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
            var dec = cmd.GetCustomAttribute<CmfCommandAttribute>();
            var cmdName = dec.Name;

            // Create command
            var cmdInstance = new Command(cmdName) { IsHidden = dec.IsHidden, Description = dec.Description };

            // Call "Configure" method
            BaseCommand cmdHandler = Activator.CreateInstance(cmd) as BaseCommand;
            cmdHandler.Configure(cmdInstance);

            // Add commands that depend on me
            var childCommands = commandTypes.Where(
                t =>
                {
                    var attr = t.GetCustomAttributes(typeof(CmfCommandAttribute), false)
                        .Cast<CmfCommandAttribute>()
                        .First();
                    // ParentId has precedence to Parent
                    return !string.IsNullOrWhiteSpace(attr.ParentId) && attr.ParentId == dec.Id ||
                        string.IsNullOrWhiteSpace(attr.ParentId) && attr.Parent == cmdName;
                });

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
                {} dirType when dirType == typeof(IDirectoryInfo) => (T)this.fileSystem.DirectoryInfo.New(path),
                {} fileType when fileType == typeof(IFileInfo) => (T)this.fileSystem.FileInfo.New(path),
                _ => throw new ArgumentOutOfRangeException("This method only parses directory or file paths")
            };
        }
    }
}
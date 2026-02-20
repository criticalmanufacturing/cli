using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace Cmf.CLI.Core.Commands
{
    /// <summary>
    ///
    /// </summary>
    public abstract class BaseCommand : IBaseCommand
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
                command.Add(childCmd);
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
            var cmdInstance = new Command(cmdName)
            {
                Hidden = dec.IsHidden,
                Description = dec.Description
            };

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
                cmdInstance.Add(childCmd);
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
            var argValue = @default;
            if (argResult?.Tokens?.Any() == true)
            {
                argValue = argResult.Tokens.First().Value;
            }

            if (string.IsNullOrEmpty(argValue))
            {
                return default(T);
            }

            return typeof(T) switch
            {
                {} dirType when dirType == typeof(IDirectoryInfo) => (T)this.fileSystem.DirectoryInfo.New(argValue),
                {} fileType when fileType == typeof(IFileInfo) => (T)this.fileSystem.FileInfo.New(argValue),
                {} stringType when stringType == typeof(string) => (T)(object)argValue,
                _ => throw new ArgumentOutOfRangeException("This method only parses directories, file paths or strings")
            };
        }

        /// <summary>
        /// Parse URI from argument result, supporting both regular URIs and UNC paths/file paths
        /// </summary>
        /// <param name="argResult">the arguments to parse</param>
        /// <returns>Parsed URI or null if no valid value</returns>
        protected Uri ParseUri(ArgumentResult argResult)
        {
            if (argResult?.Tokens?.Any() != true)
            {
                return null;
            }

            var value = argResult.Tokens.First().Value;
            return ParseUriFromString(value);
        }

        /// <summary>
        /// Parse URI array from argument result
        /// </summary>
        /// <param name="argResult">the arguments to parse</param>
        /// <returns>Array of parsed URIs or null if no tokens</returns>
        protected Uri[] ParseUriArray(ArgumentResult argResult)
        {
            if (argResult?.Tokens?.Any() != true)
            {
                return null;
            }

            var uris = new System.Collections.Generic.List<Uri>();
            foreach (var token in argResult.Tokens)
            {
                var value = token.Value;
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                var parsedUri = ParseUriFromString(value);
                if (parsedUri != null)
                {
                    uris.Add(parsedUri);
                }
            }

            return uris.Count > 0 ? uris.ToArray() : null;
        }

        /// <summary>
        /// Parse URI from string value
        /// </summary>
        private Uri ParseUriFromString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            // Try to create URI directly first
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                return uri;
            }

            // Handle UNC paths (\\server\share or \\share\path)
            if (value.StartsWith("\\\\"))
            {
                var uncPath = value.Replace("\\", "/");
                if (Uri.TryCreate($"file:{uncPath}", UriKind.Absolute, out var uncUri))
                {
                    return uncUri;
                }
            }

            // Handle Windows drive paths (C:\path or d:\xpto)
            if (value.Length >= 3 && value[1] == ':' && (value[2] == '\\' || value[2] == '/'))
            {
                var normalizedPath = value.Replace('\\', '/');
                if (Uri.TryCreate($"file:///{normalizedPath}", UriKind.Absolute, out var driveUri))
                {
                    return driveUri;
                }
            }

            // Check if it's a relative path - preserve it as a relative URI
            if (Uri.TryCreate(value, UriKind.Relative, out var relativeUri))
            {
                return relativeUri;
            }

            // Handle other formats - try to make absolute
            try
            {
                var fullPath = System.IO.Path.GetFullPath(value);
                return new Uri(fullPath);
            }
            catch
            {
                // As a last resort, try to use the value as-is in a file URI
                try
                {
                    return new Uri($"file:///{value.Replace('\\', '/')}");
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
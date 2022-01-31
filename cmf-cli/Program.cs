using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace Cmf.Common.Cli
{
    /// <summary>
    /// program entry point
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// program entry point
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            try
            {
                var rootCommand = new RootCommand
                {
                    Description = "Critical Manufacturing CLI"
                };

                rootCommand.AddOption(new Option<LogLevel>(
                    aliases: new[] { "--loglevel", "-l" },
                    description: "Log Verbosity",
                    parseArgument: argResult =>
                    {
                        var loglevel = LogLevel.Verbose;
                        string loglevelStr = "verbose";
                        if (argResult.Tokens.Any())
                        {
                            loglevelStr = argResult.Tokens.First().Value;
                        }
                        else if (System.Environment.GetEnvironmentVariable("cmf:cli:loglevel") != null)
                        {
                            loglevelStr = System.Environment.GetEnvironmentVariable("cmf:cli:loglevel");
                        }

                        if (LogLevel.TryParse(typeof(LogLevel), loglevelStr, ignoreCase: true, out object? loglevelObj))
                        {
                            loglevel = (LogLevel)loglevelObj;
                        }
                        Log.Level = loglevel;
                        return loglevel;
                    },
                    isDefault: true
                ));

                if (args.Length == 1 && args.Has("-v"))
                {
                    return rootCommand.Invoke(new[] { "--version" });
                }

                BaseCommand.AddChildCommands(rootCommand);
                BaseCommand.AddPluginCommands(rootCommand);

                return rootCommand.Invoke(args);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return -1; // TODO: set exception error codes
            }
        }
    }
}
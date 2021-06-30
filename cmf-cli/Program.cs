using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Utilities;
using System;
using System.CommandLine;
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
                Log.Error(e.Message);
                return -1; // TODO: set exception error codes
            }
        }
    }
}
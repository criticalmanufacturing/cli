using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Utilities;
using System;
using System.CommandLine;
using System.Reflection;

namespace Cmf.Common.Cli
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 1 && args.Has("-v"))
                {
                    Version version = Assembly.GetEntryAssembly().GetName().Version;
                    Log.Verbose($"{version.Major}.{version.Minor}.{version.Build}");
                    return 0;
                }

                var rootCommand = new RootCommand
                {
                    Description = "Critical Manufacturing CLI"
                };

                BaseCommand.AddChildCommands(rootCommand);
                BaseCommand.AddPluginCommands(rootCommand);

                return rootCommand.Invoke(args);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return 0;
        }
    }
}
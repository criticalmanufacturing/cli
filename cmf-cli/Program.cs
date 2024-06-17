using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.Threading.Tasks;
using Cmf.CLI.Commands;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Microsoft.Extensions.DependencyInjection;
using Cmf.CLI.Core.Enums;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using Cmf.CLI.Constants;
using Cmf.CLI.Services;

namespace Cmf.CLI
{
    /// <summary>
    /// program entry point
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args">Console application input arguments</param>
        /// <returns></returns>
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var (rootCommand, parser) = await StartupModule.Configure(
                    packageName: CliConstants.PackageName,
                    envVarPrefix: "cmf_cli",
                    description: "Critical Manufacturing CLI",
                    args: args,
                    registerExtraServices: collection => 
                        collection.AddSingleton<IDependencyVersionService, DependencyVersionService>());

                using var activity = ExecutionContext.ServiceProvider.GetService<ITelemetryService>()!.StartActivity("Main");

                var result = -1;
                
                if (rootCommand != null)
                {
                    var nonPluginCommands = rootCommand.Children.Where(symbol => symbol is Command).ToList();
                    BaseCommand.AddPluginCommands(rootCommand);
                    var pluginCommands =
                        rootCommand.Children.Where(cmd => cmd is Command && nonPluginCommands.All(np => np.Name != cmd.Name)).ToList();

                    if (args.Length > 0 && pluginCommands.FirstOrDefault(pc => pc.Name == args[0]) is Command pluginMatch)
                    {
                        // we are executing a plugin. we should forward all arguments (except the first) to the plugin
                        var pluginArgs = args[1..];

                        // we should invoke this through the System.CommandLine API but right now we'd have to generate a new pipeline. We'll revisit this in a next version, as it's expected the pipeline instantiation gets more flexible.
                        var type = pluginMatch!.Handler!.GetType();
                        var method = type.GetField("_handlerDelegate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
                        var del = method.GetValue(pluginMatch.Handler) as Delegate;
                        var pluginCommand = del!.Target as PluginCommand;
                        pluginCommand!.Execute(pluginArgs);
                        result = 0;
                    }
                    else
                    {
                        result = await parser.InvokeAsync(args);
                    }
                }
                 
                activity?.SetTag("execution.success", true);
                return result;
            }
            catch (CliException e)
            {
                Log.Error(e.Message);
                Log.Debug(e.StackTrace);
                return (int)e.ErrorCode;
            }
            catch (Exception e)
            {
                Log.Debug("Caught exception at program.");
                Log.Exception(e);
                ExecutionContext.ServiceProvider.GetService<ITelemetryService>()!.LogException(e);
                return (int)ErrorCode.Default;
            }
        }
    }
}
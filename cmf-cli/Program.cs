using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.Threading.Tasks;
using Cmf.CLI.Commands;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Microsoft.Extensions.DependencyInjection;
using Cmf.CLI.Core.Enums;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Services;
using Cmf.CLI.Core.Repository.Credentials;
using Cmf.CLI.Core.Utilities;
using System.Collections.Generic;

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
                var fileSystem = new FileSystem();

                var rootCommand = await StartupModule.Configure(
                    packageId: CliConstants.PackageName,
                    envVarPrefix: "cmf_cli",
                    description: "Critical Manufacturing CLI",
                    args: args,
                    registerExtraServices: collection =>
                    {
                        collection.AddSingleton<IDependencyVersionService, DependencyVersionService>();
                        collection.AddSingleton<IRepositoryLocator, RepositoryLocator>();
                        collection.AddSingleton<IFeaturesService>(new FeaturesService("cmf_cli"));
                        collection.AddSingleton<IRepositoryCredentials>(new PortalRepositoryCredentials(fileSystem));
                        collection.AddSingleton<IRepositoryCredentials>(new NPMRepositoryCredentials(fileSystem));
                        collection.AddSingleton<IRepositoryCredentials>(new NuGetRepositoryCredentials(fileSystem));
                        collection.AddSingleton<IRepositoryCredentials>(new DockerRepositoryCredentials());
                        collection.AddSingleton<IRepositoryCredentials>(new CIFSRepositoryCredentials());
                        collection.AddSingleton<IRepositoryAuthStore>(RepositoryAuthStore.FromEnvironmentConfig(fileSystem));
                    });

                using var activity = ExecutionContext.ServiceProvider.GetService<ITelemetryService>()!.StartActivity("Main");

                var result = -1;

                if (rootCommand != null)
                {
                    var nonPluginCommands = rootCommand.Subcommands.ToList();
                    var pluginCommands = BaseCommand.AddPluginCommands(fileSystem, rootCommand)
                        .Where(plugin => nonPluginCommands.All(np => np.Name != plugin.Key))
                        .ToDictionary(plugin => plugin.Key, plugin => plugin.Value);

                    if (TryExecutePlugin(pluginCommands, args))
                    {
                        result = 0;
                    }
                    else
                    {
                        ExecutionContext.Initialize(fileSystem);
                        ExecutionContext.ServiceProvider.GetService<IRepositoryLocator>()!
                            .InitializeClientsForRepositories(ExecutionContext.Instance.FileSystem);

                        // Global validation for all CLI core commands
                        ValidateMesVersion(ExecutionContext.Instance.ProjectConfig?.MESVersion.Major);
                        
                        // Parse and invoke using beta5 pattern
                        var parseResult = rootCommand.Parse(args);
                        result = await parseResult.InvokeAsync();
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
                Log.Exception(WrappedException.Unwrap(e));
                ExecutionContext.ServiceProvider.GetService<ITelemetryService>()!.LogException(e);
                return (int)ErrorCode.Default;
            }
        }

        /// <summary>
        /// Executes the plugin named by the first argument, if there is one.
        /// The remaining arguments are forwarded exactly as supplied: parsing them with the CLI would consume
        /// the options it also knows (e.g. --help), and the plugin would never receive them.
        /// </summary>
        /// <param name="plugins">the available plugins, indexed by command name</param>
        /// <param name="args">Console application input arguments</param>
        /// <returns>true if a plugin was executed</returns>
        internal static bool TryExecutePlugin(IReadOnlyDictionary<string, PluginCommand> plugins, string[] args)
        {
            if (args.Length == 0 || !plugins.TryGetValue(args[0], out var plugin))
            {
                return false;
            }

            plugin.Execute(args[1..]);
            return true;
        }

        /// <summary>
        /// This function will validate MES Version on every command.
        /// This server to lock the CLI from executing on any project below version 10.
        /// </summary>
        internal static void ValidateMesVersion(int? majorVersion)
        {
            if (majorVersion < 10)
            {
                throw new CliException("MES Versions under 10 are no longer supported with the newest version of the CLI. Please use cmf-cli 5.8.0 or lower.");
            }
        }
    }
}
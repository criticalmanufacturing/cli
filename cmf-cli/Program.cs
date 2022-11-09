using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Cmf.CLI.Commands;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI
{
    /// <summary>
    /// program entry point
    /// </summary>
    public static class Program
    {
        private static System.CommandLine.Parsing.ParseArgument<LogLevel> parseLogLevel = argResult =>
        {
            var loglevel = LogLevel.Verbose;
            string loglevelStr = "verbose";
            if (argResult.Tokens.Any())
            {
                loglevelStr = argResult.Tokens.First().Value;
            }
            else if (System.Environment.GetEnvironmentVariable("cmf_cli_loglevel") != null)
            {
                loglevelStr = System.Environment.GetEnvironmentVariable("cmf_cli_loglevel");
            }

            if (Enum.TryParse(typeof(LogLevel), loglevelStr, ignoreCase: true, out var loglevelObj))
            {
                loglevel = (LogLevel)loglevelObj!;
            }
            Log.Level = loglevel;
            return loglevel;
        };

        /// <summary>
        /// Root Command log verbosity option
        /// </summary>
        public static Option<LogLevel> logLevelOption = new Option<LogLevel>(
                aliases: new[] { "--loglevel", "-l" },
                description: "Log Verbosity",
                parseArgument: parseLogLevel,
                isDefault: true
            );

        /// <summary>
        /// program entry point
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<int> Main(string[] args)
        {
            try
            {
                RegisterServices();
                
                InitializeTelemetry();
                
                await VersionChecks();

                var rootCommand = new RootCommand
                {
                    Description = "Critical Manufacturing CLI"
                };

                rootCommand.AddOption(logLevelOption);

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

        private static void RegisterServices()
        {
            var sp = new ServiceCollection()
                .AddSingleton<INPMClient, NPMClient>()
                .AddSingleton<IVersionService, VersionService>()
                .AddSingleton<ITelemetryService, TelemetryService>()
                .BuildServiceProvider();

            ExecutionContext.ServiceProvider = sp;
        }

        /// <summary>
        /// Version Checker. Logs if we are running an unstable version and/or if we are outdated
        /// </summary>
        public static async Task VersionChecks()
        {
            #region version checks
            
            using var activity = ExecutionContext.ServiceProvider.GetService<ITelemetryService>()!
                .StartActivity("CLIVersion");

            var npmClient = ExecutionContext.ServiceProvider.GetService<INPMClient>();
            
            if (ExecutionContext.IsDevVersion)
            {
                Log.Information(
                    $"You are using {ExecutionContext.PackageId} development version {ExecutionContext.CurrentVersion}. This in unsupported in production and should only be used for testing.");
            }

            ExecutionContext.LatestVersion = await npmClient!.GetLatestVersion(ExecutionContext.IsDevVersion);
            if (ExecutionContext.LatestVersion != null && ExecutionContext.LatestVersion != ExecutionContext.CurrentVersion)
            {
                Log.Warning(
                    $"Using {ExecutionContext.PackageId} version {ExecutionContext.CurrentVersion} while {ExecutionContext.LatestVersion} is available. Please update.");
                // after this run, every activity will have these tags (check TelemetryService)
                activity?.SetTag("isOutdated", true);
                activity?.SetTag("latestVersion", ExecutionContext.LatestVersion);
            }

            #endregion
        }

        private static void InitializeTelemetry()
        {
            var serviceName = "@criticalmanufacturing/cli";
            var serviceVersion = ExecutionContext.CurrentVersion;

            ExecutionContext.ServiceProvider.GetService<ITelemetryService>()!
                .InitializeTracerProvider(serviceName, serviceVersion);
            ExecutionContext.ServiceProvider.GetService<ITelemetryService>()!
                .InitializeActivitySource(serviceName);
        }
    }
}
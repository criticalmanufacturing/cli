using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Objects;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("tests")]
namespace Cmf.CLI.Core
{
    /// <summary>
    /// Startup Module that can be used in .NET plugins
    /// </summary>
    public static class StartupModule
    {
        /// <summary>
        /// Configures the specified package name.
        /// </summary>
        /// <param name="packageName">Package name used for INPMClient implementations</param>
        /// <param name="envVarPrefix">Name of the telemetry service name used as prefix for ITelemetryService implementations</param>
        /// <param name="description">Description used for the root command</param>
        /// <param name="args">ar</param>
        /// <param name="npmClient">The NPM client. if is not set, we assume NPMClient implementation by default</param>
        /// <param name="registerExtraServices">function to add extra services to the ServiceProvider</param>
        public static async Task<RootCommand> Configure(string packageName, string envVarPrefix, string description, string[] args, INPMClient? npmClient = null, Action<IServiceCollection>? registerExtraServices = null)
        {
            // in a scenario that cli is not running on a terminal,
            // the AnsiConsole.Profile.Width defaults to 80,which is a low value and causes unexpected break lines.
            // in that cases we need to double the value
            if (!Log.AnsiConsole.Profile.Out.IsTerminal && Log.AnsiConsole.Profile.Width == 80)
            {
                Log.AnsiConsole.Profile.Width = 160;
            }

            RootCommand rootCommand = new(description);

            ExecutionContext.EnvVarPrefix = envVarPrefix;

            var serviceCollection = new ServiceCollection()
                .AddSingleton(_ => npmClient ?? new NPMClient())
                .AddSingleton<IVersionService>(new VersionService(packageName))
                .AddSingleton<ITelemetryService>(new TelemetryService(packageName))
                .AddSingleton<IProcessStartInfoCLI>(new ProcessStartInfoCLI())
                .AddSingleton<IProjectConfigService, ProjectConfigService>();

            if (registerExtraServices != null)
            {
                registerExtraServices(serviceCollection);
            }
            
            ExecutionContext.ServiceProvider = serviceCollection
                .BuildServiceProvider();
            
             // initialize Telemetry
            var telemetry = ExecutionContext.ServiceProvider.GetService<ITelemetryService>();
            telemetry?.InitializeActivitySource(ExecutionContext.PackageId);

            // Initialize tracer asynchronously
            var telemetryProviderInitTask = telemetry?.InitializeTracerProviderAsync(ExecutionContext.PackageId,ExecutionContext.CurrentVersion);

            _ = telemetryProviderInitTask?.ContinueWith(t =>
            {
                if (t.Exception != null) 
                {
                    Log.Error($"Telemetry initialization faulted: {t.Exception}");
                }
            }, TaskScheduler.Default);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Log.Debug("Uncaught exception!");
                if (eventArgs.ExceptionObject is Exception ex)
                {
                    Log.Exception(ex);
                    ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.LogException(ex);
                }
            };

            await VersionChecks();

            // add LogLevelOption
            rootCommand.Add(LoggerHelpers.LogLevelOption);

            BaseCommand.AddChildCommands(rootCommand);

            // Add environment variables directive
            // Note: SuggestDirective is included by default in RootCommand.Directives
            rootCommand.Directives.Add(new EnvironmentVariablesDirective());

            // Only await if provider task is still running
            if (telemetryProviderInitTask != null && !telemetryProviderInitTask.IsCompleted)
            {
                telemetryProviderInitTask.GetAwaiter().GetResult();
            }

            return rootCommand;
        }

        /// <summary>
        /// Version Checker. Logs if we are running an unstable version and/or if we are outdated
        /// </summary>
        internal static async Task VersionChecks()
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?
                .StartActivity("version");

            var npmClient = ExecutionContext.ServiceProvider?.GetService<INPMClient>();

            if (ExecutionContext.IsDevVersion)
            {
                Log.Warning(
                    $"You are using {ExecutionContext.PackageId} development version {ExecutionContext.CurrentVersion}. This in unsupported in production and should only be used for testing.");
            }

            if (npmClient != null)
            {
                ExecutionContext.LatestVersion = await npmClient.GetLatestVersion(ExecutionContext.IsDevVersion);
                if (ExecutionContext.LatestVersion != null && ExecutionContext.LatestVersion != ExecutionContext.CurrentVersion)
                {
                    Log.Warning(
                        $"Using {ExecutionContext.PackageId} version {ExecutionContext.CurrentVersion} while {ExecutionContext.LatestVersion} is available. Please update.");
                    // after this run, every activity will have these tags (check TelemetryService)
                    activity?.SetTag("isOutdated", true);
                    activity?.SetTag("latestVersion", ExecutionContext.LatestVersion);
                }
            }
        }
    }
}
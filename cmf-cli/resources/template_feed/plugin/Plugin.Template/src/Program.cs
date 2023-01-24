using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core;
using ExecutionContext = Cmf.CLI.Core.Objects.ExecutionContext;
using Cmf.CLI.Utilities;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

try
{
    // as it's an internal development tool, we keep telemetry on by default
    Environment.SetEnvironmentVariable("cmf_<%= $CLI_PARAM_PluginBinary %>_enable_telemetry", "1");
    Environment.SetEnvironmentVariable("cmf_<%= $CLI_PARAM_PluginBinary %>_enable_extended_telemetry", "1");

    var registryAddress = Environment.GetEnvironmentVariable("cmf_<%= $CLI_PARAM_PluginBinary %>_registry");

    var rootCommand = await StartupModule.Configure(
        packageName: "<%= $CLI_PARAM_PluginName %>",
        envVarPrefix: "cmf_<%= $CLI_PARAM_PluginBinary %>",
        description: "<%= $CLI_PARAM_PluginDescription %>",
        args: args);

    using var activity = ExecutionContext.ServiceProvider.GetService<ITelemetryService>()!.StartActivity("Main");

    var result = await rootCommand?.InvokeAsync(args);
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
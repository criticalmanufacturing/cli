using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Cmf.CLI.Utilities;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cmf.CLI.Core.Objects;

public interface ITelemetryService
{
    public TracerProvider Provider { get; }
    TracerProvider InitializeTracerProvider(string serviceName, string version);
    ActivitySource InitializeActivitySource(string name);
    Activity StartActivity(string name, ActivityKind kind = ActivityKind.Internal);
    Activity StartBareActivity(string name, ActivityKind kind = ActivityKind.Internal);
}

public class TelemetryService : ITelemetryService
{
    public TracerProvider Provider { get; private set; }

    private ActivitySource activitySource = null;
    
    public TracerProvider InitializeTracerProvider(string serviceName, string version)
    {
        if (!GenericUtilities.IsEnvVarTruthy("cmf_cli_enable_telemetry"))
        {
            return null;
        }
        
        var telemetryHostname = System.Environment.GetEnvironmentVariable("cmf_cli_telemetry_host");

        var builder = Sdk.CreateTracerProviderBuilder()
            .AddSource(serviceName)
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: version));
        if (GenericUtilities.IsEnvVarTruthy("cmf_cli_telemetry_enable_console_exporter"))
        {
            builder = builder.AddConsoleExporter();    
        }
        builder = builder.AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri(telemetryHostname ?? "https://telemetry.criticalmanufacturing.dev");
            opt.Protocol = OtlpExportProtocol.Grpc;
            opt.ExportProcessorType = ExportProcessorType.Simple;
        });
        Provider = builder.Build();
        return Provider;
    }

    public ActivitySource InitializeActivitySource(string name)
    {
        activitySource = new ActivitySource(name);
        return activitySource;
    }

    public Activity StartBareActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var activity = this.activitySource.StartActivity(name, kind);
        activity?.SetTag("version", ExecutionContext.CurrentVersion);
        if (ExecutionContext.IsDevVersion)
        {
            activity?.SetTag("isDev", ExecutionContext.IsDevVersion);
        }
        if (ExecutionContext.IsOutdated)
        {
            activity?.SetTag("isOutdated", true);
            activity?.SetTag("latestVersion", ExecutionContext.LatestVersion);
        }
        return activity;
    }

    public Activity StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var activity = this.StartBareActivity(name, kind);
        if (GenericUtilities.IsEnvVarTruthy("cmf_cli_enable_extended_telemetry"))
        {
            activity?.SetTag("ip", Dns.GetHostAddresses(Dns.GetHostName())
                .FirstOrDefault(ha => ha.AddressFamily == AddressFamily.InterNetwork)
                ?.ToString());
            activity?.SetTag("hostname", Environment.MachineName);
            activity?.SetTag("username", Environment.UserName);
            activity?.SetTag("cwd", Environment.CurrentDirectory);
        }
        return activity;
    }
}
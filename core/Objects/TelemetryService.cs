using System;
using System.Diagnostics;
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
}

public class TelemetryService : ITelemetryService
{
    public TracerProvider Provider { get; private set; }

    private ActivitySource activitySource = null;
    
    public TracerProvider InitializeTracerProvider(string serviceName, string version)
    {
        if (GenericUtilities.IsEnvVarTruthy("cmf_cli_disable_telemetry"))
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
            opt.Endpoint = new Uri(telemetryHostname ?? "https://cli-telemetry.criticalmanufacturing.dev");
            opt.Protocol = OtlpExportProtocol.Grpc;
            opt.ExportProcessorType = ExportProcessorType.Simple;
        });
        builder = builder.AddZipkinExporter(o =>
        {
            o.Endpoint = new Uri("http://ubuntu.wsl:9411");
        });
        Provider = builder.Build();
        return Provider;
    }

    public ActivitySource InitializeActivitySource(string name)
    {
        activitySource = new ActivitySource(name);
        return activitySource;
    }

    public Activity StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return this.activitySource.StartActivity(name, kind);
    }
}
using System.Diagnostics;
using Cmf.CLI.Core.Objects;
using OpenTelemetry.Trace;

namespace tests.Mocks;

public class MockTelemetryService : ITelemetryService
{
    public TracerProvider Provider => null;
    private ActivitySource activitySource = null;
    public TracerProvider InitializeTracerProvider(string serviceName, string version) => null;

    public ActivitySource InitializeActivitySource(string name)
    {
        activitySource = new ActivitySource(name);
        return activitySource;
    }

    public Activity StartActivity(string name, ActivityKind kind = ActivityKind.Internal) => null;
    public Activity StartBareActivity(string name, ActivityKind kind = ActivityKind.Internal) => null;
    public Activity StartExtendedActivity(string name, ActivityKind kind = ActivityKind.Internal) => null;
}
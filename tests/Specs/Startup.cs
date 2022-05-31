using System.Diagnostics;
using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Objects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
namespace tests.Specs
{
    public class Startup
    {
        #region mock services
        class MockNPMClient999 : INPMClient
        {
            public Task<string> GetLatestVersion(bool preRelease = false)
            {
                return Task.FromResult("999.99.99");
            }
        }
        
        class MockNPMClientCurrent : INPMClient
        {
            public Task<string> GetLatestVersion(bool preRelease = false)
            {
                return Task.FromResult(ExecutionContext.CurrentVersion);
            }
        }

        class MockVersionService : IVersionService
        {
            public string PackageId => "test";
            public string CurrentVersion => "1.0.0";
        }
        
        class MockVersionServiceDev : IVersionService
        {
            public string PackageId => "test";
            public string CurrentVersion => "1.0.0-0";
        }

        class MockTelemetryService : ITelemetryService
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
        }
        #endregion

        [Fact]
        public async Task NotAtLatestVersion()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<INPMClient, MockNPMClient999>()
                .AddSingleton<IVersionService, MockVersionService>()
                .AddSingleton<ITelemetryService, MockTelemetryService>()
                .BuildServiceProvider();

            var logWriter = (new Logging()).GetLogStringWriter();
            
            await Cmf.CLI.Program.VersionChecks();
            
            logWriter.ToString().Should().Contain("Please update");
        }
        
        [Fact]
        public async Task AtLatestVersion()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<INPMClient, MockNPMClientCurrent>()
                .AddSingleton<IVersionService, MockVersionService>()
                .AddSingleton<ITelemetryService, MockTelemetryService>()
                .BuildServiceProvider();

            var logWriter = (new Logging()).GetLogStringWriter();
            
            await Cmf.CLI.Program.VersionChecks();
            
            logWriter.ToString().Should().NotContain("Please update");
        }

        [Fact]
        public async Task InDevVersion()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<INPMClient, MockNPMClientCurrent>()
                .AddSingleton<IVersionService, MockVersionServiceDev>()
                .AddSingleton<ITelemetryService, MockTelemetryService>()
                .BuildServiceProvider();
            
            var logWriter = (new Logging()).GetLogStringWriter();
            
            await Cmf.CLI.Program.VersionChecks();
            
            logWriter.ToString().Should().Contain("You are using development version");
        }
        
        [Fact]
        public async Task InStableVersion()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<INPMClient, MockNPMClientCurrent>()
                .AddSingleton<IVersionService, MockVersionService>()
                .AddSingleton<ITelemetryService, MockTelemetryService>()
                .BuildServiceProvider();
            
            var logWriter = (new Logging()).GetLogStringWriter();
            
            await Cmf.CLI.Program.VersionChecks();
            
            logWriter.ToString().Should().NotContain("You are using development version");
        }
    }
}
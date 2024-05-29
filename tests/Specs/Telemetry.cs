using System;
using System.Linq;
using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using tests.Mocks;
using Xunit;

namespace tests.Specs;

public class Telemetry
{
    [Theory]
    [InlineData(null)] // default value, no env var set
    [InlineData("false")]
    [InlineData("False")]
    [InlineData("FALSE")]
    [InlineData("0")]
    [InlineData("unrecognizedValue")]
    public void NoTelemetryByDefaultOrWhenOff(string telemetrySetting)
    {
        Environment.SetEnvironmentVariable("cmf_cli_enable_telemetry", telemetrySetting);
        
        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<INPMClient, MockNPMClient>()
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton<ITelemetryService, TelemetryService>()
            .BuildServiceProvider();
        
        
        var telemetryService = ExecutionContext.ServiceProvider.GetService<ITelemetryService>();
        var tracerProvider = telemetryService!.InitializeTracerProvider("test_A", "0.0.0");
        var activitySource = telemetryService.InitializeActivitySource("test_A");
        var activity = telemetryService.StartActivity("test_A");
        
        telemetryService.Should().NotBeNull();
        tracerProvider.Should().BeNull("TelemetryService should not provide a TracerProvider if telemetry if off");
        activitySource.Should().NotBeNull();
        activity.Should().BeNull();
    }
    
    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("True")]
    [InlineData("TRUE")]
    public void OnlyBasicTelemetry(string telemetrySetting)
    {
        Environment.SetEnvironmentVariable("cmf_cli_enable_telemetry", telemetrySetting);
        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<INPMClient, MockNPMClient>()
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton<ITelemetryService, TelemetryService>()
            .BuildServiceProvider();
        
        
        var telemetryService = ExecutionContext.ServiceProvider.GetService<ITelemetryService>();
        // service name must match
        var tracerProvider = telemetryService!.InitializeTracerProvider("test_B", "0.0.0");
        var activitySource = telemetryService.InitializeActivitySource("test_B");
        var activity = telemetryService.StartActivity("test_B");
        

        telemetryService.Should().NotBeNull();
        tracerProvider.Should().NotBeNull();
        activitySource.Should().NotBeNull();
        activity.Should().NotBeNull();
    }

    [Fact]
    public void NoExtendedActivitiesIfExtendedTelemetryIsOff()
    {
        Environment.SetEnvironmentVariable("cmf_cli_enable_telemetry", "1");
        Environment.SetEnvironmentVariable("cmf_cli_enable_extended_telemetry", "0");
        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<INPMClient, MockNPMClient>()
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton<ITelemetryService, TelemetryService>()
            .BuildServiceProvider();
        
        var telemetryService = ExecutionContext.ServiceProvider.GetService<ITelemetryService>();
        var tracerProvider = telemetryService!.InitializeTracerProvider("test_NoExtended", "0.0.0");
        var activitySource = telemetryService.InitializeActivitySource("test_NoExtended");
        var activity = telemetryService.StartExtendedActivity("test_NoExtended");
        
        activity.Should().BeNull();
    }

    [Fact]
    public void ActivitiesShouldNotIncludeIdentifiableInfoIfExtendedTelemetryIsOff()
    {
        var allowedTags = new[] { "latestVersion", "isOutdated", "isDev", "version" };
        Environment.SetEnvironmentVariable("cmf_cli_enable_telemetry", "1");
        Environment.SetEnvironmentVariable("cmf_cli_enable_extended_telemetry", "0");
        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<INPMClient, MockNPMClient>()
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton<ITelemetryService, TelemetryService>()
            .BuildServiceProvider();
        
        var telemetryService = ExecutionContext.ServiceProvider.GetService<ITelemetryService>();
        var tracerProvider = telemetryService!.InitializeTracerProvider("test_Extended", "0.0.0");
        var activitySource = telemetryService.InitializeActivitySource("test_Extended");
        var activity = telemetryService.StartActivity("test_Extended");

        activity.Should().NotBeNull();
        var tagNames = activity.Tags.Select(kv => kv.Key);
        tagNames.All(tagName => allowedTags.Contains(tagName)).Should().BeTrue();
    }

    [Theory]
    [InlineData(false, false, false, new [] { "version" })]
    [InlineData(false, true, false, new [] { "version", "isDev" })]
    [InlineData(false, false, true, new [] { "version", "isOutdated", "latestVersion" })]
    [InlineData(false, true, true, new [] { "version", "isDev", "isOutdated", "latestVersion" })]
    [InlineData(true, false, false, new [] { "version", "ip", "hostname", "username", "cwd" })]
    [InlineData(true, true, false, new [] { "version", "isDev", "ip", "hostname", "username", "cwd" })]
    [InlineData(true, false, true, new [] { "version", "isOutdated", "latestVersion", "ip", "hostname", "username", "cwd" })]
    [InlineData(true, true, true, new [] { "version", "isDev", "isOutdated", "latestVersion", "ip", "hostname", "username", "cwd" })]
    public async Task ValidateExpectedBareTags(bool includeExtendedTelemetry, bool isDevVersion, bool isOutdatedVersion, string[] expectedTags)
    {
        Environment.SetEnvironmentVariable("cmf_cli_enable_telemetry", "1");
        Environment.SetEnvironmentVariable("cmf_cli_enable_extended_telemetry", includeExtendedTelemetry ? "1" : null);

        var mockVersionService = new Mock<IVersionService>();
        mockVersionService.SetupGet(service => service.CurrentVersion).Returns(isDevVersion ? "2.0.0-0" : "2.0.0");

        var mockNPMClient = new Mock<INPMClient>();
        mockNPMClient.Setup(mock => mock.GetLatestVersion(true))
            .Returns(() => Task.FromResult(isOutdatedVersion ? "3.0.0-0" : "2.0.0-0"));
        mockNPMClient.Setup(mock => mock.GetLatestVersion(false))
            .Returns(() => Task.FromResult(isOutdatedVersion ? "3.0.0" : "2.0.0"));
        ExecutionContext.LatestVersion = await mockNPMClient.Object.GetLatestVersion(isDevVersion);

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton(mockNPMClient.Object)
            .AddSingleton(mockVersionService.Object)
            .AddSingleton<ITelemetryService, TelemetryService>()
            .BuildServiceProvider();
        
        var telemetryService = ExecutionContext.ServiceProvider.GetService<ITelemetryService>();
        var tracerProvider = telemetryService!.InitializeTracerProvider("test_Bare", "0.0.0");
        var activitySource = telemetryService.InitializeActivitySource("test_Bare");
        var activity = telemetryService.StartActivity("test_Bare");
        
        var tagNames = activity.TagObjects.Select(kv => kv.Key).ToList();
        tagNames.Distinct().Count().Should().Be(expectedTags.Length);
        tagNames.All(expectedTags.Contains).Should().BeTrue();
    }
}
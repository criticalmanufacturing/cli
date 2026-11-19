using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Services;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace tests.Specs;

public class IoTDependencies
{
    private const string Registry = "https://registry.example.com/npm/";

    [Theory]
    [InlineData("11.0.0", "2.29.4", "6.0.2", "0.1.13", "0.14.4")]
    [InlineData("11.1.0", "2.29.4", "6.0.2", "0.2.2", "0.14.7")]
    [InlineData("11.3.5", "2.29.4", "6.0.2", "0.2.2", "0.14.7")]
    [InlineData("12.0.0", "2.30.1", "6.0.2", "0.2.2", "0.16.2")]
    public void KnownVersions_MatchConnectIoTRuntime(string mesVersion, string moment, string inversify, string reflectMetadata, string zone)
    {
        var deps = new DependencyVersionService().IoT(new NuGetVersion(mesVersion));

        deps.Should().Be(new IoTDeps() { Moment = moment, Inversify = inversify, ReflectMetadata = reflectMetadata, ZoneJs = zone });
    }

    [Fact]
    public void KnownVersions_UnsupportedMESVersion_ThrowsCliException()
    {
        var exception = Assert.Throws<CliException>(() => new DependencyVersionService().IoT(new NuGetVersion("10.2.7")));

        exception.Message.Should().Be("No Connect IoT dependencies defined for MES version 10.2.7");
    }

    [Fact]
    public void Resolve_UsesVersionsDeclaredByRuntimePackage()
    {
        var client = new Mock<INPMPackageInfoResolver>(MockBehavior.Strict);
        client.Setup(c => c.FetchPackageInfo(IoTDependencyResolver.EnginePackage, "release-1201"))
            .ReturnsAsync(new NpmPackageVersion()
            {
                Version = "12.0.1",
                Dependencies = new Dictionary<string, string>
                {
                    ["moment"] = "2.31.0",
                    ["inversify"] = "6.0.3",
                    ["reflect-metadata"] = "0.2.3"
                },
                PeerDependencies = new Dictionary<string, string> { ["zone.js"] = "0.16.4" }
            });
        string requestedRegistry = null;
        var resolver = new IoTDependencyResolver(new DependencyVersionService(), registry =>
        {
            requestedRegistry = registry;
            return client.Object;
        });

        var deps = resolver.Resolve(IoTDependencyResolver.EnginePackage, new NuGetVersion("12.0.1"), Registry);

        requestedRegistry.Should().Be(Registry);
        deps.Should().Be(new IoTDeps() { Moment = "2.31.0", Inversify = "6.0.3", ReflectMetadata = "0.2.3", ZoneJs = "0.16.4" });
    }

    [Fact]
    public void Resolve_UndeclaredLibrary_UsesKnownVersion()
    {
        var client = new Mock<INPMPackageInfoResolver>();
        client.Setup(c => c.FetchPackageInfo(IoTDependencyResolver.DriverPackage, "release-1135"))
            .ReturnsAsync(new NpmPackageVersion()
            {
                Version = "11.3.5",
                Dependencies = new Dictionary<string, string> { ["moment"] = "2.29.4" }
            });
        var resolver = new IoTDependencyResolver(new DependencyVersionService(), _ => client.Object);

        var deps = resolver.Resolve(IoTDependencyResolver.DriverPackage, new NuGetVersion("11.3.5"), Registry);

        deps.Should().Be(new IoTDeps() { Moment = "2.29.4", Inversify = "6.0.2", ReflectMetadata = "0.2.2", ZoneJs = "0.14.7" });
    }

    [Fact]
    public void Resolve_PackageNotFound_UsesKnownVersions()
    {
        var client = new Mock<INPMPackageInfoResolver>();
        client.Setup(c => c.FetchPackageInfo(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((NpmPackageVersion)null);
        var resolver = new IoTDependencyResolver(new DependencyVersionService(), _ => client.Object);

        var deps = resolver.Resolve(IoTDependencyResolver.EnginePackage, new NuGetVersion("12.0.0"), Registry);

        deps.Should().Be(new DependencyVersionService().IoT(new NuGetVersion("12.0.0")));
    }

    [Fact]
    public void Resolve_RegistryFailure_UsesKnownVersions()
    {
        var client = new Mock<INPMPackageInfoResolver>();
        client.Setup(c => c.FetchPackageInfo(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new TaskCanceledException("timeout"));
        var resolver = new IoTDependencyResolver(new DependencyVersionService(), _ => client.Object);

        var deps = resolver.Resolve(IoTDependencyResolver.EnginePackage, new NuGetVersion("11.1.0"), Registry);

        deps.Should().Be(new DependencyVersionService().IoT(new NuGetVersion("11.1.0")));
    }
}

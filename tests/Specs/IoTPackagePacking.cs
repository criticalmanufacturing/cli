using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Handlers;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace tests.Specs;

public class IoTPackagePacking
{
    [Theory]
    [InlineData(LogLevel.Debug, true)]
    [InlineData(LogLevel.Information, true)]
    [InlineData(LogLevel.Debug, false)]
    public void Pack_Mes10RunsBundledYeomanThroughNode(LogLevel logLevel, bool createsOutput)
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/repo/.project-config.json"] = new("""{ "MESVersion": "10.2.16" }"""),
            ["/repo/IoT Packages/angular.json"] = new("{}"),
            ["/repo/IoT Packages/cmfpackage.json"] = new("""
                {
                  "packageId": "Cmf.Custom.IoT.Packages",
                  "version": "1.0.0",
                  "packageType": "IoT",
                  "isInstallable": true,
                  "dependencies": [],
                  "steps": [],
                  "contentToPack": [{ "source": "projects/*", "target": "" }]
                }
                """),
            ["/repo/IoT Packages/projects/custom tasks/packConfig.json"] = new("{}"),
            ["/repo/IoT Packages/projects/custom tasks/index.js"] = new("module.exports = {};")
        }, "/repo/IoT Packages");
        const string runtimePackage = "/repo/packed output/runtimePackages/custom-tasks.tgz";
        var process = new Mock<IProcessStartInfoCLI>();
        process.SetupAllProperties();
        process.SetupGet(p => p.EnvironmentVariables).Returns(new StringDictionary());
        process.Setup(p => p.Start()).Callback(() =>
        {
            if (createsOutput)
            {
                fileSystem.AddFile(runtimePackage, new MockFileData("packed runtime"));
            }
        });
        using var services = new ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>()
            .AddSingleton(process.Object)
            .BuildServiceProvider();
        ExecutionContext.ServiceProvider = services;
        ExecutionContext.Initialize(fileSystem);
        var package = CmfPackage.Load(fileSystem.FileInfo.New("/repo/IoT Packages/cmfpackage.json"));
        var handler = new IoTPackageTypeHandler(package);
        var previousLogLevel = Log.Level;

        try
        {
            Log.Level = logLevel;
            Action pack = () => handler.Pack(fileSystem.DirectoryInfo.New("/repo/packed output"),
                fileSystem.DirectoryInfo.New("/repo"));
            if (createsOutput)
            {
                pack.Should().NotThrow();
                fileSystem.File.Exists(runtimePackage).Should().BeTrue();
                fileSystem.File.Exists("/repo/Cmf.Custom.IoT.Packages.1.0.0.zip").Should().BeTrue();
            }
            else
            {
                pack.Should().Throw<CliException>().WithMessage("Package Packer did not create the expected output file at *");
            }

            var yoEntryPoint = fileSystem.Path.Combine(AppContext.BaseDirectory,
                "resources", "vendors", "yo", "node_modules", "yo", "lib", "cli.js");
            process.Object.FileName.Should().Be("node");
            process.Object.Arguments.Should().Be($"\"{yoEntryPoint}\" @criticalmanufacturing/iot:packagePacker " +
                $"{(logLevel == LogLevel.Debug ? "-d" : "")} -i \"{fileSystem.Path.GetFullPath("/repo/IoT Packages/projects/custom tasks")}\" " +
                $"-o \"{fileSystem.Path.GetFullPath("/repo/packed output/runtimePackages")}\"");
            process.Object.WorkingDirectory.Should().Be(fileSystem.Path.GetFullPath("/repo/IoT Packages/projects/custom tasks"));
            process.Object.EnvironmentVariables["NO_UPDATE_NOTIFIER"].Should().Be("1");
            process.Verify(p => p.Start(), Times.Once);
        }
        finally
        {
            Log.Level = previousLogLevel;
        }
    }
}

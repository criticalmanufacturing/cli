using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Handlers;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace tests.Specs;

public class IoTPackagePacking
{
    [Theory]
    [InlineData(LogLevel.Debug, true, "/repo/IoT Packages/node_modules/@criticalmanufacturing/generator-iot", false)]
    [InlineData(LogLevel.Information, true, "/repo/IoT Packages/node_modules/@criticalmanufacturing/generator-iot", false)]
    [InlineData(LogLevel.Debug, false, "/repo/IoT Packages/node_modules/@criticalmanufacturing/generator-iot", false)]
    [InlineData(LogLevel.Debug, true, "/repo/IoT Packages/projects/custom tasks/node_modules/@criticalmanufacturing/generator-iot", false)]
    [InlineData(LogLevel.Debug, true, "/repo/node_modules/@criticalmanufacturing/generator-iot", false)]
    [InlineData(LogLevel.Debug, false, null, false)]
    [InlineData(LogLevel.Debug, false, null, true)]
    public void Pack_Mes10UsesStandalonePackerOrReportsRequiredUpgrade(LogLevel logLevel, bool createsOutput,
        string generatorDirectory, bool legacyGenerator)
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
        var standaloneEntry = generatorDirectory == null ? null : $"{generatorDirectory}/generators/packagePacker/cli.js";
        if (standaloneEntry != null)
        {
            fileSystem.AddFile(standaloneEntry, new MockFileData("// standalone entry point"));
        }
        if (legacyGenerator)
        {
            fileSystem.AddFile("/repo/IoT Packages/node_modules/@criticalmanufacturing/generator-iot/generators/packagePacker/index.js",
                new MockFileData("// legacy Yeoman generator without a standalone entry point"));
        }
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
        // Build-command constructors reinitialize the context using the real filesystem.
        // Keep MES v10 even when earlier integration tests disable the project-config cache.
        var projectConfig = new ProjectConfigV1 { MESVersion = NuGetVersion.Parse("10.2.16") };
        var projectConfigService = new Mock<IProjectConfigService>();
        projectConfigService.Setup(p => p.Load(It.IsAny<IFileSystem>())).Returns(projectConfig);
        projectConfigService.SetupGet(p => p.ProjectConfig).Returns(projectConfig);
        using var services = new ServiceCollection()
            .AddSingleton(projectConfigService.Object)
            .AddSingleton(process.Object)
            .BuildServiceProvider();
        var previousServices = ExecutionContext.ServiceProvider;
        var previousFileSystem = ExecutionContext.Instance?.FileSystem;
        var previousLogLevel = Log.Level;
        var previousCacheSetting = Environment.GetEnvironmentVariable("cmf_cli_internal_disable_projectconfig_cache");

        try
        {
            Environment.SetEnvironmentVariable("cmf_cli_internal_disable_projectconfig_cache", "1");
            ExecutionContext.ServiceProvider = services;
            ExecutionContext.Initialize(fileSystem);
            var package = CmfPackage.Load(fileSystem.FileInfo.New("/repo/IoT Packages/cmfpackage.json"));
            var handler = new IoTPackageTypeHandler(package);
            Log.Level = logLevel;
            Action pack = () => handler.Pack(fileSystem.DirectoryInfo.New("/repo/packed output"),
                fileSystem.DirectoryInfo.New("/repo"));
            if (standaloneEntry == null)
            {
                pack.Should().Throw<CliException>().WithMessage("*generator-iot*cmf-iot-pack*Update*reinstall*");
                process.Verify(p => p.Start(), Times.Never);
                fileSystem.File.Exists("/repo/Cmf.Custom.IoT.Packages.1.0.0.zip").Should().BeFalse();
                return;
            }
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

            var entryPoint = fileSystem.Path.GetFullPath(standaloneEntry);
            process.Object.FileName.Should().Be("node");
            process.Object.Arguments.Should().Be($"\"{entryPoint}\" " +
                $"{(logLevel == LogLevel.Debug ? "-d" : "")} -i \"{fileSystem.Path.GetFullPath("/repo/IoT Packages/projects/custom tasks")}\" " +
                $"-o \"{fileSystem.Path.GetFullPath("/repo/packed output/runtimePackages")}\"");
            process.Object.WorkingDirectory.Should().Be(fileSystem.Path.GetFullPath("/repo/IoT Packages/projects/custom tasks"));
            process.Verify(p => p.Start(), Times.Once);
        }
        finally
        {
            Log.Level = previousLogLevel;
            Environment.SetEnvironmentVariable("cmf_cli_internal_disable_projectconfig_cache", previousCacheSetting);
            ExecutionContext.ServiceProvider = previousServices;
            if (previousFileSystem != null)
            {
                ExecutionContext.Initialize(previousFileSystem);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Handlers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace tests.Specs;

public class GenericBuildWorkingDirectory
{
    [Theory]
    [InlineData("/repo/package", null)]
    [InlineData("/elsewhere", null)]
    [InlineData("/elsewhere", ".")]
    [InlineData("/elsewhere", "./tools")]
    [InlineData("/elsewhere", "/explicit")]
    public void Build_UsesPackageDirectoryUnlessStepOverridesIt(string invocationDirectory, string workingDirectory)
    {
        var directoryProperty = workingDirectory == null ? "" : $", \"workingDirectory\": \"{workingDirectory}\"";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/repo/package/cmfpackage.json"] = new($$"""
                {
                  "packageId": "Generic.Package",
                  "version": "1.0.0",
                  "packageType": "Generic",
                  "buildSteps": [{ "command": "tool", "args": ["build"]{{directoryProperty}} }]
                }
                """)
        }, invocationDirectory);
        var process = new Mock<IProcessStartInfoCLI>();
        process.SetupAllProperties();
        using var services = new ServiceCollection().AddSingleton(process.Object).BuildServiceProvider();
        var previousServices = ExecutionContext.ServiceProvider;
        var previousFileSystem = ExecutionContext.Instance?.FileSystem;

        try
        {
            ExecutionContext.ServiceProvider = services;
            ExecutionContext.Initialize(fileSystem);
            var package = CmfPackage.Load(fileSystem.FileInfo.New("/repo/package/cmfpackage.json"), fileSystem: fileSystem);

            new GenericPackageTypeHandler(package).Build(false);

            var expectedDirectory = workingDirectory == null
                ? fileSystem.Path.GetFullPath("/repo/package")
                : fileSystem.Path.GetFullPath(workingDirectory);
            process.Object.WorkingDirectory.Should().Be(expectedDirectory);
            process.Verify(p => p.Start(), Times.Once);
            process.Object.Arguments.Should().Be("build");
            if (workingDirectory == null)
            {
                package.BuildSteps[0].WorkingDirectory.Should().BeNull("the fallback should not change the manifest");
            }
        }
        finally
        {
            ExecutionContext.ServiceProvider = previousServices;
            if (previousFileSystem != null)
            {
                ExecutionContext.Initialize(previousFileSystem);
            }
        }
    }
}

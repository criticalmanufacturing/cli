using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Cmf.CLI.Commands;
using Cmf.CLI.Commands.New;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Services;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using Xunit;

namespace tests.Specs;

public class ConfigurationVersionCommands : IDisposable
{
    private readonly ServiceProvider previousServices = ExecutionContext.ServiceProvider;
    private readonly Mock<IDependencyVersionService> dependencies = new(MockBehavior.Strict);

    public ConfigurationVersionCommands()
    {
        ExecutionContext.ServiceProvider = new ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>()
            .AddSingleton(dependencies.Object)
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData("build")]
    [InlineData("new data")]
    [InlineData("new html")]
    public void InvalidConfiguration_RejectsCommandAtLoadTimeBeforeOperations(string name)
    {
        var fileSystem = ProjectConfigVersionValidation.CreateFileSystem(new JObject { ["MESVersion"] = "invalid-version" });
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));

        Action createCommand = () =>
        {
            switch (name)
            {
                case "build": _ = new BuildCommand(fileSystem); break;
                case "new data": _ = new DataCommand(fileSystem); break;
                case "new html": _ = new HTMLCommand(fileSystem); break;
            }
        };

        createCommand.Should().Throw<CliException>()
            .WithMessage("*.project-config.json*MESVersion*invalid-version*Expected a NuGet version*");
        dependencies.VerifyNoOtherCalls();
        fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(true, "   ")]
    public void NewTest_RequiresMesVersionBeforeOperations(bool includeField, string value)
    {
        var config = new JObject { ["TestScenariosNugetVersion"] = "11.2.0" };
        if (includeField)
        {
            config["MESVersion"] = value == null ? JValue.CreateNull() : new JValue(value);
        }
        var fileSystem = ProjectConfigVersionValidation.CreateFileSystem(config);
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));
        var handler = new Mock<TestCommand>(fileSystem) { CallBase = true };
        handler.Setup(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()));
        var command = new Command("test");
        handler.Object.Configure(command);
        var console = new TestConsole();

        var exitCode = command.Parse(Array.Empty<string>()).Invoke(console);

        exitCode.Should().NotBe(0);
        console.Error.ToString().Should().Contain("MESVersion").And.Contain(".project-config.json")
            .And.Contain("missing or empty").And.Contain("Expected a NuGet version");
        handler.Verify(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()), Times.Never);
        dependencies.VerifyNoOtherCalls();
        fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(true, "   ")]
    public void NewTest_RequiresTestScenariosVersionBeforeOperations(bool includeField, string value)
    {
        var config = new JObject { ["MESVersion"] = "11.2.0" };
        if (includeField)
        {
            config["TestScenariosNugetVersion"] = value == null ? JValue.CreateNull() : new JValue(value);
        }
        var fileSystem = ProjectConfigVersionValidation.CreateFileSystem(config);
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));
        var command = new Command("test");
        new TestCommand(fileSystem).Configure(command);
        var console = new TestConsole();

        var exitCode = command.Parse(Array.Empty<string>()).Invoke(console);

        exitCode.Should().NotBe(0);
        console.Error.ToString().Should().Contain("TestScenariosNugetVersion")
            .And.Contain(".project-config.json").And.Contain("missing or empty").And.Contain("Expected a NuGet version");
        dependencies.VerifyNoOtherCalls();
        fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1.2.3-beta..1")]
    [InlineData(null)]
    public void NewTest_InvalidPackageVersionStopsBeforeOperationsAndPreservesFiles(string value)
    {
        var fileSystem = ProjectConfigVersionValidation.CreateFileSystem(new JObject
        {
            ["MESVersion"] = "11.2.0",
            ["TestScenariosNugetVersion"] = "11.2.0"
        });
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));
        var handler = new Mock<TestCommand>(fileSystem) { CallBase = true };
        handler.Setup(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()));
        var command = new Command("test");
        handler.Object.Configure(command);
        var console = new TestConsole();
        var args = value == null ? new[] { "--version" } : new[] { "--version", value };

        var exitCode = command.Parse(args).Invoke(console);

        exitCode.Should().NotBe(0);
        console.Error.ToString().Should().Contain("--version");
        if (value != null)
        {
            console.Error.ToString().Should().Contain($"'{value}'").And.Contain("Expected a NuGet version");
        }
        handler.Verify(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()), Times.Never);
        dependencies.VerifyNoOtherCalls();
        fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData(null, "1.0.0")]
    [InlineData("1.2.3", "1.2.3")]
    [InlineData("1.2.3-beta.1+build.4", "1.2.3-beta.1+build.4")]
    [InlineData("1.2.3.4", "1.2.3.4")]
    [InlineData("1.2", "1.2")]
    [InlineData("01.2.3", "01.2.3")]
    [InlineData("1.2.3.0", "1.2.3.0")]
    [InlineData(" 1.2.3 ", " 1.2.3 ")]
    public void NewTest_ValidPackageVersionPreservesDefaultAndScaffoldingArguments(string value, string expected)
    {
        var fileSystem = ProjectConfigVersionValidation.CreateFileSystem(new JObject
        {
            ["MESVersion"] = "11.2.0",
            ["TestScenariosNugetVersion"] = "11.2.0"
        });
        var packagePath = fileSystem.Path.Combine(fileSystem.Directory.GetCurrentDirectory(), "cmfpackage.json");
        fileSystem.File.WriteAllText(packagePath,
            "{\"packageId\":\"Cmf.Custom\",\"packageType\":\"Root\",\"version\":\"1.0.0\"}");
        dependencies.Setup(x => x.DotNetTargetFramework(It.IsAny<NuGetVersion>())).Returns("net8.0");
        List<string> templateArguments = null;
        var handler = new Mock<TestCommand>(fileSystem) { CallBase = true };
        handler.Setup(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()))
            .Callback<IReadOnlyCollection<string>>(args => templateArguments = args.ToList());
        var command = new Command("test");
        handler.Object.Configure(command);
        var console = new TestConsole();

        var exitCode = command.Parse(value == null ? Array.Empty<string>() : new[] { "--version", value }).Invoke(console);

        console.Error.ToString().Should().BeEmpty();
        exitCode.Should().Be(0);
        handler.Verify(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()), Times.Once);
        templateArguments[templateArguments.IndexOf("--packageVersion") + 1].Should().Be(expected);
        var package = JObject.Parse(fileSystem.File.ReadAllText(packagePath));
        package["testPackages"].Values<JObject>().Should().HaveCount(2)
            .And.OnlyContain(dependency => dependency["version"].Value<string>() == expected);
        dependencies.Verify(x => x.DotNetTargetFramework(It.IsAny<NuGetVersion>()), Times.Once);
    }

    public void Dispose()
    {
        ExecutionContext.ServiceProvider.Dispose();
        ExecutionContext.ServiceProvider = previousServices;
    }
}

using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Commands.New;
using Cmf.CLI.Commands.New.IoT;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Cmf.Common.Cli.Commands.New;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace tests.Specs;

public class ScaffoldingProjectContext
{
    [Theory]
    [InlineData("business")]
    [InlineData("html")]
    [InlineData("help")]
    [InlineData("iot")]
    [InlineData("test")]
    [InlineData("feature")]
    [InlineData("data")]
    [InlineData("database")]
    [InlineData("reporting")]
    [InlineData("securityPortal")]
    [InlineData("iot-task")]
    [InlineData("iot-converter")]
    [InlineData("iot-taskLibrary")]
    [InlineData("iot-driver")]
    [InlineData("iot-dotnetDriver")]
    [InlineData("iot-dotnetFrameworkDriver")]
    [InlineData("iot-businessScenario")]
    public void Execute_WithoutProject_ReportsHowToInitializeBeforeDoingWork(string commandName)
    {
        var fileSystem = new MockFileSystem();
        var process = new Mock<IProcessStartInfoCLI>();
        using var services = new ServiceCollection().AddSingleton(process.Object).BuildServiceProvider();
        var previousServices = ExecutionContext.ServiceProvider;
        var previousFileSystem = ExecutionContext.Instance?.FileSystem;

        try
        {
            ExecutionContext.ServiceProvider = services;
            ExecutionContext.Initialize(fileSystem);
            var directory = fileSystem.DirectoryInfo.New(".");
            Action execute = () => Execute(commandName, fileSystem, directory);

            execute.Should().Throw<CliException>().WithMessage(
                "This command needs to run inside a project. Run `cmf init` to create a new project.");
            process.Verify(p => p.Start(), Times.Never);
            fileSystem.AllFiles.Should().BeEmpty();
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

    private static void Execute(string name, IFileSystem fileSystem, IDirectoryInfo directory)
    {
        switch (name)
        {
            case "business": new BusinessCommand(fileSystem).Execute(directory, "1.0.0", false); break;
            case "html": new HTMLCommand(fileSystem).Execute(directory, "1.0.0"); break;
            case "help": new HelpCommand(fileSystem).Execute(directory, "1.0.0"); break;
            case "iot": new IoTCommand(fileSystem).Execute(directory, "1.0.0", null, false); break;
            case "test": new TestCommand(fileSystem).Execute("1.0.0"); break;
            case "feature": new FeatureCommand(fileSystem).Execute(directory, "Feature", "1.0.0"); break;
            case "data": new DataCommand(fileSystem).Execute(directory, "1.0.0", null); break;
            case "database": new DatabaseCommand(fileSystem).Execute(directory, "1.0.0"); break;
            case "reporting": new ReportingCommand(fileSystem).Execute(directory, "1.0.0"); break;
            case "securityPortal": new SecurityPortalCommand(fileSystem).Execute(directory, "1.0.0"); break;
            case "iot-task": new GenerateTaskCommand(fileSystem).Execute(directory); break;
            case "iot-converter": new GenerateConverterCommand(fileSystem).Execute(directory); break;
            case "iot-taskLibrary": new GenerateTaskLibraryCommand(fileSystem).Execute(directory); break;
            case "iot-driver": new GenerateDriverCommand(fileSystem).Execute(directory); break;
            case "iot-dotnetDriver": new GenerateDotnetDriverCommand(fileSystem).Execute(directory); break;
            case "iot-dotnetFrameworkDriver": new GenerateDotnetFrameworkDriverCommand(fileSystem).Execute(directory); break;
            case "iot-businessScenario": new GenerateBusinessScenarioCommand(fileSystem).Execute(directory); break;
            default: throw new ArgumentOutOfRangeException(nameof(name));
        }
    }
}

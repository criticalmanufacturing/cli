using Cmf.CLI.Builders;
using Cmf.CLI.Core.Objects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions.TestingHelpers;
using tests.Mocks;
using Xunit;

namespace tests.Specs;

public class TestBuildCommand
{
    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void ProcessCommand_WithCondition(bool? condition)
    {
        var mockProcessStartInfoCLI = new Mock<IProcessStartInfoCLI>();

        mockProcessStartInfoCLI.Setup(process => process.ExitCode).Returns(0);
        mockProcessStartInfoCLI.Setup(process => process.BeginOutputReadLine());
        mockProcessStartInfoCLI.Setup(process => process.BeginErrorReadLine());
        mockProcessStartInfoCLI.Setup(process => process.WaitForExit());
        mockProcessStartInfoCLI.Setup(process => process.Dispose());
        mockProcessStartInfoCLI.Setup(ps => ps.Start()).Returns(new Process());

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton(mockProcessStartInfoCLI.Object)
            .BuildServiceProvider();

        var command = new MockProcessCommand();

        if (condition != null)
        {
            command.ConditionForExecute = () => condition ?? true;
        }

        command.Steps = new List<ProcessBuildStep>()
        {
            {
                new ProcessBuildStep() {
                    Command = "TestCommand",
                    Args = new string[]{"string1", "string2"},
                    WorkingDirectory = new MockFileSystem().DirectoryInfo.New("Test")
                }
            }
        }.ToArray();

        var _ = (condition ?? true) ? command.Condition().Should().BeTrue() : command.Condition().Should().BeFalse();

        command.Exec();

        Times times = (condition ?? true) ? Times.Once() : Times.Never();

        mockProcessStartInfoCLI.Verify(ps => ps.Start(), times);
        mockProcessStartInfoCLI.Verify(process => process.BeginOutputReadLine(), times);
        mockProcessStartInfoCLI.Verify(process => process.BeginErrorReadLine(), times);
        mockProcessStartInfoCLI.Verify(process => process.BeginOutputReadLine(), times);
        mockProcessStartInfoCLI.Verify(process => process.WaitForExit(), times);
        mockProcessStartInfoCLI.Verify(process => process.ExitCode, times);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void ExecuteCommand_WithCondition(bool? condition)
    {
        bool commandWasCalled = false;
        var command = new ExecuteCommand<MockBaseCommand>()
        {
            Command = new MockBaseCommand(),
            DisplayName = "cmf iot lib command",
            Execute = command =>
            {
                commandWasCalled = true;
            }
        };

        if (condition != null)
        {
            command.ConditionForExecute = () => condition ?? true;
        }
        command.Exec();

        var _ = (condition ?? true) ? commandWasCalled.Should().BeTrue() : commandWasCalled.Should().BeFalse();
    }
}
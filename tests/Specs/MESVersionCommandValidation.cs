using System;
using System.CommandLine;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Objects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace tests.Specs;

public class MESVersionCommandValidation
{
    [CmfCommand("test-command", Id = "test_command", MinimumMESVersion = "11.0.0")]
    private class TestCommand : BaseCommand
    {
        public bool Executed { get; private set; }

        public override void Configure(Command cmd)
        {
            cmd.SetAction(_ => { Executed = true; });
        }
    }

    [Fact]
    public void CommandWithMinimumVersion_ValidationPasses_ShouldNotAddErrors()
    {
        // Arrange
        var validationServiceMock = new Mock<IMESVersionValidationService>(MockBehavior.Strict);
        validationServiceMock
            .Setup(v => v.ValidateMinimumVersion("11.0.0"));

        var rootCmd = BuildRootCommand(validationServiceMock.Object);

        // Act
        var result = rootCmd.Parse("test-command");

        // Assert
        result.Errors.Should().BeEmpty();
        validationServiceMock.Verify(v => v.ValidateMinimumVersion("11.0.0"), Times.Once);
    }

    [Fact]
    public void CommandWithMinimumVersion_ValidationFails_ShouldReturnParseError()
    {
        // Arrange
        var validationServiceMock = new Mock<IMESVersionValidationService>(MockBehavior.Strict);
        validationServiceMock
            .Setup(v => v.ValidateMinimumVersion("11.0.0"))
            .Throws(new MESVersionValidationException("blocked by minimum MES version"));

        var rootCmd = BuildRootCommand(validationServiceMock.Object);

        // Act
        var result = rootCmd.Parse("test-command");

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Be("blocked by minimum MES version");
        validationServiceMock.Verify(v => v.ValidateMinimumVersion("11.0.0"), Times.Once);
    }

    private static RootCommand BuildRootCommand(IMESVersionValidationService validationService)
    {
        ExecutionContext.ServiceProvider = new ServiceCollection()
            .AddSingleton(validationService)
            .BuildServiceProvider();

        var rootCmd = new RootCommand();
        var testCmd = new Command("test-command");
        var testCmdHandler = new TestCommand();
        testCmdHandler.Configure(testCmd);

        // Simulate what BaseCommand.FindChildCommands does with version validation.
        var attr = typeof(TestCommand).GetCustomAttributes(typeof(CmfCommandAttribute), false)[0] as CmfCommandAttribute;
        if (!string.IsNullOrWhiteSpace(attr.MinimumMESVersion))
        {
            var resolvedValidationService = ExecutionContext.ServiceProvider?.GetService<IMESVersionValidationService>();
            testCmd.Validators.Add(commandResult =>
            {
                try
                {
                    resolvedValidationService?.ValidateMinimumVersion(attr.MinimumMESVersion);
                }
                catch (MESVersionValidationException ex)
                {
                    commandResult.AddError(ex.Message);
                }
                catch (Exception ex)
                {
                    commandResult.AddError($"Version validation error: {ex.Message}");
                }
            });
        }

        rootCmd.Add(testCmd);
        return rootCmd;
    }
}

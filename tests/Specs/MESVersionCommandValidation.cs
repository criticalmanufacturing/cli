using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Objects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests.Specs;

public class MESVersionCommandValidation
{
    const string projCfgTemplate = @"{
          ""ProjectName"": ""ExampleProject"",
          ""NPMRegistry"": ""http://npmrepo/"",
          ""NuGetRegistry"": ""https://nuget-repo/"",
          ""RepositoryURL"": ""https://example.com/repo"",
          ""Tenant"": ""ExampleClient"",
          ""MESVersion"": ""{MES_VERSION}"",
          ""DevTasksVersion"": ""1.0.0"",
          ""HTMLStarterVersion"": ""8.0.0"",
          ""DefaultDomain"": ""AD"",
          ""RESTPort"": ""443""
        }";

    [CmfCommand("test-command", Id = "test_command", MinimumMESVersion = "11.0.0")]
    private class TestCommand : BaseCommand
    {
        public bool Executed { get; private set; }

        public override void Configure(Command cmd)
        {
            cmd.SetHandler(() => { Executed = true; });
        }
    }

    [Fact]
    public void CommandWithMinimumVersion_CurrentVersionMeets_ShouldExecute()
    {
        // Arrange
        SetupExecutionContext("11.0.0");
        var rootCmd = new RootCommand();
        var testCmd = new Command("test-command");
        var testCmdHandler = new TestCommand();
        testCmdHandler.Configure(testCmd);
        
        // Simulate what BaseCommand.FindChildCommands does with version validation
        var attr = typeof(TestCommand).GetCustomAttributes(typeof(CmfCommandAttribute), false)[0] as CmfCommandAttribute;
        if (!string.IsNullOrWhiteSpace(attr.MinimumMESVersion))
        {
            testCmd.AddValidator(commandResult =>
            {
                try
                {
                    var validationService = ExecutionContext.ServiceProvider?.GetService<IMESVersionValidationService>();
                    validationService?.ValidateMinimumVersion(attr.MinimumMESVersion);
                }
                catch (MESVersionValidationException ex)
                {
                    commandResult.ErrorMessage = ex.Message;
                }
                catch (Exception ex)
                {
                    commandResult.ErrorMessage = $"Version validation error: {ex.Message}";
                }
            });
        }
        
        rootCmd.AddCommand(testCmd);
        var parser = new Parser(rootCmd);

        // Act
        var result = parser.Parse("test-command");

        // Assert
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CommandWithMinimumVersion_CurrentVersionBelowRequired_ShouldFail()
    {
        // Arrange
        SetupExecutionContext("10.0.0");
        var rootCmd = new RootCommand();
        var testCmd = new Command("test-command");
        var testCmdHandler = new TestCommand();
        testCmdHandler.Configure(testCmd);
        
        // Simulate what BaseCommand.FindChildCommands does with version validation
        var attr = typeof(TestCommand).GetCustomAttributes(typeof(CmfCommandAttribute), false)[0] as CmfCommandAttribute;
        if (!string.IsNullOrWhiteSpace(attr.MinimumMESVersion))
        {
            testCmd.AddValidator(commandResult =>
            {
                try
                {
                    var validationService = ExecutionContext.ServiceProvider?.GetService<IMESVersionValidationService>();
                    validationService?.ValidateMinimumVersion(attr.MinimumMESVersion);
                }
                catch (MESVersionValidationException ex)
                {
                    commandResult.ErrorMessage = ex.Message;
                }
                catch (Exception ex)
                {
                    commandResult.ErrorMessage = $"Version validation error: {ex.Message}";
                }
            });
        }
        
        rootCmd.AddCommand(testCmd);
        var parser = new Parser(rootCmd);

        // Act
        var result = parser.Parse("test-command");

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Contain("This command requires MES version 11.0.0 or higher");
    }

    private void SetupExecutionContext(string mesVersion)
    {
        var projConfig = projCfgTemplate.Replace("{MES_VERSION}", mesVersion);
        
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/.project-config.json", new MockFileData(projConfig)}
        });

        // Set up the service provider with required services
        var serviceCollection = new ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>()
            .AddSingleton<IMESVersionValidationService, MESVersionValidationService>();
        ExecutionContext.ServiceProvider = serviceCollection.BuildServiceProvider();
        
        // Initialize ExecutionContext which will load the project config
        ExecutionContext.Initialize(fileSystem);
    }
}

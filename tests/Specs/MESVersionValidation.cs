using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Objects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests.Specs;

public class MESVersionValidation
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

    [Fact]
    public void ValidateMinimumVersion_NoMinimumSpecified_ShouldNotThrow()
    {
        // Arrange
        SetupExecutionContext("10.0.0");
        var service = new MESVersionValidationService();

        // Act & Assert
        service.Invoking(s => s.ValidateMinimumVersion(null))
            .Should().NotThrow();
        service.Invoking(s => s.ValidateMinimumVersion(""))
            .Should().NotThrow();
        service.Invoking(s => s.ValidateMinimumVersion("   "))
            .Should().NotThrow();
    }

    [Theory]
    [InlineData("11.0.0", "11.0.0")] // Exact match
    [InlineData("11.0.0", "10.0.0")] // Current version higher
    [InlineData("11.1.5", "11.0.0")] // Minor version higher
    [InlineData("11.1.5", "11.1.0")] // Build version higher
    public void ValidateMinimumVersion_VersionMeetsRequirement_ShouldNotThrow(string currentVersion, string minimumVersion)
    {
        // Arrange
        SetupExecutionContext(currentVersion);
        var service = new MESVersionValidationService();

        // Act & Assert
        service.Invoking(s => s.ValidateMinimumVersion(minimumVersion))
            .Should().NotThrow();
    }

    [Theory]
    [InlineData("10.0.0", "11.0.0")] // Major version lower
    [InlineData("11.0.0", "11.1.0")] // Minor version lower
    [InlineData("11.1.0", "11.1.5")] // Build version lower
    public void ValidateMinimumVersion_VersionBelowRequirement_ShouldThrow(string currentVersion, string minimumVersion)
    {
        // Arrange
        SetupExecutionContext(currentVersion);
        var service = new MESVersionValidationService();

        // Act & Assert
        service.Invoking(s => s.ValidateMinimumVersion(minimumVersion))
            .Should().Throw<MESVersionValidationException>()
            .WithMessage($"This command requires MES version {minimumVersion} or higher. Current version: {currentVersion}");
    }

    [Fact]
    public void ValidateMinimumVersion_InvalidVersionFormat_ShouldThrow()
    {
        // Arrange
        SetupExecutionContext("11.0.0");
        var service = new MESVersionValidationService();

        // Act & Assert
        service.Invoking(s => s.ValidateMinimumVersion("invalid-version"))
            .Should().Throw<ArgumentException>()
            .WithMessage("Invalid minimum version format: invalid-version.*");
    }

    [Fact]
    public void ValidateMinimumVersion_NoExecutionContext_ShouldThrow()
    {
        // Arrange
        var fileSystem = new MockFileSystem(); // Initialize without project config
        var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>();
        ExecutionContext.ServiceProvider = serviceCollection.BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);
        var service = new MESVersionValidationService();

        // Act & Assert
        service.Invoking(s => s.ValidateMinimumVersion("11.0.0"))
            .Should().Throw<MESVersionValidationException>()
            .WithMessage("MES version information is not available.*");
    }

    [Theory]
    [InlineData("11.0.0", "11.0.0", true)] // Exact match
    [InlineData("11.0.0", "10.0.0", true)] // Current version higher
    [InlineData("10.0.0", "11.0.0", false)] // Current version lower
    [InlineData("11.1.0", "11.0.0", true)] // Minor version higher
    public void IsVersionCompatible_ShouldReturnCorrectResult(string currentVersion, string minimumVersion, bool expectedResult)
    {
        // Arrange
        SetupExecutionContext(currentVersion);
        var service = new MESVersionValidationService();

        // Act
        var result = service.IsVersionCompatible(minimumVersion);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void IsVersionCompatible_NoMinimumSpecified_ShouldReturnTrue()
    {
        // Arrange
        SetupExecutionContext("10.0.0");
        var service = new MESVersionValidationService();

        // Act & Assert
        service.IsVersionCompatible(null).Should().BeTrue();
        service.IsVersionCompatible("").Should().BeTrue();
        service.IsVersionCompatible("   ").Should().BeTrue();
    }

    [Fact]
    public void IsVersionCompatible_InvalidVersionFormat_ShouldReturnFalse()
    {
        // Arrange
        SetupExecutionContext("11.0.0");
        var service = new MESVersionValidationService();

        // Act
        var result = service.IsVersionCompatible("invalid-version");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsVersionCompatible_NoExecutionContext_ShouldReturnFalse()
    {
        // Arrange
        var fileSystem = new MockFileSystem(); // Initialize without project config
        var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>();
        ExecutionContext.ServiceProvider = serviceCollection.BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);
        var service = new MESVersionValidationService();

        // Act
        var result = service.IsVersionCompatible("11.0.0");

        // Assert
        result.Should().BeFalse();
    }

    private void SetupExecutionContext(string mesVersion)
    {
        var projConfig = projCfgTemplate.Replace("{MES_VERSION}", mesVersion);
        
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/.project-config.json", new MockFileData(projConfig)}
        });

        // First set up the service provider with ProjectConfigService
        var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>();
        ExecutionContext.ServiceProvider = serviceCollection.BuildServiceProvider();
        
        // Then initialize ExecutionContext which will load the project config
        ExecutionContext.Initialize(fileSystem);
    }
}

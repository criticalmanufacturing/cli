using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Cmf.CLI.Commands;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Services;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NuGet.Versioning;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace tests.Specs;

public class InitVersionValidation : IDisposable
{
    private readonly ServiceProvider previousServices = ExecutionContext.ServiceProvider;
    private readonly MockFileSystem fileSystem = new();
    private readonly Mock<INPMDistTagResolver> resolver = new(MockBehavior.Strict);
    private readonly Mock<IDependencyVersionService> dependencies = new(MockBehavior.Strict);
    private readonly Mock<InitCommand> handler;
    private readonly Command command = new("init");
    private List<string> templateArguments;

    public InitVersionValidation()
    {
        ExecutionContext.ServiceProvider = new ServiceCollection()
            .AddSingleton<IVersionService>(new VersionService("CriticalManufacturing.CLI", "6.0.1"))
            .AddSingleton(dependencies.Object)
            .BuildServiceProvider();
        fileSystem.AddFile("existing.txt", new MockFileData("Keep this file unchanged."));
        fileSystem.AddFile(".project-config.json", new MockFileData("{\"MESVersion\":\"11.0.0\"}"));
        fileSystem.AddFile("cmfpackage.json", new MockFileData("{\"version\":\"1.0.0\"}"));
        handler = new Mock<InitCommand>(fileSystem, resolver.Object) { CallBase = true };
        handler.Setup(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()))
            .Callback<IReadOnlyCollection<string>>(args => templateArguments = args.ToList());
        handler.Object.Configure(command);
    }

    public static IEnumerable<object[]> InvalidInputs()
    {
        foreach (var option in new[]
        {
            "--baseVersion", "--MESVersion", "--version", "--nugetVersion",
            "--testScenariosNugetVersion", "--DevTasksVersion", "--HTMLStarterVersion", "--yoGeneratorVersion"
        })
        {
            foreach (var value in new[] { "not-a-version", "", "   ", "1.2.3-beta..1" })
            {
                if (value == "" && (option == "--DevTasksVersion" || option == "--HTMLStarterVersion" || option == "--yoGeneratorVersion"))
                {
                    continue;
                }
                yield return new object[] { option, value };
            }
        }

        foreach (var option in new[] { "--DevTasksVersion", "--HTMLStarterVersion", "--yoGeneratorVersion" })
        {
            yield return new object[] { option, "01.2.3" };
            yield return new object[] { option, " 1.2.3 " };
        }

        foreach (var option in new[] { "--baseVersion", "--MESVersion" })
        {
            yield return new object[] { option, "9.9.9" };
            yield return new object[] { option, "9.9.9-rc.1" };
        }
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void InvalidVersion_StopsBeforeResolversOrScaffoldingAndPreservesFiles(string option, string value)
    {
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));
        var options = new List<string>();
        if (option != "--baseVersion" && option != "--MESVersion")
        {
            options.AddRange(new[] { "--baseVersion", "12.0.0-beta.1" });
        }
        options.AddRange(new[] { option, value });
        var console = new TestConsole();

        var result = command.Parse(Arguments(options.ToArray()));
        result.Errors.Should().NotBeEmpty();
        var exitCode = result.Invoke(console);

        exitCode.Should().NotBe(0);
        if ((option == "--baseVersion" || option == "--MESVersion") && NuGetVersion.TryParse(value, out var parsed) && parsed.Major < 10)
        {
            console.Error.ToString().Should().Contain("MES Versions under 10 are no longer supported with the newest version of the CLI. Please use cmf-cli 5.8.0 or lower.");
        }
        else
        {
            console.Error.ToString().Should().Contain(option).And.Contain($"'{value}'").And.Contain("Expected ");
        }
        AssertNoOperationsOrFileChanges(before);
    }

    [Fact]
    public void MissingMesVersion_StopsBeforeOperations()
    {
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));
        var console = new TestConsole();

        var exitCode = command.Parse(Arguments()).Invoke(console);

        exitCode.Should().NotBe(0);
        console.Error.ToString().Should().Contain("--baseVersion").And.Contain("required");
        AssertNoOperationsOrFileChanges(before);
    }

    [Theory]
    [InlineData("--version")]
    [InlineData("--nugetVersion")]
    [InlineData("--testScenariosNugetVersion")]
    [InlineData("--DevTasksVersion")]
    [InlineData("--HTMLStarterVersion")]
    [InlineData("--yoGeneratorVersion")]
    public void OptionWithoutValue_StopsBeforeOperations(string option)
    {
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));
        var console = new TestConsole();

        var exitCode = command.Parse(Arguments("--baseVersion", "12.0.0-beta.1", option)).Invoke(console);

        exitCode.Should().NotBe(0);
        console.Error.ToString().Should().Contain(option);
        AssertNoOperationsOrFileChanges(before);
    }

    [Theory]
    [InlineData("--baseVersion", "10.0.0")]
    [InlineData("--MESVersion", "10.0.0-alpha.1")]
    [InlineData("--MESVersion", "11.1.5")]
    [InlineData("--baseVersion", "12.0.0-beta.2+build.4")]
    [InlineData("--baseVersion", "12.0.0.0")]
    [InlineData("--baseVersion", " 12.0.0 ")]
    public void ValidMesVersion_PreservesDefaultsAndPassesTypedVersionToDependencies(string option, string input)
    {
        var version = NuGetVersion.Parse(input);
        dependencies.Setup(x => x.DotNetSdk(It.Is<NuGetVersion>(v => v.ToFullString() == version.ToFullString())))
            .Returns("10.0.301");
        var expectedSchematics = GenericUtilities.GetNpmDistTag(version);
        if (version.IsPrerelease)
        {
            var distTag = expectedSchematics;
            resolver.Setup(x => x.ResolveDistTag("@criticalmanufacturing/ngx-schematics", distTag))
                .ReturnsAsync("12.0.0-beta.99");
            expectedSchematics = "12.0.0-beta.99";
        }
        var console = new TestConsole();

        var exitCode = command.Parse(Arguments(option, input)).Invoke(console);

        exitCode.Should().Be(0, console.Error.ToString());
        console.Error.ToString().Should().BeEmpty();
        TemplateValue("--MESVersion").Should().Be(input);
        TemplateValue("--nugetVersion").Should().Be(input);
        TemplateValue("--testScenariosNugetVersion").Should().Be(input);
        TemplateValue("--packageVersion").Should().Be("1.0.0");
        TemplateValue("--DevTasksVersion").Should().BeEmpty();
        TemplateValue("--HTMLStarterVersion").Should().BeEmpty();
        TemplateValue("--yoGeneratorVersion").Should().BeEmpty();
        TemplateValue("--ngxSchematicsVersion").Should().Be(expectedSchematics);
        TemplateValue("--projectConfigV2").Should().Be((version.Major >= 12).ToString());
        handler.Verify(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()), Times.Once);
        dependencies.VerifyAll();
        resolver.VerifyAll();
    }

    [Theory]
    [InlineData("--DevTasksVersion")]
    [InlineData("--HTMLStarterVersion")]
    [InlineData("--yoGeneratorVersion")]
    public void EmptyLegacyOverride_PreservesOmittedOverrideBehavior(string option)
    {
        dependencies.Setup(x => x.DotNetSdk(It.IsAny<NuGetVersion>())).Returns("10.0.301");
        var console = new TestConsole();

        var exitCode = command.Parse(Arguments("--baseVersion", "12.0.0", option, "")).Invoke(console);

        exitCode.Should().Be(0, console.Error.ToString());
        console.Error.ToString().Should().BeEmpty();
        TemplateValue("--DevTasksVersion").Should().BeEmpty();
        TemplateValue("--HTMLStarterVersion").Should().BeEmpty();
        TemplateValue("--yoGeneratorVersion").Should().BeEmpty();
        handler.Verify(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()), Times.Once);
        dependencies.VerifyAll();
        resolver.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("1.2.3", "1.2.3.4", "1.2.3")]
    [InlineData("1.2.3-beta.1+build.4", "1.2.3-rc.1+build.4", "1.2.3-alpha.1+build.4")]
    [InlineData("1.2", "01.2.3", "1.2.3+build.4")]
    [InlineData(" 1.2.3 ", "1.2.3.0", "1.2.3-alpha.1")]
    public void ValidOverrides_PreserveVersionsBelowTenAndExplicitNpmDistTags(string packageVersion, string nugetVersion, string legacyVersion)
    {
        dependencies.Setup(x => x.DotNetSdk(It.IsAny<NuGetVersion>())).Returns("10.0.301");
        var console = new TestConsole();

        var exitCode = command.Parse(Arguments(
            "--baseVersion", "12.0.0-beta.1",
            "--version", packageVersion,
            "--nugetVersion", nugetVersion,
            "--testScenariosNugetVersion", nugetVersion,
            "--DevTasksVersion", legacyVersion,
            "--HTMLStarterVersion", legacyVersion,
            "--yoGeneratorVersion", legacyVersion,
            "--ngxSchematicsVersion", "my-custom-schematics-tag")).Invoke(console);

        exitCode.Should().Be(0, console.Error.ToString());
        console.Error.ToString().Should().BeEmpty();
        TemplateValue("--packageVersion").Should().Be(packageVersion);
        TemplateValue("--nugetVersion").Should().Be(nugetVersion);
        TemplateValue("--testScenariosNugetVersion").Should().Be(nugetVersion);
        TemplateValue("--DevTasksVersion").Should().Be(legacyVersion);
        TemplateValue("--HTMLStarterVersion").Should().Be(legacyVersion);
        TemplateValue("--yoGeneratorVersion").Should().Be(legacyVersion);
        TemplateValue("--ngxSchematicsVersion").Should().Be("my-custom-schematics-tag");
        resolver.VerifyNoOtherCalls();
    }

    private static string[] Arguments(params string[] options) => new[]
    {
        "project", "--tenant", "test",
        "--ciRepo", "https://ci.example", "--releaseRepos", "https://release.example"
    }.Concat(options).ToArray();

    private string TemplateValue(string name)
    {
        templateArguments.Should().NotBeNull();
        templateArguments.Should().Contain(name);
        return templateArguments[templateArguments.IndexOf(name) + 1];
    }

    private void AssertNoOperationsOrFileChanges(Dictionary<string, string> before)
    {
        handler.Verify(x => x.RunCommand(It.IsAny<IReadOnlyCollection<string>>()), Times.Never);
        resolver.VerifyNoOtherCalls();
        dependencies.VerifyNoOtherCalls();
        fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    public void Dispose()
    {
        ExecutionContext.ServiceProvider.Dispose();
        ExecutionContext.ServiceProvider = previousServices;
    }
}

using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Cmf.CLI.Commands;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace tests.Specs;

public class UpgradeAndBumpVersionValidation : IDisposable
{
    private readonly ServiceProvider previousServices = ExecutionContext.ServiceProvider;
    private readonly MockFileSystem fileSystem = new();
    private readonly string root;

    public UpgradeAndBumpVersionValidation()
    {
        ExecutionContext.ServiceProvider = new ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>()
            .BuildServiceProvider();
        fileSystem.Directory.CreateDirectory("/project");
        fileSystem.Directory.SetCurrentDirectory("/project");
        root = fileSystem.Directory.GetCurrentDirectory();
        fileSystem.AddFile(Path(".project-config.json"), new MockFileData("""
            {
              "MESVersion": "11.2.0",
              "NugetVersion": "11.2.0",
              "TestScenariosNugetVersion": "11.2.0"
            }
            """));
        fileSystem.AddFile(Path("cmfpackage.json"), new MockFileData("""
            {
              "packageId": "Cmf.Custom.Package",
              "packageType": "Generic",
              "version": "1.0.0",
              "dependencies": [{ "id": "Cmf.Environment", "version": "11.2.0" }]
            }
            """));
    }

    public static IEnumerable<object[]> InvalidBumpInputs()
    {
        foreach (var name in new[] { "bump", "customization", "configuration" })
        {
            foreach (var value in new[] { "not-a-version", "   ", "1.0.0-beta..1" })
            {
                yield return new object[] { name, "--version", value };
            }
            var suffixes = name == "bump"
                ? new[] { "--buildNr", "--preRelease" }
                : new[] { "--buildNrVersion", "--preReleaseVersion" };
            foreach (var suffix in suffixes)
            {
                foreach (var value in new[] { "bad label", "beta..1", "   " })
                {
                    yield return new object[] { name, suffix, value };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(InvalidBumpInputs))]
    public void InvalidBumpArguments_AreRejectedDuringParsing(string name, string option, string value)
    {
        var before = Snapshot();
        var args = new List<string> { root };
        if (option != "--version") args.AddRange(new[] { "--version", "1.1.0" });
        args.AddRange(new[] { option, value });

        var result = CreateCommand(name).Parse(args.ToArray());

        result.Errors.Should().NotBeEmpty();
        Snapshot().Should().BeEquivalentTo(before);
    }

    [Theory]
    [MemberData(nameof(InvalidBumpInputs))]
    public void InvalidBumpArguments_LeaveAllFilesUnchanged(string name, string option, string value)
    {
        var before = Snapshot();
        var options = new List<string> { root };
        if (option != "--version")
        {
            options.AddRange(new[] { "--version", "1.1.0" });
        }
        options.AddRange(new[] { option, value });

        var console = Invoke(name, options.ToArray());

        console.Error.ToString().Should().Contain(option).And.Contain($"'{value}'").And.Contain("Expected ");
        Snapshot().Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData("bump")]
    [InlineData("customization")]
    public void RequiredBumpVersion_RejectsMissingValue(string name)
    {
        var before = Snapshot();

        var console = Invoke(name, root);

        console.Error.ToString().Should().Contain("version");
        Snapshot().Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData("bump")]
    [InlineData("customization")]
    [InlineData("configuration")]
    public void MutuallyExclusiveSuffixes_RejectBeforeWrites(string name)
    {
        var before = Snapshot();
        var build = name == "bump" ? "--buildNr" : "--buildNrVersion";
        var prerelease = name == "bump" ? "--preRelease" : "--preReleaseVersion";

        var console = Invoke(name, root, "--version", "1.1.0", build, "123", prerelease, "beta.1");

        console.Error.ToString().Should().Contain("Mutually exclusive properties")
            .And.Contain(build.TrimStart('-')).And.Contain(prerelease.TrimStart('-'));
        Snapshot().Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("9.9.9")]
    [InlineData("9.9.9-beta.1")]
    public void Upgrade_RejectsInvalidBaseBeforeWrites(string value)
    {
        var before = Snapshot();
        var console = value == null ? Invoke("upgrade", root) : Invoke("upgrade", root, value);

        if (NuGet.Versioning.NuGetVersion.TryParse(value, out var parsed) && parsed.Major < 10)
        {
            console.Error.ToString().Should().Contain("MES Versions under 10 are no longer supported with the newest version of the CLI. Please use cmf-cli 5.8.0 or lower.");
        }
        else if (value == null)
        {
            console.Error.ToString().Should().Contain("Required argument missing");
        }
        else
        {
            console.Error.ToString().Should().Contain("BaseVersion").And.Contain("Expected ");
        }
        Snapshot().Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("9.9.9")]
    public void Upgrade_RejectsInvalidIoTVersionBeforeWrites(string value)
    {
        var before = Snapshot();

        var console = Invoke("upgrade", root, "12.0.0-beta.1", "--iotVersion", value);

        if (NuGet.Versioning.NuGetVersion.TryParse(value, out var parsed) && parsed.Major < 10)
        {
            console.Error.ToString().Should().Contain("MES Versions under 10 are no longer supported with the newest version of the CLI. Please use cmf-cli 5.8.0 or lower.");
        }
        else
        {
            console.Error.ToString().Should().Contain("--iotVersion").And.Contain(value).And.Contain("Expected ");
        }
        Snapshot().Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData("10.0.0-alpha.1")]
    [InlineData("12.0.0-beta.1")]
    [InlineData("12.0.0-beta.1+build.4")]
    [InlineData("12.0.0.0")]
    [InlineData(" 12.0.0 ")]
    public void Upgrade_ValidReleaseOrPrereleaseUpdatesPackageAndConfiguration(string value)
    {
        var console = Invoke("upgrade", root, value);

        console.Error.ToString().Should().BeEmpty();
        JObject.Parse(fileSystem.File.ReadAllText(Path(".project-config.json")))["MESVersion"].Value<string>().Should().Be(value);
        JObject.Parse(fileSystem.File.ReadAllText(Path("cmfpackage.json")))["dependencies"][0]["version"].Value<string>().Should().Be(value);
    }

    [Fact]
    public void Upgrade_PreflightLoadsEachDistinctConfigurationOncePerInvocation()
    {
        var originalPackage = fileSystem.File.ReadAllText(Path("cmfpackage.json"));
        fileSystem.AddFile(Path("child", "cmfpackage.json"), new MockFileData(originalPackage));
        fileSystem.AddFile(Path("nested", ".project-config.json"), new MockFileData("{\"MESVersion\":\"12.0.0\"}"));
        fileSystem.AddFile(Path("nested", "cmfpackage.json"), new MockFileData(originalPackage));
        fileSystem.AddFile(Path("nested", "child", "cmfpackage.json"), new MockFileData(originalPackage));
        var loaded = new List<string>();
        var before = Snapshot();
        var command = new UpgradeBaseCommand(fileSystem, config =>
        {
            Snapshot().Should().BeEquivalentTo(before, "all configuration loads must finish before writes");
            loaded.Add(config.FullName);
            return new ProjectConfigService().Load(config);
        });

        command.Execute(fileSystem.DirectoryInfo.New(root), NuGetVersion.Parse("12.0.0"), null, []);

        loaded.Should().BeEquivalentTo(new[] { Path(".project-config.json"), Path("nested", ".project-config.json") });
        before = Snapshot();
        command.Execute(fileSystem.DirectoryInfo.New(root), NuGetVersion.Parse("12.0.1"), null, []);
        loaded.Count(path => path == Path(".project-config.json")).Should().Be(2);
        loaded.Count(path => path == Path("nested", ".project-config.json")).Should().Be(2);
    }

    [Fact]
    public void Bump_ReusingCommandKeepsValidatedInputsLocalToEachParse()
    {
        var command = CreateCommand("bump");
        var first = command.Parse(new[] { root, "--version", "1.2.3" });
        var second = command.Parse(new[] { root, "--version", "2.3.4" });

        first.Invoke(new TestConsole()).Should().Be(0);
        JObject.Parse(fileSystem.File.ReadAllText(Path("cmfpackage.json")))["version"].Value<string>().Should().Be("1.2.3");
        second.Invoke(new TestConsole()).Should().Be(0);
        JObject.Parse(fileSystem.File.ReadAllText(Path("cmfpackage.json")))["version"].Value<string>().Should().Be("2.3.4");
    }

    [Theory]
    [InlineData("bump")]
    [InlineData("customization")]
    [InlineData("configuration")]
    public void Bump_InvalidVersionStopsBeforeExecution(string name)
    {
        var command = CreateCommand(name);
        var executed = false;
        command.SetAction(_ => { executed = true; });
        var before = Snapshot();
        var console = new TestConsole();

        var exitCode = command.Parse(new[] { root, "--version", "invalid" }).Invoke(console);

        exitCode.Should().NotBe(0);
        executed.Should().BeFalse();
        console.Error.ToString().Should().Contain("--version").And.Contain("Expected ");
        Snapshot().Should().BeEquivalentTo(before);
    }

    [Fact]
    public void Upgrade_ValidatesTargetConfigurationOutsideCurrentProjectBeforeWrites()
    {
        var target = Path("other-project");
        fileSystem.AddFile(fileSystem.Path.Combine(target, ".project-config.json"), new MockFileData("{\"MESVersion\":\"invalid\"}"));
        fileSystem.AddFile(fileSystem.Path.Combine(target, "cmfpackage.json"), new MockFileData(fileSystem.File.ReadAllText(Path("cmfpackage.json"))));
        var command = new UpgradeBaseCommand(fileSystem);
        var before = Snapshot();

        Action execute = () => command.Execute(fileSystem.DirectoryInfo.New(target), NuGetVersion.Parse("12.0.0"), null, []);

        execute.Should().Throw<CliException>().WithMessage("*other-project*.project-config.json*MESVersion*invalid*");
        Snapshot().Should().BeEquivalentTo(before);
    }

    [Fact]
    public void Upgrade_ValidatesNestedConfigurationBeforeUpdatingAnyPackage()
    {
        var target = Path("nested-project");
        fileSystem.AddFile(fileSystem.Path.Combine(target, ".project-config.json"),
            new MockFileData("{\"MESVersion\":\"12.0.0\",\"NugetVersion\":\"invalid\"}"));
        fileSystem.AddFile(fileSystem.Path.Combine(target, "cmfpackage.json"),
            new MockFileData(fileSystem.File.ReadAllText(Path("cmfpackage.json"))));
        var command = new UpgradeBaseCommand(fileSystem);
        var before = Snapshot();

        Action execute = () => command.Execute(fileSystem.DirectoryInfo.New(root), NuGetVersion.Parse("12.0.0"), null, []);

        execute.Should().Throw<CliException>().WithMessage("*nested-project*.project-config.json*NugetVersion*invalid*");
        Snapshot().Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData("1.2")]
    [InlineData("1.2.3")]
    [InlineData("1.2.3-beta.1")]
    [InlineData("1.2.3-beta.1+build.4")]
    [InlineData("1.2.3.4")]
    [InlineData("01.2.3")]
    [InlineData("1.2.3.0")]
    [InlineData(" 1.2.3 ")]
    public void Bump_ValidPackageVersionsBelowTenAreSupported(string value)
    {
        var console = Invoke("bump", root, "--version", value);

        console.Error.ToString().Should().BeEmpty();
        JObject.Parse(fileSystem.File.ReadAllText(Path("cmfpackage.json")))["version"].Value<string>().Should().Be(value);
    }

    [Theory]
    [InlineData("bump", "--buildNr")]
    [InlineData("bump", "--preRelease")]
    [InlineData("customization", "--buildNrVersion")]
    [InlineData("customization", "--preReleaseVersion")]
    [InlineData("configuration", "--buildNrVersion")]
    [InlineData("configuration", "--preReleaseVersion")]
    public void EmptyBumpSuffix_UpdatesVersionWithoutSuffix(string name, string option)
    {
        if (name == "customization")
        {
            fileSystem.AddFile(Path("cmfpackage.json"), new MockFileData("""
                { "packageId": "Cmf.Custom.IoT", "packageType": "IoT", "version": "1.0.0" }
                """));
            fileSystem.AddFile(Path("src", "selected", "package.json"), new MockFileData("{\"version\":\"1.0.0\"}"));
        }
        if (name == "configuration")
        {
            AddWorkflow("first", "1.0.0-beta.1");
        }
        var args = new List<string> { root, "--version", "1.2.3", option, "" };
        if (name == "customization")
        {
            args.AddRange(new[] { "--packageNames", "selected" });
        }

        var console = Invoke(name, args.ToArray());

        console.Error.ToString().Should().BeEmpty();
        if (name == "configuration")
        {
            WorkflowVersion("first").Should().Be("1.2.3");
        }
        else
        {
            var packagePath = name == "customization" ? Path("src", "selected", "package.json") : Path("cmfpackage.json");
            JObject.Parse(fileSystem.File.ReadAllText(packagePath))["version"].Value<string>().Should().Be("1.2.3");
        }
    }

    [Fact]
    public void Bump_PreservesSuffixWithBuildMetadata()
    {
        var console = Invoke("bump", root, "--version", "1.2.3", "--preRelease", "beta.1+build.4");

        console.Error.ToString().Should().BeEmpty();
        JObject.Parse(fileSystem.File.ReadAllText(Path("cmfpackage.json")))["version"].Value<string>()
            .Should().Be("1.2.3-beta.1+build.4");
    }

    [Fact]
    public void Configuration_ValidSuffixOnlyUpdatesAllSelectedFiles()
    {
        AddWorkflow("first", "1.0.0");
        AddWorkflow("second", "2.0.0-previous.1");

        var console = Invoke("configuration", root, "--preReleaseVersion", "beta.1", "--iot");

        console.Error.ToString().Should().BeEmpty();
        WorkflowVersion("first").Should().Be("1.0.0-beta.1");
        WorkflowVersion("second").Should().Be("2.0.0-beta.1");
    }

    [Theory]
    [InlineData("--buildNrVersion")]
    [InlineData("--preReleaseVersion")]
    public void Configuration_EmptyOptionalVersionPreservesSuffixOnlyBehavior(string suffixOption)
    {
        AddWorkflow("first", "1.0.0");
        AddWorkflow("second", "2.0.0");

        var console = Invoke("configuration", root, "--version", "", suffixOption, "beta.1", "--iot");

        console.Error.ToString().Should().BeEmpty();
        WorkflowVersion("first").Should().Be("1.0.0-beta.1");
        WorkflowVersion("second").Should().Be("2.0.0-beta.1");
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void Bump_PackageOverloadPreservesOptionalVersionAndSuffixBehavior(string version, bool buildSuffix)
    {
        var package = CmfPackage.Load(fileSystem.FileInfo.New(Path("cmfpackage.json")), fileSystem: fileSystem);
        fileSystem.AddFile(Path("legacy", "cmfpackage.json"), new MockFileData(fileSystem.File.ReadAllText(Path("cmfpackage.json"))));
        var legacyPackage = CmfPackage.Load(fileSystem.FileInfo.New(Path("legacy", "cmfpackage.json")), fileSystem: fileSystem);
        var suffix = "beta.1";

        // Compare against the unchanged package handler used before validation
        // moved into the adapter, including its existing suffix-only behavior.
        Cmf.CLI.Factories.PackageTypeFactory.GetPackageTypeHandler(legacyPackage).Bump(version, suffix,
            new Dictionary<string, object> { ["root"] = null });

        new BumpCommand(fileSystem).Execute(package, string.IsNullOrEmpty(version) ? null : NuGetVersion.Parse(version),
            buildSuffix ? suffix : null, buildSuffix ? null : suffix, null);

        JObject.Parse(fileSystem.File.ReadAllText(Path("cmfpackage.json")))["version"].Value<string>()
            .Should().Be(legacyPackage.Version);
    }

    [Theory]
    [InlineData("1.2.3", null, "1.2.3")]
    [InlineData("1.2.3-beta.1+build.4", null, "1.2.3-beta.1+build.4")]
    [InlineData("1.2.3", "beta.1+build.4", "1.2.3-beta.1+build.4")]
    [InlineData("1.2.3-0.01a+001", null, "1.2.3-0.01a+001")]
    public void Customization_ValidVersionsUpdateSelectedNpmPackages(string version, string suffix, string expected)
    {
        fileSystem.AddFile(Path("cmfpackage.json"), new MockFileData("""
            { "packageId": "Cmf.Custom.IoT", "packageType": "IoT", "version": "1.0.0" }
            """));
        var selected = Path("src", "selected", "package.json");
        var unselected = Path("src", "unselected", "package.json");
        const string original = "{\"version\":\"1.0.0\"}";
        fileSystem.AddFile(selected, new MockFileData(original));
        fileSystem.AddFile(unselected, new MockFileData(original));
        var args = new List<string> { root, "--version", version, "--packageNames", "selected" };
        if (suffix != null)
        {
            args.AddRange(new[] { "--preReleaseVersion", suffix });
        }

        var console = Invoke("customization", args.ToArray());

        console.Error.ToString().Should().BeEmpty();
        JObject.Parse(fileSystem.File.ReadAllText(selected))["version"].Value<string>().Should().Be(expected);
        fileSystem.File.ReadAllText(unselected).Should().Be(original);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    public void Customization_PackageOverloadPreservesSuffixOnlyUpdatesAndPreflightsAllSelectedVersions(string version, bool invalidSecondPackage)
    {
        fileSystem.AddFile(Path("cmfpackage.json"), new MockFileData("""
            { "packageId": "Cmf.Custom.IoT", "packageType": "IoT", "version": "1.0.0" }
            """));
        var first = Path("src", "first", "package.json");
        var second = Path("src", "second", "package.json");
        fileSystem.AddFile(first, new MockFileData("{\"version\":\"1.0.0\"}"));
        fileSystem.AddFile(second, new MockFileData(invalidSecondPackage ? "{\"version\":\"1.2\"}" : "{\"version\":\"2.0.0\"}"));
        var package = CmfPackage.Load(fileSystem.FileInfo.New(Path("cmfpackage.json")), fileSystem: fileSystem);
        var before = Snapshot();
        Action execute = () => new BumpIoTCustomizationCommand(fileSystem).Execute(package, string.IsNullOrEmpty(version) ? null : SemanticVersion.Parse(version), null, "beta.1", "first,second", false);

        if (invalidSecondPackage)
        {
            execute.Should().Throw<CliException>().WithMessage("*Expected a semantic version*");
            Snapshot().Should().BeEquivalentTo(before);
        }
        else
        {
            execute.Should().NotThrow();
            JObject.Parse(fileSystem.File.ReadAllText(first))["version"].Value<string>().Should().Be("1.0.0-beta.1");
            JObject.Parse(fileSystem.File.ReadAllText(second))["version"].Value<string>().Should().Be("2.0.0-beta.1");
        }
    }

    [Theory]
    [InlineData("1.2", null, null)]
    [InlineData("1.2.3.4", null, null)]
    [InlineData("1.2", "--buildNrVersion", "beta.1")]
    [InlineData("1.2.3.4", "--buildNrVersion", "beta.1")]
    [InlineData("1.2", "--preReleaseVersion", "beta.1")]
    [InlineData("1.2.3.4", "--preReleaseVersion", "beta.1")]
    [InlineData("01.2.3", null, null)]
    [InlineData("1.2.3", "--preReleaseVersion", "01")]
    [InlineData("1.2.3-beta.01", null, null)]
    public void Customization_InvalidNpmVersionLeavesAllSelectedFilesUnchanged(string version, string suffixOption, string suffix)
    {
        fileSystem.AddFile(Path("cmfpackage.json"), new MockFileData("""
            { "packageId": "Cmf.Custom.IoT", "packageType": "IoT", "version": "1.0.0" }
            """));
        foreach (var name in new[] { "first", "second" })
        {
            fileSystem.AddFile(Path("src", name, "package.json"), new MockFileData("{\"version\":\"1.0.0\"}"));
            fileSystem.AddFile(Path("src", name, "src", "metadata.ts"), new MockFileData("version: \"1.0.0\","));
        }
        var before = Snapshot();
        var args = new List<string> { root, "--version", version, "--packageNames", "first,second" };
        if (suffixOption != null)
        {
            args.AddRange(new[] { suffixOption, suffix });
        }

        var console = Invoke("customization", args.ToArray());

        console.Error.ToString().Should().Contain("Expected ");
        Snapshot().Should().BeEquivalentTo(before);
    }

    private void AddWorkflow(string name, string version)
    {
        var json = new JObject
        {
            ["tasks"] = new JArray(new JObject
            {
                ["reference"] = new JObject
                {
                    ["package"] = new JObject { ["name"] = "@custom/connect-iot-example", ["version"] = version }
                }
            }),
            ["converters"] = new JArray()
        };
        fileSystem.AddFile(Path("AutomationWorkflowFiles", "group", name + ".json"), new MockFileData(json.ToString(Formatting.None)));
    }

    private string WorkflowVersion(string name) => JObject.Parse(fileSystem.File.ReadAllText(
        Path("AutomationWorkflowFiles", "group", name + ".json")))["tasks"][0]["reference"]["package"]["version"].Value<string>();

    private Command CreateCommand(string name)
    {
        var command = new Command("test");
        Cmf.CLI.Core.Commands.BaseCommand handler = name switch
        {
            "upgrade" => new UpgradeBaseCommand(fileSystem),
            "bump" => new BumpCommand(fileSystem),
            "customization" => new BumpIoTCustomizationCommand(fileSystem),
            "configuration" => new BumpIoTConfigurationCommand(fileSystem),
            _ => throw new ArgumentOutOfRangeException(nameof(name))
        };
        handler.Configure(command);
        return command;
    }

    private TestConsole Invoke(string name, params string[] args)
    {
        var console = new TestConsole();
        CreateCommand(name).Parse(args).Invoke(console);
        return console;
    }

    private string Path(params string[] parts) => fileSystem.Path.Combine(new[] { root }.Concat(parts).ToArray());
    private Dictionary<string, string> Snapshot() => fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));

    public void Dispose()
    {
        ExecutionContext.ServiceProvider.Dispose();
        ExecutionContext.ServiceProvider = previousServices;
    }
}

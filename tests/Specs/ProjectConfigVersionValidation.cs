using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace tests.Specs;

public class ProjectConfigVersionValidation
{
    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(true, "   ")]
    public void Load_PreservesMissingOrBlankMesVersion(bool includeField, string value)
    {
        var json = new JObject();
        if (includeField)
        {
            json["MESVersion"] = value == null ? JValue.CreateNull() : new JValue(value);
        }
        var fileSystem = CreateFileSystem(json);

        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));

        var config = new ProjectConfigService().Load(fileSystem);

        config.Should().BeOfType<ProjectConfigV1>();
        config.MESVersion.Should().BeNull();
        fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    public static IEnumerable<object[]> MalformedVersions()
    {
        foreach (var field in new[] { "MESVersion", "NugetVersion", "TestScenariosNugetVersion" })
        {
            yield return new object[] { "11.2.0", field, "invalid-version" };
            yield return new object[] { "12.0.0-beta.1", field, "1.2.3-beta..1" };
        }
        foreach (var field in new[] { "DevTasksVersion", "HTMLStarterVersion", "YoGeneratorVersion" })
        {
            yield return new object[] { "11.2.0", field, "invalid-version" };
            yield return new object[] { "11.2.0", field, "1.2.3.4" };
        }
    }

    [Theory]
    [MemberData(nameof(MalformedVersions))]
    public void Load_RejectsMalformedVersionsWithFieldValueAndSource(string mesVersion, string field, string value)
    {
        var json = new JObject { ["MESVersion"] = mesVersion, [field] = value };
        var fileSystem = CreateFileSystem(json);

        AssertRejectedWithoutChanges(fileSystem, field, value);
    }

    [Theory]
    [InlineData("MESVersion")]
    [InlineData("NugetVersion")]
    [InlineData("TestScenariosNugetVersion")]
    [InlineData("DevTasksVersion")]
    public void Load_RejectsStructuredVersionValues(string field)
    {
        var json = new JObject { ["MESVersion"] = "11.2.0", [field] = new JObject() };

        AssertRejectedWithoutChanges(CreateFileSystem(json), field, "{}");
    }

    [Theory]
    [InlineData("9.9.9")]
    [InlineData("9.9.9-rc.1")]
    [InlineData("9.9.9-rc.1+build.4")]
    public void Load_PreservesLegacyMesVersionsWithoutApplyingTheCliSupportGate(string value)
    {
        var fileSystem = CreateFileSystem(new JObject { ["MESVersion"] = value });

        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));
        var service = new ProjectConfigService();

        var config = service.Load(fileSystem);

        config.Should().BeOfType<ProjectConfigV1>();
        config.MESVersion.ToFullString().Should().Be(value);
        service.ProjectConfig.Should().BeSameAs(config);
        fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData("10.0.0", typeof(ProjectConfigV1))]
    [InlineData("10.0.0-alpha.1", typeof(ProjectConfigV1))]
    [InlineData("11.2.0", typeof(ProjectConfigV1))]
    [InlineData("12.0.0-beta.1+build.4", typeof(ProjectConfigV2))]
    [InlineData("13.0.0", typeof(ProjectConfigV2))]
    public void Load_PreservesValidVersionsAndSelectsModel(string value, Type expectedType)
    {
        var json = new JObject
        {
            ["MESVersion"] = value,
            ["NugetVersion"] = "1.2.3.4",
            ["TestScenariosNugetVersion"] = "1.2.3-beta.1+build.4",
            ["NGXSchematicsVersion"] = "custom-dist-tag",
            ["DevTasksVersion"] = "1.2.3-alpha.1+build.4",
            ["HTMLStarterVersion"] = "1.2.3",
            ["YoGeneratorVersion"] = "1.2.3-rc.1"
        };
        var fileSystem = CreateFileSystem(json);

        var config = new ProjectConfigService().Load(fileSystem);

        config.GetType().Should().Be(expectedType);
        config.MESVersion.ToFullString().Should().Be(value);
        config.NugetVersion.ToFullString().Should().Be("1.2.3.4");
        config.TestScenariosNugetVersion.ToFullString().Should().Be("1.2.3-beta.1+build.4");
        config.NGXSchematicsVersion.Should().Be("custom-dist-tag");
        if (config is ProjectConfigV1 legacy)
        {
            legacy.DevTasksVersion.ToFullString().Should().Be("1.2.3-alpha.1+build.4");
            legacy.HTMLStarterVersion.ToFullString().Should().Be("1.2.3");
            legacy.YoGeneratorVersion.ToFullString().Should().Be("1.2.3-rc.1");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Load_PreservesBlankOptionalVersions(string value)
    {
        var json = new JObject { ["MESVersion"] = "11.2.0" };
        foreach (var field in new[] { "NugetVersion", "TestScenariosNugetVersion", "DevTasksVersion", "HTMLStarterVersion", "YoGeneratorVersion" })
        {
            json[field] = value == null ? JValue.CreateNull() : new JValue(value);
        }

        var config = (ProjectConfigV1)new ProjectConfigService().Load(CreateFileSystem(json));

        config.NugetVersion.Should().BeNull();
        config.TestScenariosNugetVersion.Should().BeNull();
        config.DevTasksVersion.Should().BeNull();
        config.HTMLStarterVersion.Should().BeNull();
        config.YoGeneratorVersion.Should().BeNull();
    }

    [Fact]
    public void Load_DoesNotRequireOptionalVersions()
    {
        var config = new ProjectConfigService().Load(CreateFileSystem(new JObject { ["MESVersion"] = "11.2.0" }));

        config.MESVersion.Major.Should().Be(11);
        config.NugetVersion.Should().BeNull();
        config.TestScenariosNugetVersion.Should().BeNull();
    }

    [Fact]
    public void Load_PreservesCaseInsensitivePropertyMatching()
    {
        var json = new JObject { ["mesVersion"] = "12.0.0-beta.1", ["nugetVersion"] = "1.0.0" };

        var config = new ProjectConfigService().Load(CreateFileSystem(json));

        config.Should().BeOfType<ProjectConfigV2>();
        config.MESVersion.ToFullString().Should().Be("12.0.0-beta.1");
        config.NugetVersion.ToFullString().Should().Be("1.0.0");
    }

    [Fact]
    public void Load_V2PreservesExistingBehaviorForUnusedLegacyFields()
    {
        var json = new JObject { ["MESVersion"] = "12.0.0", ["DevTasksVersion"] = "unused-legacy-value" };

        new ProjectConfigService().Load(CreateFileSystem(json)).Should().BeOfType<ProjectConfigV2>();
    }

    [Fact]
    public void Load_OutsideProjectReturnsNull()
    {
        new ProjectConfigService().Load(new MockFileSystem()).Should().BeNull();
    }

    [Fact]
    public void Load_DoesNotCacheInvalidConfigurationAndAllowsCorrection()
    {
        var fileSystem = CreateFileSystem(new JObject { ["MESVersion"] = "12.0.0", ["NugetVersion"] = "bad-version" });
        var service = new ProjectConfigService();

        Action load = () => service.Load(fileSystem);
        load.Should().Throw<CliException>();
        service.ProjectConfig.Should().BeNull();
        fileSystem.File.WriteAllText(ConfigPath(fileSystem), "{\"MESVersion\":\"12.0.0\",\"NugetVersion\":\"1.0.0\"}");

        service.Load(fileSystem).NugetVersion.ToFullString().Should().Be("1.0.0");
    }

    private static CliException AssertRejectedWithoutChanges(MockFileSystem fileSystem, string field, string value)
    {
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));
        var service = new ProjectConfigService();

        Action load = () => service.Load(fileSystem);

        var exception = load.Should().Throw<CliException>().Which;
        exception.Message.Should().Contain(ConfigPath(fileSystem)).And.Contain(field).And.Contain(value).And.Contain("Expected ");
        service.ProjectConfig.Should().BeNull();
        fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path)).Should().BeEquivalentTo(before);
        return exception;
    }

    internal static MockFileSystem CreateFileSystem(JObject config)
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(ConfigPath(fileSystem), new MockFileData(config.ToString(Formatting.None)));
        fileSystem.AddFile("cmfpackage.json", new MockFileData("{\"version\":\"1.0.0\"}"));
        return fileSystem;
    }

    private static string ConfigPath(MockFileSystem fileSystem)
        => fileSystem.Path.Combine(fileSystem.Directory.GetCurrentDirectory(), ".project-config.json");
}

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using Cmf.CLI.Commands;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace tests.Specs;

public class HtmlTransformPacking
{
    public HtmlTransformPacking()
    {
        ExecutionContext.ServiceProvider = new ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>()
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData("{\n  \"customSetting\": true\n}")]
    [InlineData("\"customSetting\": { \"nested\": }")]
    [InlineData("\"customSetting\": true,")]
    public void Pack_RejectsInvalidTransformBeforeCreatingArchive(string transform)
    {
        var fileSystem = CreateFileSystem(transform);
        var command = new PackCommand(fileSystem);

        var exception = Assert.Throws<CliException>(() => command.Execute(
            fileSystem.DirectoryInfo.New("/repo/ui"), fileSystem.DirectoryInfo.New("/output"), false));

        Assert.Contains("config.json", exception.Message);
        Assert.Contains("custom-transform.json", exception.Message);
        Assert.Contains("line", exception.Message);
        Assert.Empty(fileSystem.Directory.GetFiles("/output", "*.zip"));
    }

    [Theory]
    [InlineData("\"customSetting\": true")]
    [InlineData("\"customSetting\": { \"endpoint\": \"$(CUSTOM_ENDPOINT)\", \"values\": [1, 2] }")]
    public void Pack_PreservesValidFragmentsAndDeploymentTokens(string transform)
    {
        var fileSystem = CreateFileSystem(transform);
        var command = new PackCommand(fileSystem);

        command.Execute(fileSystem.DirectoryInfo.New("/repo/ui"), fileSystem.DirectoryInfo.New("/output"), false);

        using var stream = fileSystem.File.OpenRead(fileSystem.Directory.GetFiles("/output", "*.zip").Single());
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        using var reader = new StreamReader(archive.GetEntry("config.json").Open());
        var config = JObject.Parse(reader.ReadToEnd());
        Assert.Equal(JObject.Parse("{" + transform + "}")["customSetting"], config["customSetting"], JToken.EqualityComparer);
        Assert.True(config.Value<bool>("firstTransform"));
        Assert.Equal("1.0.0", config.Value<string>("customizationVersion"));
    }

    [Fact]
    public void Pack_WithoutTransforms_GeneratesValidConfig()
    {
        var fileSystem = CreateFileSystem("\"customSetting\": true");
        var packagePath = "/repo/ui/cmfpackage.json";
        var package = JObject.Parse(fileSystem.File.ReadAllText(packagePath));
        var content = (JArray)package["contentToPack"];
        content.RemoveAt(2);
        content.RemoveAt(1);
        fileSystem.File.WriteAllText(packagePath, package.ToString());

        new PackCommand(fileSystem).Execute(
            fileSystem.DirectoryInfo.New("/repo/ui"), fileSystem.DirectoryInfo.New("/output"), false);

        using var stream = fileSystem.File.OpenRead(fileSystem.Directory.GetFiles("/output", "*.zip").Single());
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        using var reader = new StreamReader(archive.GetEntry("config.json").Open());
        var config = JObject.Parse(reader.ReadToEnd());
        Assert.Equal("1.0.0", config.Value<string>("customizationVersion"));
        Assert.Single(config.Properties());
    }

    private static MockFileSystem CreateFileSystem(string transform)
    {
        return new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/repo/.project-config.json"] = new("{\"MESVersion\": \"10.0.0\"}"),
            ["/repo/ui/angular.json"] = new("{\"version\": 1, \"projects\": {}}"),
            ["/repo/ui/package.json"] = new("{\"name\": \"custom-ui\"}"),
            ["/repo/ui/dist/index.html"] = new("<html></html>"),
            ["/repo/ui/first-transform.json"] = new("\"firstTransform\": true"),
            ["/repo/ui/custom-transform.json"] = new(transform),
            ["/repo/ui/cmfpackage.json"] = new("""
                {
                    "packageId": "Cmf.Custom.HTML",
                    "version": "1.0.0",
                    "description": "Custom UI",
                    "packageType": "Html",
                    "contentToPack": [
                        { "source": "dist/*", "target": "" },
                        { "source": "first-transform.json", "action": "Transform" },
                        { "source": "custom-transform.json", "action": "Transform" }
                    ]
                }
                """)
        }, "/repo/ui");
    }
}

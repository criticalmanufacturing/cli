using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Objects;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using tests.Objects;
using Xunit;

namespace tests.Specs;

public class Build
{
    [Fact]
    public void AbstractionsDirectoryConverter_SerializeDirectoryPath()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""Root.Package"",
                  ""version"": ""1.0.0"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                  ]
                }}")}
        });
        var txtWriter = new StringWriter();
        var writer = new JsonTextWriter(txtWriter);
        var converter = new AbstractionsDirectoryConverter();
        var dirInfo = fileSystem.DirectoryInfo.FromDirectoryName("/test");
        converter.WriteJson(writer, dirInfo, JsonSerializer.Create());

        txtWriter.ToString().Should().Be($"\"{MockUnixSupport.Path("C:\\test").Replace("\\", "\\\\")}\"", "directory path does not match");
    }

    [Fact]
    public void AbstractionsDirectoryConverter_DeserializeDirectoryPath()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""Root.Package"",
                  ""version"": ""1.0.0"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                  ]
                }}")}
        });
        ExecutionContext.Initialize(fileSystem);
        var txtReader = new StringReader($"\"{MockUnixSupport.Path("C:\\test").Replace("\\", "\\\\")}\"");
        var reader = new JsonTextReader(txtReader);
        // make sure Reader is at a token
        while (reader.TokenType == JsonToken.None)
            if (!reader.Read())
                break;
        var converter = new AbstractionsDirectoryConverter();
        var dirInfo = converter.ReadJson(reader, typeof(IDirectoryInfo), null, JsonSerializer.Create()) as IDirectoryInfo;
        Assert.NotNull(dirInfo);
        dirInfo.FullName.Should().Be(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("C:\\test")).FullName,
            "expected directory path does not match");
    }

    [Fact]
    public void DeserializeBuildStep()
    {
        var stepJson = @"
        {
            ""command"": ""npm.cmd"",
            ""args"": [""run"", ""build:clean""],
            ""workingDirectory"": "".""
        }";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""Root.Package"",
                  ""version"": ""1.0.0"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                  ]
                }}")}
        });
        ExecutionContext.Initialize(fileSystem);
        var txtReader = new StringReader(stepJson);
        var reader = new JsonTextReader(txtReader);
        var serializer = new JsonSerializer();
        var step = serializer.Deserialize(reader, typeof(ProcessBuildStep));
        var expectedStep = new ProcessBuildStep()
        {
            Command = "npm.cmd", Args = new[] { "run", "build:clean" },
            WorkingDirectory = fileSystem.DirectoryInfo.FromDirectoryName(".")
        };

        expectedStep.Should().Be(step);
    }

    [Fact]
    public void SerializeBuildStep()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""Root.Package"",
                  ""version"": ""1.0.0"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                  ]
                }}")}
        });
        ExecutionContext.Initialize(fileSystem);
        var step = new ProcessBuildStep()
        {
            Command = "npm.cmd", Args = new[] { "run", "build:clean" },
            WorkingDirectory = fileSystem.DirectoryInfo.FromDirectoryName(".")
        };
        var serializer = new JsonSerializer();
        var txtWriter = new StringWriter();
        var writer = new JsonTextWriter(txtWriter);
        
        // serializer.Serialize(writer, step, typeof(ProcessBuildStep));
        DefaultContractResolver contractResolver = new()
        {
            NamingStrategy = new CamelCaseNamingStrategy(),
        };

        JsonSerializerSettings jsonSerializerSettings = new()
        {
            Formatting = Formatting.None,
            ContractResolver = contractResolver,
            NullValueHandling = NullValueHandling.Ignore
        };

        string serializedStep = JsonConvert.SerializeObject(step, jsonSerializerSettings);

        serializedStep.Should().Be(@$"{{""args"":[""run"",""build:clean""],""command"":""npm.cmd"",""workingDirectory"":""{MockUnixSupport.Path("C:\\").Replace("\\", "\\\\")}""}}");
    }
}
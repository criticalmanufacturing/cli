using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Commands;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace tests.Specs;

public class Upgrade
{
    [Fact]
    public void RootPackage()
    {
        string version = "11.1.6";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                "/cmfpackage.json",
                new MockFileData(
                    @"{
                      ""packageId"": ""Cmf.SMT.Package"",
                      ""version"": ""3.2.0"",
                      ""description"": ""Root package"",
                      ""packageType"": ""Root"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": false,
                      ""dependencies"": [
                        {
                          ""id"": ""DS.ClickHouse.Workaround"",
                          ""version"": ""1.2.0""
                        },
                        {
                          ""id"": ""Cmf.Environment"",
                          ""version"": ""11.1.3""
                        },
                        {
                          ""id"": ""criticalmanufacturing.deploymentmetadata"",
                          ""version"": ""11.1.3""
                        },
                        {
                          ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                          ""version"": ""11.1.3""
                        }
                      ]
                    }"
                )
            },
        });

        var cmd = new UpgradeCommand(fileSystem);
        cmd.Execute(fileSystem.DirectoryInfo.New("/"), version);

        string rootCmfpackageContents = fileSystem.File.ReadAllText("/cmfpackage.json");
        JObject rootCmfpackageObject = (JObject)JsonConvert.DeserializeObject(rootCmfpackageContents);

        rootCmfpackageObject["dependencies"][0]["version"].ToString().Should().Be("1.2.0");
        rootCmfpackageObject["dependencies"][1]["version"].ToString().Should().Be(version);
        rootCmfpackageObject["dependencies"][2]["version"].ToString().Should().Be(version);
        rootCmfpackageObject["dependencies"][3]["version"].ToString().Should().Be(version);
    }

    [Fact]
    public void BusinessPackage()
    {
        string version = "11.1.6";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                "/Business/cmfpackage.json",
                new MockFileData(
                    @"{
                      ""packageId"": ""Cmf.SMT.Business"",
                      ""version"": ""3.2.0"",
                      ""description"": ""Business Package"",
                      ""packageType"": ""Business"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": false
                    }"
                )
            },
            {
                "/Business/Common/a.b.c.csproj",
                new MockFileData(
                    @"
                    <Project Sdk=""Microsoft.NET.Sdk"">
                    	<ItemGroup>
                    		<PackageReference Include=""Cmf.Foundation.BusinessObjects"" Version=""11.1.5"" />
                    		<PackageReference Include=""Cmf.MessageBus.Client"" Version=""11.1.5"" />
                    		<PackageReference Include=""Cmf.Common.CustomActionUtilities"" Version=""10.1.0"" GeneratePathProperty=""true"" />
                    		<PackageReference Include=""Cmf.LoadBalancing"" Version=""11.1.5"" />
                    	</ItemGroup>
                    </Project>
                    "
                )
            }
        });

        var cmd = new UpgradeCommand(fileSystem);
        cmd.Execute(fileSystem.DirectoryInfo.New("/Business"), version);

        string csprojContents = fileSystem.File.ReadAllText("/Business/Common/a.b.c.csproj");
        csprojContents.Should().Contain($@"<PackageReference Include=""Cmf.Foundation.BusinessObjects"" Version=""{version}"" />");
        csprojContents.Should().Contain($@"<PackageReference Include=""Cmf.MessageBus.Client"" Version=""{version}"" />");
        csprojContents.Should().Contain($@"<PackageReference Include=""Cmf.Common.CustomActionUtilities"" Version=""{version}"" GeneratePathProperty=""true"" />");
        csprojContents.Should().Contain(@"<PackageReference Include=""Cmf.LoadBalancing"" Version=""11.1.5"" />");
    }

    [Fact]
    public void TestPackage()
    {
        string version = "11.1.6";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                "/cmfpackage.json",
                new MockFileData(
                    @"{
                      ""packageId"": ""Cmf.SMT.Tests"",
                      ""version"": ""3.2.0"",
                      ""description"": ""Tests Package"",
                      ""packageType"": ""Tests"",
                      ""isInstallable"": false,
                      ""isUniqueInstall"": false
                    }"
                )
            },
            {
                "/Common/a.b.c.csproj",
                new MockFileData(
                    @"
                        <Project Sdk=""Microsoft.NET.Sdk"">
                          <ItemGroup>
                            <PackageReference Include=""Cmf.Common.TestUtilities"" Version=""2.3.157590"" />
                            <PackageReference Include=""Cmf.Common.TestFramework.ConnectIoT"" Version=""1.0.131717"" />
                            <PackageReference Include=""Cmf.Dev.Mes.TestScenarios"" Version=""11.1.5"" />
                          </ItemGroup>
                        </Project>
                    "
                )
            }
        });

        var cmd = new UpgradeCommand(fileSystem);
        cmd.Execute(fileSystem.DirectoryInfo.New("/"), version);

        string csprojContents = fileSystem.File.ReadAllText("/Common/a.b.c.csproj");
        csprojContents.Should().Contain($@"<PackageReference Include=""Cmf.Dev.Mes.TestScenarios"" Version=""{version}"" />");
    }
}
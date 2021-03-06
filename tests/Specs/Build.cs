using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Handlers;
using Cmf.Common.Cli.TestUtilities;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
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
            Command = "npm.cmd",
            Args = new[] { "run", "build:clean" },
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
            Command = "npm.cmd",
            Args = new[] { "run", "build:clean" },
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

    [Fact]
    public void BusinessBuildWith_TestFromInputTrue_CommandWithTestFalse()
    {
        KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
        KeyValuePair<string, string> packageDep1 = new("Cmf.Environment", "8.3.0");
        KeyValuePair<string, string> packageDep2 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
        string url = TestUtilities.GetTmpDirectory();

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $"{url}/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }},
                    {{
                         ""id"": ""{packageDep2.Key}"",
                        ""version"": ""{packageDep2.Value}""
                    }}
                  ]
                }}")},
                { $"{url}/.project-config.json", new MockFileData(
                @$"{{
                      ""ProjectName"": ""CF58AABF"",
                      ""NPMRegistry"": ""http://npm_registry/"",
                      ""NuGetRegistry"": ""htt://nuget_registry/"",
                      ""AzureDevopsCollectionURL"": ""http://ad/collection"",
                      ""AgentPool"": ""agent_pool"",
                      ""AgentType"": ""Hosted"",
                      ""RepositoryURL"": ""https://repo_url/collection/project/_git/repo"",
                      ""EnvironmentName"": ""system_name"",
                      ""DefaultDomain"": ""DOMAIN"",
                      ""RESTPort"": ""1234"",
                      ""Tenant"": ""tenant"",
                      ""MESVersion"": ""8.2.0"",
                      ""DevTasksVersion"": ""8.1.0"",
                      ""HTMLStarterVersion"": ""8.0.0"",
                      ""YoGeneratorVersion"": ""8.1.0"",
                      ""NugetVersion"": ""8.2.0"",
                      ""TestScenariosNugetVersion"": ""8.2.0"",
                      ""IsSslEnabled"": ""True"",
                      ""vmHostname"": ""app_server_address"",
                      ""DBReplica1"": ""server1\\instance"",
                      ""DBReplica2"": ""server2\\instance"",
                      ""DBServerOnline"": ""server1\\instance"",
                      ""DBServerODS"": ""server2\\instance"",
                      ""DBServerDWH"": ""server3\\instance"",
                      ""ReportServerURI"": ""http://reporting_services/Reports"",
                      ""AlwaysOn"": ""False"",
                      ""InstallationPath"": ""install_path"",
                      ""DBBackupPath"": ""backup_share"",
                      ""TemporaryPath"": ""temp_folder"",
                      ""HTMLPort"": ""443"",
                      ""GatewayPort"": ""5678"",
                      ""ReleaseEnvironmentConfig"": ""config.json""
                }}")}
            });

        ExecutionContext.Initialize(fileSystem);
        IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"{url}/{CliConstants.CmfPackageFileName}");
        fileSystem.Directory.SetCurrentDirectory(url);

        string message = string.Empty;
        CmfPackage cmfPackage = null;
        try
        {
            // Reading cmfPackage
            cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }

        var mock = new Mock<IBuildCommand>();
        mock.Setup(m => m.Exec());
        mock.SetupAllProperties();
        mock.Object.Test = false;
        mock.Object.DisplayName = "Run Build Command";
        BusinessPackageTypeHandler businessPackageTypeHandler = new BusinessPackageTypeHandler(cmfPackage);
        businessPackageTypeHandler.BuildSteps = new IBuildCommand[]
        {
            mock.Object
        };

        StringWriter standardOutput = (new Logging()).GetLogStringWriter();
        businessPackageTypeHandler.Build(true);
        Assert.Contains("Executing 'Run Build Command'", standardOutput.ToString().Trim());
    }

    [Fact]
    public void BusinessBuildWith_TestFromInputTrue_CommandWithTestTrue()
    {
        KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
        KeyValuePair<string, string> packageDep1 = new("Cmf.Environment", "8.3.0");
        KeyValuePair<string, string> packageDep2 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
        string url = TestUtilities.GetTmpDirectory();

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $"{url}/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }},
                    {{
                         ""id"": ""{packageDep2.Key}"",
                        ""version"": ""{packageDep2.Value}""
                    }}
                  ]
                }}")},
                { $"{url}/.project-config.json", new MockFileData(
                @$"{{
                      ""ProjectName"": ""CF58AABF"",
                      ""NPMRegistry"": ""http://npm_registry/"",
                      ""NuGetRegistry"": ""htt://nuget_registry/"",
                      ""AzureDevopsCollectionURL"": ""http://ad/collection"",
                      ""AgentPool"": ""agent_pool"",
                      ""AgentType"": ""Hosted"",
                      ""RepositoryURL"": ""https://repo_url/collection/project/_git/repo"",
                      ""EnvironmentName"": ""system_name"",
                      ""DefaultDomain"": ""DOMAIN"",
                      ""RESTPort"": ""1234"",
                      ""Tenant"": ""tenant"",
                      ""MESVersion"": ""8.2.0"",
                      ""DevTasksVersion"": ""8.1.0"",
                      ""HTMLStarterVersion"": ""8.0.0"",
                      ""YoGeneratorVersion"": ""8.1.0"",
                      ""NugetVersion"": ""8.2.0"",
                      ""TestScenariosNugetVersion"": ""8.2.0"",
                      ""IsSslEnabled"": ""True"",
                      ""vmHostname"": ""app_server_address"",
                      ""DBReplica1"": ""server1\\instance"",
                      ""DBReplica2"": ""server2\\instance"",
                      ""DBServerOnline"": ""server1\\instance"",
                      ""DBServerODS"": ""server2\\instance"",
                      ""DBServerDWH"": ""server3\\instance"",
                      ""ReportServerURI"": ""http://reporting_services/Reports"",
                      ""AlwaysOn"": ""False"",
                      ""InstallationPath"": ""install_path"",
                      ""DBBackupPath"": ""backup_share"",
                      ""TemporaryPath"": ""temp_folder"",
                      ""HTMLPort"": ""443"",
                      ""GatewayPort"": ""5678"",
                      ""ReleaseEnvironmentConfig"": ""config.json""
                }}")}
            });

        ExecutionContext.Initialize(fileSystem);
        IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"{url}/{CliConstants.CmfPackageFileName}");
        fileSystem.Directory.SetCurrentDirectory(url);

        string message = string.Empty;
        CmfPackage cmfPackage = null;
        try
        {
            // Reading cmfPackage
            cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }

        var mock = new Mock<IBuildCommand>();
        mock.Setup(m => m.Exec());
        mock.SetupAllProperties();
        mock.Object.Test = true;
        mock.Object.DisplayName = "Run Business Unit Tests";
        BusinessPackageTypeHandler businessPackageTypeHandler = new BusinessPackageTypeHandler(cmfPackage);
        businessPackageTypeHandler.BuildSteps = new IBuildCommand[]
        {
            mock.Object
        };

        StringWriter standardOutput = (new Logging()).GetLogStringWriter();
        businessPackageTypeHandler.Build(true);
        Assert.Contains("Executing 'Run Business Unit Tests'", standardOutput.ToString().Trim());
    }

    [Fact]
    public void BusinessBuildWith_TestFromInputFalse_CommandWithTestTrue()
    {
        KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
        KeyValuePair<string, string> packageDep1 = new("Cmf.Environment", "8.3.0");
        KeyValuePair<string, string> packageDep2 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
        string url = TestUtilities.GetTmpDirectory();

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $"{url}/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }},
                    {{
                         ""id"": ""{packageDep2.Key}"",
                        ""version"": ""{packageDep2.Value}""
                    }}
                  ]
                }}")},
                { $"{url}/.project-config.json", new MockFileData(
                @$"{{
                      ""ProjectName"": ""CF58AABF"",
                      ""NPMRegistry"": ""http://npm_registry/"",
                      ""NuGetRegistry"": ""htt://nuget_registry/"",
                      ""AzureDevopsCollectionURL"": ""http://ad/collection"",
                      ""AgentPool"": ""agent_pool"",
                      ""AgentType"": ""Hosted"",
                      ""RepositoryURL"": ""https://repo_url/collection/project/_git/repo"",
                      ""EnvironmentName"": ""system_name"",
                      ""DefaultDomain"": ""DOMAIN"",
                      ""RESTPort"": ""1234"",
                      ""Tenant"": ""tenant"",
                      ""MESVersion"": ""8.2.0"",
                      ""DevTasksVersion"": ""8.1.0"",
                      ""HTMLStarterVersion"": ""8.0.0"",
                      ""YoGeneratorVersion"": ""8.1.0"",
                      ""NugetVersion"": ""8.2.0"",
                      ""TestScenariosNugetVersion"": ""8.2.0"",
                      ""IsSslEnabled"": ""True"",
                      ""vmHostname"": ""app_server_address"",
                      ""DBReplica1"": ""server1\\instance"",
                      ""DBReplica2"": ""server2\\instance"",
                      ""DBServerOnline"": ""server1\\instance"",
                      ""DBServerODS"": ""server2\\instance"",
                      ""DBServerDWH"": ""server3\\instance"",
                      ""ReportServerURI"": ""http://reporting_services/Reports"",
                      ""AlwaysOn"": ""False"",
                      ""InstallationPath"": ""install_path"",
                      ""DBBackupPath"": ""backup_share"",
                      ""TemporaryPath"": ""temp_folder"",
                      ""HTMLPort"": ""443"",
                      ""GatewayPort"": ""5678"",
                      ""ReleaseEnvironmentConfig"": ""config.json""
                }}")}
            });

        ExecutionContext.Initialize(fileSystem);
        IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"{url}/{CliConstants.CmfPackageFileName}");
        fileSystem.Directory.SetCurrentDirectory(url);

        string message = string.Empty;
        CmfPackage cmfPackage = null;
        try
        {
            // Reading cmfPackage
            cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }

        var mock = new Mock<IBuildCommand>();
        mock.Setup(m => m.Exec());
        mock.SetupAllProperties();
        mock.Object.Test = true;
        mock.Object.DisplayName = "Run Business Unit Tests";
        BusinessPackageTypeHandler businessPackageTypeHandler = new BusinessPackageTypeHandler(cmfPackage);
        businessPackageTypeHandler.BuildSteps = new IBuildCommand[]
        {
            mock.Object
        };

        StringWriter standardOutput = (new Logging()).GetLogStringWriter();
        businessPackageTypeHandler.Build(false);
        Assert.DoesNotContain("Executing 'Run Business Unit Tests'", standardOutput.ToString().Trim());
    }
}
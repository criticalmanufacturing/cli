using Cmf.CLI.Builders;
using Cmf.CLI.Commands;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Utilities;
using Cmf.CLI.Factories;
using Cmf.CLI.Handlers;
using Cmf.CLI.Interfaces;
using Cmf.CLI.Utilities;
using Cmf.Common.Cli.TestUtilities;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Security.Policy;
using Xunit;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
        var dirInfo = fileSystem.DirectoryInfo.New("/test");
        converter.WriteJson(writer, dirInfo, JsonSerializer.Create());

        txtWriter.ToString().Should().Be(@"""/test""", "directory path does not match");
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
        dirInfo.FullName.Should().Be(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("C:\\test")).FullName,
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
            WorkingDirectory = fileSystem.DirectoryInfo.New(".")
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
            WorkingDirectory = fileSystem.DirectoryInfo.New(".")
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

        serializedStep.Should().Be(@$"{{""args"":[""run"",""build:clean""],""command"":""npm.cmd"",""workingDirectory"":"".""}}");
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
        IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"{url}/{CliConstants.CmfPackageFileName}");
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
        IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"{url}/{CliConstants.CmfPackageFileName}");
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
        IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"{url}/{CliConstants.CmfPackageFileName}");
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DataBuild_WithBuildablePackages(bool withBuildableSteps)
    {
        KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
        KeyValuePair<string, string> packageData = new("Cmf.Custom.Data", "1.1.0");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            // project config file
            { ".project-config.json", new MockFileData("")},

            // root cmfpackage file
            { $"cmfpackage.json", new MockFileData(
            @$"{{
                ""packageId"": ""{packageRoot.Key}"",
                ""version"": ""{packageRoot.Value}"",
                ""description"": ""This package deploys Critical Manufacturing Customization"",
                ""packageType"": ""Root"",
                ""isInstallable"": true,
                ""isUniqueInstall"": false
            }}")}
        });

        if (!withBuildableSteps)
        {
            // data cmfpackage file
            fileSystem.AddFile($"{packageData.Key}/{CliConstants.CmfPackageFileName}", new MockFileData(
            @$"{{
                ""packageId"": ""{packageData.Key}"",
                ""version"": ""{packageData.Value}"",
                ""description"": ""This package deploys Critical Manufacturing Customization"",
                ""packageType"": ""Data"",
                ""isInstallable"": true,
                ""isUniqueInstall"": true,
                ""contentToPack"": [
                {{
                    ""source"": ""DEEs/*"",
                    ""target"": ""DeeRules"",
                    ""contentType"": ""DEE""
                }}
                ]
            }}"));
        }
        else
        {
            // data cmfpackage file
            fileSystem.AddFile($"{packageData.Key}/{CliConstants.CmfPackageFileName}", new MockFileData(
            @$"{{
                    ""packageId"": ""{packageData.Key}"",
                    ""version"": ""{packageData.Value}"",
                    ""description"": ""This package deploys Critical Manufacturing Customization"",
                    ""packageType"": ""Data"",
                    ""isInstallable"": true,
                    ""isUniqueInstall"": true,
                    ""contentToPack"": [
                    {{
                        ""source"": ""DEEs/*"",
                        ""target"": ""DeeRules"",
                        ""contentType"": ""DEE""
                    }}
                    ],
                    ""buildablePackages"": [
                        ""../Cmf.Custom.Business""
                    ]
                }}"));

            // business cmfpackage file
            fileSystem.AddFile("Cmf.Custom.Business/cmfpackage.json", new MockFileData(
            @$"{{
              ""packageId"": ""Cmf.Custom.Business"",
              ""version"": ""1.1.0"",
              ""description"": ""Cmf Custom Business Package"",
              ""packageType"": ""Business"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
                {{
                  ""source"": ""Release/*.dll"",
                  ""target"": """",
                  ""ignoreFiles"": [
                    "".cmfpackageignore""
                  ]
                }}
              ]
            }}"));

            // business sln
            fileSystem.AddFile("Cmf.Custom.Business/Business.sln", new MockFileData(
            @$"Microsoft Visual Studio Solution File, Format Version 12.00
                # Visual Studio Version 17
                VisualStudioVersion = 17.3.32825.248
                MinimumVisualStudioVersion = 10.0.40219.1
                Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""Cmf.Custom.Common"", ""Cmf.Custom.Common\Cmf.Custom.Common.csproj"", ""{{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}""
                Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""Cmf.Custom.Actions"", ""..\Data\DEEs\Cmf.Custom.Actions.csproj"", ""{{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}""
                EndProject
                Global
	                GlobalSection(SolutionConfigurationPlatforms) = preSolution
		                Debug|Any CPU = Debug|Any CPU
		                Release|Any CPU = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(ProjectConfigurationPlatforms) = postSolution
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.Build.0 = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.Build.0 = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(SolutionProperties) = preSolution
		                HideSolutionNode = FALSE
	                EndGlobalSection
	                GlobalSection(ExtensibilityGlobals) = postSolution
		                SolutionGuid = {{B6998FE2-5739-49CF-939B-73DB4B5DAF2E}}
	                EndGlobalSection
                EndGlobal"));

            // common csproj
            fileSystem.AddFile("Cmf.Custom.Business/Cmf.Custom.Common/Cmf.Custom.Common.csproj", new MockFileData(
            @$"<Project Sdk=""Microsoft.NET.Sdk"">
                      <PropertyGroup>
                        <TargetFramework>net6.0</TargetFramework>
                        <ImplicitUsings>enable</ImplicitUsings>
                        <Nullable>enable</Nullable>
                      </PropertyGroup>
                </Project>"));

            // class file
            fileSystem.AddFile("Cmf.Custom.Business/Cmf.Custom.Common/TestClass.cs", new MockFileData(
            @$"namespace Cmf.Custom.Actions
                {{
                    public class Class1
                    {{
                    }}
                }}"));

            // deeeactions csproj
            fileSystem.AddFile("Cmf.Custom.Data/DEEs/Cmf.Custom.Actions.csproj", new MockFileData(
            @$"<Project Sdk=""Microsoft.NET.Sdk"">
                      <PropertyGroup>
                        <TargetFramework>net6.0</TargetFramework>
                        <ImplicitUsings>enable</ImplicitUsings>
                        <Nullable>enable</Nullable>
                      </PropertyGroup>
                </Project>"));

            // deeActionfile
            fileSystem.AddFile("Cmf.Custom.Data/DEEs/TestDEE.cs", new MockFileData(
            @$"namespace Cmf.Custom.Actions
            {{
                public class Class1
                {{
                }}
            }}"));
        }

        ExecutionContext.Initialize(fileSystem);

        IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"Cmf.Custom.Data/{CliConstants.CmfPackageFileName}");
        DataPackageTypeHandlerV2 packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile) as DataPackageTypeHandlerV2;

        packageTypeHandler.BuildablePackagesHandlers
            .HasAny()
            .Should().Be(withBuildableSteps);

        packageTypeHandler.BuildablePackagesHandlers
            .Any(BuildablePackageHandler =>
            {
                bool result = false;
                if (BuildablePackageHandler is BusinessPackageTypeHandler)
                {
                    var businessPackageTypeHandler = BuildablePackageHandler as BusinessPackageTypeHandler;
                    if (businessPackageTypeHandler.BuildSteps[0] is ExecuteCommand<RestoreCommand>)
                    {
                        var buildStep = (businessPackageTypeHandler.BuildSteps[0] as ExecuteCommand<RestoreCommand>);
                        if (buildStep.Command is RestoreCommand)
                        {
                            result = true;
                        }
                    }
                }
                return result;
            })
            .Should().Be(withBuildableSteps);
    }
}
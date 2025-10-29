using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text.RegularExpressions;
using Cmf.CLI.Commands;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace tests.Specs;

public class UpgradeBase
{
    [Fact]
    public void ProjectConfig()
    {
        string projectConfigPath = @"/.project-config.json";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                projectConfigPath,
                new MockFileData(
                    @"{
                          ""ProjectName"": ""SMTTemplate"",
                          ""RepositoryType"": ""Customization"",
                          ""BaseLayer"": ""MES"",
                          ""NPMRegistry"": ""https://google.com"",
                          ""NuGetRegistry"": ""https://google.com"",
                          ""AzureDevopsCollectionURL"": ""https://google.com"",
                          ""AzureDevopsProductURL"": ""https://google.com"",
                          ""RepositoryURL"": ""https://google.com"",
                          ""EnvironmentName"": ""SMTTemplate"",
                          ""DefaultDomain"": ""CMF"",
                          ""RESTPort"": ""443"",
                          ""Tenant"": ""IndustryTemplates"",
                          ""MESVersion"": ""11.1.1"",
                          ""DevTasksVersion"": """",
                          ""HTMLStarterVersion"": """",
                          ""YoGeneratorVersion"": """",
                          ""NGXSchematicsVersion"": ""1.3.4"",
                          ""NugetVersion"": ""11.1.2"",
                          ""TestScenariosNugetVersion"": ""11.1.3"",
                          ""IsSslEnabled"": ""True""
                    }"
                )
            }
        });

        UpgradeBaseCommand cmd = new UpgradeBaseCommand(fileSystem);
        cmd.Execute(fileSystem.FileInfo.New(projectConfigPath).Directory, "11.1.6", "11.1.6", []);

        string projectConfigContents = fileSystem.File.ReadAllText(projectConfigPath);

        projectConfigContents.Should().ContainAll([
            @"""MESVersion"": ""11.1.6"",",
            @"""NugetVersion"": ""11.1.6"",",
            @"""TestScenariosNugetVersion"": ""11.1.6"",",
        ]);
        Assert.Equal(3, Regex.Matches(projectConfigContents, @"11\.1\.6").Count);
    }

    [Fact]
    public void BusinessPackage()
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
                          ""version"": ""1.2.0"",
                          ""mandatory"": false
                        },
                        {
                          ""id"": ""Cmf.Environment"",
                          ""version"": ""11.1.3"",
                          ""mandatory"": false
                        },
                        {
                          ""id"": ""criticalmanufacturing.deploymentmetadata"",
                          ""version"": ""11.1.3"",
                          ""mandatory"": false
                        },
                        {
                          ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                          ""version"": ""11.1.3"",
                          ""mandatory"": false
                        },
                        {
                          ""id"": ""Cmf.SMT.Business"",
                          ""version"": ""3.2.0""
                        }
                      ]
                    }"
                )
            },
            {
                "/Business/cmfpackage.json",
                new MockFileData(
                    @"{
                      ""packageId"": ""Cmf.SMT.Business"",
                      ""version"": ""3.2.0"",
                      ""description"": ""Business Package"",
                      ""packageType"": ""Business"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": false,
                      ""contentToPack"": [
                        {
                          ""source"": ""Release/*.dll"",
                          ""target"": """"
                        }
                      ]
                    }"
                )
            },
            {
                "/Business/Common/a.b.c.csproj",
                new MockFileData(
                    @"
                    <Project Sdk=""Microsoft.NET.Sdk"">
                    	<PropertyGroup>
                    		<TargetFramework>net8.0</TargetFramework>
                    		<OutputType>Library</OutputType>
                    		<GenerateAssemblyInfo>false</GenerateAssemblyInfo>
                    		<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
                    		<AssemblyName>Cmf.Custom.SMT.BusinessObjects.SMTDefectAction</AssemblyName>
                    		<RootNamespace>Cmf.Custom.SMT.BusinessObjects.SMTDefectAction</RootNamespace>
                    		<NoWarn>NU1605</NoWarn>
                    	</PropertyGroup>
                    	<PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|AnyCPU'"">
                    		<OutputPath>..\..\LocalEnvironment\BusinessTier</OutputPath>
                    	</PropertyGroup>
                    	<PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|AnyCPU'"">
                    		<OutputPath>..\Release</OutputPath>
                    	</PropertyGroup>
                    	<ItemGroup>
		                    <ProjectReference Include=""..\Cmf.Custom.SMT.BusinessObjects.SMTDefectAction\Cmf.Custom.SMT.BusinessObjects.SMTDefectAction.csproj"" />
		                    <ProjectReference Include=""..\Cmf.SMT.Common\Cmf.SMT.Common.csproj"" />
                    		<ProjectReference Include=""..\Cmf.SMT.Common\Cmf.SMT.Common.csproj"" />

                    		<PackageReference Include=""Cmf.Foundation.BusinessObjects"" Version=""11.1.5"" />
                            <PackageReference Include=""Cmf.Foundation.BusinessOrchestration"" Version=""11.1.5"" />
                    		<PackageReference Include=""Cmf.Navigo.BusinessObjects"" Version=""11.1.5"" />
		                    <PackageReference Include=""Cmf.Navigo.BusinessOrchestration"" Version=""11.1.5"" />
		                    <PackageReference Include=""Cmf.MessageBus.Client"" Version=""11.1.5"" />
		                    <PackageReference Include=""Cmf.Common.CustomActionUtilities"" Version=""10.1.0"" GeneratePathProperty=""true"" />
		                    <PackageReference Include=""cmf.common.customactionutilities"" Version=""10.2.6.1"" GeneratePathProperty=""true"" />
            

		                    <PackageReference Include=""Cmf.LoadBalancing"" Version=""11.1.5"" />

		                    <PackageReference Include=""FluentAssertions"" Version=""7.0.0"" />
		                    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.1.0"" />
		                    <PackageReference Include=""Moq"" Version=""4.20.72"" />
		                    <PackageReference Include=""xunit"" Version=""2.4.1"" />
                    	</ItemGroup>
                    </Project>
                    "
                )
            }
        });


        UpgradeBaseCommand cmd = new UpgradeBaseCommand(fileSystem);
        cmd.Execute(fileSystem.DirectoryInfo.New("/"), version, version, []);

        string rootCmfpackageContents = fileSystem.File.ReadAllText("/cmfpackage.json");

        JObject rootCmfpackageObject = (JObject)JsonConvert.DeserializeObject(rootCmfpackageContents);

        rootCmfpackageObject["dependencies"][0]["version"].ToString().Should().Be("1.2.0");
        rootCmfpackageObject["dependencies"][1]["version"].ToString().Should().Be(version); // Cmf.Environment
        rootCmfpackageObject["dependencies"][2]["version"].ToString().Should().Be(version); // criticalmanufacturing.deploymentmetadata
        rootCmfpackageObject["dependencies"][3]["version"].ToString().Should().Be(version); // CriticalManufacturing.DeploymentMetadata
        rootCmfpackageObject["dependencies"][4]["version"].ToString().Should().Be("3.2.0");

        Assert.Equal(3, Regex.Matches(rootCmfpackageContents, version.Replace(".", "\\.")).Count);

        string csprojContents = fileSystem.File.ReadAllText("/Business/Common/a.b.c.csproj");
        csprojContents.Should().ContainAll([
            $@"<PackageReference Include=""Cmf.Foundation.BusinessObjects"" Version=""{version}"" />",
            $@"<PackageReference Include=""Cmf.Foundation.BusinessOrchestration"" Version=""{version}"" />",
            $@"<PackageReference Include=""Cmf.Navigo.BusinessObjects"" Version=""{version}"" />",
            $@"<PackageReference Include=""Cmf.Navigo.BusinessOrchestration"" Version=""{version}"" />",
            $@"<PackageReference Include=""Cmf.MessageBus.Client"" Version=""{version}"" />",
            $@"<PackageReference Include=""Cmf.Common.CustomActionUtilities"" Version=""{version}"" GeneratePathProperty=""true"" />",
            $@"<PackageReference Include=""cmf.common.customactionutilities"" Version=""{version}"" GeneratePathProperty=""true"" />",
        ]);
        Assert.Equal(7, Regex.Matches(csprojContents, version.Replace(".", "\\.")).Count);
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DataPackage(bool iotShouldBeUpdated)
    {
        string version = "11.1.6";
        string iotVersion = "11.1.6-123";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                "/cmfpackage.json",
                new MockFileData(
                    @"{
                      ""packageId"": ""Cmf.SMT.Data"",
                      ""version"": ""3.2.0"",
                      ""description"": ""Data Package"",
                      ""packageType"": ""Data"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""contentToPack"": [
                        {
                          ""source"": ""DEEs/*"",
                          ""target"": ""DeeRules"",
                          ""ignoreFiles"": [
                            ""app.config"",
                            ""Cmf.SMT.Actions.csproj""
                          ],
                          ""contentType"": ""DEE""
                        },
                        {
                          ""source"": ""DEEs/ProcessRules/EntityTypes/*"",
                          ""target"": ""DeeRules/ProcessRules/EntityTypes/"",
                          ""contentType"": ""EntityTypes""
                        },
                        {
                          ""source"": ""DEEs/ProcessRules/Before/*"",
                          ""target"": ""DeeRules/ProcessRules/Before"",
                          ""contentType"": ""ProcessRulesPre""
                        },
                        {
                          ""source"": ""MasterData/*"",
                          ""target"": ""MasterData/"",
                          ""contentType"": ""MasterData""
                        },
                        {
                          ""source"": ""DEEs/ProcessRules/After/*"",
                          ""target"": ""DeeRules/ProcessRules/After"",
                          ""contentType"": ""ProcessRulesPost""
                        },
                        {
                          ""source"": ""MasterData/Framework/$(version)/*"",
                          ""target"": ""MasterData/$(version)/"",
                          ""contentType"": ""MasterData""
                        },
                        {
                          ""source"": ""AutomationWorkFlows/*"",
                          ""target"": ""AutomationWorkFlows"",
                          ""contentType"": ""AutomationWorkFlows""
                        }
                      ]
                    }"
                )
            },
            {
                "/AutomationWorkFlows/workflow.json",
                new MockFileData(
                    @"{
                      ""tasks"": [
                        {
                          ""id"": ""task_70864"",
                          ""reference"": {
                            ""name"": ""subWorkflowStart"",
                            ""package"": {
                              ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                              ""version"": ""11.1.5""
                            }
                          },
                          ""settings"": {
                            ""outputs"": [],
                            ""___cmf___name"": ""Start""
                          }
                        },
                        {
                          ""id"": ""task_70897"",
                          ""reference"": {
                            ""name"": ""subWorkflowEnd"",
                            ""package"": {
                              ""name"": ""@criticalmanufacturing/ignore-this-package"",
                              ""version"": ""11.1.5""
                            }
                          },
                          ""settings"": {
                            ""inputs"": [],
                            ""___cmf___name"": ""End""
                          }
                        },
                        {
                          ""id"": ""task_390"",
                          ""reference"": {
                            ""name"": ""workflow"",
                            ""package"": {
                              ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                              ""version"": ""11.1.5""
                            }
                          },
                          ""settings"": {}
                        }
                      ],
                      ""converters"": [
                        {
                          ""id"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks#anyToConstant"",
                          ""reference"": {
                            ""name"": ""anyToConstant"",
                            ""package"": {
                              ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                              ""version"": ""11.1.5""
                            }
                          }
                        },
                        {
                          ""id"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks#anyToConstant"",
                          ""reference"": {
                            ""name"": ""anyToConstant"",
                            ""package"": {
                              ""name"": ""@criticalmanufacturing/ignore-this-package"",
                              ""version"": ""1.2.3""
                            }
                          }
                        }
                      ],
                      ""links"": [
                        {
                          ""id"": ""8e3dc544-2cf8-4387-80a5-aae7b8f6c654"",
                          ""sourceId"": ""task_33039"",
                          ""targetId"": ""task_70897"",
                          ""inputName"": ""error"",
                          ""outputName"": ""error""
                        }
                      ],
                      ""layout"": {
                        ""general"": {
                          ""color"": null,
                          ""notes"": []
                        },
                        ""drawers"": {
                          ""DIAGRAM"": {
                            ""tasks"": {
                              ""task_70897"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 1631,
                                  ""y"": 105
                                },
                                ""outdated"": false
                              }
                            }
                          }
                        }
                      }
                    }"
                )
            },
            {
                "/abc/cmfpackage.json",
                new MockFileData(
                    @"{
                      ""packageId"": ""Root"",
                      ""version"": ""3.2.0"",
                      ""description"": ""Data Package"",
                      ""packageType"": ""Data"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true
                    }"
                )
            },
            {
                "/DEEs/a.b.c.csproj",
                new MockFileData(
                    @"
                    <Project Sdk=""Microsoft.NET.Sdk"">
                    	<ItemGroup>
		                    <PackageReference Include=""Cmf.MessageBus.Client"" Version=""11.1.5"" />
                    	</ItemGroup>
                    </Project>
                    "
                )
            },
            {
                "/MasterData/a.json",
                new MockFileData(
                    @"{
                      ""<DM>AutomationProtocol"": {
                        ""1"": {
                          ""Name"": ""TCPIP_Protocol"",
                          ""Description"": ""TCPIP_Protocol"",
                          ""IsTemplate"": ""No"",
                          ""Type"": ""General"",
                          ""Package"": ""@criticalmanufacturing/connect-iot-driver-tcpip"",
                          ""PackageVersion"": ""11.0.2"",
                          ""ExtendedDataDefinition"": """",
                          ""HasCommands"": ""Yes"",
                          ""HasEvents"": ""Yes"",
                          ""HasProperties"": ""Yes"",
                          ""EntityPicture"": """"
                        }
                      },
                      ""<DM>AutomationManager"": {
                        ""1"": {
                          ""Name"": ""ASYS_Manager"",
                          ""IsTemplate"": ""No"",
                          ""Description"": """",
                          ""Type"": ""General"",
                          ""LogicalAddress"": ""ASYS_Manager"",
                          ""MonitorPackageVersion"": ""11.1.3"",
                          ""ManagerPackageVersion"": ""11.1.5"",
                          ""Configuration"": """"
                        }
                      },
                      ""<DM>AutomationController"": {
                        ""1"": {
                          ""Name"": ""ASYS_Controller"",
                          ""Description"": """",
                          ""IsTemplate"": ""No"",
                          ""Type"": ""General"",
                          ""ControllerPackageVersion"": ""11.1.5"",
                          ""ObjectType"": ""Resource"",
                          ""TasksPackages"": ""[\""connect-iot-controller-engine-core-tasks\"",\""connect-iot-controller-engine-custom-utilities-tasks\""]"",
                          ""Scope"": ""ConnectIoT"",
                          ""AutomaticCheckpoints"": """",
                          ""Timeout"": """",
                          ""EntityPicture"": """",
                          ""LinkConnector"": ""Rounded"",
                          ""LinkRouter"": ""Metro"",
                          ""TasksLibraryPackages"": ""[\""@criticalmanufacturing/connect-iot-controller-engine-core-tasks@11.1.5\""]"",
                          ""DefaultWorkflowType"": ""DataFlow""
                        },
                        ""2"": {
                          ""Name"": ""Test_Controller"",
                          ""Description"": """",
                          ""IsTemplate"": ""No"",
                          ""Type"": ""General"",
                          ""ControllerPackageVersion"": ""11.1.5"",
                          ""ObjectType"": ""Resource"",
                          ""TasksPackages"": ""[]"",
                          ""Scope"": ""ConnectIoT"",
                          ""AutomaticCheckpoints"": """",
                          ""Timeout"": """",
                          ""EntityPicture"": """",
                          ""LinkConnector"": ""Rounded"",
                          ""LinkRouter"": ""Metro"",
                          ""TasksLibraryPackages"": [
                            ""@criticalmanufacturing/ignore-this-package@11.1.5"",
                            ""@criticalmanufacturing/connect-iot-random@11.1.5"",
                            ""@criticalmanufacturing/connect-iot-controller-engine-custom-utilities-tasks@5.4.3"",
                            ""@criticalmanufacturing/connect-iot-controller-engine-custom-smt-utilities-tasks@5.4.3"",
                            ""@criticalmanufacturing/connect-iot-utilities-semi-tasks@5.4.3""
                            ],
                          ""DefaultWorkflowType"": ""DataFlow""
                        },
                        ""3"": {
                          ""Name"": ""Test_Controller"",
                          ""Description"": """",
                          ""IsTemplate"": ""No"",
                          ""Type"": ""General"",
                          ""ObjectType"": ""Resource"",
                          ""TasksPackages"": ""[]"",
                          ""Scope"": ""ConnectIoT"",
                          ""AutomaticCheckpoints"": """",
                          ""Timeout"": """",
                          ""EntityPicture"": """",
                          ""LinkConnector"": ""Rounded"",
                          ""LinkRouter"": ""Metro"",
                          ""DefaultWorkflowType"": ""DataFlow""
                        },
                        ""4"": {
                          ""Name"": ""Test_Controller"",
                          ""Description"": """",
                          ""IsTemplate"": ""No"",
                          ""Type"": ""General"",
                          ""ObjectType"": ""Resource"",
                          ""TasksPackages"": ""[]"",
                          ""Scope"": ""ConnectIoT"",
                          ""AutomaticCheckpoints"": """",
                          ""Timeout"": """",
                          ""EntityPicture"": """",
                          ""LinkConnector"": ""Rounded"",
                          ""LinkRouter"": ""Metro"",
                          ""TasksLibraryPackages"": """",
                          ""DefaultWorkflowType"": ""DataFlow""
                        },
                      }
                    }"
                )
            },
            {
                "/MasterData/a.excel",
                new MockFileData("")
            }
        });

        UpgradeBaseCommand cmd = new UpgradeBaseCommand(fileSystem);
        cmd.Execute(fileSystem.DirectoryInfo.New(@"/"), version, iotShouldBeUpdated ? iotVersion : null, ["ignore-this-package"]);

        string csprojContents = fileSystem.File.ReadAllText(@"/DEEs/a.b.c.csproj");
        csprojContents.Should().Contain(
            $@"<PackageReference Include=""Cmf.MessageBus.Client"" Version=""{version}"" />"
        );

        if (iotShouldBeUpdated)
        {
            #region Masterdata validations
            string mdlContents = fileSystem.File.ReadAllText(@"/MasterData/a.json");
            mdlContents.Should().ContainAll([
                $@"""PackageVersion"": ""{iotVersion}""",
                $@"""MonitorPackageVersion"": ""{iotVersion}""",
                $@"""ManagerPackageVersion"": ""{iotVersion}""",
                $@"""ControllerPackageVersion"": ""{iotVersion}""",
                $@"@criticalmanufacturing/connect-iot-controller-engine-core-tasks@{iotVersion}",
                $@"@criticalmanufacturing/connect-iot-random@{iotVersion}",
                $@"@criticalmanufacturing/connect-iot-controller-engine-custom-utilities-tasks@5.4.3", // The industry templates iot packages should remain unchanged
                $@"@criticalmanufacturing/connect-iot-controller-engine-custom-smt-utilities-tasks@5.4.3",
                $@"@criticalmanufacturing/connect-iot-utilities-semi-tasks@5.4.3",
            ]);
            Assert.Equal(7, Regex.Matches(mdlContents, iotVersion.Replace(".", "\\.")).Count);
            #endregion IoT Masterdata validations

            #region Workflow validations
            string wflContents = fileSystem.File.ReadAllText(@"/AutomationWorkFlows/workflow.json");
            JObject workflowJsonObject = (JObject)JsonConvert.DeserializeObject(wflContents);

            // Tasks
            workflowJsonObject["tasks"][0]["reference"]["package"]["version"].ToString().Should().Be(iotVersion);
            workflowJsonObject["tasks"][1]["reference"]["package"]["version"].ToString().Should().Be("11.1.5");
            workflowJsonObject["tasks"][2]["reference"]["package"]["version"].ToString().Should().Be(iotVersion);

            // Converters
            workflowJsonObject["converters"][0]["reference"]["package"]["version"].ToString().Should().Be(iotVersion);
            workflowJsonObject["converters"][1]["reference"]["package"]["version"].ToString().Should().Be("1.2.3");

            Assert.Equal(3, Regex.Matches(wflContents, iotVersion.Replace(".", "\\.")).Count);
            Assert.Single(Regex.Matches(wflContents, "11.1.5".Replace(".", "\\."))); // Ignored task
            Assert.Single(Regex.Matches(wflContents, "1.2.3".Replace(".", "\\."))); // Ignored converter
            #endregion IoT Workflow validations
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void JSONSerialization(int jsonVariation)
    {
        string jsonPath = "/test.json";
        List<string> jsonFileVariations = [
            "{\n\"a\": []\n}",      // 0 spaces
            "{\n \"a\": []\n}",     // 1 space
            "{\n  \"a\": []\n}",    // 2 spaces
            "{\n   \"a\": []\n}",   // 3 spaces
            "{\n    \"a\": []\n}",  // 4 spaces
            "{\n     \"a\": []\n}", // 5 spaces
            "{\n\t\"a\": []\n}",    // 1 tab
            "{\n\t\t\"a\": []\n}",  // 2 tabs
            "{\"a\": []}",          // JSON in a single line
        ];

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                jsonPath,
                new MockFileData(jsonFileVariations[jsonVariation])
            }
        });

        UpgradeBaseUtilities.SerializeWithOriginalIndentation(
            jsonPath,
            jsonFileVariations[jsonVariation],
            (JObject)JsonConvert.DeserializeObject(jsonFileVariations[jsonVariation]),
            fileSystem
        );

        List<string> jsonContents = fileSystem.File.ReadAllText(jsonPath).Split("\n").ToList();

        if (jsonVariation <= 2 || jsonVariation == 8) // 0 1 2 8
        {
            jsonContents[1].Should().Be("  \"a\": []");
        }
        else if (jsonVariation <= 5) // 3 4 5
        {
            jsonContents[1].Should().Be("    \"a\": []");
        }
        else // 6 7
        {
            jsonContents[1].Should().Be("\t\"a\": []");
        }
    }

    [Fact]
    public void UpdateNPMProject()
    {
        string cmfPackagePath = "/cmfpackage.json";

        string npmPackagePath     = "/abc/package.json";
        string npmPackagelockPath = "/package-lock.json";

        string distPackagePath     = "/dist/package.json";
        string distPackagelockPath = "/dist/package-lock.json";

        string nodeModulesPackagePath     = "/node_modules/package.json";
        string nodeModulesPackagelockPath = "/node_modules/package-lock.json";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                cmfPackagePath,
                new MockFileData(@"{
                  ""packageId"": ""Cmf.SMT.Html"",
                  ""version"": ""3.2.0"",
                  ""description"": ""HTML Package"",
                  ""packageType"": ""HTML"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""steps"": [],
                  ""contentToPack"": [
                    {
                      ""source"": ""dist/cmf-smt-html/**"",
                      ""target"": """"
                    }
                  ]
                }")
            },
            {
                npmPackagePath,
                new MockFileData(@"{
                  ""name"": ""cmf-smt-html"",
                  ""version"": ""3.2.0"",
                  ""scripts"": {
                    ""ng"": ""ng"",
                    ""start"": ""ng serve"",
                  },
                  ""private"": true,
                  ""dependencies"": {
                    ""@angular/service-worker"": ""^17.3.0"",
                    ""cmf-mes-ui"": ""release-1115"",
                    ""fast-xml-parser"": ""^4.3.6""
                  },
                  ""devDependencies"": {
                    ""@angular/compiler-cli"": ""^17.3.0"",
                    ""@criticalmanufacturing/ngx-schematics"": ""release-1115"",
                    ""@types/jasmine"": ""~5.1.0""
                  }
                }")
            },
            {
                npmPackagelockPath,
                @""
            },
            {
                distPackagePath,
                new MockFileData(@"{
                  ""devDependencies"": {
                    ""@criticalmanufacturing/ngx-schematics"": ""release-1115"",
                  }
                }")
            },
            {
                distPackagelockPath,
                @""
            },
            {
                nodeModulesPackagePath,
                new MockFileData(@"{
                  ""devDependencies"": {
                    ""@criticalmanufacturing/ngx-schematics"": ""release-1115"",
                  }
                }")
            },
            {
                nodeModulesPackagelockPath,
                @""
            },
        });

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IProjectConfigService>(new ProjectConfigService())
            .BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);

        UpgradeBaseUtilities.UpdateNPMProject(fileSystem, new CmfPackage(fileSystem.FileInfo.New(cmfPackagePath)), "11.1.6");

        fileSystem.FileInfo.New(npmPackagelockPath).Exists.Should().BeFalse();
        fileSystem.FileInfo.New(distPackagelockPath).Exists.Should().BeTrue();
        fileSystem.FileInfo.New(nodeModulesPackagelockPath).Exists.Should().BeTrue();

        fileSystem.File.ReadAllText(distPackagePath).Should().Contain(@"""@criticalmanufacturing/ngx-schematics"": ""release-1115""");
        fileSystem.File.ReadAllText(nodeModulesPackagePath).Should().Contain(@"""@criticalmanufacturing/ngx-schematics"": ""release-1115""");

        fileSystem.File.ReadAllText(npmPackagePath).Should().NotContain("release-1115");
        fileSystem.File.ReadAllText(npmPackagePath).Should().ContainAll([
            @"""@criticalmanufacturing/ngx-schematics"": ""release-1116""",
            @"""cmf-mes-ui"": ""release-1116""",
        ]);
        Assert.Equal(2, Regex.Matches(fileSystem.File.ReadAllText(npmPackagePath), @"release-1116").Count);
    }

    [Fact]
    public void TestPackage()
    {
        string version = "11.1.6";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                "/.project-config.json",
                new MockFileData(
                    @"{
                          ""MESVersion"": ""11.1.5""
                    }"
                )
            },
            {
                "/cmfpackage.json",
                new MockFileData(
                    @"{
                      ""packageId"": ""Cmf.SMT.Tests"",
                      ""version"": ""3.2.0"",
                      ""description"": ""Tests Package"",
                      ""packageType"": ""Tests"",
                      ""isInstallable"": false,
                      ""isUniqueInstall"": false,
                      ""contentToPack"": [
                        {
                          ""source"": ""Release/**"",
                          ""target"": """"
                        },
                        {
                          ""source"": ""../Libs/Tests/*.dll"",
                          ""target"": """"
                        }
                      ]
                    }"
                )
            },
            {
                "/Common/a.b.c.csproj",
                new MockFileData(
                    @"
                        <?xml version=""1.0"" encoding=""utf-8""?>
                        <Project Sdk=""Microsoft.NET.Sdk"">
                          <ItemGroup>
                            <ProjectReference Include=""..\Cmf.Custom.Tests.Biz.Common\Cmf.Custom.Tests.Biz.Common.csproj"" />
                          </ItemGroup>
                          <ItemGroup>
                            <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""16.9.4"" />
                            <PackageReference Include=""MSTest.TestAdapter"" Version=""2.2.3"" />
                            <PackageReference Include=""MSTest.TestFramework"" Version=""2.2.3"" />
                          </ItemGroup>
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


        UpgradeBaseCommand cmd = new UpgradeBaseCommand(fileSystem);
        cmd.Execute(fileSystem.DirectoryInfo.New("/"), version, null, []);

        string csprojContents = fileSystem.File.ReadAllText("/Common/a.b.c.csproj");
        csprojContents.Should().ContainAll([
            $@"<PackageReference Include=""Cmf.Common.TestUtilities"" Version=""2.3.157590"" />", // Shouldn't be changed
            $@"<PackageReference Include=""Cmf.Common.TestFramework.ConnectIoT"" Version=""1.0.131717"" />", // Shouldn't be changed
            $@"<PackageReference Include=""Cmf.Dev.Mes.TestScenarios"" Version=""{version}"" />",
        ]);
        Assert.Single(Regex.Matches(csprojContents, version.Replace(".", "\\.")));
    }
}
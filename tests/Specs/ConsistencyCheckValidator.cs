using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Handlers;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.CommandLine.IO;
using tests.Objects;

namespace tests.Specs
{
    [TestClass]
    public class ConsistencyCheckValidator
    {
        [TestMethod]
        public void ConsistencyCheckValidator_HappyPath()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new MockFileData(
                    @"{
                        ""packageId"": ""Cmf.Custom.Package"",
                        ""version"": ""1.1.0"",
                        ""description"": ""This package deploys Critical Manufacturing Customization"",
                        ""packageType"": ""Root"",
                        ""isInstallable"": true,
                        ""isUniqueInstall"": false,
                        ""dependencies"": [
                            {
                                ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                                ""version"": ""8.3.0""
                            },
                            {
                                ""id"": ""Cmf.Custom.Data"",
                                ""version"": ""1.1.0""
                            },
                            {
                                ""id"": ""Cmf.Custom.IoT"",
                                ""version"": ""1.1.0""
                            }
                        ]
                    }")
                },
                { "/test/Data/cmfpackage.json", new CmfMockJsonData(
                    @"{
                      ""packageId"": ""Cmf.Custom.Data"",
                      ""version"": ""1.1.0"",
                      ""description"": ""Cmf Custom Data Package"",
                      ""packageType"": ""IoTData"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""contentToPack"": [
                        {
                            ""source"": ""MasterData/$(version)/*"",
                            ""target"": ""MasterData/$(version)/"",
                            ""contentType"": ""MasterData""
                        }
                      ]
                    }")
                },
                { "/test/IoT/cmfpackage.json", new CmfMockJsonData(
                    @"{
                      ""packageId"": ""Cmf.Custom.IoT"",
                      ""version"": ""1.1.0"",
                      ""description"": ""Cmf Custom Data Package"",
                      ""packageType"": ""IoT"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""contentToPack"": [
                        {
                            ""source"": ""src/*"",
                            ""target"": ""node_modules"",
                            ""ignoreFiles"": ["".npmignore""]
                        }
                      ]
                    }")
                }
            });
            ExecutionContext.Initialize(fileSystem);
            BuildCommand buildCommand = new BuildCommand(fileSystem.FileSystem);

            Command cmd = new Command("build");
            buildCommand.Configure(cmd);

            TestConsole console = new TestConsole();
            cmd.Invoke(new string[] {
                "test/Data/"
            }, console);

            Assert.IsTrue(console.Error == null || string.IsNullOrEmpty(console.Error.ToString()), $"Consistency Check failed {console.Error.ToString()}");

        }
        [TestMethod]
        public void ConsistencyCheckValidator_FailData()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new MockFileData(
                    @"{
                        ""packageId"": ""Cmf.Custom.Package"",
                        ""version"": ""1.1.0"",
                        ""description"": ""This package deploys Critical Manufacturing Customization"",
                        ""packageType"": ""Root"",
                        ""isInstallable"": true,
                        ""isUniqueInstall"": false,
                        ""dependencies"": [
                            {
                                ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                                ""version"": ""8.3.0""
                            },
                            {
                                ""id"": ""Cmf.Custom.Data"",
                                ""version"": ""1.2.0""
                            },
                            {
                                ""id"": ""Cmf.Custom.IoT"",
                                ""version"": ""1.1.0""
                            }
                        ]
                    }")
                },
                { "/test/Data/cmfpackage.json", new CmfMockJsonData(
                    @"{
                      ""packageId"": ""Cmf.Custom.Data"",
                      ""version"": ""1.2.0"",
                      ""description"": ""Cmf Custom Data Package"",
                      ""packageType"": ""IoTData"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""contentToPack"": [
                        {
                            ""source"": ""MasterData/$(version)/*"",
                            ""target"": ""MasterData/$(version)/"",
                            ""contentType"": ""MasterData""
                        }
                      ]
                    }")
                },
                { "/test/IoT/cmfpackage.json", new CmfMockJsonData(
                    @"{
                      ""packageId"": ""Cmf.Custom.IoT"",
                      ""version"": ""1.1.0"",
                      ""description"": ""Cmf Custom Data Package"",
                      ""packageType"": ""IoT"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""contentToPack"": [
                        {
                            ""source"": ""src/*"",
                            ""target"": ""node_modules"",
                            ""ignoreFiles"": ["".npmignore""]
                        }
                      ]
                    }")
                }
            });
            ExecutionContext.Initialize(fileSystem);
            BuildCommand buildCommand = new BuildCommand(fileSystem.FileSystem);

            Command cmd = new Command("build");
            buildCommand.Configure(cmd);

            TestConsole console = new TestConsole();
            cmd.Invoke(new string[] {
                "test/Data/"
            }, console);

            Assert.IsTrue(console.Error != null && console.Error.ToString().Contains("This root package dependencies must enforce version consistency. Root Version 1.1.0 Failed Package Version 1.2.0"), $"Consistency Check failed {console.Error.ToString()}");

        }
    }
}

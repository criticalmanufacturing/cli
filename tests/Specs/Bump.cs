using Cmf.Common.Cli.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO.Abstractions.TestingHelpers;
using tests.Objects;

namespace tests
{
    [TestClass]
    public class Bump
    {
        [TestMethod]
        public void Data_JsonValidator_HappyPath()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
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
                      ""packageType"": ""Data"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""contentToPack"": [
                        {
                            ""source"": ""MasterData/$(version)/*"",
                            ""target"": ""MasterData/$(version)/"",
                            ""contentType"": ""MasterData""
                        }
                      ],
                      ""xmlInjection"": [""../../DeploymentMetadata/ui.xml""]
                    }")
                },
                { "/test/Data/MasterData/1.1.0/Test.json", new CmfMockJsonData(
                    @"{
                        ""<SM>Config"": {
                            ""1"": {
                                ""ParentPath"": ""/SMT/BlockCheckListOnFalseParameters/"",
                                ""Name"": ""IsEnabled"",
                                ""Value"": ""Yes"",
                                ""ValueType"": ""String""
                            }
                        }
                    }")
                }
            });

            BuildCommand buildCommand = new BuildCommand(fileSystem.FileSystem);

            var cmd = new Command("build");
            buildCommand.Configure(cmd);

            var console = new TestConsole();
            cmd.Invoke(new string[] {
                "test/Data/"
            }, console);

            Assert.IsTrue(console.Error == null || string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed {console.Error.ToString()}");
        }

        [TestMethod]
        public void Data_JsonValidator_FailData()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new CmfMockJsonData(
                    @"{
                        ""packageId"": ""Cmf.Custom.Package"",
                        ""version"": ""1.1.0"",
                        ""description"": ""This package deploys Critical Manufacturing Customization"",
                        ""packageType"": ""Root"",
                        ""isInstallable"": true,
                        ""isUniqueInstall"": false,
                        ""dependencies"": [
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
                      ""packageType"": ""Data"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": false,
                      ""contentToPack"": [
                        {
                            {
                              ""source"": ""MasterData/$(version)/*"",
                              ""target"": ""MasterData /$(version)/"",
                                ""contentType"": ""MasterData""
                            },
                      ],
                      ""xmlInjection"": [""../../DeploymentMetadata/ui.xml""]
                    }")
                },
                { "/test/Data/MasterData/1.1.0/Test.json", new CmfMockJsonData(
                    @"{
                        ""<SM>Config"": {
                            ""1"": {
                                ""ParentPath"": ""/SMT/BlockCheckListOnFalseParameters/"",
                                ""Name"": ""IsEnabled"",
                                ""Value"": ""Yes"",
                                ""ValueType"": ""String""
                    }")
                }
            });

            BuildCommand buildCommand = new BuildCommand(fileSystem.FileSystem);

            var cmd = new Command("build");
            buildCommand.Configure(cmd);

            var console = new TestConsole();
            cmd.Invoke(new string[] {
                "test/Data/"
            }, console);

            Assert.IsTrue(console.Error != null && !string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed for Data Package: {console.Error.ToString()}");
        }

        [TestMethod]
        public void IoTData_JsonValidator_FailData()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new CmfMockJsonData(
                    @"{
                        ""packageId"": ""Cmf.Custom.Package"",
                        ""version"": ""1.1.0"",
                        ""description"": ""This package deploys Critical Manufacturing Customization"",
                        ""packageType"": ""Root"",
                        ""isInstallable"": true,
                        ""isUniqueInstall"": false,
                        ""dependencies"": [
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
                { "/test/IoT/cmfpackage.json", new CmfMockJsonData(
                    @"{
                      ""packageId"": ""Cmf.Custom.IoT"",
                      ""version"": ""1.1.0"",
                      ""description"": ""Cmf Custom Foxconn IoT Package"",
                      ""packageType"": ""Root"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": false,
                      ""dependencies"": [
	                    {
                          ""id"": ""Cmf.Custom.IoTData"",
                          ""version"": ""1.1.0""
                        }
                      ]
                      ""xmlInjection"": [""../DeploymentMetadata/ui.xml""]
                    }")
                },
                { "/test/IoTData/cmfpackage.json", new CmfMockJsonData(
                    @"{
                      ""packageId"": ""Cmf.Custom.IoTData"",
                      ""version"": ""1.1.0"",
                      ""description"": ""Cmf Custom Foxconn IoTData Package"",
                      ""packageType"": ""IoTData"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": false,
                      ""contentToPack"": [
                        {
                            {
                              ""source"": ""MasterData/$(version)/*"",
                              ""target"": ""MasterData /$(version)/"",
                                ""contentType"": ""MasterData""
                            },
                      ],
                      ""xmlInjection"": [""../DeploymentMetadata/ui.xml""]
                    }")
                },
                { "/test/IoTData/MasterData/1.0.0IoT.json", new CmfMockJsonData(
                    @"{
                        ""<DM>AutomationProtocol"": {
                        ""1"": {
                            ""Name"": ""TEST_Protocol"",
                            ""Description"": ""TEST_Protocol"",
                            ""Type"":"" ""General"",
                            ""Package"": ""@criticalmanufacturing/connect-iot-driver-test"",
                            ""PackageVersion"": ""test""
                }")
                },
                { "/test/IoTData/AutomationWorkflowFiles/MasterData/1.0.0/TestWorkflowIoT.json", new CmfMockJsonData(
                    @"{
	                    ""tasks"": [
                            {
                            }
	                    ],
                        ""converters"": [
                            {
                            }
                        ],
	                    ""links"": [
                            {
                            }
                        ],
	                    ""layout"": {
                            ""general"": {
                            },
		                    ""drawers"": {
                            }
                        }
                    }")
                }
            });

            BuildCommand buildCommand = new BuildCommand(fileSystem.FileSystem);

            var cmd = new Command("build");
            buildCommand.Configure(cmd);

            var console = new TestConsole();
            cmd.Invoke(new string[] {
                "test/Data/"
            }, console);

            Assert.IsTrue(console.Error != null && !string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed for IoT Data Package: {console.Error.ToString()}");
        }

        [TestMethod]
        public void IoTData_Workflow_JsonValidator_FailData()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new CmfMockJsonData(
                    @"{
                        ""packageId"": ""Cmf.Custom.Package"",
                        ""version"": ""1.1.0"",
                        ""description"": ""This package deploys Critical Manufacturing Customization"",
                        ""packageType"": ""Root"",
                        ""isInstallable"": true,
                        ""isUniqueInstall"": false,
                        ""dependencies"": [
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
                { "/test/IoT/cmfpackage.json", new CmfMockJsonData(
                    @"{
                      ""packageId"": ""Cmf.Custom.IoT"",
                      ""version"": ""1.1.0"",
                      ""description"": ""Cmf Custom Foxconn IoT Package"",
                      ""packageType"": ""Root"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": false,
                      ""dependencies"": [
	                    {
                          ""id"": ""Cmf.Custom.IoTData"",
                          ""version"": ""1.1.0""
                        }
                      ]
                      ""xmlInjection"": [""../DeploymentMetadata/ui.xml""]
                    }")
                },
                { "/test/IoTData/cmfpackage.json", new CmfMockJsonData(
                    @"{
                      ""packageId"": ""Cmf.Custom.IoTData"",
                      ""version"": ""1.1.0"",
                      ""description"": ""Cmf Custom Foxconn IoTData Package"",
                      ""packageType"": ""IoTData"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": false,
                      ""contentToPack"": [
                        {
                            {
                              ""source"": ""MasterData/$(version)/*"",
                              ""target"": ""MasterData /$(version)/"",
                                ""contentType"": ""MasterData""
                            },
                      ],
                      ""xmlInjection"": [""../DeploymentMetadata/ui.xml""]
                    }")
                },
                { "/test/IoTData/MasterData/1.0.0IoT.json", new CmfMockJsonData(
                    @"{
                        ""<DM>AutomationProtocol"": {
                        ""1"": {
                            ""Name"": ""TEST_Protocol"",
                            ""Description"": ""TEST_Protocol"",
                            ""Type"":"" ""General"",
                            ""Package"": ""@criticalmanufacturing/connect-iot-driver-test"",
                            ""PackageVersion"": ""test""
                        }
                    }
                }")
                },
                { "/test/IoTData/AutomationWorkflowFiles/MasterData/1.0.0/AutomationWorkflowFiles/TestWorkflowIoT.json", new CmfMockJsonData(
                    @"{
	                    ""tasks"": [
                            {
                            }
	                    ],
                        ""converters"": [
                            {
                            }
                        ],
	                    ""links"": [
                            {
                            }
                        ],
	                    ""layout"": {
                            ""general"": {
                            },
		                    ""drawers"": {
                    }")
                }
            });

            BuildCommand buildCommand = new BuildCommand(fileSystem.FileSystem);

            var cmd = new Command("build");
            buildCommand.Configure(cmd);

            var console = new TestConsole();
            cmd.Invoke(new string[] {
                "test/Data/"
            }, console);

            Assert.IsTrue(console.Error != null && !string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed for IoT Data Workflow Package: {console.Error.ToString()}");
        }
    }
}
using Cmf.CLI.Commands;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO.Abstractions.TestingHelpers;
using tests.Objects;
using Xunit;

namespace tests.Specs
{
    public class JsonValidator
    {
        [Fact]
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
                      ]
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

            Assert.True(console.Error == null || string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed {console.Error.ToString()}");

        }

        [Fact]
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
                      ]
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

            Assert.True(console.Error != null && !string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed for Data Package: {console.Error.ToString()}");
        }

        [Fact]
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
                      ]
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

            Assert.True(console.Error != null && !string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed for IoT Data Package: {console.Error.ToString()}");
        }

        [Fact]
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
                      ]
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

            Assert.True(!string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed for IoT Data Workflow Package: {console.Error.ToString()}");
        }

        [Fact]
        public void Data_JsonValidator_HappyPath_Workflow()
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
                      ]
                    }")
                },
                { "/test/Data/MasterData/1.1.0/Test.json", new MockFileData(
                    @"{
                        ""AutomationControllerWorkflow"": {
                            ""1"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test"",
                                ""DisplayName"": ""Test"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""Test/test.json"",
                                ""Order"": ""1""
                            },
                            ""2"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test2"",
                                ""DisplayName"": ""Test2"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""Test2/Test2.json"",
                                ""Order"": ""2""
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

            Assert.True(console.Error == null || string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed {console.Error.ToString()}");
        }

        [Fact]
        public void Data_JsonValidator_Fail_BackSlash()
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
                      ]
                    }")
                },
                { "/test/Data/MasterData/1.1.0/Test.json", new MockFileData(
                    @"{
                        ""AutomationControllerWorkflow"": {
                            ""1"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test"",
                                ""DisplayName"": ""Test"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""Test/test.json"",
                                ""Order"": ""1""
                            },
                            ""2"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""TestFail"",
                                ""DisplayName"": ""TestFail"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""TestFail\\testfail.json"",
                                ""Order"": ""2""
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

            Assert.True(!string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator did not fail for IoT Data Workflow Package: {console.Error.ToString()}");
            Assert.True(console.Error.ToString().Contains("Please normalize all slashes to be forward slashes"), $"Json Validator did not fail for IoT Data Workflow Package: {console.Error.ToString()}");
        }

        [Fact]
        public void Data_JsonValidator_HappyPath_SubWorkflow()
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
                        },
                        {
                          ""source"": ""AutomationWorkFlows/*"",
                          ""target"": ""AutomationWorkFlows"",
                          ""contentType"": ""AutomationWorkFlows""
                        }
                      ]
                    }")
                },
                { "/test/Data/MasterData/1.1.0/Test.json", new MockFileData(
                    @"{
                        ""AutomationControllerWorkflow"": {
                            ""1"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test"",
                                ""DisplayName"": ""Test"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""Test/test.json"",
                                ""Order"": ""1""
                            },
                            ""2"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test2"",
                                ""DisplayName"": ""Test2"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""Test2/Test2.json"",
                                ""Order"": ""2""
                            }
                        }
                    }")
                },
                { "/test/Data/AutomationWorkflows/Test/Test.json", new MockFileData(
                    @"{
                        ""tasks"": [
                          {
                            ""id"": ""task_356"",
                            ""reference"": {
                              ""name"": ""driverEvent"",
                              ""package"": {
                                ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                                ""version"": ""11.1.0-202409173""
                              }
                            },
                            ""driver"": ""Handler"",
                            ""settings"": {
                              ""autoEnable"": true,
                              ""event"": ""OnInitialize"",
                              ""autoSetup"": true,
                              ""___cmf___description"": ""OnInitialize""
                            }
                          },
                          {
                            ""id"": ""task_357"",
                            ""reference"": {
                              ""name"": ""equipmentConfig"",
                              ""package"": {
                                ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                                ""version"": ""11.1.0-202409173""
                              }
                            },
                            ""driver"": ""Handler"",
                            ""settings"": {
                              ""_inputs"": [
                                {
                                  ""name"": ""path"",
                                  ""label"": ""Path"",
                                  ""defaultValue"": ""c:/temp"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""path""
                                  },
                                  ""dataType"": ""String"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 0,
                                  ""description"": ""Path of the location to monitor for changes on the files. If no value is provided, no watcher is created, so no events will be triggered."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""fileMask"",
                                  ""label"": ""FileMask"",
                                  ""defaultValue"": ""**/*.request"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""fileMask""
                                  },
                                  ""dataType"": ""String"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 0,
                                  ""description"": ""Glob pattern to use for the watcher to identify the files to handle. Use a tool like https://globster.xyz/ to try a valid value to use."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""archivePath"",
                                  ""label"": ""Archive Path"",
                                  ""defaultValue"": """",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""archivePath""
                                  },
                                  ""dataType"": ""String"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 0,
                                  ""description"": ""Directory planned to use for archiving the files after processing. This value can later be used as a variable in the available operations to execute."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""watcherType"",
                                  ""label"": ""Watcher Type"",
                                  ""defaultValue"": ""Chokidar"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""watcherType""
                                  },
                                  ""dataType"": ""Enum"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 1,
                                  ""description"": ""Type of watcher to use. Depending on the selection, different settings are necessary. Chokidar is the best overall option, but CPU and Memory heavy and slower to start when many files are present. NSFW is better other scenarios. Test both to determine the best option."",
                                  ""valueReferenceType"": 6,
                                  ""settings"": {
                                    ""enumValues"": [
                                      ""Chokidar"",
                                      ""NSFW""
                                    ]
                                  },
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""ignoreInitial"",
                                  ""label"": ""Ignore Existing Files"",
                                  ""defaultValue"": ""False"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""ignoreInitial""
                                  },
                                  ""dataType"": ""Boolean"",
                                  ""automationDataType"": 8,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Flag to define if, during startup/restart, the files that were created, changed, deleted should be processed or ignored"",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""watcherMode"",
                                  ""label"": ""File Watcher Mode"",
                                  ""defaultValue"": ""Polling"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""watcherMode""
                                  },
                                  ""dataType"": ""Enum"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 1,
                                  ""description"": ""(Type=Chokidar) To successfully watch files over a network (and in other non-standard situations), it is typically necessary to use Polling, however it could lead to high CPU utilization. FileSystemEvents is the most efficient method for monitoring local files."",
                                  ""valueReferenceType"": 6,
                                  ""settings"": {
                                    ""enumValues"": [
                                      ""FileSystemEvents"",
                                      ""Polling""
                                    ]
                                  },
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""pollingInterval"",
                                  ""label"": ""Polling Interval (ms)"",
                                  ""defaultValue"": ""100"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""pollingInterval""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Interval of file system polling (in milliseconds)"",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""pollingBinaryInterval"",
                                  ""label"": ""Binary Files Polling Interval (ms)"",
                                  ""defaultValue"": ""300"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""pollingBinaryInterval""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar) Interval of file system polling for binary files (in milliseconds)"",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""alwaysStat"",
                                  ""label"": ""Always Stat"",
                                  ""defaultValue"": ""True"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""alwaysStat""
                                  },
                                  ""dataType"": ""Boolean"",
                                  ""automationDataType"": 8,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Always get additional attributes (size, timestamps, etc) of the file that was identified by the watcher. Will require additional operating system resources."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""depth"",
                                  ""label"": ""Subdirectory Depth"",
                                  ""defaultValue"": ""0"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""depth""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Number of sub directories to watch. The higher the number, more memory/cpu/time will be required for watchers."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""awaitWriteFinish"",
                                  ""label"": ""Await Write Finish"",
                                  ""defaultValue"": ""True"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""awaitWriteFinish""
                                  },
                                  ""dataType"": ""Boolean"",
                                  ""automationDataType"": 8,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Trigger watcher events only when the file finishes writing."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""awaitWriteFinishStabilityThreshold"",
                                  ""label"": ""Await Write Finish Stability (ms)"",
                                  ""defaultValue"": ""2000"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""awaitWriteFinishStabilityThreshold""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Amount of time in milliseconds for a file size to remain constant before emitting its event."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""awaitWriteFinishPollInterval"",
                                  ""label"": ""Await File Size Poll (ms)"",
                                  ""defaultValue"": ""100"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""awaitWriteFinishPollInterval""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar) File size polling interval."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""atomic"",
                                  ""label"": ""Atomic Writes Threshold"",
                                  ""defaultValue"": ""100"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""atomic""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar) Automatically filters out artifacts that occur within the defined value (in milliseconds) when using editors that use 'atomic writes' instead of writing directly to the source file. If defined to 0, this function is disabled."",
                                  ""settingKey"": ""name""
                                }
                              ],
                              ""connectingTimeout"": 30000,
                              ""setupTimeout"": 10000,
                              ""intervalBeforeReconnect"": 5000,
                              ""heartbeatInterval"": 60000,
                              ""___cmf___name"": ""Equipment Configuration""
                            }
                          },
                          {
                            ""id"": ""task_358"",
                            ""reference"": {
                              ""name"": ""driverCommand"",
                              ""package"": {
                                ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                                ""version"": ""11.1.0-202409173""
                              }
                            },
                            ""driver"": ""Handler"",
                            ""settings"": {
                              ""command"": ""Connect"",
                              ""___cmf___description"": ""Connect""
                            }
                          }
                        ],
                        ""converters"": [],
                        ""links"": [
                          {
                            ""id"": ""task_356_success-task_357_activate"",
                            ""sourceId"": ""task_356"",
                            ""targetId"": ""task_357"",
                            ""inputName"": ""activate"",
                            ""outputName"": ""success""
                          },
                          {
                            ""id"": ""task_357_success-task_358_activate"",
                            ""sourceId"": ""task_357"",
                            ""targetId"": ""task_358"",
                            ""inputName"": ""activate"",
                            ""outputName"": ""success""
                          }
                        ],
                        ""$id"": ""1"",
                        ""layout"": {
                          ""general"": {
                            ""color"": null,
                            ""notes"": []
                          },
                          ""drawers"": {
                            ""DIAGRAM"": {
                              ""tasks"": {
                                ""task_358"": {
                                  ""collapsed"": false,
                                  ""position"": {
                                    ""x"": 1100,
                                    ""y"": 100
                                  },
                                  ""outdated"": false
                                },
                                ""task_356"": {
                                  ""collapsed"": false,
                                  ""position"": {
                                    ""x"": 100,
                                    ""y"": 100
                                  },
                                  ""outdated"": false
                                },
                                ""task_357"": {
                                  ""collapsed"": false,
                                  ""position"": {
                                    ""x"": 600,
                                    ""y"": 100
                                  },
                                  ""outdated"": false
                                }
                              },
                              ""links"": {
                                ""task_356_success-task_357_activate"": {
                                  ""vertices"": []
                                },
                                ""task_357_success-task_358_activate"": {
                                  ""vertices"": []
                                }
                              },
                              ""notes"": {},
                              ""pan"": {
                                ""x"": 9.675338107580444,
                                ""y"": 3.532465519098835
                              },
                              ""zoom"": 0.78
                            }
                          }
                        }
                      }")
                },
                { "/test/Data/AutomationWorkflows/Test/Test2.json", new MockFileData(
                    @"{
                      ""tasks"": [
                        {
                          ""id"": ""task_536"",
                          ""reference"": {
                            ""name"": ""workflow"",
                            ""package"": {
                              ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                              ""version"": ""11.1.0-beta""
                            }
                          },
                          ""settings"": {
                            ""inputs"": [],
                            ""outputs"": [
                              {
                                ""name"": ""activate"",
                                ""valueType"": {
                                  ""friendlyName"": ""Activate"",
                                  ""type"": null,
                                  ""collectionType"": 0,
                                  ""referenceType"": null,
                                  ""referenceTypeName"": null,
                                  ""referenceTypeId"": null
                                }
                              }
                            ],
                            ""retries"": 30,
                            ""contextsExpirationInMilliseconds"": 60000,
                            ""executionExpirationInMilliseconds"": 1200000,
                            ""executeWhenAllInputsDefined"": false,
                            ""automationWorkflow"": {
                              ""DisplayName"": ""workflow_544"",
                              ""IsShared"": false,
                              ""Name"": ""Test""
                            },
                            ""___cmf___name"": ""Call SubWorkflow""
                          }
                        }
                      ],
                      ""converters"": [],
                      ""links"": [],
                      ""$id"": ""1"",
                      ""layout"": {
                        ""general"": {
                          ""color"": null,
                          ""notes"": []
                        },
                        ""drawers"": {
                          ""DIAGRAM"": {
                            ""tasks"": {
                              ""task_536"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 559,
                                  ""y"": 204
                                },
                                ""outdated"": false
                              },
                              ""task_751"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 1023,
                                  ""y"": 172
                                },
                                ""outdated"": false
                              },
                              ""task_762"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 759,
                                  ""y"": 404
                                },
                                ""outdated"": false
                              }
                            },
                            ""links"": {},
                            ""notes"": {},
                            ""pan"": {
                              ""x"": 0,
                              ""y"": 3
                            }
                          }
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

            Assert.True(console.Error == null || string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed {console.Error.ToString()}");
        }

        [Fact]
        public void Data_JsonValidator_FailPath_SubWorkflow_NotFoundFile()
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
                        },
                        {
                          ""source"": ""AutomationWorkFlows/*"",
                          ""target"": ""AutomationWorkFlows"",
                          ""contentType"": ""AutomationWorkFlows""
                        }
                      ]
                    }")
                },
                { "/test/Data/MasterData/1.1.0/Test.json", new MockFileData(
                    @"{
                        ""AutomationControllerWorkflow"": {
                            ""1"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test"",
                                ""DisplayName"": ""Test"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""Error"",
                                ""Order"": ""1""
                            },
                            ""2"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test2"",
                                ""DisplayName"": ""Test2"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""Test2/Test2.json"",
                                ""Order"": ""2""
                            }
                        }
                    }")
                },
                { "/test/Data/AutomationWorkflows/Test/Test.json", new MockFileData(
                    @"{
                        ""tasks"": [
                          {
                            ""id"": ""task_356"",
                            ""reference"": {
                              ""name"": ""driverEvent"",
                              ""package"": {
                                ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                                ""version"": ""11.1.0-202409173""
                              }
                            },
                            ""driver"": ""Handler"",
                            ""settings"": {
                              ""autoEnable"": true,
                              ""event"": ""OnInitialize"",
                              ""autoSetup"": true,
                              ""___cmf___description"": ""OnInitialize""
                            }
                          },
                          {
                            ""id"": ""task_357"",
                            ""reference"": {
                              ""name"": ""equipmentConfig"",
                              ""package"": {
                                ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                                ""version"": ""11.1.0-202409173""
                              }
                            },
                            ""driver"": ""Handler"",
                            ""settings"": {
                              ""_inputs"": [
                                {
                                  ""name"": ""path"",
                                  ""label"": ""Path"",
                                  ""defaultValue"": ""c:/temp"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""path""
                                  },
                                  ""dataType"": ""String"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 0,
                                  ""description"": ""Path of the location to monitor for changes on the files. If no value is provided, no watcher is created, so no events will be triggered."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""fileMask"",
                                  ""label"": ""FileMask"",
                                  ""defaultValue"": ""**/*.request"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""fileMask""
                                  },
                                  ""dataType"": ""String"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 0,
                                  ""description"": ""Glob pattern to use for the watcher to identify the files to handle. Use a tool like https://globster.xyz/ to try a valid value to use."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""archivePath"",
                                  ""label"": ""Archive Path"",
                                  ""defaultValue"": """",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""archivePath""
                                  },
                                  ""dataType"": ""String"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 0,
                                  ""description"": ""Directory planned to use for archiving the files after processing. This value can later be used as a variable in the available operations to execute."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""watcherType"",
                                  ""label"": ""Watcher Type"",
                                  ""defaultValue"": ""Chokidar"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""watcherType""
                                  },
                                  ""dataType"": ""Enum"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 1,
                                  ""description"": ""Type of watcher to use. Depending on the selection, different settings are necessary. Chokidar is the best overall option, but CPU and Memory heavy and slower to start when many files are present. NSFW is better other scenarios. Test both to determine the best option."",
                                  ""valueReferenceType"": 6,
                                  ""settings"": {
                                    ""enumValues"": [
                                      ""Chokidar"",
                                      ""NSFW""
                                    ]
                                  },
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""ignoreInitial"",
                                  ""label"": ""Ignore Existing Files"",
                                  ""defaultValue"": ""False"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""ignoreInitial""
                                  },
                                  ""dataType"": ""Boolean"",
                                  ""automationDataType"": 8,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Flag to define if, during startup/restart, the files that were created, changed, deleted should be processed or ignored"",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""watcherMode"",
                                  ""label"": ""File Watcher Mode"",
                                  ""defaultValue"": ""Polling"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""watcherMode""
                                  },
                                  ""dataType"": ""Enum"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 1,
                                  ""description"": ""(Type=Chokidar) To successfully watch files over a network (and in other non-standard situations), it is typically necessary to use Polling, however it could lead to high CPU utilization. FileSystemEvents is the most efficient method for monitoring local files."",
                                  ""valueReferenceType"": 6,
                                  ""settings"": {
                                    ""enumValues"": [
                                      ""FileSystemEvents"",
                                      ""Polling""
                                    ]
                                  },
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""pollingInterval"",
                                  ""label"": ""Polling Interval (ms)"",
                                  ""defaultValue"": ""100"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""pollingInterval""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Interval of file system polling (in milliseconds)"",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""pollingBinaryInterval"",
                                  ""label"": ""Binary Files Polling Interval (ms)"",
                                  ""defaultValue"": ""300"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""pollingBinaryInterval""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar) Interval of file system polling for binary files (in milliseconds)"",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""alwaysStat"",
                                  ""label"": ""Always Stat"",
                                  ""defaultValue"": ""True"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""alwaysStat""
                                  },
                                  ""dataType"": ""Boolean"",
                                  ""automationDataType"": 8,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Always get additional attributes (size, timestamps, etc) of the file that was identified by the watcher. Will require additional operating system resources."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""depth"",
                                  ""label"": ""Subdirectory Depth"",
                                  ""defaultValue"": ""0"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""depth""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Number of sub directories to watch. The higher the number, more memory/cpu/time will be required for watchers."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""awaitWriteFinish"",
                                  ""label"": ""Await Write Finish"",
                                  ""defaultValue"": ""True"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""awaitWriteFinish""
                                  },
                                  ""dataType"": ""Boolean"",
                                  ""automationDataType"": 8,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Trigger watcher events only when the file finishes writing."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""awaitWriteFinishStabilityThreshold"",
                                  ""label"": ""Await Write Finish Stability (ms)"",
                                  ""defaultValue"": ""2000"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""awaitWriteFinishStabilityThreshold""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Amount of time in milliseconds for a file size to remain constant before emitting its event."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""awaitWriteFinishPollInterval"",
                                  ""label"": ""Await File Size Poll (ms)"",
                                  ""defaultValue"": ""100"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""awaitWriteFinishPollInterval""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar) File size polling interval."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""atomic"",
                                  ""label"": ""Atomic Writes Threshold"",
                                  ""defaultValue"": ""100"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""atomic""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar) Automatically filters out artifacts that occur within the defined value (in milliseconds) when using editors that use 'atomic writes' instead of writing directly to the source file. If defined to 0, this function is disabled."",
                                  ""settingKey"": ""name""
                                }
                              ],
                              ""connectingTimeout"": 30000,
                              ""setupTimeout"": 10000,
                              ""intervalBeforeReconnect"": 5000,
                              ""heartbeatInterval"": 60000,
                              ""___cmf___name"": ""Equipment Configuration""
                            }
                          },
                          {
                            ""id"": ""task_358"",
                            ""reference"": {
                              ""name"": ""driverCommand"",
                              ""package"": {
                                ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                                ""version"": ""11.1.0-202409173""
                              }
                            },
                            ""driver"": ""Handler"",
                            ""settings"": {
                              ""command"": ""Connect"",
                              ""___cmf___description"": ""Connect""
                            }
                          }
                        ],
                        ""converters"": [],
                        ""links"": [
                          {
                            ""id"": ""task_356_success-task_357_activate"",
                            ""sourceId"": ""task_356"",
                            ""targetId"": ""task_357"",
                            ""inputName"": ""activate"",
                            ""outputName"": ""success""
                          },
                          {
                            ""id"": ""task_357_success-task_358_activate"",
                            ""sourceId"": ""task_357"",
                            ""targetId"": ""task_358"",
                            ""inputName"": ""activate"",
                            ""outputName"": ""success""
                          }
                        ],
                        ""$id"": ""1"",
                        ""layout"": {
                          ""general"": {
                            ""color"": null,
                            ""notes"": []
                          },
                          ""drawers"": {
                            ""DIAGRAM"": {
                              ""tasks"": {
                                ""task_358"": {
                                  ""collapsed"": false,
                                  ""position"": {
                                    ""x"": 1100,
                                    ""y"": 100
                                  },
                                  ""outdated"": false
                                },
                                ""task_356"": {
                                  ""collapsed"": false,
                                  ""position"": {
                                    ""x"": 100,
                                    ""y"": 100
                                  },
                                  ""outdated"": false
                                },
                                ""task_357"": {
                                  ""collapsed"": false,
                                  ""position"": {
                                    ""x"": 600,
                                    ""y"": 100
                                  },
                                  ""outdated"": false
                                }
                              },
                              ""links"": {
                                ""task_356_success-task_357_activate"": {
                                  ""vertices"": []
                                },
                                ""task_357_success-task_358_activate"": {
                                  ""vertices"": []
                                }
                              },
                              ""notes"": {},
                              ""pan"": {
                                ""x"": 9.675338107580444,
                                ""y"": 3.532465519098835
                              },
                              ""zoom"": 0.78
                            }
                          }
                        }
                      }")
                },
                { "/test/Data/AutomationWorkflows/Test/Test2.json", new MockFileData(
                    @"{
                      ""tasks"": [
                        {
                          ""id"": ""task_536"",
                          ""reference"": {
                            ""name"": ""workflow"",
                            ""package"": {
                              ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                              ""version"": ""11.1.0-beta""
                            }
                          },
                          ""settings"": {
                            ""inputs"": [],
                            ""outputs"": [
                              {
                                ""name"": ""activate"",
                                ""valueType"": {
                                  ""friendlyName"": ""Activate"",
                                  ""type"": null,
                                  ""collectionType"": 0,
                                  ""referenceType"": null,
                                  ""referenceTypeName"": null,
                                  ""referenceTypeId"": null
                                }
                              }
                            ],
                            ""retries"": 30,
                            ""contextsExpirationInMilliseconds"": 60000,
                            ""executionExpirationInMilliseconds"": 1200000,
                            ""executeWhenAllInputsDefined"": false,
                            ""automationWorkflow"": {
                              ""DisplayName"": ""workflow_544"",
                              ""IsShared"": false,
                              ""Name"": ""Test""
                            },
                            ""___cmf___name"": ""Call SubWorkflow""
                          }
                        }
                      ],
                      ""converters"": [],
                      ""links"": [],
                      ""$id"": ""1"",
                      ""layout"": {
                        ""general"": {
                          ""color"": null,
                          ""notes"": []
                        },
                        ""drawers"": {
                          ""DIAGRAM"": {
                            ""tasks"": {
                              ""task_536"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 559,
                                  ""y"": 204
                                },
                                ""outdated"": false
                              },
                              ""task_751"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 1023,
                                  ""y"": 172
                                },
                                ""outdated"": false
                              },
                              ""task_762"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 759,
                                  ""y"": 404
                                },
                                ""outdated"": false
                              }
                            },
                            ""links"": {},
                            ""notes"": {},
                            ""pan"": {
                              ""x"": 0,
                              ""y"": 3
                            }
                          }
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

            Assert.True(!string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator did not fail for IoT Data Workflow Package: {console.Error.ToString()}");
            Assert.True(console.Error.ToString().Contains("Could not find the path Error for the Workflow"), $"Json Validator did not fail for IoT Data Workflow Package: {console.Error.ToString()}");
        }

        [Fact]
        public void Data_JsonValidator_FailPath_SubWorkflow_NotFoundWorkflow()
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
                        },
                        {
                          ""source"": ""AutomationWorkFlows/*"",
                          ""target"": ""AutomationWorkFlows"",
                          ""contentType"": ""AutomationWorkFlows""
                        }
                      ]
                    }")
                },
                { "/test/Data/MasterData/1.1.0/Test.json", new MockFileData(
                    @"{
                        ""AutomationControllerWorkflow"": {
                            ""1"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test"",
                                ""DisplayName"": ""Test/Test.json"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""Error"",
                                ""Order"": ""1""
                            },
                            ""2"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test2"",
                                ""DisplayName"": ""Test2"",
                                ""IsFile"": ""Yes"",
                                ""Workflow"": ""Test2/Test2.json"",
                                ""Order"": ""2""
                            }
                        }
                    }")
                },
                { "/test/Data/AutomationWorkflows/Test/Test.json", new MockFileData(
                    @"{
                        ""tasks"": [
                          {
                            ""id"": ""task_356"",
                            ""reference"": {
                              ""name"": ""driverEvent"",
                              ""package"": {
                                ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                                ""version"": ""11.1.0-202409173""
                              }
                            },
                            ""driver"": ""Handler"",
                            ""settings"": {
                              ""autoEnable"": true,
                              ""event"": ""OnInitialize"",
                              ""autoSetup"": true,
                              ""___cmf___description"": ""OnInitialize""
                            }
                          },
                          {
                            ""id"": ""task_357"",
                            ""reference"": {
                              ""name"": ""equipmentConfig"",
                              ""package"": {
                                ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                                ""version"": ""11.1.0-202409173""
                              }
                            },
                            ""driver"": ""Handler"",
                            ""settings"": {
                              ""_inputs"": [
                                {
                                  ""name"": ""path"",
                                  ""label"": ""Path"",
                                  ""defaultValue"": ""c:/temp"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""path""
                                  },
                                  ""dataType"": ""String"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 0,
                                  ""description"": ""Path of the location to monitor for changes on the files. If no value is provided, no watcher is created, so no events will be triggered."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""fileMask"",
                                  ""label"": ""FileMask"",
                                  ""defaultValue"": ""**/*.request"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""fileMask""
                                  },
                                  ""dataType"": ""String"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 0,
                                  ""description"": ""Glob pattern to use for the watcher to identify the files to handle. Use a tool like https://globster.xyz/ to try a valid value to use."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""archivePath"",
                                  ""label"": ""Archive Path"",
                                  ""defaultValue"": """",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""archivePath""
                                  },
                                  ""dataType"": ""String"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 0,
                                  ""description"": ""Directory planned to use for archiving the files after processing. This value can later be used as a variable in the available operations to execute."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""watcherType"",
                                  ""label"": ""Watcher Type"",
                                  ""defaultValue"": ""Chokidar"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""watcherType""
                                  },
                                  ""dataType"": ""Enum"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 1,
                                  ""description"": ""Type of watcher to use. Depending on the selection, different settings are necessary. Chokidar is the best overall option, but CPU and Memory heavy and slower to start when many files are present. NSFW is better other scenarios. Test both to determine the best option."",
                                  ""valueReferenceType"": 6,
                                  ""settings"": {
                                    ""enumValues"": [
                                      ""Chokidar"",
                                      ""NSFW""
                                    ]
                                  },
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""ignoreInitial"",
                                  ""label"": ""Ignore Existing Files"",
                                  ""defaultValue"": ""False"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""ignoreInitial""
                                  },
                                  ""dataType"": ""Boolean"",
                                  ""automationDataType"": 8,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Flag to define if, during startup/restart, the files that were created, changed, deleted should be processed or ignored"",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""watcherMode"",
                                  ""label"": ""File Watcher Mode"",
                                  ""defaultValue"": ""Polling"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""watcherMode""
                                  },
                                  ""dataType"": ""Enum"",
                                  ""automationDataType"": 0,
                                  ""referenceType"": 1,
                                  ""description"": ""(Type=Chokidar) To successfully watch files over a network (and in other non-standard situations), it is typically necessary to use Polling, however it could lead to high CPU utilization. FileSystemEvents is the most efficient method for monitoring local files."",
                                  ""valueReferenceType"": 6,
                                  ""settings"": {
                                    ""enumValues"": [
                                      ""FileSystemEvents"",
                                      ""Polling""
                                    ]
                                  },
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""pollingInterval"",
                                  ""label"": ""Polling Interval (ms)"",
                                  ""defaultValue"": ""100"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""pollingInterval""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Interval of file system polling (in milliseconds)"",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""pollingBinaryInterval"",
                                  ""label"": ""Binary Files Polling Interval (ms)"",
                                  ""defaultValue"": ""300"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""pollingBinaryInterval""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar) Interval of file system polling for binary files (in milliseconds)"",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""alwaysStat"",
                                  ""label"": ""Always Stat"",
                                  ""defaultValue"": ""True"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""alwaysStat""
                                  },
                                  ""dataType"": ""Boolean"",
                                  ""automationDataType"": 8,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Always get additional attributes (size, timestamps, etc) of the file that was identified by the watcher. Will require additional operating system resources."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""depth"",
                                  ""label"": ""Subdirectory Depth"",
                                  ""defaultValue"": ""0"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""depth""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Number of sub directories to watch. The higher the number, more memory/cpu/time will be required for watchers."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""awaitWriteFinish"",
                                  ""label"": ""Await Write Finish"",
                                  ""defaultValue"": ""True"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""awaitWriteFinish""
                                  },
                                  ""dataType"": ""Boolean"",
                                  ""automationDataType"": 8,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Trigger watcher events only when the file finishes writing."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""awaitWriteFinishStabilityThreshold"",
                                  ""label"": ""Await Write Finish Stability (ms)"",
                                  ""defaultValue"": ""2000"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""awaitWriteFinishStabilityThreshold""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar|NSFW) Amount of time in milliseconds for a file size to remain constant before emitting its event."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""awaitWriteFinishPollInterval"",
                                  ""label"": ""Await File Size Poll (ms)"",
                                  ""defaultValue"": ""100"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""awaitWriteFinishPollInterval""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar) File size polling interval."",
                                  ""settingKey"": ""name""
                                },
                                {
                                  ""name"": ""atomic"",
                                  ""label"": ""Atomic Writes Threshold"",
                                  ""defaultValue"": ""100"",
                                  ""parameter"": {
                                    ""$type"": ""Cmf.Foundation.BusinessObjects.AutomationProtocolParameter, Cmf.Foundation.BusinessObjects"",
                                    ""Name"": ""atomic""
                                  },
                                  ""dataType"": ""Integer"",
                                  ""automationDataType"": 5,
                                  ""referenceType"": 0,
                                  ""description"": ""(Type=Chokidar) Automatically filters out artifacts that occur within the defined value (in milliseconds) when using editors that use 'atomic writes' instead of writing directly to the source file. If defined to 0, this function is disabled."",
                                  ""settingKey"": ""name""
                                }
                              ],
                              ""connectingTimeout"": 30000,
                              ""setupTimeout"": 10000,
                              ""intervalBeforeReconnect"": 5000,
                              ""heartbeatInterval"": 60000,
                              ""___cmf___name"": ""Equipment Configuration""
                            }
                          },
                          {
                            ""id"": ""task_358"",
                            ""reference"": {
                              ""name"": ""driverCommand"",
                              ""package"": {
                                ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                                ""version"": ""11.1.0-202409173""
                              }
                            },
                            ""driver"": ""Handler"",
                            ""settings"": {
                              ""command"": ""Connect"",
                              ""___cmf___description"": ""Connect""
                            }
                          }
                        ],
                        ""converters"": [],
                        ""links"": [
                          {
                            ""id"": ""task_356_success-task_357_activate"",
                            ""sourceId"": ""task_356"",
                            ""targetId"": ""task_357"",
                            ""inputName"": ""activate"",
                            ""outputName"": ""success""
                          },
                          {
                            ""id"": ""task_357_success-task_358_activate"",
                            ""sourceId"": ""task_357"",
                            ""targetId"": ""task_358"",
                            ""inputName"": ""activate"",
                            ""outputName"": ""success""
                          }
                        ],
                        ""$id"": ""1"",
                        ""layout"": {
                          ""general"": {
                            ""color"": null,
                            ""notes"": []
                          },
                          ""drawers"": {
                            ""DIAGRAM"": {
                              ""tasks"": {
                                ""task_358"": {
                                  ""collapsed"": false,
                                  ""position"": {
                                    ""x"": 1100,
                                    ""y"": 100
                                  },
                                  ""outdated"": false
                                },
                                ""task_356"": {
                                  ""collapsed"": false,
                                  ""position"": {
                                    ""x"": 100,
                                    ""y"": 100
                                  },
                                  ""outdated"": false
                                },
                                ""task_357"": {
                                  ""collapsed"": false,
                                  ""position"": {
                                    ""x"": 600,
                                    ""y"": 100
                                  },
                                  ""outdated"": false
                                }
                              },
                              ""links"": {
                                ""task_356_success-task_357_activate"": {
                                  ""vertices"": []
                                },
                                ""task_357_success-task_358_activate"": {
                                  ""vertices"": []
                                }
                              },
                              ""notes"": {},
                              ""pan"": {
                                ""x"": 9.675338107580444,
                                ""y"": 3.532465519098835
                              },
                              ""zoom"": 0.78
                            }
                          }
                        }
                      }")
                },
                { "/test/Data/AutomationWorkflows/Test/Test2.json", new MockFileData(
                    @"{
                      ""tasks"": [
                        {
                          ""id"": ""task_536"",
                          ""reference"": {
                            ""name"": ""workflow"",
                            ""package"": {
                              ""name"": ""@criticalmanufacturing/connect-iot-controller-engine-core-tasks"",
                              ""version"": ""11.1.0-beta""
                            }
                          },
                          ""settings"": {
                            ""inputs"": [],
                            ""outputs"": [
                              {
                                ""name"": ""activate"",
                                ""valueType"": {
                                  ""friendlyName"": ""Activate"",
                                  ""type"": null,
                                  ""collectionType"": 0,
                                  ""referenceType"": null,
                                  ""referenceTypeName"": null,
                                  ""referenceTypeId"": null
                                }
                              }
                            ],
                            ""retries"": 30,
                            ""contextsExpirationInMilliseconds"": 60000,
                            ""executionExpirationInMilliseconds"": 1200000,
                            ""executeWhenAllInputsDefined"": false,
                            ""automationWorkflow"": {
                              ""DisplayName"": ""workflow_544"",
                              ""IsShared"": false,
                              ""Name"": ""Error""
                            },
                            ""___cmf___name"": ""Call SubWorkflow""
                          }
                        }
                      ],
                      ""converters"": [],
                      ""links"": [],
                      ""$id"": ""1"",
                      ""layout"": {
                        ""general"": {
                          ""color"": null,
                          ""notes"": []
                        },
                        ""drawers"": {
                          ""DIAGRAM"": {
                            ""tasks"": {
                              ""task_536"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 559,
                                  ""y"": 204
                                },
                                ""outdated"": false
                              },
                              ""task_751"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 1023,
                                  ""y"": 172
                                },
                                ""outdated"": false
                              },
                              ""task_762"": {
                                ""collapsed"": false,
                                ""position"": {
                                  ""x"": 759,
                                  ""y"": 404
                                },
                                ""outdated"": false
                              }
                            },
                            ""links"": {},
                            ""notes"": {},
                            ""pan"": {
                              ""x"": 0,
                              ""y"": 3
                            }
                          }
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

            Assert.True(!string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator did not fail for IoT Data Workflow Package: {console.Error.ToString()}");
            Assert.True(console.Error.ToString().Contains("The subworkflow Error is mentioned but there is no workflow declared with that name"), $"Json Validator did not fail for IoT Data Workflow Package: {console.Error.ToString()}");
        }

        [Fact]
        public void Data_JsonValidator_HappyPath_EmptyIsFile()
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
                      ]
                    }")
                },
                { "/test/Data/MasterData/1.1.0/Test.json", new MockFileData(
                    @"{
                        ""AutomationControllerWorkflow"": {
                            ""1"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test"",
                                ""DisplayName"": ""Test"",
                                ""IsFile"": """",
                                ""Order"": ""1""
                            },
                            ""2"": {
                                ""AutomationController"": ""TestController"",
                                ""Name"": ""Test2"",
                                ""DisplayName"": ""Test2"",
                                ""IsFile"": ""No"",
                                ""Order"": ""2""
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

            Assert.True(console.Error == null || string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed with empty IsFile: {console.Error?.ToString()}");
        }

        [Fact]
        public void Data_JsonValidator_Repeated_Keys_IoT_Event_Definition()
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
                                ""id"": ""Cmf.Custom.IoT"",
                                ""version"": ""1.1.0""
                            }
                        ]
                    }"
                )},
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
                    }"
                )},
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
                      ]
                    }"
                )},
                { "/test/IoTData/MasterData/1.0.0IoT.json", new CmfMockJsonData(
                    @"{
                        ""<DM>AutomationProtocol"": {
                        ""1"": {
                            ""Name"": ""TEST_Protocol"",
                            ""Description"": ""TEST_Protocol"",
                            ""Type"":"" ""General"",
                            ""Package"": ""@criticalmanufacturing/connect-iot-driver-test"",
                            ""PackageVersion"": ""test""
                    }"
                )},
                { "/test/IoTData/AutomationWorkflowFiles/MasterData/1.0.0/TestWorkflowIoT.json", new CmfMockJsonData(
                    @"{
	                     ""tasks"": [
                            {
                                ""id"": ""task_22180"",
                                ""reference"": {
                                    ""name"": ""equipmentCommand""
                                },
                                ""settings"": {
                                    ""_command"": {
                                        ""$type"": ""aa"",
                                        ""Name"": ""bb"",
                                        ""Parameters"": [
                                            {
                                                ""$type"": ""cc"",
                                                ""Name"": ""dd""
                                            }
                                        ],
                                        ""DeviceCommandId"": """",
                                        ""ExtendedData"": """"
                                    },
                                    ""_inputs"": [
                                    ]
                                },
                                ""driver"": ""RestClient""
                            },
                            {
                                ""id"": ""task_16195"",
                                ""reference"": {
                                    ""name"": ""ddddd""
                                },
                                ""settings"": {
                                    ""inputs"": [
                                    ],
                                    ""outputs"": [
                                    ]
                                }
                            }
                        ]
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
                "test/"
            }, console);

            Assert.True(console.Error == null || string.IsNullOrEmpty(console.Error.ToString()), $"Json Validator failed with repeated keys on different levels in arrays: {console.Error?.ToString()}");
        }

    }
}

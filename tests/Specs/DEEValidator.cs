using Cmf.CLI.Commands;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO.Abstractions.TestingHelpers;
using tests.Objects;
using Xunit;

namespace tests.Specs
{
    public class DeeValidator
    {
        private const string MOCKDEE = @"
                        using Cmf.Common.CustomActionUtilities.Abstractions;
                        using Cmf.Navigo.BusinessOrchestration.MaterialManagement.InputObjects;

                        namespace Cmf.Custom.Test.Active.Actions.MaterialActions
                        {
                            public class test : DeeDevBase
                            {
                                /// <summary>
                                /// Dees the test condition.
                                /// </summary>
                                /// <param name=""Input"">The input.</param>
                                /// <returns>
                                /// Return true if is to execute action.
                                /// </returns>
                                public override bool DeeTestCondition(Dictionary<string, object> Input)
                                {
                                    //---Start DEE Condition Code---

                                    /// <summary>
                                    /// Summary text: Assign the running mode on the resource processing the material
                                    /// Actions groups:
                                    ///     * BusinessObjects.MaterialCollection.TrackIn.Pre
                                    /// Depends On:
                                    /// Is Dependency For:
                                    /// Exceptions:
                                    /// </summary>

                                    return true;

                                    //---End DEE Condition Code---
                                }

                                /// <summary>
                                /// Dees the action code.
                                /// </summary>
                                /// <param name=""Input"">The input.</param>
                                /// <returns>
                                /// Return the Input for the next Action
                                /// </returns>
                                public override Dictionary<string, object> DeeActionCode(Dictionary<string, object> Input)
                                {
                                    //---Start DEE Code---

                                    // Navigo
                                    UseReference(""Cmf.Navigo.BusinessObjects.dll"", ""Cmf.Navigo.BusinessObjects"");
                                    UseReference(""Cmf.Navigo.BusinessOrchestration.dll"", ""Cmf.Navigo.BusinessOrchestration.MaterialManagement.InputObjects"");

                                    // System
                                    UseReference(""%MicrosoftNetPath%System.Private.CoreLib.dll"", ""System.Text"");

                                    // MES
                                    UseReference(""Cmf.Foundation.Common.dll"", ""Cmf.Foundation.Common.LocalizationService"");

                                    // Custom
                                    UseReference(""Cmf.Custom.Test.Active.Common.dll"", ""Cmf.Custom.Test.Active.Common.Constants"");
                                    UseReference(""Cmf.Custom.Test.Active.Common.dll"", ""Cmf.Custom.Test.Active.Common.Utilities.Abstractions"");
                                    UseReference(""Cmf.Custom.Test.Baseline.Common.dll"", ""Cmf.Custom.Test.Baseline.Common.Utilities.Abstractions"");
                                    UseReference(""Cmf.Common.CustomActionUtilities.dll"", ""Cmf.Common.CustomActionUtilities"");
                                    UseReference(""Cmf.Common.CustomActionUtilities.dll"", ""Cmf.Common.CustomActionUtilities.Abstractions"");
                                    UseReference(""Cmf.Custom.Test.Active.Orchestration.dll"", ""Cmf.Custom.Test.Active.Orchestration.InputObjects"");
                                    UseReference(""Cmf.Custom.Test.Active.Orchestration.dll"", ""Cmf.Custom.Test.Active.Orchestration.OutputObjects"");
                                    UseReference(""Cmf.Custom.Test.Active.Orchestration.dll"", ""Cmf.Custom.Test.Active"");
                                    UseReference(""Cmf.Custom.Test.Baseline.ExternalServices.dll"", ""Cmf.Custom.Test.Baseline.ExternalServices.GetManufacturingOrderDetail"");
                                    UseReference(""Cmf.Custom.Test.Baseline.ExternalServices.dll"", ""Cmf.Custom.Test.Baseline.ExternalServices.GetStockList"");

                                    var serviceProvider = (IServiceProvider)Input[""ServiceProvider""];
                                    var localizationService = serviceProvider.GetService<ILocalizationService>();
                                    var deeContextUtilities = serviceProvider.GetService<IDeeContextUtilities>();
                                    var activeCustomUtilities = serviceProvider.GetService<ITestActiveUtilitiesCustom>();
                                    var templateCustomUtilities = serviceProvider.GetService<ITestBaselineUtilitiesCustom>();
                                    var templateGenericUtilities = serviceProvider.GetService<ITestBaselineUtilitiesGeneric>();
                                    var orchestration = serviceProvider.GetService<IACTIVEOrchestration>();
                                    var entityFactory = serviceProvider.GetService<IEntityFactory>();

                                    //---End DEE Code---
                                    return Input;
                                }
                            }
                        }";

        [Fact]
        public void Data_DEEValidator_HappyPath()
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
                            ""source"": ""DEEs/*"",
                            ""target"": ""DeeRules"",
                            ""contentType"": ""DEE""
                        }
                      ]
                    }")
                },
                { "/test/Data/DEEs/test/test.cs", new MockFileData(MOCKDEE)
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

        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        public void Data_DEEValidator_FailPath_Indicators(bool noStartCondition, bool noFinishCondition, bool noStartExecute, bool noFinishExecute)
        {
            var dee = MOCKDEE;
            if (noStartCondition)
            {
                dee = dee.Replace("//---Start DEE Condition Code---", "");
            }

            if (noFinishCondition)
            {
                dee = dee.Replace("//---End DEE Condition Code---", "");
            }

            if (noStartExecute)
            {
                dee = dee.Replace("//---Start DEE Code---", "");
            }

            if (noFinishExecute)
            {
                dee = dee.Replace("//---End DEE Code---", "");
            }

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
                            ""source"": ""DEEs/*"",
                            ""target"": ""DeeRules"",
                            ""contentType"": ""DEE""
                        }
                      ]
                    }")
                },
                { "/test/Data/DEEs/test/test.cs", new MockFileData(dee)
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
            Assert.True(console.Error.ToString().Contains("It does not have all the valid indicators"), $"Json Validator did not fail for IoT Data Workflow Package: {console.Error.ToString()}");
        }

        [Fact]
        public void Data_DEEValidator_FailPath_UseReference()
        {
            var dee = MOCKDEE.Replace("UseReference(\"Cmf.Custom.Test.Active.Common.dll\", \"Cmf.Custom.Test.Active.Common.Constants\");", "UseReference(\"Cmf.Custom.Test.Active.Common.dll \", \"Cmf.Custom.Test.Active.Common.Constants\");");

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
                            ""source"": ""DEEs/*"",
                            ""target"": ""DeeRules"",
                            ""contentType"": ""DEE""
                        }
                      ]
                    }")
                },
                { "/test/Data/DEEs/test/test.cs", new MockFileData(dee)
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
            Assert.True(console.Error.ToString().Contains("UseReference contains a whitespace, please refer to the valid format UseReference"), $"Json Validator did not fail for IoT Data Workflow Package: {console.Error.ToString()}");
        }
    }
}
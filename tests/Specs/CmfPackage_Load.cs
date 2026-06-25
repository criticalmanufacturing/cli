using Cmf.CLI.Constants;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Objects;
using Xunit;

namespace tests.Specs
{
    public class CmfPackage_Load
    {
        [Fact]
        public void Root_HappyPath()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Environment", "8.3.0");
            KeyValuePair<string, string> packageDep2 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""isToForceInstall"": true,
                  ""forceRerunAfterDatabaseRestore"": true,
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
                }}")}
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"repo/{CliConstants.CmfPackageFileName}");

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

            Assert.Equal(string.Empty, message);
            Assert.NotNull(cmfPackage);
            Assert.Equal(2, cmfPackage.Dependencies.Count);
            Assert.Equal(packageDep1.Value, cmfPackage.Dependencies[0].Version);
            Assert.Equal(packageDep2.Value, cmfPackage.Dependencies[1].Version);
            Assert.True(cmfPackage.Dependencies[0].IsMissing);
        }

        [Fact(Skip = "awaiting product fix")]
        public void IoT_WithoutMandatoryDependencies()
        {
            KeyValuePair<string, string> packageIoT = new("Cmf.Custom.IoT", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageIoT.Key}"",
                  ""version"": ""{packageIoT.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""IoT"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""isToForceInstall"": true,
                  ""forceRerunAfterDatabaseRestore"": true,
                  ""contentToPack"": [
                    {{
                      ""source"": ""src/packages/*"",
                      ""target"": ""node_modules"",
                      ""ignoreFiles"": [
                        "".npmignore""
                      ]
                    }}
                  ]
                }}")}
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"repo/{CliConstants.CmfPackageFileName}");

            string message = string.Empty;
            try
            {
                // Reading cmfPackage
                CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            Assert.Equal("Mandatory Dependency cmf.connectiot.packages. not found", message);
        }

        [Fact]
        public void IoTData_HappyPath()
        {
            KeyValuePair<string, string> packageIoTData = new("Cmf.Custom.IoT.Data", "1.1.0");
            KeyValuePair<string, string> packageContent1 = new("MasterData/$(version)/*", "MasterData/$(version)");
            KeyValuePair<string, string> packageContent2 = new("AutomationWorkFlows/*", "AutomationWorkFlows");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageIoTData.Key}"",
                  ""version"": ""{packageIoTData.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""IoTData"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""isToForceInstall"": true,
                  ""forceRerunAfterDatabaseRestore"": true,
                  ""contentToPack"": [
                    {{
                      ""source"": ""{packageContent1.Key}"",
                      ""target"": ""{packageContent1.Value}"",
                      ""contentType"": ""{packageContent1.Value.Split('/')[0]}""
                    }},
                    {{
                      ""source"": ""{packageContent2.Key}"",
                      ""target"": ""{packageContent2.Value}"",
                      ""contentType"": ""{packageContent2.Value.Split('/')[0]}""

                    }}
                  ]
                }}")}
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"repo/{CliConstants.CmfPackageFileName}");

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

            Assert.Equal(string.Empty, message);
            Assert.NotNull(cmfPackage);
            Assert.Equal(2, cmfPackage.ContentToPack.Count);
            Assert.Equal(packageContent1.Key, cmfPackage.ContentToPack[0].Source);
            Assert.Equal(packageContent1.Value, cmfPackage.ContentToPack[0].Target);
            Assert.Equal(packageContent1.Value.Split('/')[0], cmfPackage.ContentToPack[0].ContentType.ToString());
            Assert.Equal(packageContent2.Key, cmfPackage.ContentToPack[1].Source);
            Assert.Equal(packageContent2.Value, cmfPackage.ContentToPack[1].Target);
            Assert.Equal(packageContent2.Value.Split('/')[0], cmfPackage.ContentToPack[1].ContentType.ToString());
        }

        [Fact]
        public void IoTPackages_HappyPath()
        {
            KeyValuePair<string, string> packageIoTPackages = new("Cmf.Custom.IoT.Packages", "1.1.0");
            KeyValuePair<string, string> packageContent1 = new("projects/*", "node_modules");
            string[] packageContent1IgnoreFiles = new string[] { ".npmignore" };
            string packageXmlInjection = "ui.xml";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageIoTPackages.Key}"",
                  ""version"": ""{packageIoTPackages.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""IoT"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""isToForceInstall"": true,
                  ""forceRerunAfterDatabaseRestore"": true,
                  ""contentToPack"": [
                    {{
                      ""source"": ""{packageContent1.Key}"",
                      ""target"": ""{packageContent1.Value}"",
                      ""ignoreFiles"": [
                        ""{packageContent1IgnoreFiles[0]}""
                      ]
                    }}
                  ],
                  ""xmlInjection"": [
                    ""{packageXmlInjection}""
                  ]
                }}")}
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"repo/{CliConstants.CmfPackageFileName}");

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

            Assert.Equal(string.Empty, message);
            Assert.NotNull(cmfPackage);
            Assert.Single(cmfPackage.ContentToPack);
            Assert.Equal(packageContent1.Key, cmfPackage.ContentToPack[0].Source);
            Assert.Equal(packageContent1.Value, cmfPackage.ContentToPack[0].Target);
            Assert.Single(cmfPackage.ContentToPack[0].IgnoreFiles);
            Assert.Equal(packageContent1IgnoreFiles[0], cmfPackage.ContentToPack[0].IgnoreFiles[0]);
            Assert.Single(cmfPackage.XmlInjection);
            Assert.Equal(packageXmlInjection, cmfPackage.XmlInjection[0]);
        }
    }
}

using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace tests
{
    public class CmfPackage_Load
    {
        [Fact]
        public void Root_HappyPath()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep1 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");

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
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }}
                  ]
                }}")}
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");

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
            Assert.Equal(packageDep1.Value, cmfPackage.Dependencies[0].Version);
            Assert.Equal(packageDep1.Value, cmfPackage.Dependencies[0].Version);
            Assert.True(cmfPackage.Dependencies[0].IsMissing);
        }

        [Fact]
        public void Root_WithoutMandatoryDependencies()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");

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
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }}
                  ]
                }}")}
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");

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

            Assert.Equal("Mandatory Dependency criticalmanufacturing.deploymentmetadata or cmf.environment. not found", message);
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
                  ""contentToPack"": [
                  {{
                      ""source"": ""src/packages/*"",
                      ""target"": ""node_modules"",
                      ""ignoreFiles"": [
                      "".npmignore""
                      ]
                  }}]
                }}")}
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");

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
        public void Business_WithoutContentToPack()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Business", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Business"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false
                }}")}
            });
            ExecutionContext.Initialize(fileSystem);

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");

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
            string fileLocation = fileSystem.FileInfo.FromFileName("/repo/cmfpackage.json").FullName;

            Assert.Equal(@$"Missing mandatory property ContentToPack in file { fileLocation }", message);
        }
    }
}

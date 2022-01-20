using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests
{
    [TestClass]
    public class CmfPackage_Load
    {
        [TestMethod]
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

            Assert.AreEqual(string.Empty, message);
            Assert.IsNotNull(cmfPackage);
            Assert.AreEqual(packageDep1.Value, cmfPackage.Dependencies[0].Version);
            Assert.AreEqual(packageDep1.Value, cmfPackage.Dependencies[0].Version);
            Assert.AreEqual(true, cmfPackage.Dependencies[0].IsMissing);
        }

        [TestMethod]
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

            Assert.AreEqual("Mandatory Dependency criticalmanufacturing.deploymentmetadata or cmf.environment. not found", message);
        }

        // When is fixed by the product team, ignore can be removed
        [TestMethod, Ignore]
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

            Assert.AreEqual("Mandatory Dependency cmf.connectiot.packages. not found", message);
        }

        [TestMethod]
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

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");

            string message = string.Empty;
            CmfPackage cmfPackage = null;
            try
            {
                // Reading cmfPackage
                cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true, fileSystem);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            string fileLocation = fileSystem.FileInfo.FromFileName("/repo/cmfpackage.json").FullName;

            Assert.AreEqual(@$"Missing mandatory property ContentToPack in file { fileLocation }", message);
        }
    }
}

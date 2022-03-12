using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace tests.Specs
{
    /// <summary>
    /// The cmf package_ save.
    /// </summary>
    public class CmfPackage_Save
    {
        /// <summary>
        /// Validates that the enums are serialized as string and not as int
        /// </summary>
        [Fact]
        public void KeepEnumStrings()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Data", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Data Package"",
                      ""packageType"": ""Data"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""steps"": [
                        {{ ""order"": ""1"", ""type"": ""CreateIntegrationEntries"", ""messageType"": ""ImportObject"" }}
                      ],
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """",
                            ""contentType"": ""Generic"",
                            ""action"": ""Pack""
                        }}
                        ]
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            cmfPackageObj.SaveCmfPackage();

            dynamic cmfpackageFileContent = JsonConvert.DeserializeObject(fileSystem.File.ReadAllText(cmfpackageFile.FullName));
            Assert.Equal("Data", cmfpackageFileContent.packageType.ToString());
            Assert.Equal("CreateIntegrationEntries", cmfpackageFileContent.steps[0].type.ToString());
            Assert.Equal("ImportObject", cmfpackageFileContent.steps[0].messageType.ToString());
            Assert.Equal("Generic", cmfpackageFileContent.contentToPack[0].contentType.ToString());
            Assert.Equal("Pack", cmfpackageFileContent.contentToPack[0].action.ToString());
        }

        /// <summary>
        /// Validates that the handler version is not serialized
        /// </summary>
        [Fact]
        public void IgnoreHandlerVersion()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Data", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Data Package"",
                      ""packageType"": ""Data"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """"
                        }}
                        ]
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            Assert.False(cmfpackageFileContent.Contains("handlerVersion"), "Package.json should not have handler version");
        }

        /// <summary>
        /// Validates that the handler version is kept during serialization if value is one
        /// </summary>
        [Fact]
        public void KeepHandlerVersionIfHasValueOne()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Data", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Data Package"",
                      ""packageType"": ""Data"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """"
                        }}
                        ],
                      ""handlerVersion"": 1
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            Assert.True(cmfpackageFileContent.Contains("handlerVersion"), "Package.json should have handler version");
        }

        /// <summary>
        /// Validates that the handler version is kept during serialization if value is two
        /// </summary>
        [Fact]
        public void KeepHandlerVersionIfHasValueTwo()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Data", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Data Package"",
                      ""packageType"": ""Data"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """"
                        }}
                        ],
                      ""handlerVersion"": 2
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            Assert.True(cmfpackageFileContent.Contains("handlerVersion"), "Package.json should have handler version");
        }

        /// <summary>
        /// Validates that the isMandatory property in Dependency is not kept during serialization if value is true
        /// </summary>
        [TestMethod]
        public void DoNotKeepIsMandatoryPropInDependencyIfValueIsTrue()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Package", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Custom Package"",
                      ""packageType"": ""Generic"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""dependencies"": [
                        {{
                            ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                            ""version"": ""8.3.*"",
                            ""mandatory"": ""true""
                        }}
                      ],
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """"
                        }}
                        ],
                      ""handlerVersion"": 2
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            Assert.IsNotNull(cmfPackageObj.HandlerVersion);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            Assert.IsFalse(cmfpackageFileContent.Contains("mandatory"), "Package.json should not have ismandatory in dependency");
        }

        /// <summary>
        /// Validates that the isMandatory property in Dependency is not added during serialization
        /// </summary>
        [TestMethod]
        public void DoNotAddIsMandatoryPropInDependency()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Package", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Custom Package"",
                      ""packageType"": ""Generic"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""dependencies"": [
                        {{
                            ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                            ""version"": ""8.3.*""
                        }}
                      ],
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """"
                        }}
                        ],
                      ""handlerVersion"": 2
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            Assert.IsNotNull(cmfPackageObj.HandlerVersion);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            Assert.IsFalse(cmfpackageFileContent.Contains("mandatory"), "Package.json should not have ismandatory in dependency");
        }

        /// <summary>
        /// Validates that the isMandatory property in Dependency is kept during serialization if value is false
        /// </summary>
        [TestMethod]
        public void KeepIsMandatoryPropInDependencyIfValueIsFalse()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Package", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Custom Package"",
                      ""packageType"": ""Generic"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""dependencies"": [
                        {{
                            ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                            ""version"": ""8.3.*"",
                            ""mandatory"": ""false""
                        }}
                      ],
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """"
                        }}
                        ],
                      ""handlerVersion"": 2
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            Assert.IsNotNull(cmfPackageObj.HandlerVersion);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            Assert.IsTrue(cmfpackageFileContent.Contains("mandatory"), "Package.json should have ismandatory in dependency");
        }

        /// <summary>
        /// Validates that the isMandatory property in Dependency is not kept during serialization if value is true
        /// </summary>
        [TestMethod]
        public void DoNotKeepIsMandatoryPropInDependencyIfValueIsTrue()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Package", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Custom Package"",
                      ""packageType"": ""Generic"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""dependencies"": [
                        {{
                            ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                            ""version"": ""8.3.*"",
                            ""mandatory"": ""true""
                        }}
                      ],
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """"
                        }}
                        ],
                      ""handlerVersion"": 2
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            Assert.IsNotNull(cmfPackageObj.HandlerVersion);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            Assert.IsFalse(cmfpackageFileContent.Contains("mandatory"), "Package.json should not have ismandatory in dependency");
        }

        /// <summary>
        /// Validates that the isMandatory property in Dependency is not added during serialization
        /// </summary>
        [TestMethod]
        public void DoNotAddIsMandatoryPropInDependency()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Package", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Custom Package"",
                      ""packageType"": ""Generic"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""dependencies"": [
                        {{
                            ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                            ""version"": ""8.3.*""
                        }}
                      ],
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """"
                        }}
                        ],
                      ""handlerVersion"": 2
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            Assert.IsNotNull(cmfPackageObj.HandlerVersion);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            Assert.IsFalse(cmfpackageFileContent.Contains("mandatory"), "Package.json should not have ismandatory in dependency");
        }

        /// <summary>
        /// Validates that the isMandatory property in Dependency is kept during serialization if value is false
        /// </summary>
        [TestMethod]
        public void KeepIsMandatoryPropInDependencyIfValueIsFalse()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Package", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Custom Package"",
                      ""packageType"": ""Generic"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": true,
                      ""dependencies"": [
                        {{
                            ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                            ""version"": ""8.3.*"",
                            ""mandatory"": ""false""
                        }}
                      ],
                      ""contentToPack"": [
                        {{
                            ""source"": ""*"",
                            ""target"": """"
                        }}
                        ],
                      ""handlerVersion"": 2
                }}")}
            });

            IFileInfo cmfpackageFile = fileSystem.FileInfo.FromFileName($"repo/{CliConstants.CmfPackageFileName}");
            CmfPackage cmfPackageObj = CmfPackage.Load(cmfpackageFile, fileSystem: fileSystem);

            Assert.IsNotNull(cmfPackageObj.HandlerVersion);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            Assert.IsTrue(cmfpackageFileContent.Contains("mandatory"), "Package.json should have ismandatory in dependency");
        }
    }
}

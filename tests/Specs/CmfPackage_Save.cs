using Cmf.CLI.Commands;
using Cmf.CLI.Constants;
using Cmf.CLI.Objects;
using FluentAssertions;
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
using Cmf.CLI.Core.Objects;
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
            cmfpackageFileContent.Should().NotContain("handlerVersion", "cmfpackage.json should not have handler version");
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
            cmfpackageFileContent.Should().Contain("handlerVersion", "cmfpackage.json should have handler version");
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
            cmfpackageFileContent.Should().Contain("handlerVersion", "cmfpackage.json should have handler version");
        }

        /// <summary>
        /// Validates that the isMandatory property in Dependency is not kept during serialization if value is true
        /// </summary>
        [Fact]
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

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            cmfpackageFileContent.Should().NotContain("mandatory", "cmfpackage.json should not have ismandatory in dependency");
        }

        /// <summary>
        /// Validates that the isMandatory property in Dependency is not added during serialization
        /// </summary>
        [Fact]
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

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            cmfpackageFileContent.Should().NotContain("mandatory", "cmfpackage.json should not have ismandatory in dependency");
        }

        /// <summary>
        /// Validates that the isMandatory property in Dependency is kept during serialization if value is false
        /// </summary>
        [Fact]
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

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            cmfpackageFileContent.Should().Contain("mandatory", "cmfpackage.json should have ismandatory in dependency");
        }

        /// <summary>
        /// Validates that the DFPackageType property is not added during serialization
        /// </summary>
        [Fact]
        public void DoNotAddDFPackageTypeProp()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Random.Package", "1.1.0");

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

            Assert.Null(cmfPackageObj.DFPackageType);

            cmfPackageObj.SaveCmfPackage();

            var cmfpackageFileContent = fileSystem.File.ReadAllText(cmfpackageFile.FullName);
            cmfpackageFileContent.Should().NotContain("dfPackageType", "cmfpackage.json should not have DFPackageType");
        }

        /// <summary>
        /// Validates that the DFPackageType property is kept during serialization
        /// </summary>
        [Fact]
        public void KeepDFPackageTypeProp()
        {
            KeyValuePair<string, string> package = new("Cmf.Custom.Random.Package", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                      ""packageId"": ""{ package.Key }"",
                      ""version"": ""{ package.Value }"",
                      ""description"": ""Custom Package"",
                      ""packageType"": ""Generic"",
                      ""dFPackageType"": ""Data"",
                      ""isUniqueInstall"": true,
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
            cmfpackageFileContent.Should().Contain("dfPackageType", "cmfpackage.json should have DFPackageType");
        }
    }
}

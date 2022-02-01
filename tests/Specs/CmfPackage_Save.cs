using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace tests.Specs
{
    /// <summary>
    /// The cmf package_ save.
    /// </summary>
    [TestClass]
    public class CmfPackage_Save
    {
        /// <summary>
        /// Validates that the enums are serialized as string and not as int
        /// </summary>
        [TestMethod]
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
            Assert.AreEqual("Data", cmfpackageFileContent.packageType.ToString());
            Assert.AreEqual("CreateIntegrationEntries", cmfpackageFileContent.steps[0].type.ToString());
            Assert.AreEqual("ImportObject", cmfpackageFileContent.steps[0].messageType.ToString());
            Assert.AreEqual("Generic", cmfpackageFileContent.contentToPack[0].contentType.ToString());
            Assert.AreEqual("Pack", cmfpackageFileContent.contentToPack[0].action.ToString());
        }

        /// <summary>
        /// Validates that the handler version is not serialized
        /// </summary>
        [TestMethod]
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

            Assert.IsNotNull(cmfPackageObj.HandlerVersion);

            cmfPackageObj.SaveCmfPackage();

            dynamic cmfpackageFileContent = JsonConvert.DeserializeObject(fileSystem.File.ReadAllText(cmfpackageFile.FullName));
            Assert.IsNull(cmfpackageFileContent.handlerVersion);
        }
    }
}

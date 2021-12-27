using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using tests.Objects;

namespace tests.Specs
{
    [TestClass]
    public class Assemble
    {
        [TestMethod]
        public void Assemble_FromCIRepo()
        {
            string cirepo = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep.Key}"",
                        ""version"": ""{packageDep.Value}""
                    }},
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }},
                    {{
                        ""id"": ""{packageDep2.Key}"",
                        ""version"": ""{packageDep2.Value}""
                    }}
                  ]
                }}")},
                { assembleOutputDir.Key, assembleOutputDir.Value },
                { @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip", new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value, new Dictionary<string, string>() { { packageDep1.Key, packageDep1.Value}  }).ToMockFileData() },
                { @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData() },
                { @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData() }
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            assembleCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName("test"), fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key), new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, false);

            IEnumerable<string> assembledFiles = fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key).EnumerateFiles("*.zip").Select(f => f.Name);
            Assert.AreEqual(3, assembledFiles.Count());

            Assert.IsTrue(assembledFiles.Contains($"{packageRoot.Key}.{packageRoot.Value}.zip"));
            Assert.IsTrue(assembledFiles.Contains($"{packageDep1.Key}.{packageDep1.Value}.zip"));
            Assert.IsTrue(assembledFiles.Contains($"{packageDep2.Key}.{packageDep2.Value}.zip"));

            IFileInfo dependenciesJsonFile = fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key).EnumerateFiles(CliConstants.FileDependencies).FirstOrDefault();
            Assert.IsNotNull(dependenciesJsonFile);
            Assert.IsTrue(dependenciesJsonFile.Exists);
            Assert.AreEqual("{}", dependenciesJsonFile.OpenText().ReadToEnd());
        }

        [TestMethod]
        public void Assemble_WithMissingPackages()
        {
            string cirepo1 = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep.Key}"",
                        ""version"": ""{packageDep.Value}""
                    }},
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }},
                    {{
                        ""id"": ""{packageDep2.Key}"",
                        ""version"": ""{packageDep2.Value}""
                    }}
                  ]
                }}")},
                { @$"{cirepo1}/{packageRoot.Key}.{packageRoot.Value}.zip", new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value).ToMockFileData() },
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            string message = string.Empty;
            try
            {
                assembleCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName("test"), fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key), new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo1 }.Uri, null, false);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            List<string> missingPackages = new();
            missingPackages.Add($"{packageDep1.Key}@{packageDep1.Value}");
            missingPackages.Add($"{packageDep2.Key}@{packageDep2.Value}");

            string expectedErrorMessage = string.Format("Some packages were not found: {0}", string.Join(", ", missingPackages));
            Assert.AreEqual(expectedErrorMessage, message);
        }

        [TestMethod]
        public void Assemble_WithMultipleRepos()
        {
            string cirepo = @"/cirepo";
            string repo1 = MockUnixSupport.Path(@"x:\repo1");
            string repo2 = MockUnixSupport.Path(@"y:\repo2");
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep.Key}"",
                        ""version"": ""{packageDep.Value}""
                    }},
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }},
                    {{
                        ""id"": ""{packageDep2.Key}"",
                        ""version"": ""{packageDep2.Value}""
                    }}
                  ]
                }}")},
                { assembleOutputDir.Key, assembleOutputDir.Value },
                { @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip", new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value, new Dictionary<string, string>() { { packageDep1.Key, packageDep1.Value}  }).ToMockFileData() },
                { @$"{repo1}/{packageDep1.Key}.{packageDep1.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData() },
                { @$"{repo2}/{packageDep2.Key}.{packageDep2.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData() }
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            Uri[] repos = new[] { new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = repo1 }.Uri, new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = repo2 }.Uri };
            assembleCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName("test"), fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key), new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, repos, false);

            IEnumerable<string> assembledFiles = fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key).EnumerateFiles("*.zip").Select(f => f.Name);
            Assert.AreEqual(1, assembledFiles.Count());

            Assert.IsTrue(assembledFiles.Contains($"{packageRoot.Key}.{packageRoot.Value}.zip"));
            Assert.IsFalse(assembledFiles.Contains($"{packageDep1.Key}.{packageDep1.Value}.zip"));
            Assert.IsFalse(assembledFiles.Contains($"{packageDep2.Key}.{packageDep2.Value}.zip"));

            IFileInfo dependenciesJsonFile = fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key).EnumerateFiles(CliConstants.FileDependencies).FirstOrDefault();
            Assert.IsNotNull(dependenciesJsonFile);
            Assert.IsTrue(dependenciesJsonFile.Exists);
            string expectedContent = @$"{{""{packageDep1.Key}@{packageDep1.Value}"":""{MockUnixSupport.Path($@"{repo1}\{packageDep1.Key}.{packageDep1.Value}.zip").Replace("\\", "\\\\")}"",""{packageDep2.Key}@{packageDep2.Value}"":""{MockUnixSupport.Path($@"{repo2}\{packageDep2.Key}.{packageDep2.Value}.zip").Replace("\\", "\\\\")}""}}";
            Assert.AreEqual(expectedContent, dependenciesJsonFile.OpenText().ReadToEnd());
        }

        [TestMethod]
        public void Assemble_FromCIRepo_WithTestPackage()
        {
            string cirepo = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, string> packageTest = new("Cmf.Custom.Tests", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep.Key}"",
                        ""version"": ""{packageDep.Value}""
                    }},
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }},
                    {{
                        ""id"": ""{packageDep2.Key}"",
                        ""version"": ""{packageDep2.Value}""
                    }}
                  ],
                  ""testPackages"": [
                    {{
                         ""id"": ""{packageTest.Key}"",
                        ""version"": ""{packageTest.Value}""
                    }}
                  ]
                }}")},
                { assembleOutputDir.Key, assembleOutputDir.Value },
                { @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip", new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value, new() { { packageDep1.Key, packageDep1.Value}  }, new() { { packageTest.Key, packageTest.Value} }).ToMockFileData() },
                { @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData() },
                { @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData() },
                { @$"{cirepo}/{packageTest.Key}.{packageTest.Value}.zip",  new DFPackageBuilder().CreateEntry($"{packageTest.Key}.{packageTest.Value}.zip", string.Empty).ToMockFileData() }
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            assembleCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName("test"), fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key), new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, true);

            IEnumerable<string> assembledFiles = fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key).EnumerateFiles("*.zip").Select(f => f.Name);
            Assert.AreEqual(3, assembledFiles.Count());

            Assert.IsTrue(assembledFiles.Contains($"{packageRoot.Key}.{packageRoot.Value}.zip"));
            Assert.IsTrue(assembledFiles.Contains($"{packageDep1.Key}.{packageDep1.Value}.zip"));
            Assert.IsTrue(assembledFiles.Contains($"{packageDep2.Key}.{packageDep2.Value}.zip"));

            IEnumerable<string> assembledTestFiles = fileSystem.DirectoryInfo.FromDirectoryName(@$"{assembleOutputDir.Key}/Tests").EnumerateFiles("*.zip").Select(f => f.Name);
            Assert.AreEqual(1, assembledTestFiles.Count());

            Assert.IsTrue(assembledTestFiles.Contains($"{packageTest.Key}.{packageTest.Value}.zip"));
        }

        [TestMethod]
        public void Assemble_WithDefaultDependenciesToIgnore()
        {
            string cirepo = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep1 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");            
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new MockFileData(
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
                }}")},
                { @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip", new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value, new Dictionary<string, string>() { { packageDep1.Key, packageDep1.Value}  }).ToMockFileData() },
                { @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData() },
            });

            var assembleCommand = new AssembleCommand(fileSystem);

            assembleCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName("test"), fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key), new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, false);

            IEnumerable<string> assembledFiles = fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key).EnumerateFiles("*.zip").Select(f => f.Name);
            Assert.AreEqual(1, assembledFiles.Count());

            Assert.IsTrue(assembledFiles.Contains($"{packageRoot.Key}.{packageRoot.Value}.zip"));
            Assert.IsFalse(assembledFiles.Contains($"{packageDep1.Key}.{packageDep1.Value}.zip"));

            IFileInfo dependenciesJsonFile = fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key).EnumerateFiles(CliConstants.FileDependencies).FirstOrDefault();
            Assert.IsNotNull(dependenciesJsonFile);
            Assert.IsTrue(dependenciesJsonFile.Exists);
            Assert.AreEqual("{}", dependenciesJsonFile.OpenText().ReadToEnd());
        }

        [TestMethod]
        public void Assemble_FromCIRepo_WithoutTestPackage()
        {
            string cirepo = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, string> packageTest = new("Cmf.Custom.Tests", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageDep.Key}"",
                        ""version"": ""{packageDep.Value}""
                    }},
                    {{
                         ""id"": ""{packageDep1.Key}"",
                        ""version"": ""{packageDep1.Value}""
                    }},
                    {{
                        ""id"": ""{packageDep2.Key}"",
                        ""version"": ""{packageDep2.Value}""
                    }}
                  ],
                  ""testPackages"": [
                    {{
                         ""id"": ""{packageTest.Key}"",
                        ""version"": ""{packageTest.Value}""
                    }}
                  ]
                }}")},
                { assembleOutputDir.Key, assembleOutputDir.Value },
                { @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip", new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value, new() { { packageDep1.Key, packageDep1.Value}  }, new() { { packageTest.Key, packageTest.Value} }).ToMockFileData() },
                { @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData() },
                { @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData() }
            });


            try
            {
                var assembleCommand = new AssembleCommand(fileSystem);
                assembleCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName("test"), fileSystem.DirectoryInfo.FromDirectoryName(assembleOutputDir.Key), new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, true);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains($"Some packages were not found: {packageTest.Key}.{packageTest.Value}.zip"), ex.Message.ToString());
            }

            

            
        }
    }
}

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Cmf.CLI.Commands;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using FluentAssertions;
using tests.Objects;
using Xunit;

namespace tests.Specs
{
    public class Assemble
    {
        [Fact]
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
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                }}")
                },
                { assembleOutputDir.Key, assembleOutputDir.Value },
                {
                    @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value,
                        new Dictionary<string, string>() { { packageDep1.Key, packageDep1.Value } }).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData()
                }
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, false);

            IEnumerable<string> assembledFiles = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles("*.zip").Select(f => f.Name).ToList();
            Assert.Equal(3, assembledFiles.Count());

            Assert.Contains($"{packageRoot.Key}.{packageRoot.Value}.zip", assembledFiles);
            Assert.Contains($"{packageDep1.Key}.{packageDep1.Value}.zip", assembledFiles);
            Assert.Contains($"{packageDep2.Key}.{packageDep2.Value}.zip", assembledFiles);

            IFileInfo dependenciesJsonFile = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles(CliConstants.FileDependencies).FirstOrDefault();
            Assert.NotNull(dependenciesJsonFile);
            Assert.True(dependenciesJsonFile?.Exists ?? false, "Dependencies file does not exist");
            Assert.Equal("{}", dependenciesJsonFile.OpenText().ReadToEnd());
        }

        [Fact]
        public void Assemble_FromCIRepoTwice_OriginalFileIsDeleted()
        {
             string cirepo = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                }}")
                },
                { assembleOutputDir.Key, assembleOutputDir.Value },
                {
                    @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value,
                        new Dictionary<string, string>() { { packageDep1.Key, packageDep1.Value } }).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData()
                }
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, false);

            IEnumerable<IFileInfo> assembledFiles = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles("*.zip").ToList();
            assembledFiles.Should().HaveCount(3);

            var originalFile = assembledFiles.FirstOrDefault(q =>
                q.Name.IgnoreCaseEquals($"{packageRoot.Key}.{packageRoot.Value}.zip"));
            originalFile.Name.Should().NotBeNull();
            
            assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, false);

            assembledFiles = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles("*.zip").ToList();
            assembledFiles.Should().HaveCount(3);
            originalFile.CreationTime.Should().NotBe(assembledFiles.FirstOrDefault(q =>
                q.Name.IgnoreCaseEquals($"{packageRoot.Key}.{packageRoot.Value}.zip"))?.CreationTime);
        }

        [Fact]
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
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                }}")
                },
                {
                    @$"{cirepo1}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value).ToMockFileData()
                },
            });

            var assembleCommand = new AssembleCommand(fileSystem);


            List<string> missingPackages = new();
            missingPackages.Add($"{packageDep1.Key}@{packageDep1.Value}");
            missingPackages.Add($"{packageDep2.Key}@{packageDep2.Value}");

            string expectedErrorMessage =
                string.Format("Some packages were not found: {0}", string.Join(", ", missingPackages));
            var act = () => assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo1 }.Uri, null, false);
            act.Should().Throw<Exception>().WithMessage(expectedErrorMessage);
        }
        
        [Fact]
        public void Assemble_MainPackageIsNotRoot()
            {
            string cirepo = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {
                    "/test/cmfpackage.json", new MockFileData(
                        @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Generic"",
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
                }}")
                },
                { assembleOutputDir.Key, assembleOutputDir.Value },
                {
                    @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value,
                        new Dictionary<string, string>() { { packageDep1.Key, packageDep1.Value } }).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData()
            }
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            
            var act = () => assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, false);
            act.Should().Throw<Exception>().WithMessage("This is not a root package");
        }
                
        [Fact]
        public void Assemble_MainPackageDoesNotExist()
            {
            string cirepo = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                }}")
                },
                { assembleOutputDir.Key, assembleOutputDir.Value },
                {
                    @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData()
            }
            });

            var assembleCommand = new AssembleCommand(fileSystem);

            var act = () => assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, false);
            act.Should().Throw<Exception>().WithMessage($"{packageRoot.Key}.{packageRoot.Value} not found!");
        }

        [Fact]
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
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                }}")
                },
                { assembleOutputDir.Key, assembleOutputDir.Value },
                {
                    @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value,
                        new Dictionary<string, string>() { { packageDep1.Key, packageDep1.Value } }).ToMockFileData()
                },
                {
                    @$"{repo1}/{packageDep1.Key}.{packageDep1.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData()
                },
                {
                    @$"{repo2}/{packageDep2.Key}.{packageDep2.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData()
                }
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            Uri[] repos = new[]
            {
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = repo1 }.Uri,
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = repo2 }.Uri
            };
            assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, repos, false);

            IEnumerable<string> assembledFiles = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles("*.zip").Select(f => f.Name);
            Assert.Single(assembledFiles);

            Assert.Contains($"{packageRoot.Key}.{packageRoot.Value}.zip", assembledFiles);
            Assert.DoesNotContain($"{packageDep1.Key}.{packageDep1.Value}.zip", assembledFiles);
            Assert.DoesNotContain($"{packageDep2.Key}.{packageDep2.Value}.zip", assembledFiles);

            IFileInfo dependenciesJsonFile = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles(CliConstants.FileDependencies).FirstOrDefault();
            Assert.NotNull(dependenciesJsonFile);
            Assert.True(dependenciesJsonFile.Exists);
            string expectedContent =
                @$"{{""{packageDep1.Key}@{packageDep1.Value}"":""{MockUnixSupport.Path($@"{repo1}\{packageDep1.Key}.{packageDep1.Value}.zip").Replace("\\", "\\\\")}"",""{packageDep2.Key}@{packageDep2.Value}"":""{MockUnixSupport.Path($@"{repo2}\{packageDep2.Key}.{packageDep2.Value}.zip").Replace("\\", "\\\\")}""}}";
            Assert.Equal(expectedContent, dependenciesJsonFile.OpenText().ReadToEnd());
        }

        [Fact]
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
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                }}")
                },
                { assembleOutputDir.Key, assembleOutputDir.Value },
                {
                    @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value,
                        new() { { packageDep1.Key, packageDep1.Value } },
                        new() { { packageTest.Key, packageTest.Value } }).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageTest.Key}.{packageTest.Value}.zip",
                    new DFPackageBuilder().CreateEntry($"{packageTest.Key}.{packageTest.Value}.zip", string.Empty)
                        .ToMockFileData()
                }
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, true);

            IEnumerable<string> assembledFiles = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles("*.zip").Select(f => f.Name).ToList();
            Assert.Equal(3, assembledFiles.Count());

            Assert.Contains($"{packageRoot.Key}.{packageRoot.Value}.zip", assembledFiles);
            Assert.Contains($"{packageDep1.Key}.{packageDep1.Value}.zip", assembledFiles);
            Assert.Contains($"{packageDep2.Key}.{packageDep2.Value}.zip", assembledFiles);

            IEnumerable<string> assembledTestFiles = fileSystem.DirectoryInfo.New(@$"{assembleOutputDir.Key}/Tests")
                .EnumerateFiles("*.zip").Select(f => f.Name).ToList();
            Assert.Single(assembledTestFiles);

            Assert.Contains($"{packageTest.Key}.{packageTest.Value}.zip", assembledTestFiles);
        }

        [Fact]
        public void Assemble_FromCIRepo_IncludeTestPackagesFlagActive_NoPackagesGiven()
        {
            string cirepo = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                  ""testPackages"": []
                }}")
                },
                { assembleOutputDir.Key, assembleOutputDir.Value },
                {
                    @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value,
                        new() { { packageDep1.Key, packageDep1.Value } }, new() { }).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData()
                },
            });

            var assembleCommand = new AssembleCommand(fileSystem);
            StringWriter standardOutput = (new Logging()).GetLogStringWriter();
            assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, true);

            IEnumerable<string> assembledFiles = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles("*.zip").Select(f => f.Name).ToList();
            Assert.Equal(3, assembledFiles.Count());

            Assert.Contains($"{packageRoot.Key}.{packageRoot.Value}.zip", assembledFiles);
            Assert.Contains($"{packageDep1.Key}.{packageDep1.Value}.zip", assembledFiles);
            Assert.Contains($"{packageDep2.Key}.{packageDep2.Value}.zip", assembledFiles);

            IEnumerable<string> assembledTestFiles = fileSystem.DirectoryInfo.New(@$"{assembleOutputDir.Key}/Tests")
                .EnumerateFiles("*.zip").Select(f => f.Name).ToList();
            assembledTestFiles.Should().BeEmpty();
            standardOutput.ToString().Trim().Should()
                .Contain($"Package {packageRoot.Key}.{packageRoot.Value} has no test packages");
        }

        [Fact]
        public void Assemble_WithDefaultDependenciesToIgnore()
        {
            string cirepo = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep1 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");            
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                }}")
                },
                {
                    @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value,
                        new Dictionary<string, string>() { { packageDep1.Key, packageDep1.Value } }).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData()
                },
            });

            var assembleCommand = new AssembleCommand(fileSystem);

            assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, false);

            IEnumerable<string> assembledFiles = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles("*.zip").Select(f => f.Name).ToList();
            Assert.Single(assembledFiles);

            Assert.Contains($"{packageRoot.Key}.{packageRoot.Value}.zip", assembledFiles);
            Assert.DoesNotContain($"{packageDep1.Key}.{packageDep1.Value}.zip", assembledFiles);

            IFileInfo dependenciesJsonFile = fileSystem.DirectoryInfo.New(assembleOutputDir.Key)
                .EnumerateFiles(CliConstants.FileDependencies).FirstOrDefault();
            Assert.NotNull(dependenciesJsonFile);
            Assert.True(dependenciesJsonFile!.Exists);
            Assert.Equal("{}", dependenciesJsonFile.OpenText().ReadToEnd());
        }

        [Fact]
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
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                }}")
                },
                { assembleOutputDir.Key, assembleOutputDir.Value },
                {
                    @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value,
                        new() { { packageDep1.Key, packageDep1.Value } },
                        new() { { packageTest.Key, packageTest.Value } }).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData()
                },
                {
                    @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData()
                }
            });
            ExecutionContext.Initialize(fileSystem);

            var assembleCommand = new AssembleCommand(fileSystem);
            var act = () => assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key),
                new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = cirepo }.Uri, null, true);
            act.Should().Throw<Exception>()
                .WithMessage($"Some packages were not found: {packageTest.Key}.{packageTest.Value}.zip");
        }

        [Fact]
        public void Assemble_WithoutDefiningCIRepo()
        {
            string cirepo1 = @"/cirepo";
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new("/test/assemble/", new());

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {
                    "/test/cmfpackage.json", new MockFileData(
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
                }}")
                },
            {
                    @$"{cirepo1}/{packageRoot.Key}.{packageRoot.Value}.zip",
                    new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value).ToMockFileData()
                },
            });

                var assembleCommand = new AssembleCommand(fileSystem);

            var act = () => assembleCommand.Execute(fileSystem.DirectoryInfo.New("test"),
                fileSystem.DirectoryInfo.New(assembleOutputDir.Key), null, null, false);
            act.Should().Throw<Exception>().WithMessage($"Missing mandatory option cirepo");
            }

        [Fact]
        public void Assemble_ConfigureCommand()
            {
            Command cmd = new Command("assemble", "Assemble a package");
            new AssembleCommand().Configure(cmd);
            var parseResult = cmd.Parse("dir --outputDir output");

            cmd.Options.Should().HaveCount(4);
            cmd.Options.Should().Contain(o => o.Name == "--outputDir" || o.Aliases.Any(a => a == "--outputDir"));
            cmd.Options.Should().Contain(o => o.Name == "--cirepo" || o.Aliases.Any(a => a == "--cirepo"));
            cmd.Options.Should().Contain(o => o.Name == "--repo" || o.Aliases.Any(a => a == "--repo" || a == "--repos"));
            cmd.Options.Should().Contain(o => o.Name == "--includeTestPackages" || o.Aliases.Any(a => a == "--includeTestPackages"));
            
            cmd.Arguments.Should().HaveCount(1);
            cmd.Arguments.Should().Contain(a => a.Name == "workingDir");

            // In beta5, Command.Handler property was removed - handlers are managed differently
            // The handler is set internally via SetHandler, but there's no public Handler property to check
            // cmd.Handler.Should().NotBeNull(); // Removed in beta5

            parseResult.Should().NotBeNull();
            // In beta5, GetValueForArgument/GetValueForOption signatures changed - use GetValue<T>() with explicit type
            var workingDirArg = cmd.Arguments.ElementAt(0) as Argument<IDirectoryInfo>;
            var outputDirOption = cmd.Options.First(o=> o.Name == "--outputDir" || o.Aliases.Any(a => a == "--outputDir")) as Option<IDirectoryInfo>;
            parseResult.GetValue(workingDirArg)?.Name.Should().Be("dir");
            parseResult.GetValue(outputDirOption)?.Name.Should().Be("output");
        }

        [Fact]
        public void Assemble_FromRepositoriesJson()
        {
            string cirepo = MockUnixSupport.Path(@"y:\cirepo");
            string approvedrepo = MockUnixSupport.Path(@"y:\approvedrepo");
            string npmrepo = "https://server.io/api/npm/1234/dev";
            string root = MockUnixSupport.Path(@"x:\test");
            
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Business", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Html", "1.1.0");
            KeyValuePair<string, string> packageTest = new("Cmf.Custom.Tests", "1.1.0");
            KeyValuePair<string, MockDirectoryData> assembleOutputDir = new($"/test/assemble/", new());
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { MockUnixSupport.Path($"{root}/cmfpackage.json"), new MockFileData(
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
                {
                    MockUnixSupport.Path($"{root}/repositories.json"), new MockFileData(
                    @$"{{
                        ""CIRepository"": ""{cirepo.Replace(@"\", @"\\")}"",
                        ""Repositories"": [
                            ""{npmrepo.Replace(@"\", @"\\")}"",
                            ""{approvedrepo.Replace(@"\", @"\\")}""
                        ]
                    }}"
                )},
                { assembleOutputDir.Key, assembleOutputDir.Value },
                { @$"{cirepo}/{packageRoot.Key}.{packageRoot.Value}.zip", new DFPackageBuilder().CreateManifest(packageRoot.Key, packageRoot.Value, new() { { packageDep1.Key, packageDep1.Value}  }, new() { { packageTest.Key, packageTest.Value} }).ToMockFileData() },
                { @$"{cirepo}/{packageDep1.Key}.{packageDep1.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep1.Key, packageDep1.Value).ToMockFileData() },
                { @$"{cirepo}/{packageDep2.Key}.{packageDep2.Value}.zip", new DFPackageBuilder().CreateManifest(packageDep2.Key, packageDep2.Value).ToMockFileData() },
                { @$"{cirepo}/{packageTest.Key}.{packageTest.Value}.zip", new DFPackageBuilder().CreateManifest(packageTest.Key, packageTest.Value).ToMockFileData() },
                { $"{approvedrepo}/.gitkeep", ""}
            });

            fileSystem.Directory.SetCurrentDirectory(root);
            ExecutionContext.Initialize(fileSystem);
            
            var assembleCommand = new AssembleCommand(fileSystem);
            assembleCommand.Execute(fileSystem.DirectoryInfo.New(root), fileSystem.DirectoryInfo.New(assembleOutputDir.Key), null, null, true);
        }
    }
}

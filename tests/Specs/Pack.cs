using System;
using System.Collections.Generic;
using Cmf.CLI.Commands;
using Xunit;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.IO;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Handlers;
using Cmf.CLI.Utilities;
using Cmf.Common.Cli.TestUtilities;
using FluentAssertions;
using tests.Objects;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Factories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NuGet.Packaging;

namespace tests.Specs
{
    public class Pack
    {
        public Pack()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<IProjectConfigService>(new ProjectConfigService())
                .BuildServiceProvider();
        }

        [Fact]
        public void Args_Paths_BothSpecified()
        {
            string _workingDir = null;
            string _outputDir = null;
            bool? _force = null;

            var packCommand = new PackCommand();
            var cmd = new Command("pack");
            packCommand.Configure(cmd);

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, string, bool, bool>(
            (workingDir, outputDir, repo, force, skipDependencies) =>
            {
                _workingDir = workingDir.Name;
                _outputDir = outputDir.Name;
                _force = force;
            });

            var console = new TestConsole();
            cmd.Invoke(new[] {
                "-o", "test_package_dir", "working_dir"
            }, console);

            Assert.Equal("working_dir", _workingDir);
            Assert.Equal("test_package_dir", _outputDir);
            Assert.NotNull(_force);
            Assert.False(_force ?? true);
        }

        [Fact]
        public void Args_Paths_WorkDirSpecified()
        {
            string _workingDir = null;
            string _outputDir = null;
            bool? _force = null;

            var packCommand = new PackCommand();
            var cmd = new Command("pack");
            packCommand.Configure(cmd);

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, string, bool, bool>(
            (workingDir, outputDir, repo, force, skipDependencies) =>
            {
                _workingDir = workingDir.Name;
                _outputDir = outputDir.Name;
                _force = force;
            });

            var console = new TestConsole();
            cmd.Invoke(new[] {
                "working_dir"
            }, console);

            Assert.Equal("working_dir", _workingDir);
            Assert.Equal("Package", _outputDir);
            Assert.NotNull(_force);
            Assert.False(_force ?? true);
        }

        [Fact]
        public void Args_Paths_OutDirSpecified()
        {
            string _workingDir = null;
            string _outputDir = null;
            bool? _force = null;

            var packCommand = new PackCommand();
            var cmd = new Command("pack");
            packCommand.Configure(cmd);

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, string, bool, bool>(
            (workingDir, outputDir, repo, force, skipDependencies) =>
            {
                _workingDir = workingDir.Name;
                _outputDir = outputDir.Name;
                _force = force;
            });

            var console = new TestConsole();
            cmd.Invoke(new[] {
                "-o", "test_package_dir"
            }, console);

            var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            Assert.Equal(curDir.Name, _workingDir);
            Assert.Equal("test_package_dir", _outputDir);
            Assert.NotNull(_force);
            Assert.False(_force ?? true);
        }

        [Fact]
        public void Args_Paths_NoneSpecified()
        {
            string _workingDir = null;
            string _outputDir = null;
            bool? _force = null;

            var packCommand = new PackCommand();
            var cmd = new Command("pack");
            packCommand.Configure(cmd);

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, string, bool, bool>(
            (workingDir, outputDir, repo, force, skipDependencies) =>
            {
                _workingDir = workingDir.Name;
                _outputDir = outputDir.Name;
                _force = force;
            });

            var console = new TestConsole();
            cmd.Invoke(new string[] {
            }, console);

            var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            Assert.Equal(curDir.Name, _workingDir);
            Assert.Equal("Package", _outputDir);
            Assert.NotNull(_force);
            Assert.False(_force ?? true);
        }

        [Fact]
        public void HTML()
        {
            var fileSystem = MockPackage.Html;

            var packCommand = new PackCommand(fileSystem);
            packCommand.Execute(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\ui")), fileSystem.DirectoryInfo.New("output"), false);

            IEnumerable<IFileInfo> assembledFiles = fileSystem.DirectoryInfo.New("output").EnumerateFiles("Cmf.Custom.HTML.1.1.0.zip").ToList();
            Assert.Single(assembledFiles);

            using (Stream zipToOpen = fileSystem.FileStream.New(assembledFiles.First().FullName, FileMode.Open))
            {
                using (ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read))
                {
                    // these tuples allow us to rewrite entry paths
                    var entriesToExtract = new List<Tuple<ZipArchiveEntry, string>>();
                    entriesToExtract.AddRange(zip.Entries.Select(selector: entry => new Tuple<ZipArchiveEntry, string>(entry, entry.FullName)));

                    List<string> expectedFiles = new()
                    {
                        "config.json",
                        "manifest.xml",
                        "node_modules/customization.package/package.json",
                        "node_modules/customization.package/customization.common.js"
                    };
                    Assert.Equal(expectedFiles.Count, entriesToExtract.Count);
                    foreach (var expectedFile in expectedFiles)
                    {
                        Assert.NotNull(entriesToExtract.FirstOrDefault(x => x.Item2.Equals(expectedFile)));
                    }
                }
            }
        }

        [Fact(Skip = "No System.IO.Abstractions support for searching outside the current directory")]
        public void HTML_OnlyLBOs()
        {
            var fileSystem = MockPackage.Html_OnlyLBOs;

            var packCommand = new PackCommand(fileSystem);
            packCommand.Execute(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\ui")), fileSystem.DirectoryInfo.New("output"), false);

            IEnumerable<IFileInfo> assembledFiles = fileSystem.DirectoryInfo.New("output").EnumerateFiles("Cmf.Custom.HTML.1.1.0.zip").ToList();
            Assert.Single(assembledFiles);

            using (Stream zipToOpen = fileSystem.FileStream.New(assembledFiles.First().FullName, FileMode.Open))
            {
                using (ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read))
                {
                    // these tuples allow us to rewrite entry paths
                    var entriesToExtract = new List<Tuple<ZipArchiveEntry, string>>();
                    entriesToExtract.AddRange(zip.Entries.Select(selector: entry => new Tuple<ZipArchiveEntry, string>(entry, entry.FullName)));

                    List<string> expectedFiles = new()
                    {
                        "manifest.xml",
                        MockUnixSupport.Path(@"node_modules\cmf.lbos\cmf.lbos.js"),
                        MockUnixSupport.Path(@"node_modules\cmf.lbos\APIReference.js")
                    };
                    Assert.Equal(expectedFiles.Count, entriesToExtract.Count);
                    foreach (var expectedFile in expectedFiles)
                    {
                        Assert.NotNull(entriesToExtract.FirstOrDefault(x => x.Item2.Equals(expectedFile)));
                    }
                }
            }
        }

        [Fact]
        public void CheckThatContentWasPacked_FailBecauseNoContentFound()
        {
            var fileSystem = MockPackage.Html_MissingDeclaredContent;

            var packCommand = new PackCommand(fileSystem);
            var exception = Assert.Throws<CliException>(() => packCommand.Execute(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\ui")), fileSystem.DirectoryInfo.New("output"), false));
            exception.Message.Should().Contain("Nothing was found on ContentToPack Sources");
        }

        [Fact]
        public void CheckThatContentWasPacked_DoNothingBecauseContentWasAlreadyPacked()
        {
            var fileSystem = MockPackage.Html;
            var outputDir = fileSystem.DirectoryInfo.New("output");

            var packCommand = new PackCommand(fileSystem);
            packCommand.Execute(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\ui")), outputDir, false);
            IEnumerable<IFileInfo> assembledFiles = fileSystem.DirectoryInfo.New("output").EnumerateFiles("Cmf.Custom.HTML.1.1.0.zip").ToList();
            packCommand.Execute(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\ui")), outputDir, false);
            
            IEnumerable<IFileInfo> assembledFilesOnSecondRun = fileSystem.DirectoryInfo.New("output").EnumerateFiles("Cmf.Custom.HTML.1.1.0.zip").ToList();
            assembledFilesOnSecondRun.Should().HaveCount(1);
            assembledFilesOnSecondRun.Should().HaveCount(assembledFiles.Count());
            assembledFiles.ElementAt(0).LastWriteTime.Should().Be(assembledFilesOnSecondRun.ElementAt(0).LastWriteTime);
        }

        [Fact]
        public void DontCheckThatContentWasPacked_IfMeta()
        {
            var fileSystem = MockPackage.Root_Empty;

            var packCommand = new PackCommand(fileSystem);
            packCommand.Execute(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\repo")), fileSystem.DirectoryInfo.New("output"), false);
        }

        [Fact]
        public void DontCheckThatContentWasPacked_IfContentToPackIsEmpty()
        {
            var fileSystem = MockPackage.Html_EmptyContentToPack;

            var packCommand = new PackCommand(fileSystem);
            var exception = Assert.Throws<CliException>(() => packCommand.Execute(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\ui")), fileSystem.DirectoryInfo.New("output"), false));
            exception.Message.Should().Contain("Missing mandatory property ContentToPack in file");
        }

        [Fact]
        public void Pack_SecurityPortal()
        {
            string dir = $"{TestUtilities.GetTmpDirectory()}/securityPortal";
            TestUtilities.CopyFixture("pack/securityPortal", new DirectoryInfo(dir));

            var projCfg = Path.Join(dir, ".project-config.json");
            if (File.Exists(projCfg))
            {
                File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                    .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                    .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                    .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                );
            }
            
            Directory.SetCurrentDirectory(dir);

            string _workingDir = dir;

            PackCommand packCommand = new PackCommand();
            Command cmd = new Command("pack");
            packCommand.Configure(cmd);

            TestConsole console = new TestConsole();
            cmd.Invoke(new string[] {
            }, console);

            DirectoryInfo curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            Assert.True(Directory.Exists($"{dir}/Package"), "Package folder is missing");
            Assert.True(File.Exists($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip"), "Zip package is missing");

            List<string> entries = TestUtilities.GetFileEntriesFromZip($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip");
            Assert.True(entries.HasAny(), "Zip package is empty");
            Assert.True(entries.HasAny(entry => entry == "manifest.xml"), "Manifest file does not exist");
            Assert.True(entries.HasAny(entry => entry == "config.json"), "Config file does not exist");

            string configJsonContent = FileSystemUtilities.GetFileContentFromPackage($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip", "config.json");

            Assert.True(configJsonContent.Contains("$.tenants.config.tenant.strategies"), "Config file does not have correct tenant");
        }

        [Fact]
        public void Pack_App()
        {
            var cur = Directory.GetCurrentDirectory();

            try
            {
                string dir = $"{TestUtilities.GetTmpDirectory()}/app";
                string packageName = "Cmf.Custom.Package.1.0.0.zip";
                string appfilesName = "MockName@1.0.0.zip";
                TestUtilities.CopyFixture("pack/app", new DirectoryInfo(dir));

                var projCfg = Path.Join(dir, ".project-config.json");
                if (File.Exists(projCfg))
                {
                    File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                        .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                        .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                        .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                    );
                }
                Directory.SetCurrentDirectory(dir);

                string _workingDir = dir;

                PackCommand packCommand = new();
                Command cmd = new("pack");
                packCommand.Configure(cmd);

                TestConsole console = new();
                cmd.Invoke(Array.Empty<string>(), console);

                DirectoryInfo curDir = new(System.IO.Directory.GetCurrentDirectory());

                Assert.True(Directory.Exists($"{dir}/Package"), "Package folder is missing");

                Assert.True(File.Exists($"{dir}/Package/{packageName}"), "Zip package is missing");

                List<string> entries = TestUtilities.GetFileEntriesFromZip($"{dir}/Package/{packageName}");
                Assert.True(entries.HasAny(), "Zip package is empty");
                Assert.True(entries.HasAny(entry => entry == "manifest.xml"), "Manifest file does not exist");

                Assert.True(File.Exists($"{dir}/Package/{appfilesName}"), "Zip app files is missing");

                List<string> appEntries = TestUtilities.GetFileEntriesFromZip($"{dir}/Package/{appfilesName}");
                Assert.True(appEntries.HasAny(entry => entry == "app_manifest.xml"), "App manifest file does not exist");
                Assert.True(appEntries.HasAny(entry => entry == "app_icon.png"), "App Icon does not exist");
                Assert.True(appEntries.HasAny(entry => entry == "app_deployment_manifest.xml"), "Deployment manifest does not exist");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
            }
        }

        [Fact]
        public void Pack_SecurityPortalV2()
        {
            string dir = $"{TestUtilities.GetTmpDirectory()}/securityPortal";
            TestUtilities.CopyFixture("pack/securityPortalV2", new DirectoryInfo(dir));

            var projCfg = Path.Join(dir, ".project-config.json");
            if (File.Exists(projCfg))
            {
                File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                    .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                    .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                    .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                );
            }

            Directory.SetCurrentDirectory(dir);

            string _workingDir = dir;

            PackCommand packCommand = new PackCommand();
            Command cmd = new Command("pack");
            packCommand.Configure(cmd);

            TestConsole console = new TestConsole();
            cmd.Invoke(new string[] {
            }, console);

            DirectoryInfo curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            Assert.True(Directory.Exists($"{dir}/Package"), "Package folder is missing");
            Assert.True(File.Exists($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip"), "Zip package is missing");

            List<string> entries = TestUtilities.GetFileEntriesFromZip($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip");
            Assert.True(entries.HasAny(), "Zip package is empty");
            Assert.True(entries.HasAny(entry => entry == "manifest.xml"), "Manifest file does not exist");
            Assert.True(entries.HasAny(entry => entry == "config.json"), "Config file does not exist");

            string configJsonContent = FileSystemUtilities.GetFileContentFromPackage($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip", "config.json");

            Assert.True(configJsonContent.Contains("$.tenants.config.tenant.strategies"), "Config file does not have correct tenant");

            // validate transform file
            string manifestXMLContent = FileSystemUtilities.GetFileContentFromPackage($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip", "manifest.xml");
            Assert.Contains("<step type=\"TransformFile\" file=\"config.json\" tagFile=\"true\" relativePath=\"./src/\" />", manifestXMLContent);
        }

        [Theory]
        [InlineData("8.1.0", new[] { StepType.DeployRepositoryFiles, StepType.GenerateRepositoryIndex })]
        [InlineData("9.1.0", new StepType[0])]
        public void IoTDFStepsForVersion(string version, StepType[] forbiddenStepTypes)
        {
            var mockFS = new MockFileSystem(new Dictionary<string, MockFileData>
            {{ MockUnixSupport.Path(@"c:\.project-config.json"), new MockFileData(
    $@"{{
                ""MESVersion"": ""{version}""
                }}")
            }, {
                MockUnixSupport.Path(@"c:\.pkg.json"), new MockFileData(
                    $@"{{
                ""type"": ""{PackageType.IoT}"",
                ""packageId"": ""xxxxx"",
                ""version"": ""9.9.9"",
                ""contentToPack"": [{{}}]
                }}")
            }});

            ExecutionContext.Initialize(mockFS);
            var pkg = CmfPackage.Load(mockFS.FileSystem.FileInfo.New(MockUnixSupport.Path(@"c:\.pkg.json")), true,
                mockFS);
            var _ = new IoTPackageTypeHandler(pkg);

            pkg.Steps.Any(step => forbiddenStepTypes.ToList().Contains(step.Type ?? StepType.Generic)).Should()
                .BeFalse();
        }

        [Theory]
        [InlineData("Html", "1.1.0")]
        [InlineData("IoT", null)]
        public void GeneratePresentationConfigFile(string packageType, string version)
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                // project config file
                { ".project-config.json", new MockFileData(
                    @$"{{
                        ""MESVersion"": ""9.0.0""
                    }}")},

                // root cmfpackage file
                { $"cmfpackage.json", new MockFileData(
                @$"{{
                    ""packageId"": ""{packageRoot.Key}"",
                    ""version"": ""{packageRoot.Value}"",
                    ""description"": ""This package deploys Critical Manufacturing Customization"",
                    ""packageType"": ""Root"",
                    ""isInstallable"": true,
                    ""isUniqueInstall"": false,
                    ""dependencies"": [
                    {{ ""id"": ""Cmf.Environment"", ""version"": ""0.0.0"" }}
                    ]
                }}")},

                // cmfpackage file
                { $"Cmf.Custom.{packageType}/cmfpackage.json", new MockFileData(
                $@"{{
                  ""packageId"": ""Cmf.Custom.{packageType}"",
                  ""version"": ""{version}"",
                  ""description"": ""Cmf Custom {packageType} Package"",
                  ""packageType"": ""{packageType}"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""contentToPack"": [
                    {{
                      ""source"": ""{MockUnixSupport.Path("src\\packages\\*").Replace("\\", "\\\\")}"",
                      ""target"": ""node_modules"",
                      ""ignoreFiles"": [
                        "".npmignore""
                      ]
                    }}
                  ]
                }}")},

                // js package
                { $"Cmf.Custom.{packageType}/src/packages/customization.common/package.json", new MockFileData(@"{""name"": ""customization.package""}")},
                { $"Cmf.Custom.{packageType}/src/packages/customization.common/customization.common.js", new MockFileData("")},
                { $"Cmf.Custom.{packageType}/package.json", new MockFileData(@"{""name"": ""customization.package""}")},

                // output dir
                { $"output", new MockDirectoryData() },
            });

            ExecutionContext.Initialize(fileSystem);

            IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"Cmf.Custom.{packageType}/cmfpackage.json");
            PresentationPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, true) as PresentationPackageTypeHandler;

            packageTypeHandler.GeneratePresentationConfigFile(fileSystem.DirectoryInfo.New("output"));

            IFileInfo configJsonFile = fileSystem.FileInfo.New(MockUnixSupport.Path(@"output\\config.json").Replace("\\", "\\\\"));
            dynamic configJsonFileContent = JsonConvert.DeserializeObject(fileSystem.File.ReadAllText(configJsonFile.FullName));

            string customizationVersion = configJsonFileContent.customizationVersion?.ToString();

            customizationVersion.Should().Be(version);
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

            IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"repo/{CliConstants.CmfPackageFileName}");

            string message = string.Empty;
            try
            {
                var packCommand = new PackCommand(fileSystem);
                packCommand.Execute(cmfpackageFile.Directory, fileSystem.DirectoryInfo.New("output"), false);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            string fileLocation = fileSystem.FileInfo.New("/repo/cmfpackage.json").FullName;

            Assert.Equal(@$"Missing mandatory property ContentToPack in file {fileLocation}", message);
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
            IFileInfo cmfpackageFile = fileSystem.FileInfo.New($"repo/{CliConstants.CmfPackageFileName}");

            string message = string.Empty;
            try
            {
                var packCommand = new PackCommand(fileSystem);
                packCommand.Execute(cmfpackageFile.Directory, fileSystem.DirectoryInfo.New("output"), false);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            Assert.Equal("Mandatory Dependency criticalmanufacturing.deploymentmetadata and cmf.environment. not found", message);
        }

        [Fact]
        public void Data_WithRelatedPackages()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Data", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.Data.Related", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
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
                { "/repo/Cmf.Custom.Data/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageDep1.Key}"",
                  ""version"": ""{packageDep1.Value}"",
                  ""packageType"": ""Data"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {{
                        ""source"": ""{MockUnixSupport.Path("folder1\\file1.txt").Replace("\\", "\\\\")}"",
                        ""target"": """"
                    }}
                  ],
                  ""relatedPackages"": [
                    {{
                        ""path"": ""Cmf.Custom.Data.Related"",
                        ""postPack"": true
                    }}
                  ]
                }}")},
                { "/repo/Cmf.Custom.Data/Cmf.Custom.Data.Related/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageDep2.Key}"",
                  ""version"": ""{packageDep2.Value}"",
                  ""packageType"": ""Data"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {{
                        ""source"": ""{MockUnixSupport.Path("folder2\\file2.txt").Replace("\\", "\\\\")}"",
                        ""target"": """"
                    }}
                  ]
                }}")},
                { "/repo/Cmf.Custom.Data/folder1/file1.txt", new MockFileData("file1-content")},
                { "/repo/Cmf.Custom.Data/Cmf.Custom.Data.Related/folder2/file2.txt", new MockFileData("file2-content")},
            });

            var packCommand = new PackCommand(fileSystem);
            var outputFolder = fileSystem.DirectoryInfo.New("output");
            packCommand.Execute(fileSystem.DirectoryInfo.New("/repo/Cmf.Custom.Data"), outputFolder, false);
            IEnumerable<IFileInfo> packedFiles = outputFolder.EnumerateFiles().ToList();


            var depFile1 = packedFiles.FirstOrDefault(x => x.Name.Equals($"{packageDep1.Key}.{packageDep1.Value}.zip"));
            var depFile2 = packedFiles.FirstOrDefault(x => x.Name.Equals($"{packageDep2.Key}.{packageDep2.Value}.zip"));

            depFile1.Should().NotBeNull();
            depFile2.Should().NotBeNull();

            TestUtilities.ValidateZipContent(fileSystem, depFile1, new() { "Cmf.Foundation.Services.HostService.dll.config", "manifest.xml", "file1.txt" });
            TestUtilities.ValidateZipContent(fileSystem, depFile2, new() { "Cmf.Foundation.Services.HostService.dll.config", "manifest.xml", "file2.txt" });
        }
        
        [Fact]
        public void HTML_ShouldPackWithDefaultValuesIfRelated()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep1 = new("Cmf.Custom.Data", "1.1.0");
            KeyValuePair<string, string> packageDep2 = new("Cmf.Custom.HTML.Related", "1.1.0");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                // project config file
                { ".project-config.json", new MockFileData(
                    @$"{{
                        ""MESVersion"": ""10.1.0""
                    }}")},
                {
                    $"/repo/{packageDep1.Key}/{packageDep2.Key}/angular.json", new MockFileData(
                        $@"{{
                            ""projects"": {{
                                ""{packageDep2.Key}"": {{""projectType"": ""application""}}
                            }}
                        }}"
                )},
                {
                    $"/repo/{packageDep1.Key}/{packageDep2.Key}/package.json", new MockFileData(
                        $@"{{""name"": ""{packageDep2.Key.ToKebabCase()}""}}"
                        )
                },
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
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
                { $"/repo/{packageDep1.Key}/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageDep1.Key}"",
                  ""version"": ""{packageDep1.Value}"",
                  ""packageType"": ""Data"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {{
                        ""source"": ""{MockUnixSupport.Path("folder1\\file1.txt").Replace("\\", "\\\\")}"",
                        ""target"": """"
                    }}
                  ],
                  ""relatedPackages"": [
                    {{
                        ""path"": ""{packageDep2.Key}"",
                        ""postPack"": true
                    }}
                  ]
                }}")},
                { $"/repo/{packageDep1.Key}/{packageDep2.Key}/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageDep2.Key}"",
                  ""version"": ""{packageDep2.Value}"",
                  ""packageType"": ""HTML"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {{
                        ""source"": ""{MockUnixSupport.Path("folder2\\file2.txt").Replace("\\", "\\\\")}"",
                        ""target"": """"
                    }}
                  ]
                }}")},
                { $"/repo/{packageDep1.Key}/folder1/file1.txt", new MockFileData("file1-content")},
                { $"/repo/{packageDep1.Key}/{packageDep2.Key}/folder2/file2.txt", new MockFileData("file2-content")},
            });

            var packCommand = new PackCommand(fileSystem);
            var outputFolder = fileSystem.DirectoryInfo.New("output");
            packCommand.Execute(fileSystem.DirectoryInfo.New("/repo/Cmf.Custom.Data"), outputFolder, false);
            IEnumerable<IFileInfo> packedFiles = outputFolder.EnumerateFiles().ToList();
            
            var depFile2 = packedFiles.FirstOrDefault(x => x.Name.Equals($"{packageDep2.Key}.{packageDep2.Value}.zip"));
            depFile2.Should().NotBeNull();

            var manifest = FileSystemUtilities.GetManifestFromPackage(depFile2.FullName, fileSystem);
            XElement rootNode = manifest.Element("deploymentPackage", true);
            if (rootNode == null)
            {
                throw new CliException(string.Format(CoreMessages.InvalidManifestFile));
            }

            var steps = rootNode.Elements().FirstOrDefault(e => e.Name.LocalName == "steps");
            Assert.NotNull(steps);
            steps.Elements().Count().Should().Be(4, "HTML package should have 4 installation steps");
        }

    }
}
using System;
using System.Collections.Generic;
using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Utilities;
using Xunit;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using Cmf.Common.Cli.Utilities;
using FluentAssertions;
using tests.Objects;

namespace tests.Specs
{
    public class Pack
    {
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
            packCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("c:\\ui")), fileSystem.DirectoryInfo.FromDirectoryName("output"), false);

            IEnumerable<IFileInfo> assembledFiles = fileSystem.DirectoryInfo.FromDirectoryName("output").EnumerateFiles("Cmf.Custom.HTML.1.1.0.zip").ToList();
            Assert.Single(assembledFiles);

            using (Stream zipToOpen = fileSystem.FileStream.Create(assembledFiles.First().FullName, FileMode.Open))
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
                        MockUnixSupport.Path(@"node_modules\customization.package\package.json"),
                        MockUnixSupport.Path(@"node_modules\customization.package\customization.common.js")
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
            var exception = Assert.Throws<CliException>(() => packCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("c:\\ui")), fileSystem.DirectoryInfo.FromDirectoryName("output"), false));
            exception.Message.Should().Contain("Nothing was found on ContentToPack Sources");
        }
        
        [Fact]
        public void DontCheckThatContentWasPacked_IfMeta()
        {
            var fileSystem = MockPackage.Root_Empty;
            
            var packCommand = new PackCommand(fileSystem);
            packCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("c:\\repo")), fileSystem.DirectoryInfo.FromDirectoryName("output"), false);
        }
        
        [Fact]
        public void DontCheckThatContentWasPacked_IfContentToPackIsEmpty()
        {
            var fileSystem = MockPackage.Html_EmptyContentToPack;
            
            var packCommand = new PackCommand(fileSystem);
            var exception = Assert.Throws<CliException>(() => packCommand.Execute(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("c:\\ui")), fileSystem.DirectoryInfo.FromDirectoryName("output"), false));
            exception.Message.Should().Contain("Missing mandatory property ContentToPack in file");
        }
        [Fact]
        public void Pack_SecurityPortal()
        {
            string dir = $"{TestUtilities.GetTmpDirectory()}/securityPortal";
            TestUtilities.CopyFixture("pack/securityPortal", new DirectoryInfo(dir));

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
    }
}
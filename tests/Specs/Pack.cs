using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.IO.Abstractions;

namespace tests.Specs
{
    [TestClass]
    public class Pack
    {
        [TestMethod]
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

            Assert.AreEqual("working_dir", _workingDir);
            Assert.AreEqual("test_package_dir", _outputDir);
            Assert.IsNotNull(_force);
            Assert.IsFalse(_force ?? true);
        }

        [TestMethod]
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

            Assert.AreEqual("working_dir", _workingDir);
            Assert.AreEqual("Package", _outputDir);
            Assert.IsNotNull(_force);
            Assert.IsFalse(_force ?? true);
        }

        [TestMethod]
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
            Assert.AreEqual(curDir.Name, _workingDir);
            Assert.AreEqual("test_package_dir", _outputDir);
            Assert.IsNotNull(_force);
            Assert.IsFalse(_force ?? true);
        }

        [TestMethod]
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

            Assert.AreEqual(curDir.Name, _workingDir);
            Assert.AreEqual("Package", _outputDir);
            Assert.IsNotNull(_force);
            Assert.IsFalse(_force ?? true);
        }

        [TestMethod]
        public void Pack_SecurityPortal()
        {
            string dir = $"{TestUtilities.GetTmpDirectory()}/securityPortal";
            TestUtilities.CopyFixture("pack/securityPortal", new DirectoryInfo(dir));

            Directory.SetCurrentDirectory(dir);

            string _workingDir = dir;
            string _outputDir = "./Package";
            bool? _force = true;

            PackCommand packCommand = new PackCommand();
            Command cmd = new Command("pack");
            packCommand.Configure(cmd);

            TestConsole console = new TestConsole();
            cmd.Invoke(new string[] {
            }, console);

            DirectoryInfo curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            Assert.IsTrue(Directory.Exists($"{dir}/Package"), "Package folder is missing");
            Assert.IsTrue(File.Exists($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip"), "Zip package is missing");

            List<string> entries = TestUtilities.GetFileEntriesFromZip($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip");
            Assert.IsTrue(entries.HasAny(), "Zip package is empty");
            Assert.IsTrue(entries.HasAny(entry => entry == "manifest.xml"), "Manifest file does not exist");
            Assert.IsTrue(entries.HasAny(entry => entry == "config.json"), "Config file does not exist");

            string configJsonContent = FileSystemUtilities.GetFileContentFromPackage($"{dir}/Package/Cmf.Custom.SecurityPortal.1.0.0.zip", "config.json");

            Assert.IsTrue(configJsonContent.Contains("$.tenants.config.tenant.strategies"), "Config file does not have correct tenant");
        }
    }
}
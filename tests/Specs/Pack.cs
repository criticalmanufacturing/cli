using Cmf.Common.Cli.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            string _repo = null;
            bool? _force = null;
            bool? _skipDependencies = null;

            var packCommand = new PackCommand();
            var cmd = new Command("pack");
            packCommand.Configure(cmd);

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, string, bool, bool>(
            (workingDir, outputDir, repo, force, skipDependencies) =>
            {
                _workingDir = workingDir.Name;
                _outputDir = outputDir.Name;
                _repo = repo;
                _force = force;
                _skipDependencies = skipDependencies;
            });


            var console = new TestConsole();
            cmd.Invoke(new[] {
                "-o", "test_package_dir", "working_dir"
            }, console);

            Assert.AreEqual("working_dir", _workingDir);
            Assert.AreEqual("test_package_dir", _outputDir);
            Assert.IsNull(_repo);
            Assert.IsNotNull(_force);
            Assert.IsFalse(_force ?? true);
            Assert.IsNotNull(_skipDependencies);
            Assert.IsFalse(_skipDependencies ?? true);
        }

        [TestMethod]
        public void Args_Paths_WorkDirSpecified()
        {
            string _workingDir = null;
            string _outputDir = null;
            string _repo = null;
            bool? _force = null;
            bool? _skipDependencies = null;

            var packCommand = new PackCommand();
            var cmd = new Command("pack");
            packCommand.Configure(cmd);

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, string, bool, bool>(
            (workingDir, outputDir, repo, force, skipDependencies) =>
            {
                _workingDir = workingDir.Name;
                _outputDir = outputDir.Name;
                _repo = repo;
                _force = force;
                _skipDependencies = skipDependencies;
            });


            var console = new TestConsole();
            cmd.Invoke(new[] {
                "working_dir"
            }, console);

            Assert.AreEqual("working_dir", _workingDir);
            Assert.AreEqual("Package", _outputDir);
            Assert.IsNull(_repo);
            Assert.IsNotNull(_force);
            Assert.IsFalse(_force ?? true);
            Assert.IsNotNull(_skipDependencies);
            Assert.IsFalse(_skipDependencies ?? true);
        }

        [TestMethod]
        public void Args_Paths_OutDirSpecified()
        {
            string _workingDir = null;
            string _outputDir = null;
            string _repo = null;
            bool? _force = null;
            bool? _skipDependencies = null;

            var packCommand = new PackCommand();
            var cmd = new Command("pack");
            packCommand.Configure(cmd);

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, string, bool, bool>(
            (workingDir, outputDir, repo, force, skipDependencies) =>
            {
                _workingDir = workingDir.Name;
                _outputDir = outputDir.Name;
                _repo = repo;
                _force = force;
                _skipDependencies = skipDependencies;
            });


            var console = new TestConsole();
            cmd.Invoke(new[] {
                "-o", "test_package_dir"
            }, console);

            var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            Assert.AreEqual(curDir.Name, _workingDir);
            Assert.AreEqual("test_package_dir", _outputDir);
            Assert.IsNull(_repo);
            Assert.IsNotNull(_force);
            Assert.IsFalse(_force ?? true);
            Assert.IsNotNull(_skipDependencies);
            Assert.IsFalse(_skipDependencies ?? true);
        }

        [TestMethod]
        public void Args_Paths_NoneSpecified()
        {
            string _workingDir = null;
            string _outputDir = null;
            string _repo = null;
            bool? _force = null;
            bool? _skipDependencies = null;

            var packCommand = new PackCommand();
            var cmd = new Command("pack");
            packCommand.Configure(cmd);

            cmd.Handler = CommandHandler.Create<IDirectoryInfo, IDirectoryInfo, string, bool, bool>(
            (workingDir, outputDir, repo, force, skipDependencies) =>
            {
                _workingDir = workingDir.Name;
                _outputDir = outputDir.Name;
                _repo = repo;
                _force = force;
                _skipDependencies = skipDependencies;
            });


            var console = new TestConsole();
            cmd.Invoke(new string[] {
            }, console);

            var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            Assert.AreEqual(curDir.Name, _workingDir);
            Assert.AreEqual("Package", _outputDir);
            Assert.IsNull(_repo);
            Assert.IsNotNull(_force);
            Assert.IsFalse(_force ?? true);
            Assert.IsNotNull(_skipDependencies);
            Assert.IsFalse(_skipDependencies ?? true);
        }
    }
}

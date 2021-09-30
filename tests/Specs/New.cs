using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.IO;
using Cmf.Common.Cli.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests.Specs
{
    [TestClass]
    public class New
    {
        private string GetFixturePath(string fixture, string item)
        {
            return System.IO.Path.GetFullPath(
                System.IO.Path.Join(
            AppDomain.CurrentDomain.BaseDirectory, 
                        "..", "..", "..", "Fixtures", fixture, item));
        }
        [TestInitialize]
        public void Reset()
        {
            var newCommand = new NewCommand();
            var cmd = new Command("x");
            newCommand.Configure(cmd);

            var console = new TestConsole();
            cmd.Invoke(new[] {
                "--reset"
            }, console);
        }
        
        [TestMethod]
        public void Init()
        {
            var initCommand = new InitCommand();
            var cmd = new Command("x");
            initCommand.Configure(cmd);

            var rnd = new Random();
            var tmp = Path.Join(Path.GetTempPath(), Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));
            Directory.CreateDirectory(tmp);

            Debug.WriteLine("Generating at " + tmp);
            
            var projectName = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var repoUrl = "https://repo_url/collection/project/_git/repo";
            var deploymentDir = "deployment_dir";
            var isoLocation = "iso_location";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";
            
            var console = new TestConsole();
            cmd.Invoke(new[] {
                projectName, 
                "--infra", this.GetFixturePath("init","infrastructure.json"), 
                "-c", this.GetFixturePath("init","config.json"),
                "--repositoryUrl", repoUrl, 
                "--MESVersion", "8.2.0",
                "--DevTasksVersion", "8.1.0",
                "--HTMLStarterVersion", "8.0.0",
                "--yoGeneratorVersion", "8.1.0",
                "--nugetVersion", "8.2.0",
                "--testScenariosNugetVersion", "8.2.0",
                "--deploymentDir", deploymentDir,
                "--ISOLocation", isoLocation,
                "--version", pkgVersion,
                tmp
            }, console);
        }
    }
}
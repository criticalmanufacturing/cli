using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            var tmp = Path.Join(Path.GetTempPath(), Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8));
            Directory.CreateDirectory(tmp);

            Debug.WriteLine("Generating at " + tmp);
            
            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var repoUrl = "https://repo_url/collection/project/_git/repo";
            var deploymentDir = "\\\\share\\deployment_dir";
            var isoLocation = "\\\\share\\iso_location";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);
                cmd.Invoke(new[]
                {
                    projectName,
                    "--infra", this.GetFixturePath("init", "infrastructure.json"),
                    "-c", this.GetFixturePath("init", "config.json"),
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
                    "Cmf.Custom.Package",
                    tmp
                }, console);

                var extractFileName = new Func<string, string>(s => s.Split(Path.DirectorySeparatorChar).LastOrDefault());
                
                Assert.IsTrue(File.Exists(".project-config.json"), "project config is missing");
                Assert.IsTrue(File.Exists("cmfpackage.json"), "root cmfpackage is missing");
                Assert.IsTrue(File.Exists("global.json"), "global .NET versioning is missing");
                Assert.IsTrue(File.Exists("NuGet.config"), "global NuGet feeds config is missing");
                
                Assert.IsTrue(Directory.Exists(Path.Join(tmp, "Builds")), "pipelines are missing");
                Assert.IsTrue(
                    new []{ "CI-Changes.json",
                        "CI-Package.json",
                        "CI-Publish.json",
                        "CI-Release.json",
                        "PR-Changes.json",
                        "PR-Package.json" }
                            .ToList()
                            .All(f => Directory
                                .GetFiles("Builds")
                                .Select(extractFileName)
                                .Contains(f)), "Missing pipeline metadata");
                Assert.IsTrue(
                    new []{ "CI-Changes.yml",
                            "CI-Package.yml",
                            "CI-Publish.yml",
                            "CI-Release.yml",
                            "PR-Changes.yml",
                            "PR-Package.yml" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("Builds")
                            .Select(extractFileName)
                            .Contains(f)), "Missing pipeline source");
                Assert.IsTrue(
                    new []{ "policies-master.json",
                            "policies-development.json" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("Builds")
                            .Select(extractFileName)
                            .Contains(f)), "Missing policy metadata");
                Assert.IsTrue(Directory.Exists(Path.Join(tmp, "DeploymentMetadata")), "Deployment Metadata is missing");
                Assert.IsTrue(Directory.Exists(Path.Join(tmp, "EnvironmentConfigs")), "environment configs are missing");
                Assert.IsTrue(
                    new []{ "GlobalVariables.yml" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("EnvironmentConfigs")
                            .Select(extractFileName)
                            .Contains(f)), "Missing global variables");
                Assert.IsTrue(
                    new []{ "config.json" } // this should be a constant when moving to a mock
                        .ToList()
                        .All(f => Directory
                            .GetFiles("EnvironmentConfigs")
                            .Select(extractFileName)
                            .Contains(f)), "Missing environment configuration");
                Assert.IsTrue(Directory.Exists(Path.Join(tmp, "Libs")), "Libs are missing");
            } 
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }
    }
}
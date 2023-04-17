using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text.Json;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Commands;
using Cmf.CLI.Commands.New;
using Cmf.CLI.Core.Objects;
using FluentAssertions;
using Xunit;
using Assert = tests.AssertWithMessage;
using Cmf.Common.Cli.TestUtilities;
using Moq;
using Newtonsoft.Json;
using Cmf.CLI.Core.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace tests.Specs
{
    public class Init
    {
        [Fact]
        public void Init_()
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var isoLocation = "\\\\share\\iso_location";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
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

                Assert.True(File.Exists(".project-config.json"), "project config is missing");
                Assert.True(File.Exists("cmfpackage.json"), "root cmfpackage is missing");
                Assert.True(File.Exists("global.json"), "global .NET versioning is missing");
                Assert.True(File.Exists("NuGet.Config"), "global NuGet feeds config is missing");
                Assert.True(
                    new[] { "config.json" } // this should be a constant when moving to a mock
                        .ToList()
                        .All(f => Directory
                            .GetFiles("EnvironmentConfigs")
                            .Select(extractFileName)
                            .Contains(f)), "Missing environment configuration");
                Assert.True(Directory.Exists(Path.Join(tmp, "Libs")), "Libs are missing");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""RepositoryType"": ""Customization""", "Default repository type was not Customization");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""BaseLayer"": ""MES""", "Base Layer should be MES");
                File.ReadAllText(Path.Join(tmp, "cmfpackage.json"))
                    .Should().Contain(@"CriticalManufacturing.DeploymentMetadata", "VM Dependency should be included in root package");
                File.ReadAllText(Path.Join(tmp, "cmfpackage.json"))
                    .Should().Contain(@"Cmf.Environment", "Container Dependency should be included in root package");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }      

        [Fact]
        public void Init_Fail_MissingMandatoryArgumentsAndOptions()
        {
            var console = new TestConsole();

            var initCommand = new InitCommand();
            var cmd = new Command("x"); // this is the command name used in help text
            initCommand.Configure(cmd);

            TestUtilities.GetParser(cmd).Invoke(Array.Empty<string>(), console);

            Assert.Contains("Required argument missing for command: 'x'", console.Error.ToString());
            foreach (var optionName in new[]
                 {
                     "BaseVersion", "nugetVersion", "testScenariosNugetVersion", "deploymentDir"
                 })
            {
                Assert.Contains($"Option '--{optionName}' is required.", console.Error.ToString());
            }
        }

        [Fact]
        public void Init_Fail_MissingOptionsForLTv10()
        {
            var console = new TestConsole();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x"); // this is the command name used in help text
                initCommand.Configure(cmd);

                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "8.2.0",
                    "--nugetVersion", "8.2.0",
                    "--testScenariosNugetVersion", "8.2.0",
                    "--nugetRegistry", "http://nuget.example/feed",
                    "--npmRegistry", "http://npm.example/feed",
                    "--ISOLocation", "dummy",
                    "--deploymentDir", deploymentDir,
                }, console);

                Assert.Contains("DevTasksVersion is required", console.Error.ToString());
                Assert.Contains("HTMLStarterVersion is required", console.Error.ToString());
                Assert.Contains("yoGeneratorVersion is required", console.Error.ToString());
                console.Error.ToString().Should().NotContain("ngxSchematicsVersion is required");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_Fail_MissingOptionsForGTv10()
        {
            var console = new TestConsole();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x"); // this is the command name used in help text
                initCommand.Configure(cmd);

                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "10.2.0",
                    "--nugetVersion", "8.2.0",
                    "--testScenariosNugetVersion", "8.2.0",
                    "--nugetRegistry", "http://nuget.example/feed",
                    "--npmRegistry", "http://npm.example/feed",
                    "--ISOLocation", "dummy",
                    "--deploymentDir", deploymentDir,
                }, console);

                Assert.Contains("ngxSchematicsVersion is required", console.Error.ToString());
                console.Error.ToString().Should().NotContain("DevTasksVersion is required");
                console.Error.ToString().Should().NotContain("HTMLStarterVersion is required");
                console.Error.ToString().Should().NotContain("yoGeneratorVersion is required");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_DefaultRepoTypeIsCustomization()
        {
            var console = new TestConsole();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new Mock<InitCommand>();
                var cmd = new Command("x"); // this is the command name used in help text
                initCommand.Setup(command =>
                    command.RunCommand(
                        It.Is<IReadOnlyCollection<string>>(args => args.Contains("--repoType"))));
                initCommand.Object.Configure(cmd);

                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "8.2.0",
                    "--nugetVersion", "8.2.0",
                    "--testScenariosNugetVersion", "8.2.0",
                    "--nugetRegistry", "http://nuget.example/feed",
                    "--npmRegistry", "http://npm.example/feed",
                    "--ISOLocation", "dummy",
                    "--deploymentDir", deploymentDir,
                }, console);
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_Fail_MissingInfra()
        {
            var console = new TestConsole();
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x"); // this is the command name used in help text
                initCommand.Configure(cmd);

                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "8.2.0",
                    "--DevTasksVersion", "8.1.0",
                    "--HTMLStarterVersion", "8.0.0",
                    "--yoGeneratorVersion", "8.1.0",
                    "--nugetVersion", "8.2.0",
                    "--testScenariosNugetVersion", "8.2.0",
                    "--deploymentDir", deploymentDir,
                }, console);

                Assert.Contains("Missing infrastructure options", console.Error.ToString());
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_App()
        {
            var initCommand = new InitCommand();
            var cmd = new Command("x");
            initCommand.Configure(cmd);

            var rnd = new Random();
            var tmp = Path.Join(Path.GetTempPath(), Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8));
            Directory.CreateDirectory(tmp);

            Debug.WriteLine("Generating at " + tmp);

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var isoLocation = "\\\\share\\iso_location";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);
                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--repositoryType", "App",
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

                var extractFileName = new Func<string, string>(s => s.Split(Path.DirectorySeparatorChar).LastOrDefault());

                Assert.True(File.Exists(".project-config.json"), "project config is missing");
                Assert.True(File.Exists("cmfpackage.json"), "root cmfpackage is missing");
                Assert.True(File.Exists("global.json"), "global .NET versioning is missing");
                Assert.True(File.Exists("NuGet.Config"), "global NuGet feeds config is missing");
               
                Assert.True(Directory.Exists(Path.Join(tmp, "Libs")), "Libs are missing");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""RepositoryType"": ""App""", "Applied repository type was not App");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""BaseLayer"": ""Core""", "Base Layer should be Core");
                File.ReadAllText(Path.Join(tmp, "cmfpackage.json"))
                    .Should().NotContain(@"CriticalManufacturing.DeploymentMetadata", "VM Dependency should not be included in root package");
                File.ReadAllText(Path.Join(tmp, "cmfpackage.json"))
                    .Should().Contain(@"Cmf.Environment", "Container Dependency should be included in root package");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }
    }
}

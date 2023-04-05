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
    public class New
    {
        public New()
        {
            System.Environment.SetEnvironmentVariable("cmf_cli_internal_disable_projectconfig_cache", "1");
                
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<IProjectConfigService>(new ProjectConfigService())
                .BuildServiceProvider();

            var newCommand = new NewCommand();
            var cmd = new Command("x");
            newCommand.Configure(cmd);

            var console = new TestConsole();
            cmd.Invoke(new[] {
                "--reset"
            }, console);
        }

        [Fact]
        public void Init()
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

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

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
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

                Assert.True(File.Exists(".project-config.json"), "project config is missing");
                Assert.True(File.Exists("cmfpackage.json"), "root cmfpackage is missing");
                Assert.True(File.Exists("global.json"), "global .NET versioning is missing");
                Assert.True(File.Exists("NuGet.Config"), "global NuGet feeds config is missing");

                Assert.True(Directory.Exists(Path.Join(tmp, "Builds")), "pipelines are missing");
                Assert.True(
                    new[]{ "CI-Changes.json",
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
                Assert.True(
                    new[]{ "CI-Changes.yml",
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
                Assert.True(
                    new[]{ "policies-master.json",
                            "policies-development.json" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("Builds")
                            .Select(extractFileName)
                            .Contains(f)), "Missing policy metadata");
                Assert.True(Directory.Exists(Path.Join(tmp, "EnvironmentConfigs")), "environment configs are missing");
                Assert.True(
                    new[] { "global.yml" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("Builds/.vars")
                            .Select(extractFileName)
                            .Contains(f)), "Missing global variables");
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
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("testNamespace")]
        public void Init_PipelineFolders(string folder)
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

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

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--repositoryUrl", repoUrl,
                    "--MESVersion", "8.2.0",
                    "--DevTasksVersion", "8.1.0",
                    "--HTMLStarterVersion", "8.0.0",
                    "--yoGeneratorVersion", "8.1.0",
                    "--nugetVersion", "8.2.0",
                    "--testScenariosNugetVersion", "8.2.0",
                    "--deploymentDir", deploymentDir,
                    "--ISOLocation", isoLocation,
                    "--version", pkgVersion
                }.Concat(folder != null ? new []{ "--pipelinesFolder", folder } : Array.Empty<string>())
                    .Concat(new [] {
                    "Cmf.Custom.Package",
                    tmp
                }).ToArray(), console);

                Assert.True(Directory.Exists(Path.Join(tmp, "Builds")), "pipelines are missing");

                File.ReadAllText(Path.Join(tmp, "Builds", "CI-Changes.yml"))
                    .Should().Contain(string.IsNullOrWhiteSpace(folder) ? @"\CI-Builds" : @$"\{folder}\CI-Builds", "Wrong CI pipeline folder name in source");
                File.ReadAllText(Path.Join(tmp, "Builds", "PR-Changes.yml"))
                    .Should().Contain(string.IsNullOrWhiteSpace(folder) ? @"\PR-Builds" : @$"\{folder}\PR-Builds", "Wrong PR pipeline folder name in source");
                
                Directory
                    .GetFiles("Builds")
                    .Where(f => f.EndsWith(".json") && (f.Contains("CD-") || f.Contains("PR-") || f.Contains("CD-")))
                    .Select(File.ReadAllText)
                    .Should().AllSatisfy(s => s.Should().MatchRegex(string.IsNullOrWhiteSpace(folder) ? "\"path\":\\s\"\\\\\\\\[^\"]+" : $"\"path\":\\s\"\\\\\\\\{folder}\\\\\\\\[^\"]+"), "Wrong agent pool name");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }
        
        [Fact]
        public void Init_PoolName()
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var poolName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var repoUrl = "https://repo_url/collection/project/_git/repo";
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
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
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
                    // infra options
                    "--nugetRegistry", "http://nuget.example/feed",
                    "--npmRegistry", "http://npm.example/feed",
                    "--azureDevOpsCollectionUrl", "http://azure.example/org/project",
                    "--agentPool", poolName,
                    "--agentType", "Hosted",
                    "Cmf.Custom.Package",
                    tmp
                }, console);

                var extractFileName = new Func<string, string>(s => s.Split(Path.DirectorySeparatorChar).LastOrDefault());

                Assert.True(Directory.Exists(Path.Join(tmp, "Builds")), "pipelines are missing");
                Assert.True(
                    new[]{ "CI-Changes.json",
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

                Directory
                    .GetFiles("Builds")
                    .Where(f => f.EndsWith(".yml"))
                    .Select(File.ReadAllText)
                    .Should().AllSatisfy(s => s.Should().MatchRegex($@"pool:\r?\n\s\sname:\s{poolName}"), "Wrong agent pool name");
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
                     "repositoryUrl", "BaseVersion",
                     "nugetVersion", "testScenariosNugetVersion", "deploymentDir"
                 })
            {
                Assert.Contains($"Option '--{optionName}' is required.", console.Error.ToString());
            }
        }
        
        [Fact]
        public void Init_Fail_MissingOptionsForLTv10()
        {
            var console = new TestConsole();
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var repoUrl = "https://repo_url/collection/project/_git/repo";
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
                    "--repositoryUrl", repoUrl,
                    "--MESVersion", "8.2.0",
                    "--nugetVersion", "8.2.0",
                    "--testScenariosNugetVersion", "8.2.0",
                    "--nugetRegistry", "http://nuget.example/feed",
                    "--npmRegistry", "http://npm.example/feed",
                    "--azureDevOpsCollectionUrl", "http://azure.example/org/project",
                    "--agentPool", "poolName",
                    "--agentType", "Hosted",
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
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var repoUrl = "https://repo_url/collection/project/_git/repo";
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
                    "--repositoryUrl", repoUrl,
                    "--MESVersion", "10.2.0",
                    "--nugetVersion", "8.2.0",
                    "--testScenariosNugetVersion", "8.2.0",
                    "--nugetRegistry", "http://nuget.example/feed",
                    "--npmRegistry", "http://npm.example/feed",
                    "--azureDevOpsCollectionUrl", "http://azure.example/org/project",
                    "--agentPool", "poolName",
                    "--agentType", "Hosted",
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
        
        [Fact(Skip = "not ready")]
        public void Init_DefaultRepoTypeIsCustomization()
        {
            var console = new TestConsole();
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var repoUrl = "https://repo_url/collection/project/_git/repo";
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
                    "--repositoryUrl", repoUrl,
                    "--MESVersion", "8.2.0",
                    "--nugetVersion", "8.2.0",
                    "--testScenariosNugetVersion", "8.2.0",
                    "--nugetRegistry", "http://nuget.example/feed",
                    "--npmRegistry", "http://npm.example/feed",
                    "--azureDevOpsCollectionUrl", "http://azure.example/org/project",
                    "--agentPool", "poolName",
                    "--agentType", "Hosted",
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
            var repoUrl = "https://repo_url/collection/project/_git/repo";
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
                    "--repositoryUrl", repoUrl,
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
        public void Init_Containers()
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
                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
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
                    "--releaseCustomerEnvironment", "cmf-environment",
                    "--releaseSite", "cmf-site",
                    "--releaseDeploymentPackage", @"\@criticalmanufacturing\mes:8.3.1",
                    "--releaseLicense", "cmf-license",
                    "--releaseDeploymentTarget", "cmf-target",
                    tmp
                }, console);

                var extractFileName = new Func<string, string>(s => s.Split(Path.DirectorySeparatorChar).LastOrDefault());

                Assert.True(File.Exists(".project-config.json"), "project config is missing");
                Assert.True(File.Exists("cmfpackage.json"), "root cmfpackage is missing");
                Assert.True(File.Exists("global.json"), "global .NET versioning is missing");
                Assert.True(File.Exists("NuGet.Config"), "global NuGet feeds config is missing");

                Assert.True(Directory.Exists(Path.Join(tmp, "Builds")), "pipelines are missing");
                Assert.True(
                    new[]{ "CI-Changes.json",
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
                Assert.True(
                    new[]{ "CI-Changes.yml",
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
                Assert.True(
                    new[]{ "policies-master.json",
                            "policies-development.json" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("Builds")
                            .Select(extractFileName)
                            .Contains(f)), "Missing policy metadata");
                Assert.True(Directory.Exists(Path.Join(tmp, "EnvironmentConfigs")), "environment configs are missing");
                Assert.True(
                    new[] { "global.yml" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("Builds/.vars")
                            .Select(extractFileName)
                            .Contains(f)), "Missing global variables");
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
            var repoUrl = "https://repo_url/collection/project/_git/repo";
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
                    "--releaseCustomerEnvironment", "cmf-environment",
                    "--releaseSite", "cmf-site",
                    "--releaseDeploymentPackage", @"\@criticalmanufacturing\mes:8.3.1",
                    "--releaseLicense", "cmf-license",
                    "--releaseDeploymentTarget", "cmf-target",
                    tmp
                }, console);

                var extractFileName = new Func<string, string>(s => s.Split(Path.DirectorySeparatorChar).LastOrDefault());

                Assert.True(File.Exists(".project-config.json"), "project config is missing");
                Assert.True(File.Exists("cmfpackage.json"), "root cmfpackage is missing");
                Assert.True(File.Exists("global.json"), "global .NET versioning is missing");
                Assert.True(File.Exists("NuGet.Config"), "global NuGet feeds config is missing");

                Assert.True(Directory.Exists(Path.Join(tmp, "Builds")), "pipelines are missing");
                Assert.True(
                    new[]{ "CI-Changes.json",
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
                Assert.True(
                    new[]{ "CI-Changes.yml",
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
                Assert.True(
                    new[]{ "policies-master.json",
                            "policies-development.json" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("Builds")
                            .Select(extractFileName)
                            .Contains(f)), "Missing policy metadata");
                Assert.True(Directory.Exists(Path.Join(tmp, "EnvironmentConfigs")), "environment configs are missing");
                Assert.True(
                    new[] { "global.yml" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("Builds/.vars")
                            .Select(extractFileName)
                            .Contains(f)), "Missing global variables");
                Assert.True(
                    new[] { "config.json" } // this should be a constant when moving to a mock
                        .ToList()
                        .All(f => Directory
                            .GetFiles("EnvironmentConfigs")
                            .Select(extractFileName)
                            .Contains(f)), "Missing environment configuration");
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

        [Theory]
        [InlineData("https://azure-devops.example/collection/project/_git/repository", "repository")]
        [InlineData("https://azure-devops.example/collection/_git/repository", "repository")]
        [InlineData("https://github.example/org/repository", "defaultProject")]
        public void Init_RepoName_Calc(string repoUrl, string repoName)
        {
            string url = TestUtilities.GetTmpDirectory();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
            });

            ExecutionContext.Initialize(fileSystem);
            var initMoq = new Moq.Mock<InitCommand>();

            var args = new InitArguments()
            {
                projectName = "defaultProject",
                workingDir = fileSystem.DirectoryInfo.New($"."),
                repositoryUrl = new Uri(repoUrl),
                nugetRegistry = new Uri("http://nuget.example"),
                npmRegistry = new Uri("http://npm.example"),
                azureDevOpsCollectionUrl = new Uri("http://azure-devops.example/collection"),
                ISOLocation = fileSystem.FileInfo.New("."),
                agentPool = "agents",
                BaseVersion = "9.9.9",
                DevTasksVersion = "1.1.1",
                HTMLStarterVersion = "1.1.1",
                yoGeneratorVersion = "1.1.1"
            };
            initMoq.Object.Execute(args);
            
            initMoq
                .Verify(x =>
                    x.RunCommand(It.Is<IReadOnlyCollection<string>>(value => string.Join("_", value).Contains($"--repositoryName_{repoName}")))
                );
        }

        [Theory]
        [InlineData(BaseLayer.Core)]
        [InlineData(BaseLayer.MES)]
        public void Business(BaseLayer layer)
        {
            RunNew(new BusinessCommand(), "Cmf.Custom.Business", mesVersion: "9.1.0", baseLayer: layer, extraAsserts: args =>
            {
                var (pkgVersion, dir) = args;
                Assert.True(File.Exists($"Cmf.Custom.Business/Cmf.Custom.Common/tenantConstants.cs"), "Constants file is missing or has wrong name");
                Assert.True(File.Exists($"Cmf.Custom.Business/Cmf.Custom.Common/Cmf.Custom.tenant.Common.csproj"), "Common project file is missing or has wrong name");
                // namespace checks
                Assert.True(File.ReadAllText("Cmf.Custom.Business/Cmf.Custom.Common/tenantConstants.cs").Contains("namespace Cmf.Custom.tenant.Common"), "Constants namespace is wrong name");
                // assembly name checks
                var commonCsproj = File.ReadAllText("Cmf.Custom.Business/Cmf.Custom.Common/Cmf.Custom.tenant.Common.csproj");
                Assert.True(commonCsproj.Contains("<AssemblyName>Cmf.Custom.tenant.Common</AssemblyName>"), "Constants assembly name is wrong name");
                if (layer == BaseLayer.Core)
                {
                    commonCsproj.Should().NotContain("Cmf.Navigo");
                    commonCsproj.Should().NotContain("Pkgcmf_common_customactionutilities");
                    commonCsproj.Should().NotContain("cmf.common.customactionutilities");
                }
                else
                {
                    commonCsproj.Should().Contain("Cmf.Navigo");
                    commonCsproj.Should().Contain("Pkgcmf_common_customactionutilities");
                    commonCsproj.Should().Contain("cmf.common.customactionutilities");
                }
            });
        }

        [Fact]
        public void Data()
        {
            RunNew(new DataCommand(), "Cmf.Custom.Data");
        }

        [Theory, Trait("TestCategory", "LongRunning")]
        [InlineData(BaseLayer.MES)]
        [InlineData(BaseLayer.Core)]
        public void UI(BaseLayer layer)
        {
            UI_internal(null, layer);
        }
        
        private void UI_internal(string scaffoldingDir, BaseLayer layer)
        {
            RunNew(new HTMLCommand(), "Cmf.Custom.HTML", scaffoldingDir: scaffoldingDir, baseLayer: layer, extraArguments: new string[]
            {
                "--htmlPkg", TestUtilities.GetFixturePath("prodPkg", "Cmf.Presentation.HTML.9.9.9.zip"),
            }, extraAsserts: args =>
            {
                var configJson = File.ReadAllText("Cmf.Custom.HTML/apps/customization.web/config.json");
                try
                {
                    JsonConvert.DeserializeObject(configJson);
                }
                catch (Exception e)
                {
                    Assert.Fail($"config.json is malformed: {e.Message}");
                }
                Assert.True(File.Exists($"Cmf.Custom.HTML/.dev-tasks.json"), "dev-tasks file is missing or has wrong name. Was cloning HTML-starter unsuccessful?");
                Assert.True(File.ReadAllText("Cmf.Custom.HTML/.dev-tasks.json").Contains("\"isWebAppCompilable\": true"), "Web app is not compilable");
                Assert.True(Directory.Exists($"Cmf.Custom.HTML/apps/customization.web"), "WebApp dir is missing or has wrong name. Was running the application generator unsuccessful?");
                Assert.True(File.Exists($"Cmf.Custom.HTML/apps/customization.web/config.json"), "Config file is missing or has wrong name");
                Assert.True(configJson.Contains("test.package"), "Product package is not in config.json");
                Assert.True(File.Exists($"Cmf.Custom.HTML/apps/customization.web/index.html"), "Index file is missing or has wrong name");
                if (layer == BaseLayer.Core)
                {
                    configJson
                        .Should().Contain("core-ui-web", "wrong base package in config.json");
                    File.ReadAllText($"Cmf.Custom.HTML/apps/customization.web/package.json").Should()
                        .Contain("@criticalmanufacturing/core-ui-web");
                    File.ReadAllText($"Cmf.Custom.HTML/cmfpackage.json").Should()
                        .Contain("node_modules/@criticalmanufacturing/core-ui-web/bundles");
                    configJson
                        .Should().NotContain("cmf.mes", "config.json should not have any MES packages");    
                }
                else
                {
                    configJson
                        .Should().Contain("mes-ui-web", "wrong base package in config.json");
                    File.ReadAllText($"Cmf.Custom.HTML/apps/customization.web/package.json").Should()
                        .Contain("@criticalmanufacturing/mes-ui-web");
                    File.ReadAllText($"Cmf.Custom.HTML/cmfpackage.json").Should()
                        .Contain("node_modules/@criticalmanufacturing/mes-ui-web/bundles");
                    configJson
                        .Should().Contain("cmf.mes", "config.json should not have a MES packages");
                }
                
                
            });
        }
        
        [Theory, Trait("TestCategory", "LongRunning")]
        [InlineData("8.2.0", false)]
        [InlineData("9.1.0", true)]
        public void UI_WithAppsPackage(string mesVersion, bool isCoreAppPresent)
        {
            RunNew(new HTMLCommand(), "Cmf.Custom.HTML", mesVersion: mesVersion, extraArguments: new string[]
            {
                "--htmlPkg", TestUtilities.GetFixturePath("prodPkg", "Cmf.Presentation.HTML.9.9.9.zip"),
            }, extraAsserts: args =>
            {
                Assert.True(File.Exists($"Cmf.Custom.HTML/.dev-tasks.json"), "dev-tasks file is missing or has wrong name. Was cloning HTML-starter unsuccessful?");
                Assert.True(File.ReadAllText("Cmf.Custom.HTML/.dev-tasks.json").Contains("\"isWebAppCompilable\": true"), "Web app is not compilable");
                Assert.True(Directory.Exists($"Cmf.Custom.HTML/apps/customization.web"), "WebApp dir is missing or has wrong name. Was running the application generator unsuccessful?");
                Assert.True(File.Exists($"Cmf.Custom.HTML/apps/customization.web/config.json"), "Config file is missing or has wrong name");
                Assert.True(File.ReadAllText("Cmf.Custom.HTML/apps/customization.web/config.json").Contains("test.package"), "Product package is not in config.json");
                File.ReadAllText("Cmf.Custom.HTML/apps/customization.web/config.json").Contains("cmf.core.app").Should()
                    .Be(isCoreAppPresent, $"Apps package {(isCoreAppPresent ? "is not" : "is")} in config.json even though we are targeting {mesVersion}");
                Assert.True(File.Exists($"Cmf.Custom.HTML/apps/customization.web/index.html"), "Index file is missing or has wrong name");
            });
        }

        [Theory]
        [InlineData("9.0.0", true)]
        [InlineData("10.0.0", false), Trait("TestCategory", "LongRunning")]
        public void UI_FailNoPackage(string mesVersion, bool shouldDisplayError)
        {
            var console = RunNew(new HTMLCommand(), "Cmf.Custom.HTML", defaultAsserts: false, mesVersion: mesVersion);
            var stderr = console.Error.ToString();
            if (shouldDisplayError)
            {
                (stderr ?? "").Trim()
                    .Should().Contain("--htmlPkg option is required for MES versions up to 9.x",
                        "Should exit with missing package error");
            }
            else
            {
                (stderr ?? "").Trim()
                    .Should().NotContain("--htmlPkg option is required for MES versions up to 9.x",
                        "Should exit with missing package error");
            }
        }

        [Fact, Trait("TestCategory", "LongRunning")]
        public void Help()
        {
            Help_internal();
        }

        private void Help_internal(string scaffoldingDir = null)
        {
            RunNew(new Cmf.CLI.Commands.New.HelpCommand(), "Cmf.Custom.Help", scaffoldingDir: scaffoldingDir, extraArguments: new string[] {
                "--docPkg", TestUtilities.GetFixturePath("prodPkg", "Cmf.Documentation.9.9.9.zip"),
            }, extraAsserts: args =>
            {
                Assert.True(File.Exists($"Cmf.Custom.Help/.dev-tasks.json"), "dev-tasks file is missing or has wrong name. Was cloning HTML-starter unsuccessful?");
                Assert.True(Directory.Exists($"Cmf.Custom.Help/apps/cmf.docs.area.web"), "WebApp dir is missing or has wrong name. Was running the application generator unsuccessful?");
                Assert.True(File.Exists($"Cmf.Custom.Help/apps/cmf.docs.area.web/config.json"), "Config file is missing or has wrong name");
                Assert.True(File.ReadAllText("Cmf.Custom.Help/apps/cmf.docs.area.web/config.json").Contains("test.package"), "Product package is not in config.json");
                Assert.True(File.Exists($"Cmf.Custom.Help/apps/cmf.docs.area.web/index.html"), "Index file is missing or has wrong name");
                Assert.True(File.ReadAllText("Cmf.Custom.Help/apps/cmf.docs.area.web/index.html").Contains("Documentation website"), "Index content is not expected");
                Assert.True(File.ReadAllText("Cmf.Custom.Help/apps/cmf.docs.area.web/index.html").Contains("<base href=\"/\">"), "Index base path was not changed correctly");
            });
        }

        [Theory]
        [InlineData("9.0.0", true)]
        [InlineData("10.0.0", false), Trait("TestCategory", "LongRunning")]
        public void Help_FailNoPackage(string mesVersion, bool shouldDisplayError)
        {
            var console = RunNew(new Cmf.CLI.Commands.New.HelpCommand(), "Cmf.Custom.Help", defaultAsserts: false, mesVersion: mesVersion);
            var stderr = console.Error.ToString();
            if (shouldDisplayError)
            {
                (stderr ?? "").Trim()
                    .Should().Contain("--docPkg option is required for MES versions up to 9.x",
                        "Should exit with missing package error");
            }
            else
            {
                (stderr ?? "").Trim()
                    .Should().NotContain("--docPkg option is required for MES versions up to 9.x",
                        "Should exit with missing package error");
            }
        }

        [Fact]
        public void IoT()
        {
            RunNew(new IoTCommand(), "Cmf.Custom.IoT");
        }

        [Fact]
        public void IoTData()
        {
            string dir = TestUtilities.GetTmpDirectory();
            string packageId = "Cmf.Custom.IoT";
            string packageIdData = "Cmf.Custom.IoT.Data";
            string packageFolder = "IoTData";

            TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
            var projCfg = Path.Join(dir, ".project-config.json");
            if (File.Exists(projCfg))
            {
                File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                    .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                    .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                    .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                );
            }
            RunNew(new IoTCommand(), packageId, dir);

            // Validate IoT Data
            Assert.True(Directory.Exists($"{packageId}/{packageFolder}"), "Package folder is missing");
            Assert.True(Directory.Exists($"{packageId}/{packageFolder}/MasterData"), "Folder MasterData is missing");
            Assert.True(Directory.Exists($"{packageId}/{packageFolder}/AutomationWorkFlows"), "Folder AutomationWorkFlows is missing");

            Assert.Equal(packageIdData, TestUtilities.GetPackageProperty("packageId", $"{packageId}/{packageFolder}/cmfpackage.json"), "Package Id does not match expected");
            Assert.Equal(PackageType.IoTData.ToString(), TestUtilities.GetPackageProperty("packageType", $"{packageId}/{packageFolder}/cmfpackage.json"), "Package Type does not match expected");
            Assert.Equal(TestUtilities.GetPackageProperty("version", $"{packageId}/cmfpackage.json"),
                TestUtilities.GetPackageProperty("version", $"{packageId}/{packageFolder}/cmfpackage.json"), "Version does not match expected");
        }

        [Fact]
        public void IoTPackage()
        {
            string dir = TestUtilities.GetTmpDirectory();
            string packageId = "Cmf.Custom.IoT";
            string packageIdData = "Cmf.Custom.IoT.Packages";
            string packageFolder = "IoTPackages";

            TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
            var projCfg = Path.Join(dir, ".project-config.json");
            if (File.Exists(projCfg))
            {
                File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                    .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                    .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                    .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                );
            }
            RunNew(new IoTCommand(), packageId, dir);

            // Validate IoT Package
            Assert.True(Directory.Exists($"{packageId}/{packageFolder}"), "Package folder is missing");
            Assert.True(Directory.Exists($"{packageId}/{packageFolder}/src"), "Folder MasterData is missing");
            Assert.True(File.Exists($"{packageId}/{packageFolder}/.dev-tasks.json"), "Folder AutomationWorkFlows is missing");
            Assert.True(File.Exists($"{packageId}/{packageFolder}/package.json"), "Folder AutomationWorkFlows is missing");

            Assert.Equal(packageIdData, TestUtilities.GetPackageProperty("packageId", $"{packageId}/{packageFolder}/cmfpackage.json"), "Package Id does not match expected");
            Assert.Equal(PackageType.IoT.ToString(), TestUtilities.GetPackageProperty("packageType", $"{packageId}/{packageFolder}/cmfpackage.json"), "Package Type does not match expected");
            Assert.Equal(TestUtilities.GetPackageProperty("version", $"{packageId}/cmfpackage.json"),
                TestUtilities.GetPackageProperty("version", $"{packageId}/{packageFolder}/cmfpackage.json"), "Version does not match expected");
            File.ReadAllText(Path.Join(dir, packageId, "cmfpackage.json"))
                .Should().Contain(@"CriticalManufacturing.DeploymentMetadata", "VM Dependency should not be included in root package");
            File.ReadAllText(Path.Join(dir, packageId, "cmfpackage.json"))
                .Should().Contain(@"Cmf.Environment", "VM Dependency should not be included in root package");
        }

        [Fact]
        public void Database()
        {
            RunDatabase(null);
        }

        private void RunDatabase(string scaffoldingDir)
        {
            RunNew(new DatabaseCommand(), "Cmf.Custom.Database", scaffoldingDir: scaffoldingDir, defaultAsserts: false, extraAsserts: args =>
            {
                var (packageVersion, _) = args;
                Assert.True(Directory.Exists("Cmf.Custom.Database"), "Package folder is missing");
                Assert.True(File.Exists($"Cmf.Custom.Database/Pre/cmfpackage.json"), "Pre Package cmfpackage.json is missing");
                Assert.True(File.Exists($"Cmf.Custom.Database/Post/cmfpackage.json"), "Post Package cmfpackage.json is missing");
                Assert.True(File.Exists($"Cmf.Custom.Database/Reporting/cmfpackage.json"), "Reports Package cmfpackage.json is missing");
                Assert.Equal("Cmf.Custom.Database.Pre", TestUtilities.GetPackageProperty("packageId", $"Cmf.Custom.Database/Pre/cmfpackage.json"), "Pre Package Id does not match expected");
                Assert.Equal(packageVersion, TestUtilities.GetPackageProperty("version", $"Cmf.Custom.Database/Pre/cmfpackage.json"), "Pre Package version does not match expected");
                Assert.Equal("Cmf.Custom.Database.Post", TestUtilities.GetPackageProperty("packageId", $"Cmf.Custom.Database/Post/cmfpackage.json"), "Post Package Id does not match expected");
                Assert.Equal(packageVersion, TestUtilities.GetPackageProperty("version", $"Cmf.Custom.Database/Post/cmfpackage.json"), "Post Package version does not match expected");
                Assert.Equal("Cmf.Custom.Reporting", TestUtilities.GetPackageProperty("packageId", $"Cmf.Custom.Database/Reporting/cmfpackage.json"), "Reporting Package Id does not match expected");
                Assert.Equal(packageVersion, TestUtilities.GetPackageProperty("version", $"Cmf.Custom.Database/Reporting/cmfpackage.json"), "Reporting Package version does not match expected");
                var pkg = TestUtilities.GetPackage("cmfpackage.json");
                var search = pkg.GetProperty("dependencies").EnumerateArray().ToArray().FirstOrDefault(d => d.GetProperty("id").GetString() == "Cmf.Custom.Database");
                Assert.Equal(JsonValueKind.Undefined, search.ValueKind, "Package was found in root package dependencies");
            });
        }

        // Tests doesn't work with RunNew (Execute is invoked on LayerTemplateCommand)
        [Fact]
        public void Tests()
        {
            Tests_internal(null);
        }

        private void Tests_internal(string scaffoldingDir = null)
        {
            var packageId = "Cmf.Custom.Tests";
            var dir = scaffoldingDir ?? TestUtilities.GetTmpDirectory();

            var rnd = new Random();
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            var console = new TestConsole();
            try
            {
                Directory.SetCurrentDirectory(dir);

                // place new fixture: an init'd repository
                TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
                
                var projCfg = Path.Join(dir, ".project-config.json");
                if (File.Exists(projCfg))
                {
                    File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                        .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                        .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                        .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                    );
                }

                var newCommand = new TestCommand();
                var cmd = new Command("x");
                newCommand.Configure(cmd);
                var args = new List<string>
                {
                    "--version", pkgVersion
                };
                TestUtilities.GetParser(cmd).Invoke(args.ToArray(), console);

                string errors = console.Error.ToString().Trim();
                Assert.True(errors.Length == 0, $"Errors found in console: {errors}");
                Assert.True(Directory.Exists(packageId), "Package folder is missing");
                Assert.True(File.Exists($"{packageId}/cmfpackage.json"), "Package cmfpackage.json is missing");
                Assert.Equal(packageId, TestUtilities.GetPackageProperty("packageId", $"{packageId}/cmfpackage.json"), "Package Id does not match expected");
                Assert.Equal(pkgVersion, TestUtilities.GetPackageProperty("version", $"{packageId}/cmfpackage.json"), "Package version does not match expected");
                var pkg = TestUtilities.GetPackage("cmfpackage.json");
                pkg.GetProperty("testPackages").EnumerateArray().ToArray().FirstOrDefault(d => d.GetProperty("id").GetString() == packageId).Should().NotBeNull("Package not found in root package dependencies");
                var masterDataPackageId = "Cmf.Custom.Tests.MasterData";
                pkg.GetProperty("testPackages").EnumerateArray().ToArray().FirstOrDefault(d => d.GetProperty("id").GetString() == masterDataPackageId).Should().NotBeNull("Package not found in root package dependencies");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                int tries = 3;
                while (tries > 0)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        break;
                    }
                    catch
                    {
                        tries--;
                    }
                }
            }
        }

        [Fact, Trait("TestCategory", "LongRunning")]
        public void Traditional()
        {
            var dir = TestUtilities.GetTmpDirectory();

            var rnd = new Random();
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(dir);
                TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
                var projCfg = Path.Join(dir, ".project-config.json");
                if (File.Exists(projCfg))
                {
                    File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                        .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                        .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                        .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                    );
                }

                RunNew(new BusinessCommand(), "Cmf.Custom.Business", scaffoldingDir: dir);
                RunNew(new DataCommand(), "Cmf.Custom.Data", scaffoldingDir: dir);
                // UI_internal(dir);
                // Help_internal(dir);
                RunNew(new IoTCommand(), "Cmf.Custom.IoT", scaffoldingDir: dir);
                RunDatabase(dir);
                // Tests_internal(dir);
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Feature_WithPrefix()
        {
            const string packageId = "Cmf.Custom.Feature";
            var console = RunNew(new FeatureCommand(), packageId, extraArguments: new[] { packageId }, defaultAsserts: false, extraAsserts: args =>
            {
                var (packageVersion, _) = args;
                Assert.True(Directory.Exists($"Features/{packageId}"), "Package folder is missing");
                Assert.True(File.Exists($"Features/{packageId}/cmfpackage.json"), "Package cmfpackage.json is missing");
                Assert.Equal(packageId, TestUtilities.GetPackageProperty("packageId", $"Features/{packageId}/cmfpackage.json"), "Package Id does not match expected");
                Assert.Equal(packageVersion, TestUtilities.GetPackageProperty("version", $"Features/{packageId}/cmfpackage.json"), "Package version does not match expected");
            });
            string errors = console.Error.ToString().Trim();
            Assert.True(errors.Length == 0, $"Errors found in console: {errors}");
        }

        [Fact]
        public void Feature_WithoutPrefix()
        {
            RunFeature_WithoutPrefix(null);
        }

        private void RunFeature_WithoutPrefix(string scaffoldingDir)
        {
            const string packageId = "TestFeature";
            var console = RunNew(new FeatureCommand(), packageId, scaffoldingDir: scaffoldingDir, extraArguments: new[] { packageId }, defaultAsserts: false, extraAsserts: args =>
            {
                var (packageVersion, _) = args;
                Assert.True(Directory.Exists($"Features/{packageId}"), "Package folder is missing");
                Assert.True(File.Exists($"Features/{packageId}/cmfpackage.json"), "Package cmfpackage.json is missing");
                Assert.Equal(packageId, TestUtilities.GetPackageProperty("packageId", $"Features/{packageId}/cmfpackage.json"), "Package Id does not match expected");
                Assert.Equal(packageVersion, TestUtilities.GetPackageProperty("version", $"Features/{packageId}/cmfpackage.json"), "Package version does not match expected");
            });
            string errors = console.Error.ToString().Trim();
            Assert.True(errors.Length == 0, $"Errors found in console: {errors}");
        }

        [Fact]
        public void Features_RootPackageWithFeature()
        {
            var dir = TestUtilities.GetTmpDirectory();

            var rnd = new Random();
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(dir);
                TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
                
                var projCfg = Path.Join(dir, ".project-config.json");
                if (File.Exists(projCfg))
                {
                    File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                        .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                        .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                        .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                    );
                }

                RunFeature_WithoutPrefix(scaffoldingDir: dir);
                var console = RunNew(new BusinessCommand(), "Cmf.Custom.Business", scaffoldingDir: dir, defaultAsserts: false);
                // TODO: logger isn't using IConsole. This means we can only catch errors coming from Exception, which is not the case here
                //string errors = console.Error.ToString().Trim();
                //Assert.True(errors.Contains("Cannot create a root-level layer package when features already exist."), "Expected to find specific error in console but instead found: {0}", errors);
                // however we can test that the package was not created
                Directory.Exists("Cmf.Custom.Business").Should().BeFalse("Package folder is present and shouldn't have been created");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Features_Business()
        {
            var dir = TestUtilities.GetTmpDirectory();

            var rnd = new Random();
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(dir);
                TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
                TestUtilities.CopyFixture("featureBase", new DirectoryInfo(dir));
                
                var projCfg = Path.Join(dir, ".project-config.json");
                if (File.Exists(projCfg))
                {
                    File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                        .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                        .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                        .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                    );
                }

                var pkgDir = Path.Join(dir, "Features", "TestFeature");
                const string packageId = "Cmf.Custom.TestFeature.Business";
                var console = RunNew(new BusinessCommand(), packageId, scaffoldingDir: pkgDir, extraAsserts: args =>
                {
                    var (pkgVersion, _) = args;
                    Assert.True(File.Exists(Path.Join(dir, "Features/TestFeature", $"{packageId}/Cmf.Custom.Common/tenantConstants.cs")), "Constants file is missing or has wrong name");
                    Assert.True(File.Exists(Path.Join(dir, "Features/TestFeature", $"{packageId}/Cmf.Custom.Common/Cmf.Custom.tenant.TestFeature.Common.csproj")), "Common project file is missing or has wrong name");
                    // namespace checks
                    Assert.True(File.ReadAllText(Path.Join(dir, "Features/TestFeature", $"{packageId}/Cmf.Custom.Common/tenantConstants.cs")).Contains("namespace Cmf.Custom.tenant.TestFeature.Common"), "Constants namespace is wrong name");
                    // assembly name checks
                    Assert.True(File.ReadAllText(Path.Join(dir, "Features/TestFeature", $"{packageId}/Cmf.Custom.Common/Cmf.Custom.tenant.TestFeature.Common.csproj")).Contains("<AssemblyName>Cmf.Custom.tenant.TestFeature.Common</AssemblyName>"), "Constants assembly name is wrong name");
                });
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(dir, true);
            }
        }

        [Fact, Trait("TestCategory", "LongRunning")]
        public void Features_Help()
        {
            var dir = TestUtilities.GetTmpDirectory();

            var rnd = new Random();
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(dir);
                TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
                TestUtilities.CopyFixture("featureBase", new DirectoryInfo(dir));
                
                var projCfg = Path.Join(dir, ".project-config.json");
                if (File.Exists(projCfg))
                {
                    File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                        .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                        .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                        .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                    );
                }

                var pkgDir = Path.Join(dir, "Features", "TestFeature");
                const string packageId = "Cmf.Custom.TestFeature.Help";
                var console = RunNew(new Cmf.CLI.Commands.New.HelpCommand(), packageId, extraArguments: new string[] {
                    "--docPkg", TestUtilities.GetFixturePath("prodPkg", "Cmf.Documentation.9.9.9.zip"),
                }, scaffoldingDir: pkgDir, extraAsserts: args =>
                {
                    var (pkgVersion, _) = args;
                    Assert.True(Directory.Exists(Path.Join(dir, "Features/TestFeature", $"{packageId}/src/packages/cmf.docs.area.{packageId.ToLowerInvariant()}")), "Help package is missing or has wrong name");
                });
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                int tries = 3;
                while (tries > 0)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        break;
                    }
                    catch
                    {
                        tries--;
                    }
                }
            }
        }

        [Fact, Trait("TestCategory", "LongRunning")]
        public void Features_UI()
        {
            var dir = TestUtilities.GetTmpDirectory();

            var rnd = new Random();
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(dir);
                TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
                TestUtilities.CopyFixture("featureBase", new DirectoryInfo(dir));
                var projCfg = Path.Join(dir, ".project-config.json");
                if (File.Exists(projCfg))
                {
                    File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                        .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                        .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                        .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                    );
                }

                var pkgDir = Path.Join(dir, "Features", "TestFeature");
                const string packageId = "Cmf.Custom.TestFeature.HTML";
                var console = RunNew(new Cmf.CLI.Commands.New.HTMLCommand(), packageId, extraArguments: new string[] {
                    "--htmlPkg", TestUtilities.GetFixturePath("prodPkg", "Cmf.Presentation.HTML.9.9.9.zip"),
                }, scaffoldingDir: pkgDir);
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                int tries = 3;
                while (tries > 0)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        break;
                    }
                    catch
                    {
                        tries--;
                    }
                }
            }
        }

        [Fact]
        public void SecurityPortal()
        {
            string dir = TestUtilities.GetTmpDirectory();
            TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
            var projCfg = Path.Join(dir, ".project-config.json");
            if (File.Exists(projCfg))
            {
                File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                    .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                    .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                    .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                );
            }

            Random rnd = new Random();
            string pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            string cur = Directory.GetCurrentDirectory();

            const string packageId = "Cmf.Custom.SecurityPortal";
            TestConsole console = RunNew(new Cmf.Common.Cli.Commands.New.SecurityPortalCommand(), packageId, scaffoldingDir: dir);

            Assert.True(File.Exists($"{dir}/Cmf.Custom.SecurityPortal/cmfpackage.json"), "Package cmfpackage.json is missing");
            Assert.True(File.Exists($"{dir}/Cmf.Custom.SecurityPortal/config.json"), "Package config.json is missing");
        }

        private TestConsole RunNew<T>(T newCommand, string packageId, string scaffoldingDir = null,
            string[] extraArguments = null, bool defaultAsserts = true, Action<(string, string)> extraAsserts = null,
            string mesVersion = "8.2.0",
            BaseLayer baseLayer = BaseLayer.MES,
            RepositoryType repositoryType = RepositoryType.Customization) where T : TemplateCommand
        {
            var dir = scaffoldingDir ?? TestUtilities.GetTmpDirectory();

            var rnd = new Random();
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            var console = new TestConsole();
            try
            {
                Directory.SetCurrentDirectory(dir);

                // place new fixture: an init'd repository
                if (scaffoldingDir == null)
                {
                    TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
                    var projCfg = Path.Join(dir, ".project-config.json");
                    if (File.Exists(projCfg))
                    {
                        File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                            .Replace(@"""MESVersion"": ""8.2.0""", $@"""MESVersion"": ""{mesVersion}""")
                            .Replace(@"""BaseLayer"": ""MES""", $@"""BaseLayer"": ""{baseLayer}""")
                            .Replace(@"""RepositoryType"": ""Customization""", $@"""RepositoryType"": ""{repositoryType}""")
                            .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                            .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                            .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                        );
                    }
                }

                ExecutionContext.Initialize(new FileSystem());

                var cmd = new Command("x");
                newCommand.Configure(cmd);
                var args = new List<string>
                {
                    "--version", pkgVersion,
                    dir
                };
                if (extraArguments != null)
                {
                    args.InsertRange(0, extraArguments);
                }
                TestUtilities.GetParser(cmd).Invoke(args.ToArray(), console);

                if (defaultAsserts)
                {
                    string errors = console.Error.ToString().Trim();
                    Assert.True(errors.Length == 0, $"Errors found in console: {errors}");
                    Assert.True(Directory.Exists(packageId), "Package folder is missing");
                    Assert.True(File.Exists($"{packageId}/cmfpackage.json"), "Package cmfpackage.json is missing");
                    Assert.Equal(packageId, TestUtilities.GetPackageProperty("packageId", $"{packageId}/cmfpackage.json"), "Package Id does not match expected");
                    Assert.Equal(pkgVersion, TestUtilities.GetPackageProperty("version", $"{packageId}/cmfpackage.json"), "Package version does not match expected");
                    var pkg = TestUtilities.GetPackage("cmfpackage.json");
                    pkg.GetProperty("dependencies").EnumerateArray().ToArray().FirstOrDefault(d => d.GetProperty("id").GetString() == packageId).Should().NotBeNull("Package not found in root package dependencies");
                }

                if (extraAsserts != null)
                {
                    extraAsserts((pkgVersion, dir));
                }
            }
            finally
            {
                if (scaffoldingDir == null)
                {
                    Directory.SetCurrentDirectory(cur);
                    int tries = 3;
                    while (tries > 0)
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                            break;
                        }
                        catch
                        {
                            tries--;
                        }
                    }
                }
            }
            return console;
        }
    }
}

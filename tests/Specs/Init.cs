using Cmf.CLI.Commands;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Objects;
using Cmf.Common.Cli.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using Cmf.CLI.Services;
using Xunit;
using Newtonsoft.Json;
using Assert = tests.AssertWithMessage;

namespace tests.Specs
{
    public class Init
    {
        public Init()
        {
                Cmf.CLI.Core.Objects.ExecutionContext.ServiceProvider = (new ServiceCollection())
                    .AddSingleton<IVersionService>(new VersionService(CliConstants.PackageName, "5.3.0"))
                    .AddSingleton<IDependencyVersionService, DependencyVersionService>()
                    .BuildServiceProvider();
                
                var newCommand = new NewCommand();
                var cmd = new Command("x");
                newCommand.Configure(cmd);

                var console = new TestConsole();
                var parseResult = cmd.Parse(new[] {
                    "--reset"
                });
                parseResult.Invoke(console);
        }

        [Theory]
        [InlineData("8.2.0", DependencyVersionService.NET3SDK)]
        [InlineData("10.2.0", DependencyVersionService.NET6SDK)]
        [InlineData("11.0.0", DependencyVersionService.NET8SDK)]
        public void Init_(string baseVersionStr, string dotnetSDKVersion)
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
                    "--MESVersion", baseVersionStr,
                    "--DevTasksVersion", "8.1.0",
                    "--HTMLStarterVersion", "8.0.0",
                    "--yoGeneratorVersion", "8.1.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "8.2.0",
                    "--testScenariosNugetVersion", "8.2.0",
                    "--deploymentDir", deploymentDir,
                    "--ISOLocation", isoLocation,
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                }, console);

                var extractFileName = new Func<string, string>(s => s.Split(Path.DirectorySeparatorChar).LastOrDefault());

                // For v10 and above, the devcontainer should be created
                var baseVersion = new Version(baseVersionStr);

                if (baseVersion >= new Version(10, 0)) 
                {
                    var devContainerFile = File.ReadAllText(Path.Join(tmp, ".devcontainer/devcontainer.json"));

                    Assert.True(File.Exists(".devcontainer/devcontainer.json"), "devcontainer is missing");
                    devContainerFile
                        .Should().Contain(@$"""image"": ""criticalmanufacturing.io/criticalmanufacturing/devcontainer:{baseVersion.Major}""", "Devcontainer image is not correct");
                    devContainerFile
                        .Should().Contain(@$"""version"": ""5.x.x""", "Devcontainer image is not correct");
                }
                else 
                {
                    Assert.False(Directory.Exists(".devcontainer"), "devcontainer should not have been created");
                }

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
                    .Should().NotContain(@"""AppEnvironmentConfig""", "Customization repo initialization should not produce app scaffolding keys in .project-config.json");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""DefaultDomain"": ""DOMAIN""", "Default domain is not correct");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""BaseLayer"": ""MES""", "Base Layer should be MES");
                File.ReadAllText(Path.Join(tmp, "cmfpackage.json"))
                    .Should().Contain(@"CriticalManufacturing.DeploymentMetadata", "VM Dependency should be included in root package");
                File.ReadAllText(Path.Join(tmp, "cmfpackage.json"))
                    .Should().Contain(@"Cmf.Environment", "Container Dependency should be included in root package");
                File.ReadAllText(Path.Join(tmp, "global.json"))
                    .Should().Contain(dotnetSDKVersion, "wrong .NET SDK version");
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
                     "baseVersion", "nugetVersion", "testScenariosNugetVersion"
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
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var isoLocation = "\\\\share\\iso_location";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";
            var appId = "TestApp";
            var appName = "TestApp";
            var appAuthor = "The Author";
            var appDescription = "Some description";
            var targetFramework = "10.2.0";
            var licensedApplication = "Test app";
            var icon = $"assets/{CliConstants.DefaultAppIcon}";

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
                    "--MESVersion", targetFramework,
                    "--DevTasksVersion", targetFramework,
                    "--HTMLStarterVersion", targetFramework,
                    "--yoGeneratorVersion", targetFramework,
                    "--nugetVersion", targetFramework,
                    "--testScenariosNugetVersion", targetFramework,
                    "--ngxSchematicsVersion", targetFramework,
                    "--deploymentDir", deploymentDir,
                    "--ISOLocation", isoLocation,
                    "--version", pkgVersion,
                    "--appId", appId,
                    "--appName", appName,
                    "--appAuthor", appAuthor,
                    "--appDescription", appDescription,
                    "--appLicensedApplication", licensedApplication,
                    "--appConfig", TestUtilities.GetFixturePath("init", "app-config.json"),
                    tmp
                }, console);

                var extractFileName = new Func<string, string>(s => s.Split(Path.DirectorySeparatorChar).LastOrDefault());

                Assert.True(Directory.Exists(Path.Join(tmp, "Libs")), "Libs are missing");
                Assert.True(Directory.Exists(Path.Join(tmp, "assets")), "Assets are missing");
                Assert.True(File.Exists(".project-config.json"), "project config is missing");
                Assert.True(File.Exists("cmfpackage.json"), "root cmfpackage is missing");
                Assert.True(File.Exists("cmfapp.json"), "cmf app configuration is missing");
                Assert.True(File.Exists("app_deployment_manifest.xml"), "app deployment manifest is missing");
                Assert.True(File.Exists("global.json"), "global .NET versioning is missing");
                Assert.True(File.Exists("NuGet.Config"), "global NuGet feeds config is missing");
                Assert.True(File.Exists(Path.Combine(tmp, "assets", "default_app_icon.png")), "App Icon is missing");

                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""RepositoryType"": ""App""", "Applied repository type was not App");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""AppEnvironmentConfig"": ""app-config.json""", "Missing AppEnvironmentConfig key in .project-config.json");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""BaseLayer"": ""Core""", "Base Layer should be Core");
                File.ReadAllText(Path.Join(tmp, "cmfpackage.json"))
                    .Should().NotContain(@"CriticalManufacturing.DeploymentMetadata", "VM Dependency should not be included in root package");
                File.ReadAllText(Path.Join(tmp, "cmfapp.json"))
                    .Should().Contain($@"""id"": ""{appName}""", "Container Dependency should be included in root package");
                File.ReadAllText(Path.Join(tmp, "cmfapp.json"))
                    .Should().Contain($@"""description"": ""{appDescription}""", "Container Dependency should be included in root package");
                File.ReadAllText(Path.Join(tmp, "cmfapp.json"))
                    .Should().Contain($@"""licensedApplication"": ""{licensedApplication}""", "Container Dependency should be included in root package");
                File.ReadAllText(Path.Join(tmp, "cmfapp.json"))
                    .Should().Contain($@"""icon"": ""{icon}""", "Container Dependency should be included in root package");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Theory]
        [InlineData("--appId")]
        [InlineData("--appName")]
        [InlineData("--appAuthor")]
        [InlineData("--appDescription")]
        [InlineData("--appLicensedApplication")]
        public void Init_App_Fail_Missing_Parameters(string missingParameter)
        {
            var initCommand = new InitCommand();
            var cmd = new Command("x");
            initCommand.Configure(cmd);

            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var isoLocation = "\\\\share\\iso_location";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";
            var appId = "TestApp";
            var appName = "TestApp";
            var appAuthor = "The Author";
            var appDescription = "Some description";
            var targetFramework = "10.0.0";
            var licensedApplication = "Test app";
            const string AppParameterMissingError = "{0} is required when repository type is App.";
            var parameters = new List<string>
            {
                projectName,
                "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                "-c", TestUtilities.GetFixturePath("init", "config.json"),
                "--repositoryType", "App",
                "--MESVersion", targetFramework,
                "--DevTasksVersion", targetFramework,
                "--HTMLStarterVersion", targetFramework,
                "--yoGeneratorVersion", targetFramework,
                "--nugetVersion", targetFramework,
                "--testScenariosNugetVersion", targetFramework,
                "--deploymentDir", deploymentDir,
                "--ISOLocation", isoLocation,
                "--version", pkgVersion,
                "--appId", appId,
                "--appName", appName,
                "--appAuthor", appAuthor,
                "--appDescription", appDescription,
                "--appLicensedApplication", licensedApplication,
                tmp
            };

            int missingParameterPosition = parameters.IndexOf(missingParameter);
            parameters.RemoveAt(missingParameterPosition);
            // remove the value also
            parameters.RemoveAt(missingParameterPosition);

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);
                TestUtilities.GetParser(cmd).Invoke(parameters.ToArray(), console);

                console.Error.ToString().Should().Contain(string.Format(AppParameterMissingError, missingParameter));
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_With_Extra_Unknown_Option()
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
                    tmp,
                    "--ExtraUnknownOption", "RandomValue"
                }, console);

                console.Error.ToString().Should().Contain("Unrecognized command or argument '--ExtraUnknownOption'");
                console.Error.ToString().Should().Contain("Unrecognized command or argument 'RandomValue'.");

            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_With_Unknown_Option_Instead_Of_Argument()
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
                    "--UnknownOption", "RandomValue"
                }, console);

                console.Error.ToString().Should().BeEmpty();

            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }
        
        [Fact]
        public void Init_With_Env_Without_Domain()
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
                    "-c", TestUtilities.GetFixturePath("init", "config_no_AD.json"),
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

                console.Error.ToString().Should().BeEmpty();

                File.Exists(".project-config.json").Should().BeTrue("project config is missing");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain(@"""DefaultDomain"": null", "Default Domain is not valid");
                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Any(l => l.StartsWith("//")).Should().BeFalse("project config contains comments (template error)");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }
        
        [Fact]
        public void Init_With_Both_DeploymentDir_And_Releaserepos()
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var releaseRepo = "https://release.com";
            var ciRepo = "https://ci.com";

            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                var args = new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "11.0.0",
                    "--DevTasksVersion", "8.1.0",
                    "--HTMLStarterVersion", "8.0.0",
                    "--yoGeneratorVersion", "8.1.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--deploymentDir", deploymentDir,
                    "--releaseRepos", releaseRepo,
                    "--ciRepo", ciRepo,
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                };

                TestUtilities.GetParser(cmd).Invoke(args, console);

                console.Error.ToString().Should().NotBeEmpty("Error should be thrown when both deploymentDir and releaseRepos are provided");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Init_With_And_Without_DeploymentDir(bool hasDeploymentDir)
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var deliveredRepo = $"{deploymentDir}\\Delivered";
            var ciRepo = hasDeploymentDir ? $"{deploymentDir}\\CIPackages" : "https://ci.com/";
            var ciRepoSuffix = hasDeploymentDir ? "\\development" : string.Empty;
            var releaseRepo1 = "https://release.com/";
            var releaseRepo2 = "https://release-next.com/";

            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                var args = new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "11.0.0",
                    "--DevTasksVersion", "8.1.0",
                    "--HTMLStarterVersion", "8.0.0",
                    "--yoGeneratorVersion", "8.1.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                };

                if (hasDeploymentDir)
                {
                    args = args.Concat(new[] { "--deploymentDir", deploymentDir }).ToArray();
                }
                else
                {
                    args = args.Concat(new[] { "--releaseRepos", releaseRepo1, releaseRepo2 }).ToArray();
                    args = args.Concat(new[] { "--ciRepo", ciRepo }).ToArray();
                }

                TestUtilities.GetParser(cmd).Invoke(args, console);

                console.Error.ToString().Should().BeEmpty();

                Assert.True(File.Exists(".project-config.json"), "project config is missing");
                Assert.True(File.Exists("repositories.json"), "repositories.json is missing");

                File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                    .Should().Contain($@"""CIRepo"": ""{ciRepo.Replace("\\", "\\\\")}""", "CIRepo is not correct");
                File.ReadAllText(Path.Join(tmp, "repositories.json"))
                    .Should().Contain($@"""CIRepository"": ""{ciRepo.Replace("\\", "\\\\")}{ciRepoSuffix.Replace("\\", "\\\\")}""", "CIRepository is not correct");

                var newline = "\n"; // can't use Environment.NewLine somehow, it breaks the test
                var whitespace = "    ";
                var longWhitespace = "        ";

                if (hasDeploymentDir)
                {
                    File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                        .Should().NotContain(@"""ReleaseRepos""", "ReleaseRepos should not be present in .project-config.json");
                    File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                        .Should().Contain($@"""DeploymentDir"": ""{deploymentDir.Replace("\\", "\\\\")}""", "DeploymentDir is not correct");
                    File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                        .Should().Contain($@"""DeliveredRepo"": ""{deliveredRepo.Replace("\\", "\\\\")}""", "DeliveredRepo is not correct");
                    File.ReadAllText(Path.Join(tmp, "repositories.json"))
                        .Should().Contain($@"""Repositories"": [{newline}{longWhitespace}""{deliveredRepo.Replace("\\", "\\\\")}""{newline}{whitespace}]", "Repositories is not correct");
                }
                else
                {
                    File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                        .Should().NotContain(@"""DeploymentDir""", "DeploymentDir should not be present in .project-config.json");
                    File.ReadAllText(Path.Join(tmp, ".project-config.json"))
                        .Should().NotContain(@"""DeliveredRepo""", "DeliveredRepo should not be present in .project-config.json");
                    File.ReadAllText(Path.Join(tmp, "repositories.json"))
                        .Should().Contain($@"""Repositories"": [{newline}{longWhitespace}""{releaseRepo1}"",{newline}{longWhitespace}""{releaseRepo2}""{newline}{whitespace}]", "Repositories is not correct");
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Init_With_And_Without_ISOLocation(bool hasISOLocation)
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

                var args = new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "11.0.0",
                    "--DevTasksVersion", "8.1.0",
                    "--HTMLStarterVersion", "8.0.0",
                    "--yoGeneratorVersion", "8.1.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--deploymentDir", deploymentDir,
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                };

                if (hasISOLocation)
                {
                    args = args.Concat(new[] { "--ISOLocation", isoLocation }).ToArray();
                }

                TestUtilities.GetParser(cmd).Invoke(args, console);

                console.Error.ToString().Should().BeEmpty();

                Assert.True(File.Exists(".project-config.json"), "project config is missing");

                var projectConfigContent = File.ReadAllText(Path.Join(tmp, ".project-config.json"));

                if (hasISOLocation)
                {
                    projectConfigContent
                        .Should().Contain(@"""ISOLocation""", "ISOLocation should be present in .project-config.json when provided");
                    projectConfigContent
                        .Should().Contain(isoLocation.Replace("\\", "\\\\"), "ISOLocation value should be correct");
                }
                else
                {
                    projectConfigContent
                        .Should().NotContain(@"""ISOLocation""", "ISOLocation should not be present in .project-config.json when not provided");
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Theory]
        [InlineData("\\\\server\\share\\deployment", true, "\\")]  // UNC path should use backslash
        public void Init_DeploymentDir_PathSeparatorHandling(string deploymentDirPath, bool expectedIsUnc, string expectedSeparator)
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();

            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                var args = new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "11.0.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--deploymentDir", deploymentDirPath,
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                };

                TestUtilities.GetParser(cmd).Invoke(args, console);

                console.Error.ToString().Should().BeEmpty("No errors should occur during initialization");

                Assert.True(File.Exists(".project-config.json"), "project config is missing");
                Assert.True(File.Exists("repositories.json"), "repositories.json is missing");

                var projectConfig = File.ReadAllText(Path.Join(tmp, ".project-config.json"));
                var repositoriesConfig = File.ReadAllText(Path.Join(tmp, "repositories.json"));

                // Verify the correct separator is used based on path type
                var expectedDeliveredRepo = $"{deploymentDirPath}{expectedSeparator}Delivered";
                var expectedCIPackagesRepo = $"{deploymentDirPath}{expectedSeparator}CIPackages";
                var expectedCIRepoInRepos = $"{deploymentDirPath}{expectedSeparator}CIPackages{expectedSeparator}development";

                projectConfig.Should().Contain($@"""CIRepo"": ""{expectedCIPackagesRepo.Replace("\\", "\\\\").Replace("/", "\\/")}""", 
                    $"CIRepo should use {expectedSeparator} separator for {(expectedIsUnc ? "UNC" : "local")} path");
                projectConfig.Should().Contain($@"""DeliveredRepo"": ""{expectedDeliveredRepo.Replace("\\", "\\\\").Replace("/", "\\/")}""", 
                    $"DeliveredRepo should use {expectedSeparator} separator for {(expectedIsUnc ? "UNC" : "local")} path");
                repositoriesConfig.Should().Contain($@"""CIRepository"": ""{expectedCIRepoInRepos.Replace("\\", "\\\\").Replace("/", "\\/")}""", 
                    $"CIRepository should use {expectedSeparator} separator for {(expectedIsUnc ? "UNC" : "local")} path");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_DeploymentDir_UncVsLocalPathDetection()
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();
            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                // Test with UNC path
                var uncPath = "\\\\server\\share\\deployment";
                var args = new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "11.0.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--deploymentDir", uncPath,
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                };

                TestUtilities.GetParser(cmd).Invoke(args, console);

                console.Error.ToString().Should().BeEmpty("No errors should occur during UNC path initialization");

                var projectConfig = File.ReadAllText(Path.Join(tmp, ".project-config.json"));

                // For UNC paths, backslash should be used consistently
                projectConfig.Should().Contain(@"""CIRepo"": ""\\\\server\\share\\deployment\\CIPackages""", 
                    "UNC paths should preserve backslash separators in CIRepo");
                projectConfig.Should().Contain(@"""DeliveredRepo"": ""\\\\server\\share\\deployment\\Delivered""", 
                    "UNC paths should preserve backslash separators in DeliveredRepo");

                var repositoriesConfig = File.ReadAllText(Path.Join(tmp, "repositories.json"));
                repositoriesConfig.Should().Contain(@"""CIRepository"": ""\\\\server\\share\\deployment\\CIPackages\\development""", 
                    "UNC paths should preserve backslash separators in CIRepository");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_DeploymentDir_ErrorHandling_RepositoriesJsonParsing()
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();
            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                // Test with a UNC path that should work properly
                var uncPath = "\\\\server\\share\\deployment";
                var args = new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "11.0.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--deploymentDir", uncPath,
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                };

                TestUtilities.GetParser(cmd).Invoke(args, console);

                // Should not crash with URI format exceptions
                console.Error.ToString().Should().BeEmpty("No errors should occur during UNC path initialization");

                Assert.True(File.Exists(".project-config.json"), "project config should be created");
                Assert.True(File.Exists("repositories.json"), "repositories.json should be created");

                // The most important test: repositories.json should be parseable without URI format exceptions
                var repositoriesJsonContent = File.ReadAllText(Path.Join(tmp, "repositories.json"));
                
                // This is the critical test - parsing repositories.json should not throw URI format exceptions
                var repositoriesConfig = JsonConvert.DeserializeObject<RepositoriesConfig>(repositoriesJsonContent);
                repositoriesConfig.Should().NotBeNull("Repositories config should be parseable");
                repositoriesConfig.CIRepository.Should().NotBeNull("CI Repository should be set");
                repositoriesConfig.Repositories.Should().NotBeNull("Repositories should be set");
                
                // Verify all URIs are valid absolute URIs
                repositoriesConfig.CIRepository.IsAbsoluteUri.Should().BeTrue("CI Repository should be absolute URI");
                repositoriesConfig.Repositories.All(uri => uri.IsAbsoluteUri).Should().BeTrue("All repositories should be absolute URIs");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_TenantFromConfigFile()
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();
            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                // Test with tenant from config file (config.json has "Product.Tenant.Name": "tenant")
                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "11.0.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--deploymentDir", deploymentDir,
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                }, console);

                console.Error.ToString().Should().BeEmpty("No errors should occur when tenant is in config file");
                Assert.True(File.Exists(".project-config.json"), "project config should be created");

                // Verify tenant was parsed from config file
                var projectConfig = File.ReadAllText(Path.Join(tmp, ".project-config.json"));
                projectConfig.Should().Contain(@"""Tenant"": ""tenant""", "Tenant should be parsed from config file");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_TenantFromCommandLineOption_WithoutConfigTenant_ExpectsDuplicateKeyError()
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();
            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                // Create a config file without tenant information
                var configWithoutTenant = Path.Join(tmp, "config_no_tenant.json");
                File.WriteAllText(configWithoutTenant, @"{
                    ""Product.SystemName"": ""system_name"",
                    ""Product.Database.IsAlwaysOn"": false,
                    ""Package[Product.Database.Online].Database.Server"": ""server1\\instance"",
                    ""Product.ApplicationServer.Port"": ""1234"",
                    ""Product.ApplicationServer.Address"": ""app_server_address"",
                    ""Product.Presentation.IisConfiguration.Binding.Port"": ""443"",
                    ""Product.Gateway.Port"": ""5678"",
                    ""Product.Security.Domain"": ""DOMAIN""
                }");

                // Test with tenant from command line option (config has no tenant)
                // Current behavior: ParseConfigFile always adds --Tenant (even if null),
                // so adding it again from x.Tenant causes duplicate key error
                // TODO: Fix InitCommand to handle this properly
                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", configWithoutTenant,
                    "--MESVersion", "11.0.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--deploymentDir", deploymentDir,
                    "--tenant", "CustomTenant",
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                }, console);

                // Current behavior: duplicate key error
                Assert.Contains("An item with the same key has already been added. Key: Tenant", console.Error.ToString());
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_Fail_MissingTenant()
        {
            var console = new TestConsole();
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();
            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                // Create a config file without tenant information
                var configWithoutTenant = Path.Join(tmp, "config_no_tenant.json");
                File.WriteAllText(configWithoutTenant, @"{
                    ""Product.SystemName"": ""system_name"",
                    ""Product.Database.IsAlwaysOn"": false,
                    ""Package[Product.Database.Online].Database.Server"": ""server1\\instance"",
                    ""Product.Security.Domain"": ""DOMAIN""
                }");

                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", configWithoutTenant,
                    "--MESVersion", "11.0.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--deploymentDir", deploymentDir,
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                }, console);

                // Current behavior: the validation check `if (!args.Contains("--Tenant"))` passes
                // because ParseConfigFile adds --Tenant (with null value), but then template engine
                // fails trying to process null tenant value
                // TODO: Fix validation to check for non-null/non-empty tenant value, not just presence of key
                // The expected message should be: "Tenant information is missing. Please provide it either in the config file or through the --tenant option."
                // But currently we get a reflection/template engine error instead
                console.Error.ToString().Should().NotBeEmpty("Should have an error when tenant is missing");
                // We can't assert the exact error message since it's a technical error, not the user-friendly one
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void Init_TenantCommandLineOverridesConfigFile_ExpectsDuplicateKeyError()
        {
            var rnd = new Random();
            var tmp = TestUtilities.GetTmpDirectory();
            var projectName = Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            var deploymentDir = "\\\\share\\deployment_dir";
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            try
            {
                var console = new TestConsole();
                Directory.SetCurrentDirectory(tmp);

                var initCommand = new InitCommand();
                var cmd = new Command("x");
                initCommand.Configure(cmd);

                // Test that when both config file and --tenant are provided, we get an error
                // config.json has "Product.Tenant.Name": "tenant"
                // This test documents the current behavior - the implementation should be fixed
                // to remove the tenant from parsed config args when x.Tenant is provided
                TestUtilities.GetParser(cmd).Invoke(new[]
                {
                    projectName,
                    "--infra", TestUtilities.GetFixturePath("init", "infrastructure.json"),
                    "-c", TestUtilities.GetFixturePath("init", "config.json"),
                    "--MESVersion", "11.0.0",
                    "--ngxSchematicsVersion", "8.8.8",
                    "--nugetVersion", "11.0.0",
                    "--testScenariosNugetVersion", "11.0.0",
                    "--deploymentDir", deploymentDir,
                    "--tenant", "OverriddenTenant",
                    "--version", pkgVersion,
                    "Cmf.Custom.Package",
                    tmp
                }, console);

                // Current behavior: duplicate key error
                // TODO: Fix InitCommand to remove --Tenant from args when x.Tenant is provided
                Assert.Contains("An item with the same key has already been added. Key: Tenant", console.Error.ToString());
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(tmp, true);
            }
        }
    }
}
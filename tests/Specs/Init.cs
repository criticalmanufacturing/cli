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
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Cmf.CLI.Services;
using Xunit;
using Assert = tests.AssertWithMessage;

namespace tests.Specs
{
    public class Init
    {
        public Init()
        {
                Cmf.CLI.Core.Objects.ExecutionContext.ServiceProvider = (new ServiceCollection())
                    .AddSingleton<IVersionService>(new VersionService(CliConstants.PackageName))
                    .AddSingleton<IDependencyVersionService, DependencyVersionService>()
                    .BuildServiceProvider();
        }

        [Theory]
        [InlineData("8.2.0", DependencyVersionService.NET3SDK)]
        [InlineData("10.2.0", DependencyVersionService.NET6SDK)]
        [InlineData("11.0.0", DependencyVersionService.NET8SDK)]
        public void Init_(string baseVersion, string dotnetSDKVersion)
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
                    "--MESVersion", baseVersion,
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
                     "baseVersion", "nugetVersion", "testScenariosNugetVersion", "deploymentDir"
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
            var icon = "";

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
        
        
        
    }
}

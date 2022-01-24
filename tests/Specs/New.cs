using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Commands.New;
using Cmf.Common.Cli.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests.Specs
{
    [TestClass]
    public class New
    {
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

                cmd.Invoke(new[]
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
                
                Assert.IsTrue(File.Exists(".project-config.json"), "project config is missing");
                Assert.IsTrue(File.Exists("cmfpackage.json"), "root cmfpackage is missing");
                Assert.IsTrue(File.Exists("global.json"), "global .NET versioning is missing");
                Assert.IsTrue(File.Exists("NuGet.Config"), "global NuGet feeds config is missing");
                
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

        [TestMethod]
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
                cmd.Invoke(new[]
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

                Assert.IsTrue(File.Exists(".project-config.json"), "project config is missing");
                Assert.IsTrue(File.Exists("cmfpackage.json"), "root cmfpackage is missing");
                Assert.IsTrue(File.Exists("global.json"), "global .NET versioning is missing");
                Assert.IsTrue(File.Exists("NuGet.Config"), "global NuGet feeds config is missing");

                Assert.IsTrue(Directory.Exists(Path.Join(tmp, "Builds")), "pipelines are missing");
                Assert.IsTrue(
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
                Assert.IsTrue(
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
                Assert.IsTrue(
                    new[]{ "policies-master.json",
                            "policies-development.json" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("Builds")
                            .Select(extractFileName)
                            .Contains(f)), "Missing policy metadata");
                Assert.IsTrue(Directory.Exists(Path.Join(tmp, "EnvironmentConfigs")), "environment configs are missing");
                Assert.IsTrue(
                    new[] { "GlobalVariables.yml" }
                        .ToList()
                        .All(f => Directory
                            .GetFiles("EnvironmentConfigs")
                            .Select(extractFileName)
                            .Contains(f)), "Missing global variables");
                Assert.IsTrue(
                    new[] { "config.json" } // this should be a constant when moving to a mock
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

        [TestMethod]
        public void Business()
        {
            RunNew(new BusinessCommand(), "Cmf.Custom.Business", extraAsserts: args =>
            {
                var (pkgVersion, dir) = args;
                Assert.IsTrue(File.Exists($"Cmf.Custom.Business/Cmf.Custom.Common/tenantConstants.cs"), "Constants file is missing or has wrong name");
                Assert.IsTrue(File.Exists($"Cmf.Custom.Business/Cmf.Custom.Common/Cmf.Custom.tenant.Common.csproj"), "Common project file is missing or has wrong name");
                // namespace checks
                Assert.IsTrue(File.ReadAllText("Cmf.Custom.Business/Cmf.Custom.Common/tenantConstants.cs").Contains("namespace Cmf.Custom.tenant.Common"), "Constants namespace is wrong name");
                // assembly name checks
                Assert.IsTrue(File.ReadAllText("Cmf.Custom.Business/Cmf.Custom.Common/Cmf.Custom.tenant.Common.csproj").Contains("<AssemblyName>Cmf.Custom.tenant.Common</AssemblyName>"), "Constants assembly name is wrong name");
            });
        }
        [TestMethod]
        public void Data()
        {
            RunNew(new DataCommand(), "Cmf.Custom.Data");
        }

        [TestMethod, TestCategory("LongRunning")]
        public void UI()
        {
            RunNew(new HTMLCommand(), "Cmf.Custom.HTML", extraArguments: new string[]
            {
                "--htmlPkg", TestUtilities.GetFixturePath("prodPkg", "Cmf.Presentation.HTML.9.9.9.zip"),
            }, extraAsserts: args =>
            {
                Assert.IsTrue(File.Exists($"Cmf.Custom.HTML/.dev-tasks.json"), "dev-tasks file is missing or has wrong name. Was cloning HTML-starter unsuccessful?");
                Assert.IsTrue(Directory.Exists($"Cmf.Custom.HTML/apps/customization.web"), "WebApp dir is missing or has wrong name. Was running the application generator unsuccessful?");
                Assert.IsTrue(File.Exists($"Cmf.Custom.HTML/apps/customization.web/config.json"), "Config file is missing or has wrong name");
                Assert.IsTrue(File.ReadAllText("Cmf.Custom.HTML/apps/customization.web/config.json").Contains("test.package"), "Product package is not in config.json");
                Assert.IsTrue(File.Exists($"Cmf.Custom.HTML/apps/customization.web/index.html"), "Index file is missing or has wrong name");
            });
        }

        [TestMethod]
        public void UI_FailNoPackage()
        {
            var console = RunNew(new HTMLCommand(), "Cmf.Custom.HTML", defaultAsserts: false);
            var stderr = console.Error.ToString();
            Assert.IsTrue(stderr.Trim().Equals("Option '--htmlPkg' is required."), "Should exit with missing package error");
        }

        [TestMethod, TestCategory("LongRunning")]
        public void Help()
        {
            RunNew(new Cmf.Common.Cli.Commands.New.HelpCommand(), "Cmf.Custom.Help", extraArguments: new string[] {
                "--docPkg", TestUtilities.GetFixturePath("prodPkg", "Cmf.Documentation.9.9.9.zip"),
            }, extraAsserts: args =>
            {
                Assert.IsTrue(File.Exists($"Cmf.Custom.Help/.dev-tasks.json"), "dev-tasks file is missing or has wrong name. Was cloning HTML-starter unsuccessful?");
                Assert.IsTrue(Directory.Exists($"Cmf.Custom.Help/apps/cmf.docs.area.web"), "WebApp dir is missing or has wrong name. Was running the application generator unsuccessful?");
                Assert.IsTrue(File.Exists($"Cmf.Custom.Help/apps/cmf.docs.area.web/config.json"), "Config file is missing or has wrong name");
                Assert.IsTrue(File.ReadAllText("Cmf.Custom.Help/apps/cmf.docs.area.web/config.json").Contains("test.package"), "Product package is not in config.json");
                Assert.IsTrue(File.Exists($"Cmf.Custom.Help/apps/cmf.docs.area.web/index.html"), "Index file is missing or has wrong name");
                Assert.IsTrue(File.ReadAllText("Cmf.Custom.Help/apps/cmf.docs.area.web/index.html").Contains("Documentation website"), "Index content is not expected");
                Assert.IsTrue(File.ReadAllText("Cmf.Custom.Help/apps/cmf.docs.area.web/index.html").Contains("<base href=\"/\">"), "Index base path was not changed correctly");
            });
        }

        [TestMethod]
        public void Help_FailNoPackage()
        {
            var console = RunNew(new Cmf.Common.Cli.Commands.New.HelpCommand(), "Cmf.Custom.Help", defaultAsserts: false);
            var stderr = console.Error.ToString();
            Assert.IsTrue(stderr.Trim().Equals("Option '--docPkg' is required."), "Should exit with missing package error");
        }

        [TestMethod]
        public void IoT()
        {
            RunNew(new IoTCommand(), "Cmf.Custom.IoT");
        }
        [TestMethod]
        public void Database()
        {
            RunDatabase(null);
        }

        private void RunDatabase(string scaffoldingDir)
        {
            RunNew(new DatabaseCommand(), "Cmf.Custom.Database", scaffoldingDir: scaffoldingDir, defaultAsserts: false, extraAsserts: args =>
            {
                var (packageVersion, _) = args;
                Assert.IsTrue(Directory.Exists("Cmf.Custom.Database"), "Package folder is missing");
                Assert.IsTrue(File.Exists($"Cmf.Custom.Database/Pre/cmfpackage.json"), "Pre Package cmfpackage.json is missing");
                Assert.IsTrue(File.Exists($"Cmf.Custom.Database/Post/cmfpackage.json"), "Post Package cmfpackage.json is missing");
                Assert.IsTrue(File.Exists($"Cmf.Custom.Database/Post/Reporting/cmfpackage.json"), "Reports Package cmfpackage.json is missing");
                Assert.AreEqual("Cmf.Custom.Database.Pre", TestUtilities.GetPackageProperty("packageId", $"Cmf.Custom.Database/Pre/cmfpackage.json"), "Pre Package Id does not match expected");
                Assert.AreEqual(packageVersion, TestUtilities.GetPackageProperty("version", $"Cmf.Custom.Database/Pre/cmfpackage.json"), "Pre Package version does not match expected");
                Assert.AreEqual("Cmf.Custom.Database.Post", TestUtilities.GetPackageProperty("packageId", $"Cmf.Custom.Database/Post/cmfpackage.json"), "Post Package Id does not match expected");
                Assert.AreEqual(packageVersion, TestUtilities.GetPackageProperty("version", $"Cmf.Custom.Database/Post/cmfpackage.json"), "Post Package version does not match expected");
                Assert.AreEqual("Cmf.Custom.Reporting", TestUtilities.GetPackageProperty("packageId", $"Cmf.Custom.Database/Post/Reporting/cmfpackage.json"), "Reporting Package Id does not match expected");
                Assert.AreEqual(packageVersion, TestUtilities.GetPackageProperty("version", $"Cmf.Custom.Database/Post/Reporting/cmfpackage.json"), "Reporting Package version does not match expected");
                var pkg = TestUtilities.GetPackage("cmfpackage.json");
                var search = pkg.GetProperty("dependencies").EnumerateArray().ToArray().FirstOrDefault(d => d.GetProperty("id").GetString() == "Cmf.Custom.Database");
                Assert.AreEqual(JsonValueKind.Undefined, search.ValueKind, "Package was found in root package dependencies");
            });
        }

        // TODO: Tests doesn't work with RunNew (Execute is invoked on LayerTemplateCommand)
        [TestMethod]
        public void Tests()
        {
            var packageId = "Cmf.Custom.Tests";
            var dir = TestUtilities.GetTmpDirectory();

            var rnd = new Random();
            var pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            var cur = Directory.GetCurrentDirectory();
            var console = new TestConsole();
            try
            {
                Directory.SetCurrentDirectory(dir);

                // place new fixture: an init'd repository
                TestUtilities.CopyFixture("new", new DirectoryInfo(dir));

                var newCommand = new TestCommand();
                var cmd = new Command("x");
                newCommand.Configure(cmd);
                var args = new List<string>
                {
                    "--version", pkgVersion
                };
                cmd.Invoke(args.ToArray(), console);

                string errors = console.Error.ToString().Trim();
                Assert.IsTrue(errors.Length == 0, "Errors found in console: {0}", errors);
                Assert.IsTrue(Directory.Exists(packageId), "Package folder is missing");
                Assert.IsTrue(File.Exists($"{packageId}/cmfpackage.json"), "Package cmfpackage.json is missing");
                Assert.AreEqual(packageId, TestUtilities.GetPackageProperty("packageId", $"{packageId}/cmfpackage.json"), "Package Id does not match expected");
                Assert.AreEqual(pkgVersion, TestUtilities.GetPackageProperty("version", $"{packageId}/cmfpackage.json"), "Package version does not match expected");
                var pkg = TestUtilities.GetPackage("cmfpackage.json");
                Assert.IsNotNull(pkg.GetProperty("testPackages").EnumerateArray().ToArray().FirstOrDefault(d => d.GetProperty("id").GetString() == packageId), "Package not found in root package dependencies");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
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

        [TestMethod]
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

                RunNew(new BusinessCommand(), "Cmf.Custom.Business", scaffoldingDir: dir);
                RunNew(new DataCommand(), "Cmf.Custom.Data", scaffoldingDir: dir);
                //UI(dir);
                //Help(dir);
                RunNew(new IoTCommand(), "Cmf.Custom.IoT", scaffoldingDir: dir);
                RunDatabase(dir);
                //Tests(dir);
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void Feature_WithPrefix()
        {
            const string packageId = "Cmf.Custom.Feature";
            var console = RunNew(new FeatureCommand(), packageId, extraArguments: new[] { packageId }, defaultAsserts: false, extraAsserts: args =>
            {
                var (packageVersion, _) = args;
                Assert.IsTrue(Directory.Exists($"Features/{packageId}"), "Package folder is missing");
                Assert.IsTrue(File.Exists($"Features/{packageId}/cmfpackage.json"), "Package cmfpackage.json is missing");
                Assert.AreEqual(packageId, TestUtilities.GetPackageProperty("packageId", $"Features/{packageId}/cmfpackage.json"), "Package Id does not match expected");
                Assert.AreEqual(packageVersion, TestUtilities.GetPackageProperty("version", $"Features/{packageId}/cmfpackage.json"), "Package version does not match expected");
            });
            string errors = console.Error.ToString().Trim();
            Assert.IsTrue(errors.Length == 0, "Errors found in console: {0}", errors);
        }

        [TestMethod]
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
                Assert.IsTrue(Directory.Exists($"Features/{packageId}"), "Package folder is missing");
                Assert.IsTrue(File.Exists($"Features/{packageId}/cmfpackage.json"), "Package cmfpackage.json is missing");
                Assert.AreEqual(packageId, TestUtilities.GetPackageProperty("packageId", $"Features/{packageId}/cmfpackage.json"), "Package Id does not match expected");
                Assert.AreEqual(packageVersion, TestUtilities.GetPackageProperty("version", $"Features/{packageId}/cmfpackage.json"), "Package version does not match expected");
            });
            string errors = console.Error.ToString().Trim();
            Assert.IsTrue(errors.Length == 0, "Errors found in console: {0}", errors);
        }

        [TestMethod]
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

                RunFeature_WithoutPrefix(scaffoldingDir: dir);
                var console = RunNew(new BusinessCommand(), "Cmf.Custom.Business", scaffoldingDir: dir, defaultAsserts: false);
                // TODO: logger isn't using IConsole. This means we can only catch errors coming from Exception, which is not the case here
                //string errors = console.Error.ToString().Trim();
                //Assert.IsTrue(errors.Contains("Cannot create a root-level layer package when features already exist."), "Expected to find specific error in console but instead found: {0}", errors);
                // however we can test that the package was not created
                Assert.IsFalse(Directory.Exists("Cmf.Custom.Business"), "Package folder is present and shouldn't have been created");
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
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

                var pkgDir = Path.Join(dir, "Features", "TestFeature");
                const string packageId = "Cmf.Custom.TestFeature.Business";
                var console = RunNew(new BusinessCommand(), packageId, scaffoldingDir: pkgDir, extraAsserts: args =>
                {
                    var (pkgVersion, _) = args;
                    Assert.IsTrue(File.Exists(Path.Join(dir, "Features/TestFeature", $"{packageId}/Cmf.Custom.Common/tenantConstants.cs")), "Constants file is missing or has wrong name");
                    Assert.IsTrue(File.Exists(Path.Join(dir, "Features/TestFeature", $"{packageId}/Cmf.Custom.Common/Cmf.Custom.tenant.TestFeature.Common.csproj")), "Common project file is missing or has wrong name");
                    // namespace checks
                    Assert.IsTrue(File.ReadAllText(Path.Join(dir, "Features/TestFeature", $"{packageId}/Cmf.Custom.Common/tenantConstants.cs")).Contains("namespace Cmf.Custom.tenant.TestFeature.Common"), "Constants namespace is wrong name");
                    // assembly name checks
                    Assert.IsTrue(File.ReadAllText(Path.Join(dir, "Features/TestFeature", $"{packageId}/Cmf.Custom.Common/Cmf.Custom.tenant.TestFeature.Common.csproj")).Contains("<AssemblyName>Cmf.Custom.tenant.TestFeature.Common</AssemblyName>"), "Constants assembly name is wrong name");
                });
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
                Directory.Delete(dir, true);
            }
        }

        [TestMethod, TestCategory("LongRunning")]
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

                var pkgDir = Path.Join(dir, "Features", "TestFeature");
                const string packageId = "Cmf.Custom.TestFeature.Help";
                var console = RunNew(new Cmf.Common.Cli.Commands.New.HelpCommand(), packageId, extraArguments: new string[] {
                    "--docPkg", TestUtilities.GetFixturePath("prodPkg", "Cmf.Documentation.9.9.9.zip"),
                }, scaffoldingDir: pkgDir, extraAsserts: args =>
                {
                    var (pkgVersion, _) = args;
                    Assert.IsTrue(Directory.Exists(Path.Join(dir, "Features/TestFeature", $"{packageId}/src/packages/cmf.docs.area.tenant")), "Help package is missing or has wrong name");
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

        [TestMethod, TestCategory("LongRunning")]
        public void Features_HTML()
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

                var pkgDir = Path.Join(dir, "Features", "TestFeature");
                const string packageId = "Cmf.Custom.TestFeature.HTML";
                var console = RunNew(new Cmf.Common.Cli.Commands.New.HTMLCommand(), packageId, extraArguments: new string[] {
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

        [TestMethod]
        public void SecurityPortal()
        {
            string dir = TestUtilities.GetTmpDirectory();
            TestUtilities.CopyFixture("new", new DirectoryInfo(dir));

            Random rnd = new Random();
            string pkgVersion = $"{rnd.Next(10)}.{rnd.Next(10)}.{rnd.Next(10)}";

            string cur = Directory.GetCurrentDirectory();

            const string packageId = "Cmf.Custom.SecurityPortal";
            TestConsole console = RunNew(new Cmf.Common.Cli.Commands.New.SecurityPortalCommand(), packageId, scaffoldingDir: dir);

            Assert.IsTrue(File.Exists($"{dir}/Cmf.Custom.SecurityPortal/cmfpackage.json"), "Package cmfpackage.json is missing");
            Assert.IsTrue(File.Exists($"{dir}/Cmf.Custom.SecurityPortal/config.json"), "Package config.json is missing");
        }

        private TestConsole RunNew<T>(T newCommand, string packageId, string scaffoldingDir = null, string[] extraArguments = null, bool defaultAsserts = true, Action<(string, string)> extraAsserts = null) where T: TemplateCommand {
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
                }

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
                cmd.Invoke(args.ToArray(), console);

                if (defaultAsserts)
                {
                    string errors = console.Error.ToString().Trim();
                    Assert.IsTrue(errors.Length == 0, "Errors found in console: {0}", errors);
                    Assert.IsTrue(Directory.Exists(packageId), "Package folder is missing");
                    Assert.IsTrue(File.Exists($"{packageId}/cmfpackage.json"), "Package cmfpackage.json is missing");
                    Assert.AreEqual(packageId, TestUtilities.GetPackageProperty("packageId", $"{packageId}/cmfpackage.json"), "Package Id does not match expected");
                    Assert.AreEqual(pkgVersion, TestUtilities.GetPackageProperty("version", $"{packageId}/cmfpackage.json"), "Package version does not match expected");
                    var pkg = TestUtilities.GetPackage("cmfpackage.json");
                    Assert.IsNotNull(pkg.GetProperty("dependencies").EnumerateArray().ToArray().FirstOrDefault(d => d.GetProperty("id").GetString() == packageId), "Package not found in root package dependencies");
                }

                if (extraAsserts != null)
                {
                    extraAsserts((pkgVersion, dir));
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
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
                        } catch {
                            tries--;
                        }
                    }
                }
            }
            return console;
        }


    }
}
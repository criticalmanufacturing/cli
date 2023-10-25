using Cmf.CLI.Commands;
using Cmf.CLI.Commands.New;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.Common.Cli.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Packaging;
using System.Linq;
using System.Text.Json;
using Xunit;
using Assert = tests.AssertWithMessage;

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

        [Fact]
        public void Data_WithBusiness()
        {
            var dir = TestUtilities.GetTmpDirectory();
            // init
            CopyNewFixture(dir);


            var packageId = "Cmf.Custom.Data";
            var businessPackageName = "Cmf.Custom.Business";
            var targetDir = new DirectoryInfo(dir);
            TestUtilities.CopyFixturePackage(businessPackageName, targetDir);
            string businessPackageLocation = Path.Join(targetDir.FullName, businessPackageName);

            RunNew(new DataCommand(), packageId, scaffoldingDir: dir,
                mesVersion: "10.1.2",
                extraArguments: new string[]
                {
                    "--businessPackage", businessPackageLocation,
                },
                extraAsserts: args =>
                {
                    var relatedPackages = TestUtilities.GetPackage($"{packageId}/cmfpackage.json").GetProperty("relatedPackages")[0];
                    relatedPackages.GetProperty("path").GetString().Should().Be(MockUnixSupport.Path($"..\\{businessPackageName}"));
                    relatedPackages.GetProperty("preBuild").GetBoolean().Should().BeTrue();
                    relatedPackages.GetProperty("postBuild").GetBoolean().Should().BeFalse();
                    relatedPackages.GetProperty("prePack").GetBoolean().Should().BeFalse();
                    relatedPackages.GetProperty("postPack").GetBoolean().Should().BeFalse();
                });
        }

        [Theory, Trait("TestCategory", "LongRunning"), Trait("TestCategory", "Node12")]
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

        [Theory, Trait("TestCategory", "LongRunning"), Trait("TestCategory", "Node12")]
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

        [Fact, Trait("TestCategory", "LongRunning"), Trait("TestCategory", "Node12")]
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

        [Theory]
        [InlineData("9.0.0")]
        [InlineData("10.0.0"), Trait("TestCategory", "LongRunning"), Trait("TestCategory", "Internal")]
        [InlineData("10.0.0", true), Trait("TestCategory", "LongRunning"), Trait("TestCategory", "Internal")]
        public void IoT(string mesVersion, bool htmlPackageLocationFullPath = false)
        {
            string dir = TestUtilities.GetTmpDirectory();
            string packageId = "Cmf.Custom.IoT";

            string packageIdPackages = "Cmf.Custom.IoT.Packages";
            string packageFolderPackages = mesVersion == "9.0.0" ? "IoTPackages" : packageIdPackages;

            string packageIdData = "Cmf.Custom.IoT.Data";
            string packageFolderData = mesVersion == "9.0.0" ? "IoTData" : packageIdData;

            CopyNewFixture(dir, mesVersion: mesVersion);
            if (mesVersion == "9.0.0")
            {
                RunNew(new IoTCommand(), packageId, scaffoldingDir: dir);
            }
            else
            {
                var htmlPackageName = "Cmf.Custom.Html";
                var targetDir = new DirectoryInfo(dir);

                TestUtilities.CopyFixturePackage(htmlPackageName, targetDir);
                string htmlPackageLocation = htmlPackageLocationFullPath ? htmlPackageName : Path.Join(targetDir.FullName, htmlPackageName);
                RunNew(new IoTCommand(), packageId, mesVersion: mesVersion, scaffoldingDir: dir, extraArguments: new string[] { "--htmlPackageLocation", htmlPackageLocation });
            }

            File.ReadAllText(Path.Join(dir, packageId, "cmfpackage.json"))
                .Should().Contain(@"CriticalManufacturing.DeploymentMetadata");
            File.ReadAllText(Path.Join(dir, packageId, "cmfpackage.json"))
                .Should().Contain(@"Cmf.Environment");


            Directory.Exists($"{packageId}/{packageFolderPackages}").Should().BeTrue();
            TestUtilities.GetPackageProperty("packageId", $"{packageId}/{packageFolderPackages}/cmfpackage.json").Should().Be(packageIdPackages);
            TestUtilities.GetPackageProperty("packageType", $"{packageId}/{packageFolderPackages}/cmfpackage.json").Should().Be(PackageType.IoT.ToString());
            File.Exists($"{packageId}/{packageFolderPackages}/package.json").Should().BeTrue();

            Directory.Exists($"{packageId}/{packageFolderData}").Should().BeTrue();
            TestUtilities.GetPackageProperty("packageId", $"{packageId}/{packageFolderData}/cmfpackage.json").Should().Be(packageIdData);
            TestUtilities.GetPackageProperty("packageType", $"{packageId}/{packageFolderData}/cmfpackage.json").Should().Be(PackageType.IoTData.ToString());
            Directory.Exists($"{packageId}/{packageFolderData}/MasterData").Should().BeTrue();
            Directory.Exists($"{packageId}/{packageFolderData}/AutomationWorkFlows").Should().BeTrue();

            if(mesVersion == "9.0.0")
            {
                File.Exists($"{packageId}/{packageFolderPackages}/.dev-tasks.json").Should().BeTrue();
                Directory.Exists($"{packageId}/{packageFolderPackages}/src").Should().BeTrue();
                
            }
            else
            {
                var relatedPackages = TestUtilities.GetPackage($"{packageId}/{packageFolderPackages}/cmfpackage.json").GetProperty("relatedPackages")[0];
                relatedPackages.GetProperty("path").GetString().Should().Be(MockUnixSupport.Path("..\\..\\Cmf.Custom.Html"));
                relatedPackages.GetProperty("preBuild").GetBoolean().Should().BeFalse();
                relatedPackages.GetProperty("postBuild").GetBoolean().Should().BeTrue();
                relatedPackages.GetProperty("prePack").GetBoolean().Should().BeFalse();
                relatedPackages.GetProperty("postPack").GetBoolean().Should().BeTrue();
            }

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

        [Fact, Trait("TestCategory", "LongRunning"), Trait("TestCategory", "Node12")]
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

        [Fact, Trait("TestCategory", "LongRunning"), Trait("TestCategory", "Node12")]
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

        [Fact, Trait("TestCategory", "LongRunning"), Trait("TestCategory", "Internal")]
        public void UI_v10()
        {
            UI_internal_v10();
        }

        private void UI_internal_v10()
        {
            RunNew(new HTMLCommand(), "Cmf.Custom.HTML", mesVersion: "10.0.2", extraAsserts: args =>
            {
                var configJson = File.ReadAllText("Cmf.Custom.HTML/src/assets/config.json");
                try
                {
                    JsonConvert.DeserializeObject(configJson);
                }
                catch (Exception e)
                {
                    Assert.Fail($"config.json is malformed: {e.Message}");
                }
            });
        }

        private TestConsole RunNew<T>(T newCommand, string packageId, string scaffoldingDir = null,
            string[] extraArguments = null, bool defaultAsserts = true, Action<(string, string)> extraAsserts = null,
            string mesVersion = "8.2.0",
            string ngxSchematicsVersion = "1.1.0",
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
                    CopyNewFixture(dir, mesVersion, ngxSchematicsVersion, baseLayer, repositoryType);
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
                    errors.Length.Should().Be(0, $"Errors found in console: {errors}");
                    Directory.Exists(packageId).Should().BeTrue();
                    File.Exists($"{packageId}/cmfpackage.json").Should().BeTrue();
                    TestUtilities.GetPackageProperty("packageId", $"{packageId}/cmfpackage.json").Should().Be(packageId);
                    TestUtilities.GetPackageProperty("version", $"{packageId}/cmfpackage.json").Should().Be(pkgVersion);

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

        private void CopyNewFixture(string dir,
            string mesVersion = "8.2.0",
            string ngxSchematicsVersion = "1.1.0",
            BaseLayer baseLayer = BaseLayer.MES,
            RepositoryType repositoryType = RepositoryType.Customization)
        {
            TestUtilities.CopyFixture("new", new DirectoryInfo(dir));
            var projCfg = Path.Join(dir, ".project-config.json");
            if (File.Exists(projCfg))
            {
                File.WriteAllText(projCfg, File.ReadAllText(projCfg)
                    .Replace(@"""MESVersion"": ""8.2.0""", $@"""MESVersion"": ""{mesVersion}""")
                    .Replace(@"""BaseLayer"": ""MES""", $@"""BaseLayer"": ""{baseLayer}""")
                    .Replace(@"""RepositoryType"": ""Customization""", $@"""RepositoryType"": ""{repositoryType}""")
                    .Replace(@"""NGXSchematicsVersion"": ""10.0.0""", $@"""NGXSchematicsVersion"": ""{ngxSchematicsVersion}""")
                    .Replace(@"""NPMRegistry"": ""http://npm_registry/""", $@"""NPMRegistry"": ""http://cmf-nuget:4873/""")
                    .Replace("install_path", MockUnixSupport.Path(@"x:\install_path").Replace(@"\", @"\\"))
                    .Replace("backup_share", MockUnixSupport.Path(@"y:\backup_share").Replace(@"\", @"\\"))
                    .Replace("temp_folder", MockUnixSupport.Path(@"z:\temp_folder").Replace(@"\", @"\\"))
                );
            }
        }
    }
}
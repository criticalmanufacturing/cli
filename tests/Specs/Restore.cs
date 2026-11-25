using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using tests.Objects;
using Xunit;
using Assert = tests.AssertWithMessage;
using Cmf.CLI.Handlers;

namespace tests.Specs
{
    public class Restore
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task LoadDependencies_RegistryTimeout_TriesLocalSourceBeforeReportingError(bool localFound)
        {
            var dependencyId = $"Cmf.TimeoutFallback.{Guid.NewGuid():N}";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["/source/cmfpackage.json"] = new MockFileData($$"""
                    {"packageId":"Cmf.TimeoutFallback.Root","version":"1.0.0","dependencies":[{"id":"{{dependencyId}}","version":"1.0.0"}]}
                    """),
                ["/source/dependency/cmfpackage.json"] = new MockFileData($$"""
                    {"packageId":"{{dependencyId}}","version":"1.0.0"}
                    """)
            });
            var root = CmfPackageController.FromSourceManifest(fileSystem.FileInfo.New("/source/cmfpackage.json"));
            var localPackage = CmfPackageController.FromSourceManifest(fileSystem.FileInfo.New("/source/dependency/cmfpackage.json"));
            var timeout = new CliException($"Download of package {dependencyId}@1.0.0 timed out after 1 seconds.",
                new OperationCanceledException());
            var localClient = new Mock<IRepositoryClient>();
            localClient.Setup(client => client.Find(dependencyId, "1.0.0"))
                .ReturnsAsync(localFound ? localPackage : null);
            var locator = new Mock<IRepositoryLocator>();
            locator.Setup(client => client.FindPackage(dependencyId, "1.0.0")).ThrowsAsync(timeout);
            locator.Setup(client => client.GetSourceClient(fileSystem)).Returns(localClient.Object);
            Cmf.CLI.Core.Objects.ExecutionContext.ServiceProvider = new ServiceCollection()
                .AddSingleton(locator.Object)
                .BuildServiceProvider();

            Func<Task> load = () => new CmfPackageController(root, fileSystem).LoadDependencies(null, null);

            if (localFound)
            {
                await load.Should().NotThrowAsync();
                root.Dependencies.Single().CmfPackageV1.Should().BeSameAs(localPackage);
            }
            else
            {
                var exception = await load.Should().ThrowAsync<CliException>();
                exception.Which.Should().BeSameAs(timeout);
            }
            localClient.Verify(client => client.Find(dependencyId, "1.0.0"), Times.Once);
        }

        [Fact]
        public void RestoreDependencies()
        {
            var gitRepo = MockUnixSupport.Path(@"c:\test");
            var ciRepo = MockUnixSupport.Path(@"c:\cirepo");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {
                    $"{gitRepo}/cmfpackage.json", new MockFileData(
                        @"{
                  ""packageId"": ""Cmf.Custom.Package"",
                  ""version"": ""1.1.0"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {
                         ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                        ""version"": ""8.3.0""
                    },
                    {
                         ""id"": ""Cmf.Custom.Business"",
                        ""version"": ""1.1.0""
                    },
                    {
                        ""id"": ""Cmf.Custom.HTML"",
                        ""version"": ""1.1.0""
                    }
                  ]
                }")
                },
                // this should not be used for this scenario
                {
                    $"{ciRepo}/Cmf.Custom.Package.1.1.0.zip", new MockFileData(new DFPackageBuilder()
                        .CreateEntry("manifest.xml",
                            @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <deploymentPackage>
                          <packageId>Cmf.Custom.Package</packageId>
                          <version>1.1.0</version>
                          <dependencies>
                            <dependency id=""Cmf.Custom.Business"" version=""1.1.0"" mandatory=""true"" isMissing=""true"" />
                            <dependency id=""Cmf.Custom.Html"" version=""1.1.0"" mandatory=""true"" isMissing=""true"" />
                          </dependencies>
                        </deploymentPackage>").ToByteArray())
                },
                {
                    $"{ciRepo}/Cmf.Custom.Business.1.1.0.zip", new MockFileData(new DFPackageBuilder()
                        .CreateEntry("manifest.xml",
                            @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <deploymentPackage>
                          <packageId>Cmf.Custom.Business</packageId>
                          <version>1.1.0</version>
                        </deploymentPackage>")
                        .CreateEntry("content.txt", "business")
                        .ToByteArray())
                },
                {
                    $"{ciRepo}/Cmf.Custom.HTML.1.1.0.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml",
                        @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <deploymentPackage>
                          <packageId>Cmf.Custom.HTML</packageId>
                          <version>1.1.0</version>
                        </deploymentPackage>")
                        .CreateEntry("content.txt", "HTML")
                        .ToByteArray())
                }
            });

            ExecutionContext.Initialize(fileSystem);
            fileSystem.Directory.SetCurrentDirectory(MockUnixSupport.Path(@"c:\test"));

            CmfPackage cmfpackageFile = CmfPackage.Load(fileSystem.FileInfo.New("/test/cmfpackage.json"), setDefaultValues: true, fileSystem);
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);

            var repo = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = ciRepo }.Uri;

            Assert.False(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\Dependencies")).Exists, "Dependencies folder already exists!");

            packageTypeHandler.RestoreDependencies(new[] { repo });

            Assert.True(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\test\\Dependencies")).Exists, "Dependencies folder not found");
            Assert.True(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.HTML@1.1.0")).Exists, "HTML Dependency folder not found");
            Assert.True(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.Business@1.1.0")).Exists, "Business Dependency folder not found");
            Assert.True(fileSystem.FileInfo.New(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.Business@1.1.0\\content.txt")).Exists, "Business Dependency content not found");
            Assert.True(fileSystem.FileInfo.New(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.HTML@1.1.0\\content.txt")).Exists, "HTML Dependency content not found");
            Assert.Equal("business", fileSystem.FileInfo.New(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.Business@1.1.0\\content.txt")).OpenText().ReadToEnd(), "Business Dependency content does not match");
            Assert.Equal("HTML", fileSystem.FileInfo.New(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.HTML@1.1.0\\content.txt")).OpenText().ReadToEnd(), "HTML Dependency content does not match");
        }

        [Fact]
        public void RestoreDependencies_RepoNotFound()
        {
            var gitRepo = MockUnixSupport.Path(@"c:\test");
            var ciRepo = MockUnixSupport.Path(@"c:\cirepo");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {
                    $"{gitRepo}/.project-config.json", new MockFileData(
                        @"{
                    }")
                },
                {
                    $"{gitRepo}/cmfpackage.json", new MockFileData(
                        @"{
                  ""packageId"": ""Cmf.Custom.Package"",
                  ""version"": ""1.1.0"",
                  ""description"": ""This package deploys Critical Manufacturing Customization"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {
                         ""id"": ""CriticalManufacturing.DeploymentMetadata"",
                        ""version"": ""8.3.0""
                    },
                    {
                         ""id"": ""Cmf.Custom.Business"",
                        ""version"": ""1.1.0""
                    },
                    {
                        ""id"": ""Cmf.Custom.HTML"",
                        ""version"": ""1.1.0""
                    }
                  ]
                }")
                },
                // this should not be used for this scenario
                {
                    $"{ciRepo}/Cmf.Custom.Package.1.1.0.zip", new MockFileData(new DFPackageBuilder()
                        .CreateEntry("manifest.xml",
                            @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <deploymentPackage>
                          <packageId>Cmf.Custom.Package</packageId>
                          <version>1.1.0</version>
                          <dependencies>
                            <dependency id=""Cmf.Custom.Business"" version=""1.1.0"" mandatory=""true"" isMissing=""true"" />
                            <dependency id=""Cmf.Custom.Html"" version=""1.1.0"" mandatory=""true"" isMissing=""true"" />
                          </dependencies>
                        </deploymentPackage>").ToByteArray())
                },
                {
                    $"{ciRepo}/Cmf.Custom.Business.1.1.0.zip", new MockFileData(new DFPackageBuilder()
                        .CreateEntry("manifest.xml",
                            @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <deploymentPackage>
                          <packageId>Cmf.Custom.Business</packageId>
                          <version>1.1.0</version>
                        </deploymentPackage>")
                        .CreateEntry("content.txt", "business")
                        .ToByteArray())
                },
                {
                    $"{ciRepo}/Cmf.Custom.HTML.1.1.0.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml",
                        @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <deploymentPackage>
                          <packageId>Cmf.Custom.HTML</packageId>
                          <version>1.1.0</version>
                        </deploymentPackage>")
                        .CreateEntry("content.txt", "HTML")
                        .ToByteArray())
                }
            });

            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<IRepositoryLocator, RepositoryLocator>()
                .BuildServiceProvider();
            ExecutionContext.Initialize(fileSystem);
            fileSystem.Directory.SetCurrentDirectory(MockUnixSupport.Path(@"c:\test"));

            CmfPackage cmfpackageFile = CmfPackage.Load(fileSystem.FileInfo.New("/test/cmfpackage.json"), setDefaultValues: true, fileSystem);
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);

            var repo = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = MockUnixSupport.Path(@"c:\missingRepo") }.Uri;

            Assert.False(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\Dependencies")).Exists, "Dependencies folder already exists!");

            var logWriter = (new Logging()).GetLogStringWriter();
            
            var exception = Record.Exception(() => packageTypeHandler.RestoreDependencies(new[] { repo }));
            exception.Should().BeNull();
            
            logWriter.ToString().Should().Contain("No present remote dependencies to restore. Exiting...");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RestoreDependencies_HelpMkDocs(bool useRepositoryClients)
        {
            var previousServiceProvider = ExecutionContext.ServiceProvider;
            try
            {
                RestoreDependencies_HelpMkDocsInternal(useRepositoryClients);
            }
            finally
            {
                ExecutionContext.ServiceProvider = previousServiceProvider;
            }
        }

        private void RestoreDependencies_HelpMkDocsInternal(bool useRepositoryClients)
        {
            var gitRepo = MockUnixSupport.Path(@"c:\test");
            var ciRepo = MockUnixSupport.Path(@"c:\cirepo");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { MockUnixSupport.Path($@"{gitRepo}\.project-config.json"), new MockFileData(@"{
                    ""ProjectName"": ""localtestrj"",
                    ""RepositoryType"": ""Customization"",
                    ""BaseLayer"": ""MES"",
                    ""Tenant"": ""MyTenant"",
                    ""MESVersion"": ""12.0.0""
                }") },
                { $"{gitRepo}/cmfpackage.json", new MockFileData(@"{
                    ""packageId"": ""Cmf.Custom.Help2"",
                    ""version"": ""1.0.0"",
                    ""description"": ""Cmf Custom cmf Cmf.Custom.Help Package"",
                    ""packageType"": ""Help"",
                    ""targetLayer"": ""reference"",
                    ""isInstallable"": true,
                    ""isUniqueInstall"": false,
                    ""contentToPack"": [
                        { ""source"": ""site/**"", ""target"": ""site"" },
                        { ""source"": ""docs/MyProject/**"", ""target"": ""docs/MyProject"" }
                    ],
                    ""dependencies"": [
                        { ""id"": ""Cmf.Custom.Help"", ""version"": ""1.0.0"" }
                    ]
                }") },
                { $"{gitRepo}/requirements.txt", new MockFileData("mkdocs") },
                { $"{gitRepo}/docs/other.txt", new MockFileData("original other") },
                { $"{gitRepo}/docs/LocalProject/index.md", new MockFileData("# Local project documentation") },
                { $"{gitRepo}/docs/MyProject/old-file.txt", new MockFileData("old content") },
                { $"{gitRepo}/docs/MyProject/index.html", new MockFileData("<html>old index</html>") },
                { $"{ciRepo}/Cmf.Custom.Help.1.0.0.zip", new MockFileData(new DFPackageBuilder()
                    .CreateEntry("manifest.xml", @"<?xml version=""1.0"" encoding=""utf-8""?><deploymentPackage><packageId>Cmf.Custom.Help</packageId><version>1.0.0</version></deploymentPackage>")
                    .CreateEntry("index.html", "<html>generated website</html>")
                    .CreateEntry("assets/theme.css", "body {}")
                    .CreateEntry("search/search_index.json", "{}")
                    .CreateEntry("docs/MyProject/index.html", "<html>index</html>")
                    .CreateEntry("docs/MyProject/getting-started.html", "<html>getting started</html>")
                    .CreateEntry("docs/MyProject/reference.md", "# Reference")
                    .CreateEntry("docs/Features/OtherFeature/index.md", "# Feature documentation")
                    .CreateEntry("docs/Features/OtherFeature/assets/diagram.svg", "<svg/>")
                    .CreateEntry("docs/other.txt", "restored other")
                    .CreateEntry("docs/assets/icon.png", "icon data")
                    .ToByteArray()) }
            });

            ExecutionContext.ServiceProvider = new ServiceCollection()
                .AddSingleton<IProjectConfigService>(new ProjectConfigService())
                .AddSingleton(Mock.Of<IFeaturesService>(features => features.UseRepositoryClients == useRepositoryClients))
                .AddSingleton<IRepositoryLocator, RepositoryLocator>()
                .BuildServiceProvider();

            fileSystem.Directory.SetCurrentDirectory(gitRepo);
            ExecutionContext.Initialize(fileSystem);

            var package = CmfPackage.Load(fileSystem.FileInfo.New($"{gitRepo}/cmfpackage.json"), true, fileSystem);
            var handler = PackageTypeFactory.GetPackageTypeHandler(package, false);
            handler.Should().BeOfType<HelpMkDocsPackageTypeHandler>();

            var repositoryPath = useRepositoryClients ? $"{ciRepo}/Cmf.Custom.Help.1.0.0.zip" : ciRepo;
            var repo = new UriBuilder { Scheme = Uri.UriSchemeFile, Host = "", Path = repositoryPath }.Uri;
            if (useRepositoryClients)
            {
                ExecutionContext.ServiceProvider.GetRequiredService<IRepositoryLocator>()
                    .InitializeClientsForRepositories(fileSystem, new[] { repo });
            }

            handler.RestoreDependencies(new[] { repo });

            var docs = MockUnixSupport.Path($@"{gitRepo}\docs\MyProject");

            // Verify that the restore replaced all docs tenant files and did not remove any other files
            Assert.False(fileSystem.FileInfo.New($"{docs}/old-file.txt").Exists, "Old file was not replaced");
            Assert.True(fileSystem.FileInfo.New($"{docs}/.gitignore").Exists, ".gitignore not found");
            Assert.Equal("**", fileSystem.FileInfo.New($"{docs}/.gitignore").OpenText().ReadToEnd().Trim());
            Assert.Equal("<html>index</html>", fileSystem.FileInfo.New($"{docs}/index.html").OpenText().ReadToEnd());
            Assert.Equal("<html>getting started</html>", fileSystem.FileInfo.New($"{docs}/getting-started.html").OpenText().ReadToEnd());
            Assert.Equal("# Reference", fileSystem.FileInfo.New($"{docs}/reference.md").OpenText().ReadToEnd());

            Assert.Equal("# Feature documentation", fileSystem.File.ReadAllText($"{gitRepo}/docs/Features/OtherFeature/index.md"));
            Assert.Equal("<svg/>", fileSystem.File.ReadAllText($"{gitRepo}/docs/Features/OtherFeature/assets/diagram.svg"));
            Assert.Equal("# Local project documentation", fileSystem.File.ReadAllText($"{gitRepo}/docs/LocalProject/index.md"));

            // Repeated restores refresh the dependency tree while preserving local project sources.
            fileSystem.File.WriteAllText($"{docs}/obsolete.md", "stale dependency page");
            handler.RestoreDependencies(new[] { repo });
            Assert.False(fileSystem.File.Exists($"{docs}/obsolete.md"), "Stale dependency pages should be removed");
            Assert.Equal("# Reference", fileSystem.File.ReadAllText($"{docs}/reference.md"));
            Assert.Equal("# Local project documentation", fileSystem.File.ReadAllText($"{gitRepo}/docs/LocalProject/index.md"));
            Assert.Equal("# Feature documentation", fileSystem.File.ReadAllText($"{gitRepo}/docs/Features/OtherFeature/index.md"));

            // Verify files outside docs/tenant were not touched or restored
            Assert.True(fileSystem.FileInfo.New(MockUnixSupport.Path($@"{gitRepo}\docs\other.txt")).Exists, "File outside docs/tenant was removed");
            Assert.Equal("original other", fileSystem.FileInfo.New(MockUnixSupport.Path($@"{gitRepo}\docs\other.txt")).OpenText().ReadToEnd());
            Assert.False(fileSystem.FileInfo.New(MockUnixSupport.Path($@"{gitRepo}\docs\assets\icon.png")).Exists, "Assets folder should not be restored");
            Assert.False(fileSystem.File.Exists($"{gitRepo}/docs/index.html"), "Generated home page should not be restored as a source");
            Assert.False(fileSystem.Directory.Exists($"{gitRepo}/docs/search"), "Generated search files should not be restored");
            Assert.False(fileSystem.File.Exists($"{gitRepo}/docs/assets/theme.css"), "Generated assets should not be restored");
        }
    }
}

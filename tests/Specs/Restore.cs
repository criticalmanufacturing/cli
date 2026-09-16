using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using tests.Objects;
using Xunit;
using Assert = tests.AssertWithMessage;
using Cmf.CLI.Handlers;

namespace tests.Specs
{
    public class Restore
    {
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

        [Fact]
        public void RestoreDependencies_HelpMkDocs()
        {
            var gitRepo = MockUnixSupport.Path(@"c:\test");
            var ciRepo = MockUnixSupport.Path(@"c:\cirepo");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { MockUnixSupport.Path(@"c:\.project-config.json"), new MockFileData(@"{
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
                        { ""source"": ""docs/MyTenant/**"", ""target"": ""docs/MyTenant"" }
                    ],
                    ""dependencies"": [
                        { ""id"": ""Cmf.Custom.Help"", ""version"": ""1.0.0"" }
                    ]
                }") },
                { $"{gitRepo}/requirements.txt", new MockFileData("mkdocs") },
                { $"{gitRepo}/docs/other.txt", new MockFileData("original other") },
                { $"{gitRepo}/docs/MyTenant/old-file.txt", new MockFileData("old content") },
                { $"{gitRepo}/docs/MyTenant/index.html", new MockFileData("<html>old index</html>") },
                { $"{ciRepo}/Cmf.Custom.Help.1.0.0.zip", new MockFileData(new DFPackageBuilder()
                    .CreateEntry("manifest.xml", @"<?xml version=""1.0"" encoding=""utf-8""?><deploymentPackage><packageId>Cmf.Custom.Help</packageId><version>1.0.0</version></deploymentPackage>")
                    .CreateEntry("docs/MyTenant/index.html", "<html>index</html>")
                    .CreateEntry("docs/MyTenant/getting-started.html", "<html>getting started</html>")
                    .CreateEntry("docs/MyTenant/reference.md", "# Reference")
                    .CreateEntry("docs/other.txt", "restored other")
                    .CreateEntry("docs/assets/icon.png", "icon data")
                    .ToByteArray()) }
            });

            ExecutionContext.ServiceProvider = new ServiceCollection()
                .AddSingleton<IProjectConfigService>(new ProjectConfigService())
                .AddSingleton<IRepositoryLocator, RepositoryLocator>()
                .BuildServiceProvider();

            fileSystem.Directory.SetCurrentDirectory(gitRepo);
            ExecutionContext.Initialize(fileSystem);

            var package = CmfPackage.Load(fileSystem.FileInfo.New($"{gitRepo}/cmfpackage.json"), true, fileSystem);
            var handler = PackageTypeFactory.GetPackageTypeHandler(package, false);
            handler.Should().BeOfType<HelpMkDocsPackageTypeHandler>();

            var repo = new UriBuilder { Scheme = Uri.UriSchemeFile, Host = "", Path = ciRepo }.Uri;

            handler.RestoreDependencies(new[] { repo });

            var docs = MockUnixSupport.Path(@"c:\test\docs\MyTenant");

            // Verify that the restore replaced all docs tenant files and did not remove any other files
            Assert.False(fileSystem.FileInfo.New($"{docs}/old-file.txt").Exists, "Old file was not replaced");
            Assert.True(fileSystem.FileInfo.New($"{docs}/.gitignore").Exists, ".gitignore not found");
            Assert.Equal("**", fileSystem.FileInfo.New($"{docs}/.gitignore").OpenText().ReadToEnd().Trim());
            Assert.Equal("<html>index</html>", fileSystem.FileInfo.New($"{docs}/index.html").OpenText().ReadToEnd());
            Assert.Equal("<html>getting started</html>", fileSystem.FileInfo.New($"{docs}/getting-started.html").OpenText().ReadToEnd());
            Assert.Equal("# Reference", fileSystem.FileInfo.New($"{docs}/reference.md").OpenText().ReadToEnd());

            // Verify files outside docs/tenant were not touched or restored
            Assert.True(fileSystem.FileInfo.New(MockUnixSupport.Path(@"c:\test\docs\other.txt")).Exists, "File outside docs/tenant was removed");
            Assert.Equal("original other", fileSystem.FileInfo.New(MockUnixSupport.Path(@"c:\test\docs\other.txt")).OpenText().ReadToEnd());
            Assert.False(fileSystem.FileInfo.New(MockUnixSupport.Path(@"c:\test\docs\assets\icon.png")).Exists, "Assets folder should not be restored");
        }
    }
}
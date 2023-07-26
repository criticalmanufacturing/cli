using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using tests.Objects;
using Xunit;
using Assert = tests.AssertWithMessage;

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

            var repo = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = MockUnixSupport.Path(@"c:\missingRepo") }.Uri;

            Assert.False(fileSystem.DirectoryInfo.New(MockUnixSupport.Path("c:\\Dependencies")).Exists, "Dependencies folder already exists!");

            Assert.Throws<CliException>(() => packageTypeHandler.RestoreDependencies(new[] { repo }));
        }
    }
}
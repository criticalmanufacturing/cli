using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Cmf.Common.Cli.Factories;
using Cmf.Common.Cli.Interfaces;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using tests.Objects;

namespace tests.Specs
{
    [TestClass]
    public class Restore
    {
        [TestMethod]
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
            
            CmfPackage cmfpackageFile = CmfPackage.Load(fileSystem.FileInfo.FromFileName("/test/cmfpackage.json"), setDefaultValues: true, fileSystem);
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);

            var repo = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = ciRepo }.Uri;
            
            Assert.IsFalse(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("c:\\Dependencies")).Exists, "Dependencies folder already exists!");
            
            packageTypeHandler.RestoreDependencies(new []{ repo });
            
            Assert.IsTrue(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("c:\\test\\Dependencies")).Exists, "Dependencies folder not found");
            Assert.IsTrue(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.HTML@1.1.0")).Exists, "HTML Dependency folder not found");
            Assert.IsTrue(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.Business@1.1.0")).Exists, "Business Dependency folder not found");
            Assert.IsTrue(fileSystem.FileInfo.FromFileName(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.Business@1.1.0\\content.txt")).Exists, "Business Dependency content not found");
            Assert.IsTrue(fileSystem.FileInfo.FromFileName(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.HTML@1.1.0\\content.txt")).Exists, "HTML Dependency content not found");
            Assert.AreEqual("business", fileSystem.FileInfo.FromFileName(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.Business@1.1.0\\content.txt")).OpenText().ReadToEnd(), "Business Dependency content does not match");
            Assert.AreEqual("HTML", fileSystem.FileInfo.FromFileName(MockUnixSupport.Path("c:\\test\\Dependencies\\Cmf.Custom.HTML@1.1.0\\content.txt")).OpenText().ReadToEnd(), "HTML Dependency content does not match");
        }
        
        [TestMethod]
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
            
            CmfPackage cmfpackageFile = CmfPackage.Load(fileSystem.FileInfo.FromFileName("/test/cmfpackage.json"), setDefaultValues: true, fileSystem);
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile, setDefaultValues: false);

            var repo = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = MockUnixSupport.Path(@"c:\missingRepo")}.Uri;
            
            Assert.IsFalse(fileSystem.DirectoryInfo.FromDirectoryName(MockUnixSupport.Path("c:\\Dependencies")).Exists, "Dependencies folder already exists!");

            try
            {
                packageTypeHandler.RestoreDependencies(new[] { repo });
            }
            catch (CliException)
            {
                return;
            }

            Assert.Fail("Should have thrown an error");
        }
    }
}
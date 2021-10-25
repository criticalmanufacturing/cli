using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using Cmf.Common.Cli.Objects;

namespace tests
{
    [TestClass]
    public class ListDependencies
    {
        [TestMethod]
        public void LocalDependencies_HappyPath()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/test/cmfpackage.json", new MockFileData(
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
    },
    {
        ""id"": ""CriticalManufacturing"",
        ""version"": ""8.1.1""
    }
  ]
}") },
                { "/test/UI/html/cmfpackage.json", new MockFileData(
@"{
  ""packageId"": ""Cmf.Custom.HTML"",
  ""version"": ""1.1.0"",
  ""description"": ""Cmf Custom HTML Package"",
  ""packageType"": ""Html"",
  ""isInstallable"": true,
  ""isUniqueInstall"": false,
  ""contentToPack"": [
    {
      ""source"": ""src/packages/*"",
      ""target"": ""node_modules"",
      ""ignoreFiles"": [
        "".npmignore""
      ]
    }
  ],
  ""xmlInjection"": [""../../DeploymentMetadata/ui.xml""]
}") },
                { "/test/Business/cmfpackage.json", new MockFileData(
@"{
  ""packageId"": ""Cmf.Custom.Business"",
  ""version"": ""1.1.0"",
  ""description"": ""Cmf Custom Business Package"",
  ""packageType"": ""Business"",
  ""isInstallable"": true,
  ""isUniqueInstall"": false,
  ""contentToPack"": [
    {
      ""source"": ""Release/*.dll"",
      ""target"": """"
    }
  ],
  ""xmlInjection"": [""../DeploymentMetadata/ui.xml""]
}") }
            });


            //var ls = new ListDependenciesCommand(fileSystem);
            //ls.Execute(fileSystem.DirectoryInfo.FromDirectoryName(@"c:\test"), null);

            CmfPackage cmfPackage = CmfPackage.Load(fileSystem.FileInfo.FromFileName("/test/cmfpackage.json"), setDefaultValues: true);
            var loadedPackage = cmfPackage.LoadDependencies(null, true);
            Assert.AreEqual("Cmf.Custom.Package", loadedPackage.PackageId, "Root package name doesn't match expected");
            Assert.AreEqual(3, loadedPackage.Dependencies.Count, "Root package doesn't have expected dependencies");

            var busPackage = loadedPackage.Dependencies[0];
            Assert.IsFalse(busPackage.IsMissing, "Business package is missing");
            Assert.IsNotNull(busPackage.CmfPackage, "Business package couldn't be loaded");
            Assert.AreEqual("Cmf.Custom.Business", busPackage.CmfPackage.PackageId, "Business package name doesn't match expected");
            Assert.IsNull(busPackage.CmfPackage.Dependencies, "Business package has unexpected dependencies");

            var htmlPackage = loadedPackage.Dependencies[1];
            Assert.IsFalse(htmlPackage.IsMissing, "HTML package is missing");
            Assert.IsNotNull(htmlPackage.CmfPackage, "HTML package couldn't be loaded");
            Assert.AreEqual("Cmf.Custom.HTML", htmlPackage.CmfPackage.PackageId, "HTML package name doesn't match expected");
            Assert.IsNull(htmlPackage.CmfPackage.Dependencies, "HTML package has unexpected dependencies");

            var productPackage = loadedPackage.Dependencies[2];
            Assert.IsTrue(productPackage.IsMissing, "Product package isn't missing");
            Assert.IsNull(productPackage.CmfPackage, "Product package could be loaded");
        }
    }
}

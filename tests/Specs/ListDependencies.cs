using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using tests.Objects;

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
        ""id"": ""CriticalManufacturing.DeploymentMetadata"",
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
  ]
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
  ]
}") }
            });


            //var ls = new ListDependenciesCommand(fileSystem);
            //ls.Execute(fileSystem.DirectoryInfo.FromDirectoryName(@"c:\test"), null);

            CmfPackage cmfPackage = CmfPackage.Load(fileSystem.FileInfo.FromFileName("/test/cmfpackage.json"), setDefaultValues: true, fileSystem);
            cmfPackage.LoadDependencies(null, true);
            Assert.AreEqual("Cmf.Custom.Package", cmfPackage.PackageId, "Root package name doesn't match expected");
            Assert.AreEqual(3, cmfPackage.Dependencies.Count, "Root package doesn't have expected dependencies");

            var busPackage = cmfPackage.Dependencies[0];
            Assert.IsFalse(busPackage.IsMissing, "Business package is missing");
            Assert.IsNotNull(busPackage.CmfPackage, "Business package couldn't be loaded");
            Assert.AreEqual("Cmf.Custom.Business", busPackage.CmfPackage.PackageId, "Business package name doesn't match expected");
            Assert.IsNull(busPackage.CmfPackage.Dependencies, "Business package has unexpected dependencies");

            var htmlPackage = cmfPackage.Dependencies[1];
            Assert.IsFalse(htmlPackage.IsMissing, "HTML package is missing");
            Assert.IsNotNull(htmlPackage.CmfPackage, "HTML package couldn't be loaded");
            Assert.AreEqual("Cmf.Custom.HTML", htmlPackage.CmfPackage.PackageId, "HTML package name doesn't match expected");
            Assert.IsNull(htmlPackage.CmfPackage.Dependencies, "HTML package has unexpected dependencies");

            var productPackage = cmfPackage.Dependencies[2];
            Assert.IsTrue(productPackage.IsMissing, "Product package isn't missing");
            Assert.IsNull(productPackage.CmfPackage, "Product package could be loaded");
        }
        
        [TestMethod]
        public void LocalDependencies_RepositoryPackages()
        {
            var repoUri = OperatingSystem.IsWindows() ? new Uri("\\\\share\\dir") : new Uri("file:///repoDir");
            var repo = OperatingSystem.IsWindows() ? "\\\\share\\dir" : "/repoDir";
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
        ""id"": ""CriticalManufacturing.DeploymentMetadata"",
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
  ]
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
  ]
}") },
                { $"{repo}/CriticalManufacturing.DeploymentMetadata.8.1.1.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml",
                  @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <deploymentPackage>
                          <packageId>CriticalManufacturing</packageId>
                          <version>8.1.1</version>
                          <dependencies>
                            <dependency id=""Inner.Package"" version=""0.0.1"" mandatory=""true"" isMissing=""true"" />
                          </dependencies>
                        </deploymentPackage>").ToByteArray()) }
            });
            
            CmfPackage cmfPackage = CmfPackage.Load(fileSystem.FileInfo.FromFileName("/test/cmfpackage.json"), setDefaultValues: true, fileSystem);
            ExecutionContext.Initialize(fileSystem);
            cmfPackage.LoadDependencies(new []{ repoUri }, true);
            Assert.AreEqual("Cmf.Custom.Package", cmfPackage.PackageId, "Root package name doesn't match expected");
            Assert.AreEqual(3, cmfPackage.Dependencies.Count, "Root package doesn't have expected dependencies");

            var busPackage = cmfPackage.Dependencies[0];
            Assert.IsFalse(busPackage.IsMissing, "Business package is missing");
            Assert.IsNotNull(busPackage.CmfPackage, "Business package couldn't be loaded");
            Assert.AreEqual("Cmf.Custom.Business", busPackage.CmfPackage.PackageId, "Business package name doesn't match expected");
            Assert.IsNull(busPackage.CmfPackage.Dependencies, "Business package has unexpected dependencies");

            var htmlPackage = cmfPackage.Dependencies[1];
            Assert.IsFalse(htmlPackage.IsMissing, "HTML package is missing");
            Assert.IsNotNull(htmlPackage.CmfPackage, "HTML package couldn't be loaded");
            Assert.AreEqual("Cmf.Custom.HTML", htmlPackage.CmfPackage.PackageId, "HTML package name doesn't match expected");
            Assert.IsNull(htmlPackage.CmfPackage.Dependencies, "HTML package has unexpected dependencies");

            var productPackage = cmfPackage.Dependencies[2];
            Assert.IsFalse(productPackage.IsMissing, "Product package is missing");
            Assert.IsNotNull(productPackage.CmfPackage, "Product package couldn't be loaded");
            Assert.AreEqual(productPackage.CmfPackage.Location, PackageLocation.Repository,
              "Product package is not located in Repository");
            Assert.AreEqual("CriticalManufacturing", productPackage.CmfPackage.PackageId, "Product package name doesn't match expected");
            Assert.AreEqual(1, productPackage.CmfPackage.Dependencies.Count, "Product package doesn't have expected dependencies");
            
            var productInnerPackage = productPackage.CmfPackage.Dependencies[0];
            Assert.IsTrue(productInnerPackage.IsMissing, "Product Inner package isn't missing");
            Assert.IsNull(productInnerPackage.CmfPackage, "Product Inner package could be loaded");
            
        }
        
        [TestMethod]
        public void Args_MultiRepo()
        {
          string _workingDir = null;
          string[] _repos = null;
          
          var listDependenciesCommand = new ListDependenciesCommand();
          var cmd = new Command("ls");
          listDependenciesCommand.Configure(cmd);

          cmd.Handler = CommandHandler.Create<IDirectoryInfo, Uri[]>(
            (workingDir, repos) =>
            {
              _workingDir = workingDir.Name;
              _repos = repos?.Select(uri => uri.OriginalString).ToArray();
            });

          var console = new TestConsole();
          cmd.Invoke(new[] {
            "-r", "d:\\xpto", "e:\\packages"
          }, console);
          
          var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
          Assert.AreEqual(curDir.Name, _workingDir, "working dir does not match expected");
          Assert.AreEqual(2, _repos.Length, "Expecting 2 repositories");
          Assert.AreEqual("d:\\xpto", _repos[0], "Wrong repository location");
          Assert.AreEqual("e:\\packages", _repos[1], "Wrong repository location");
        }
        
        [TestMethod]
        public void Args_MultiRepo_UrlLocalMix()
        {
          string _workingDir = null;
          Uri[] _repos = null;
          
          var listDependenciesCommand = new ListDependenciesCommand();
          var cmd = new Command("ls");
          listDependenciesCommand.Configure(cmd);

          cmd.Handler = CommandHandler.Create<IDirectoryInfo, Uri[]>(
            (workingDir, repos) =>
            {
              _workingDir = workingDir.Name;
              _repos = repos;
            });

          var console = new TestConsole();
          cmd.Invoke(new[] {
            "-r", "d:\\xpto", "http://repository.example"
          }, console);
          
          var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
          Assert.AreEqual(curDir.Name, _workingDir, "working dir does not match expected");
          Assert.AreEqual(2, _repos.Length, "Expecting 2 repositories");
          Assert.AreEqual("file:///d:/xpto", _repos[0].AbsoluteUri, "Wrong repository location");
          Assert.AreEqual("http://repository.example/", _repos[1].AbsoluteUri, "Wrong repository location");
          Assert.IsTrue(_repos[0].IsDirectory(), "First repo should be a directory");
          Assert.IsFalse(_repos[1].IsDirectory(), "Second repo not should be a directory");
        }
        
        [TestMethod]
        public void Args_MultiRepo_RelativeDirectory()
        {
          string _workingDir = null;
          Uri[] _repos = null;
          
          var listDependenciesCommand = new ListDependenciesCommand();
          var cmd = new Command("ls");
          listDependenciesCommand.Configure(cmd);

          cmd.Handler = CommandHandler.Create<IDirectoryInfo, Uri[]>(
            (workingDir, repos) =>
            {
              _workingDir = workingDir.Name;
              _repos = repos;
            });

          var console = new TestConsole();
          cmd.Invoke(new[] {
            "-r", "..\\xpto", "\\root_dir"
          }, console);
          
          var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
          Assert.AreEqual(curDir.Name, _workingDir, "working dir does not match expected");
          Assert.AreEqual(2, _repos.Length, "Expecting 2 repositories");
          Assert.AreEqual("..\\xpto", _repos[0].OriginalString, "Wrong repository location");
          Assert.AreEqual("\\root_dir", _repos[1].OriginalString, "Wrong repository location");
          // TODO: use mock filesystem to resolve relative urls
        }
        
        [TestMethod]
        public void Args_SingleRepo()
        {
          string _workingDir = null;
          string[] _repos = null;
          
          var listDependenciesCommand = new ListDependenciesCommand();
          var cmd = new Command("ls");
          listDependenciesCommand.Configure(cmd);

          cmd.Handler = CommandHandler.Create<IDirectoryInfo, Uri[]>(
            (workingDir, repos) =>
            {
              _workingDir = workingDir.Name;
              _repos = repos?.Select(uri => uri.OriginalString).ToArray();
            });

          var console = new TestConsole();
          cmd.Invoke(new[] {
            "-r", "d:\\xpto"
          }, console);
          
          var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
          Assert.AreEqual(curDir.Name, _workingDir, "working dir does not match expected");
          Assert.AreEqual(1, _repos.Length, "Expecting 2 repositories");
          Assert.AreEqual("d:\\xpto", _repos[0], "Wrong repository location");
        }
    }
    
    
}

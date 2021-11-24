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
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;

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

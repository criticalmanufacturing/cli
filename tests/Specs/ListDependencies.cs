using Cmf.CLI.Commands;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using tests.Objects;
using Xunit;
using Assert = tests.AssertWithMessage;

namespace tests.Specs
{
    public class ListDependencies
    {
        [Fact]
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
            //ls.Execute(fileSystem.DirectoryInfo.New(@"c:\test"), null);
            ExecutionContext.Initialize(fileSystem);
            CmfPackage cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/test/cmfpackage.json"), setDefaultValues: true, fileSystem);
            cmfPackage.LoadDependencies(null, null, true);
            Assert.Equal("Cmf.Custom.Package", cmfPackage.PackageId, "Root package name doesn't match expected");
            Assert.Equal(3, cmfPackage.Dependencies.Count, "Root package doesn't have expected dependencies");

            var busPackage = cmfPackage.Dependencies[0];
            Assert.False(busPackage.IsMissing, "Business package is missing");
            Assert.NotNull(busPackage.CmfPackage, "Business package couldn't be loaded");
            Assert.Equal("Cmf.Custom.Business", busPackage.CmfPackage.PackageId, "Business package name doesn't match expected");
            Assert.Null(busPackage.CmfPackage.Dependencies, "Business package has unexpected dependencies");

            var htmlPackage = cmfPackage.Dependencies[1];
            Assert.False(htmlPackage.IsMissing, "HTML package is missing");
            Assert.NotNull(htmlPackage.CmfPackage, "HTML package couldn't be loaded");
            Assert.Equal("Cmf.Custom.HTML", htmlPackage.CmfPackage.PackageId, "HTML package name doesn't match expected");
            Assert.Null(htmlPackage.CmfPackage.Dependencies, "HTML package has unexpected dependencies");

            var productPackage = cmfPackage.Dependencies[2];
            Assert.True(productPackage.IsMissing, "Product package isn't missing");
            Assert.Null(productPackage.CmfPackage, "Product package could be loaded");
        }

        [Fact]
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

            CmfPackage cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/test/cmfpackage.json"), setDefaultValues: true, fileSystem);
            ExecutionContext.Initialize(fileSystem);
            cmfPackage.LoadDependencies(new[] { repoUri }, null, true);
            Assert.Equal("Cmf.Custom.Package", cmfPackage.PackageId, "Root package name doesn't match expected");
            Assert.Equal(3, cmfPackage.Dependencies.Count, "Root package doesn't have expected dependencies");

            var busPackage = cmfPackage.Dependencies[0];
            Assert.False(busPackage.IsMissing, "Business package is missing");
            Assert.NotNull(busPackage.CmfPackage, "Business package couldn't be loaded");
            Assert.Equal("Cmf.Custom.Business", busPackage.CmfPackage.PackageId, "Business package name doesn't match expected");
            Assert.Null(busPackage.CmfPackage.Dependencies, "Business package has unexpected dependencies");

            var htmlPackage = cmfPackage.Dependencies[1];
            Assert.False(htmlPackage.IsMissing, "HTML package is missing");
            Assert.NotNull(htmlPackage.CmfPackage, "HTML package couldn't be loaded");
            Assert.Equal("Cmf.Custom.HTML", htmlPackage.CmfPackage.PackageId, "HTML package name doesn't match expected");
            Assert.Null(htmlPackage.CmfPackage.Dependencies, "HTML package has unexpected dependencies");

            var productPackage = cmfPackage.Dependencies[2];
            Assert.False(productPackage.IsMissing, "Product package is missing");
            Assert.NotNull(productPackage.CmfPackage, "Product package couldn't be loaded");
            Assert.Equal(productPackage.CmfPackage.Location, PackageLocation.Repository,
              "Product package is not located in Repository");
            Assert.Equal("CriticalManufacturing", productPackage.CmfPackage.PackageId, "Product package name doesn't match expected");
            Assert.Equal(1, productPackage.CmfPackage.Dependencies.Count, "Product package doesn't have expected dependencies");

            var productInnerPackage = productPackage.CmfPackage.Dependencies[0];
            Assert.True(productInnerPackage.IsMissing, "Product Inner package isn't missing");
            Assert.Null(productInnerPackage.CmfPackage, "Product Inner package could be loaded");

        }

        [Fact]
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
            "-r", "d:\\xpto", "-r", "e:\\packages"
          }, console);

            var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            Assert.Equal(curDir.Name, _workingDir, "working dir does not match expected");
            Assert.Equal(2, _repos.Length, "Expecting 2 repositories");
            Assert.Equal("d:\\xpto", _repos[0], "Wrong repository location");
            Assert.Equal("e:\\packages", _repos[1], "Wrong repository location");
        }

        [Fact]
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
            "-r", "d:\\xpto", "-r", "http://repository.example"
          }, console);

            var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            Assert.Equal(curDir.Name, _workingDir, "working dir does not match expected");
            Assert.Equal(2, _repos.Length, "Expecting 2 repositories");
            Assert.Equal("file:///d:/xpto", _repos[0].AbsoluteUri, "Wrong repository location");
            Assert.Equal("http://repository.example/", _repos[1].AbsoluteUri, "Wrong repository location");
            Assert.True(_repos[0].IsDirectory(), "First repo should be a directory");
            Assert.False(_repos[1].IsDirectory(), "Second repo not should be a directory");
        }

        [Fact]
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
            "-r", "..\\xpto", "-r", "\\root_dir"
          }, console);

            var curDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            Assert.Equal(curDir.Name, _workingDir, "working dir does not match expected");
            Assert.Equal(2, _repos.Length, "Expecting 2 repositories");
            Assert.Equal("..\\xpto", _repos[0].OriginalString, "Wrong repository location");
            Assert.Equal("\\root_dir", _repos[1].OriginalString, "Wrong repository location");
            // TODO: use mock filesystem to resolve relative urls
        }

        [Fact]
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
            Assert.Equal(curDir.Name, _workingDir, "working dir does not match expected");
            Assert.Equal(1, _repos.Length, "Expecting 2 repositories");
            Assert.Equal("d:\\xpto", _repos[0], "Wrong repository location");
        }
    }


}

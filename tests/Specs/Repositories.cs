using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using tests.Mocks;
using tests.Objects;
using Xunit;
using ExecutionContext = Cmf.CLI.Core.Objects.ExecutionContext;

namespace tests.Specs;

public class Repositories
{
  [Theory]
  [InlineData("c:\\folder", typeof(LocalRepositoryClient))]
  [InlineData("c:\\parent\\folder", typeof(LocalRepositoryClient))]
  [InlineData("c:\\parent\\folder\\file.zip", typeof(ArchiveRepositoryClient))]
  [InlineData("c:\\parent\\folder\\file.tgz", typeof(ArchiveRepositoryClient))]
  [InlineData("\\\\server\\folder", typeof(ICIFSRepositoryClient))]
  [InlineData("https://feed.example", typeof(NPMRepositoryClient))]
  public void GetRepositoryClients(string path, Type client)
  {
    var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
    repositoryAuthStoreMock.Setup(x => x.Load()).Returns(Task.FromResult(new CmfAuthFile()));

    ExecutionContext.ServiceProvider = (new ServiceCollection())
        .AddSingleton<IVersionService, MockVersionService>()
        .AddSingleton(repositoryAuthStoreMock.Object)
        .BuildServiceProvider();
    IRepositoryLocator loc = new RepositoryLocator();
    var uri = new Uri(MockUnixSupport.Path(path), UriKind.Absolute);
    var x = loc.GetRepositoryClient(uri, new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { MockUnixSupport.Path("c:\\parent\\folder\\file.zip"), string.Empty },
            { MockUnixSupport.Path("c:\\parent\\folder\\file.tgz"), string.Empty }
        }));
    x.Should().BeAssignableTo(client);
    if (client == typeof(ICIFSRepositoryClient))
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        x.Should().BeOfType<ArchiveRepositoryClient>();
      }
      else
      {
        x.Should().BeOfType<CIFSRepositoryClient>();
      }
    }
  }

  [Fact]
  public void SingletonClientForUri()
  {
    var fs = new MockFileSystem();
    IRepositoryLocator loc = new RepositoryLocator();
    var uri = new Uri(MockUnixSupport.Path($"c:\\repo"), UriKind.Absolute);
    var x = loc.GetRepositoryClient(uri, fs);
    var y = loc.GetRepositoryClient(uri, fs);

    x.Should().NotBeNull("Repository client should be created");
    x.Should().BeSameAs(y, "because the repository client should return the same instance for several requests for the same URI");
  }

  [Fact]
  public async Task LocalRepositoryClient_Get()
  {
    var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path("c:/test/cmfpackage.json"), new MockFileData(
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
            { MockUnixSupport.Path("c:/test/UI/html/cmfpackage.json"), new MockFileData(
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
            { MockUnixSupport.Path("c:/test/Business/cmfpackage.json"), new MockFileData(
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

    ExecutionContext.Initialize(fileSystem);
    var client = new LocalRepositoryClient(MockUnixSupport.Path("c:/test"), fileSystem);
    var cmfPackage = await client.Find("Cmf.Custom.Package", "1.1.0");
    cmfPackage.PackageId.Should().Be("Cmf.Custom.Package");

    var busPackage = await client.Find("Cmf.Custom.Business", "1.1.0");
    busPackage.Should().NotBeNull();
    busPackage.PackageId.Should().Be("Cmf.Custom.Business");

    var htmlPackage = await client.Find("Cmf.Custom.HTML", "1.1.0");
    htmlPackage.Should().NotBeNull();
    htmlPackage.PackageId.Should().Be("Cmf.Custom.HTML");
  }

  [Fact]
  public async Task LocalRepositoryClient_List()
  {
    var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path("c:/test/cmfpackage.json"), new MockFileData(
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
            { MockUnixSupport.Path("c:/test/UI/html/cmfpackage.json"), new MockFileData(
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
            { MockUnixSupport.Path("c:/test/Business/cmfpackage.json"), new MockFileData(
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

    ExecutionContext.Initialize(fileSystem);
    var client = new LocalRepositoryClient(MockUnixSupport.Path("c:/test"), fileSystem);
    var cmfPackages = await client.List();
    cmfPackages.Should().AllBeOfType<CmfPackageV1>();
    cmfPackages.Should().ContainSingle(package => package.PackageId == "Cmf.Custom.Business");
    cmfPackages.Should().ContainSingle(package => package.PackageId == "Cmf.Custom.Package");
    cmfPackages.Should().ContainSingle(package => package.PackageId == "Cmf.Custom.HTML");
  }

  [Fact]
  public async Task WindowsShareRepositoryClient_Get()
  {
    var packageId = "CriticalManufacturing.DeploymentMetadata";
    var version = "8.1.1";
    var repo = OperatingSystem.IsWindows() ? "\\\\share\\dir" : "/repoDir";
    var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{repo}/{packageId}.{version}.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml",
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                                     <deploymentPackage>
                                       <packageId>{packageId}</packageId>
                                       <version>{version}</version>
                                       <dependencies>
                                         <dependency id="Inner.Package" version="0.0.1" mandatory="true" isMissing="true" />
                                       </dependencies>
                                     </deploymentPackage>
                 """).ToByteArray()) }
        });

    var client = new ArchiveRepositoryClient(repo, fileSystem);
    var pkg = await client.Find(packageId, version);
    pkg.Should().NotBeNull();
    pkg.Version.Should().Be(version);
    pkg.PackageId.Should().Be(packageId);
  }

  [Fact]
  public async Task WindowsShareRepositoryClient_List()
  {
    var packageId = "CriticalManufacturing.DeploymentMetadata";
    var version = "8.1.1";
    var packageId2 = "Cmf.Custom.Baseline.Package";
    var version2 = "1.2.3";
    var packageId3 = "Cmf.Custom.Baseline.Metadata";
    var version3 = "1.2.2";
    var packageId4 = "Cmf.Custom.Baseline.Excluded";
    var version4 = "9.9.9";
    var packageId5 = "Cmf.Custom.Baseline.TarExample";
    var version5 = "3.2.1";
    var repo = OperatingSystem.IsWindows() ? "\\\\share\\dir" : "/repoDir";
    var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{repo}/{packageId}.{version}.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml",
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                                     <deploymentPackage>
                                       <packageId>{packageId}</packageId>
                                       <version>{version}</version>
                                       <dependencies>
                                         <dependency id="Inner.Package" version="0.0.1" mandatory="true" isMissing="true" />
                                       </dependencies>
                                     </deploymentPackage>
                 """).ToByteArray()) },
            { $"{repo}/{packageId2}.{version2}.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml",
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                                     <deploymentPackage>
                                       <packageId>{packageId2}</packageId>
                                       <version>{version2}</version>
                                       <dependencies>
                                         <dependency id="Inner.Package" version="0.0.1" mandatory="true" isMissing="true" />
                                       </dependencies>
                                     </deploymentPackage>
                 """).ToByteArray()) },
            { $"{repo}/{packageId3}.{version3}.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml",
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                                     <deploymentPackage>
                                       <packageId>{packageId3}</packageId>
                                       <version>{version3}</version>
                                       <dependencies>
                                         <dependency id="Inner.Package" version="0.0.1" mandatory="true" isMissing="true" />
                                       </dependencies>
                                     </deploymentPackage>
                 """).ToByteArray()) },
            { $"{repo}/inner/{packageId4}.{version4}.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml",
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                                     <deploymentPackage>
                                       <packageId>{packageId4}</packageId>
                                       <version>{version4}</version>
                                       <dependencies>
                                         <dependency id="Inner.Package" version="0.0.1" mandatory="true" isMissing="true" />
                                       </dependencies>
                                     </deploymentPackage>
                 """).ToByteArray()) },
            { $"{repo}/{packageId5}.{version5}.tar.gz", new DFTGZPackageBuilder().CreateEntry("package.json",
                $$"""
                 {
                    "name": "{{packageId5}}",
                    "version": "{{version5}}",
                     "dependencies": {
                        "Cmf.Database": "[11.1.0, 11.1.0]"
                     }
                 }
                 """).CreateEntry("manifest.xml",
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                                     <deploymentPackage>
                                       <packageId>{packageId5}</packageId>
                                       <version>{version5}</version>
                                       <dependencies>
                                         <dependency id="Inner.Package" version="0.0.1" mandatory="true" isMissing="true" />
                                       </dependencies>
                                     </deploymentPackage>
                 """).ToMockFileData() }
        });

    var client = new ArchiveRepositoryClient(repo, fileSystem);
    var cmfPackages = await client.List();
    cmfPackages.Should().NotBeNull();
    cmfPackages.Should().AllBeOfType<CmfPackageV1>();
    cmfPackages.Should().ContainSingle(package => package.PackageId == packageId);
    cmfPackages.Should().ContainSingle(package => package.PackageId == packageId2);
    cmfPackages.Should().ContainSingle(package => package.PackageId == packageId3);
    cmfPackages.Should().NotContain(package => package.PackageId == packageId4);
  }

  [Fact]
  public async Task NPMRepositoryClient_Publish()
  {
    var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
    repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));

    ExecutionContext.ServiceProvider = (new ServiceCollection())
        .AddSingleton<IVersionService, MockVersionService>()
        .AddSingleton(repositoryAuthStoreMock.Object)
        .BuildServiceProvider();

    var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
                { MockUnixSupport.Path("c:/pkgs/Cmf.Custom.Data.1.0.0.zip"), new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml",
                    $"""
                    <?xml version="1.0" encoding="utf-8"?>
                         <deploymentPackage>
                           <packageId>Cmf.Custom.Data</packageId>
                           <version>1.0.0</version>
                           <steps>
                             <step type="DeployFiles" contentPath="*.example" />
                           </steps>
                         </deploymentPackage>
                    """).CreateEntry("payload.example", "this is an example payload")
                    .ToByteArray()) }
        });

    var localRepoClient = new ArchiveRepositoryClient(MockUnixSupport.Path("c:/pkgs"), fileSystem);
    var pkg = await localRepoClient.Find("Cmf.Custom.Data", "1.0.0");
    var ctlr = new CmfPackageController(pkg, fileSystem);
    var feed = "https://example.repo/";
    // Mock HttpMessageHandler to intercept the HttpClient request
    var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

    // Setup the protected method SendAsync
    mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri == $"{feed}Cmf.Custom.Data".ToLowerInvariant() && req.Content.Headers.GetValues("content-type").FirstOrDefault() == "application/json"),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
          Content = new StringContent("{\"status\":\"success\"}", Encoding.UTF8, "application/json")
        });

    // Create HttpClient with the mocked handler
    var httpClient = new HttpClient(mockHandler.Object)
    {
      BaseAddress = new Uri(feed.TrimEnd('/'))
    };
    var npmClient = new NPMClient(baseUrl: feed, client: httpClient);
    var client = new NPMRepositoryClient(feed, fileSystem, npmClient);
    await client.Put(ctlr.CmfPackage);
  }

  [Fact]
  public async Task NPMRepositoryClient_Get()
  {
    var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
    repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));

    ExecutionContext.ServiceProvider = (new ServiceCollection())
        .AddSingleton<IVersionService, MockVersionService>()
        .AddSingleton(repositoryAuthStoreMock.Object)
        .BuildServiceProvider();
    var packageId = "Cmf.Custom.Data";
    var version = "1.0.0";

    var feed = "https://example.repo/";
    // Mock HttpMessageHandler to intercept the HttpClient request
    var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

    // Setup the protected method SendAsync
    mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri == $"{feed}Cmf.Custom.Data".ToLowerInvariant()),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
          Content = new StringContent(
                """
                    {
                    "_id": "cmf.custom.data",
                    "name": "cmf.custom.data",
                    "dist-tags": {
                      "latest": "1.0.0"
                    },
                    "versions": {
                      "1.0.0": {
                        "name": "cmf.custom.data",
                        "version": "1.0.0",
                        "author": "Critical Manufacturing",
                        "keywords": [
                          "cmf-deployment-package",
                          "cmf-deployment-installable"
                        ],
                        "isUniqueInstall": false,
                        "deployment": {
                          "isInstallable": false,
                          "packageType": 0,
                          "targetDirectory": "",
                          "targetLayer": "",
                          "steps": [
                            {
                              "order": 0,
                              "type": "DeployFiles",
                              "messageType": "ImportObject",
                              "contentPath": "*.example"
                            }
                          ],
                          "packageDemands": []
                        },
                        "dependencies": {},
                        "mandatoryDependencies": {},
                        "conditionalDependencies": {},
                        "_id": "cmf.custom.data@1.0.0",
                        "dist": {
                          "shasum": "1234",
                          "tarball": "https://example.repo/cmf.custom.data/-/cmf.custom.data-1.0.0.tgz",
                          "fileCount": 5,
                          "integrity": "sha512-dummy",
                          "signatures": [
                            {
                              "sig": "dummy",
                              "keyid": "SHA256:dummy"
                            }
                          ],
                          "unpackedSize": 10345
                        },
                        "_cliVersion": "9.9.9",
                        "description": "Cmf Custom Data",
                        "directories": {}
                        },
                    }}
                    """
                , Encoding.UTF8, "application/json")
        });

    // Create HttpClient with the mocked handler
    var httpClient = new HttpClient(mockHandler.Object)
    {
      BaseAddress = new Uri(feed.TrimEnd('/'))
    };
    var npmClient = new NPMClient(baseUrl: feed, client: httpClient);

    var client = new NPMRepositoryClient(feed, new MockFileSystem(), npmClient);
    var pkg = await client.Find(packageId, version);
    pkg.Should().NotBeNull();
    pkg.Version.Should().Be(version);
    pkg.PackageId.Should().Be(packageId.ToLowerInvariant()); // NPM package names are always lowercase
  }

  [Fact]
  public async Task NPMRepositoryClient_List()
  {
    var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
    repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));

    ExecutionContext.ServiceProvider = (new ServiceCollection())
        .AddSingleton<IVersionService, MockVersionService>()
        .AddSingleton(repositoryAuthStoreMock.Object)
        .BuildServiceProvider();
    var feed = "https://example.repo/";
    // Mock HttpMessageHandler to intercept the HttpClient request
    var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

    // Setup the protected method SendAsync
    mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri == $"{feed}-/v1/search?text=keywords:cmf-deployment-package".ToLowerInvariant()),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
          Content = new StringContent("""
                                            {
                                              "objects": [
                                                {
                                                  "score": {
                                                    "detail": {
                                                      "quality": 0.0,
                                                      "popularity": 0.0,
                                                      "maintenance": 0.0
                                                    },
                                                    "final": 0.0
                                                  },
                                                  "searchScore": 1.0,
                                                  "package": {
                                                    "name": "cmf.custom.data",
                                                    "version": "1.0.0",
                                                    "description": null,
                                                    "keywords": [
                                                      "cmf-deployment-package"
                                                    ],
                                                    "date": "1970-01-01T12:00:00.000Z",
                                                    "links": {
                                                      "npm": null,
                                                      "homepage": null,
                                                      "repository": null,
                                                      "bugs": null
                                                    },
                                                    "publisher": {
                                                      "username": "Critical Manufacturing",
                                                      "email": null
                                                    },
                                                    "maintainers": [
                                                      {
                                                        "username": "Critical Manufacturing",
                                                        "email": null
                                                      }
                                                    ]
                                                  }
                                                }
                                              ],
                                              "total": 1,
                                              "time": "Sun Jan 1 1970 12:00:00 GMT+0000 (UTC)"
                                            }
                                            """, Encoding.UTF8, "application/json")
        });

    // Create HttpClient with the mocked handler
    var httpClient = new HttpClient(mockHandler.Object)
    {
      BaseAddress = new Uri(feed.TrimEnd('/'))
    };
    var npmClient = new NPMClient(baseUrl: feed, client: httpClient);

    var client = new NPMRepositoryClient(feed, new MockFileSystem(), npmClient);
    var pkgs = await client.List();
    pkgs.Count.Should().Be(0);

  }

  // [Fact]
  // public void ConvertZip()
  // {
  //     var fs = new FileSystem();
  //     var zipFile = fs.DirectoryInfo.New(@"x:\repo\Cmf.Custom.IoT.Data")
  //         .GetFiles("Cmf.Common.IoT.Utilities.IoTPackages.0.1.0.zip").FirstOrDefault();
  //     var tarGzFile = zipFile.FileSystem.FileInfo.New(zipFile.FullName.Replace(".zip", ".tgz"));
  //     CmfPackageController.ConvertZipToTarGz(zipFile, tarGzFile, true);
  // }

  // [Fact]
  // public void ConvertTGz()
  // {
  //     var fs = new FileSystem();
  //     var tarGzFile = fs.DirectoryInfo.New(@"x:\repo\Cmf.Custom.IoT.Data")
  //         .GetFiles("Cmf.Common.IoT.Utilities.IoTPackages.0.1.0.tgz").FirstOrDefault();
  //     var zipFile = tarGzFile.FileSystem.FileInfo.New(tarGzFile.FullName.Replace(".tgz", ".zip"));
  //     CmfPackageController.ConvertTarGzToZip(tarGzFile, zipFile);
  // }

  [Fact]
  public void Idempotent_Manifest_Conversion()
  {
    var packageId = "CriticalManufacturing.DeploymentMetadata";
    var version = "8.1.1";
    var repo = OperatingSystem.IsWindows() ? "\\\\share\\dir" : "/repoDir";
    var manifestContent = $"""
                               <?xml version="1.0" encoding="utf-8"?>
                                                   <deploymentPackage>
                                                     <packageId>{packageId}</packageId>
                                                     <version>{version}</version>
                                                     <dependencies>
                                                       <dependency id="Inner.Package" version="0.0.1" mandatory="true" isMissing="true" />
                                                     </dependencies>
                                                   </deploymentPackage>
                               """;
    var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{repo}/{packageId}.{version}.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml", manifestContent).ToByteArray()) }
        });

    CmfPackageController.ConvertZipToTarGz(fileSystem.FileInfo.New($"{repo}/{packageId}.{version}.zip"), fileSystem.FileInfo.New($"{repo}/{packageId}.{version}.tgz"));
    CmfPackageController.ConvertTarGzToZip(fileSystem.FileInfo.New($"{repo}/{packageId}.{version}.tgz"), fileSystem.FileInfo.New($"{repo}/{packageId}.{version}.zip"));
    // var ctrlr = new CmfPackageController(fileSystem.FileInfo.New($"{repo}/{packageId}.{version}.zip"));
    string newManifestContent = FileSystemUtilities.GetFileContentFromPackage($"{repo}/{packageId}.{version}.zip", "manifest.xml", fileSystem);
    newManifestContent.Should().Be(manifestContent);
  }

  [Fact]
  public void ConvertZipToTgz_HappyPath()
  {
    KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.0.0");
    const string cliPackageType = "Root";

    string manifestContent =
      @$"<deploymentPackage>
            <packageId>{packageRoot.Key}</packageId>
            <name>Critical Manufacturing Customization</name>
            <packageType>Generic</packageType>
            <cliPackageType>{cliPackageType}</cliPackageType>
            <description>This package deploys Critical Manufacturing Customization</description>
            <version>{packageRoot.Value}</version>
            <isInstallable>True</isInstallable>
            <isUniqueInstall>False</isUniqueInstall>
            <keywords>cmf-root-package</keywords>
            <dependencies>
              <dependency id=""Cmf.Environment"" version=""11.0.0"" mandatory=""false"" isIgnorable=""true"" />
            </dependencies>
            <steps>
              <step type=""MasterData"" title=""Master Data"" filePath=""MasterData/001-MD01.json"" createInCollection=""false"" importXMLObjectPath=""ExportedObjects"" targetPlatform=""Self"" />
              <step type=""MasterData"" title=""Master Data"" filePath=""MasterData/002-MD02.json"" createInCollection=""false"" importXMLObjectPath=""ExportedObjects"" targetPlatform=""Self"" />
            </steps>
        </deploymentPackage>";

    var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
      {
          { $"repo/{packageRoot.Key}.{packageRoot.Value}.zip", new MockFileData(new DFPackageBuilder().CreateEntry("manifest.xml", manifestContent).ToByteArray()) }
      });
    ExecutionContext.Initialize(fileSystem);

    IFileInfo tgzPackageFile = fileSystem.FileInfo.New($"repo/{packageRoot.Key}.{packageRoot.Value}.tgz");

    // test conversion zip->tgz
    CmfPackageController.ConvertZipToTarGz(fileSystem.FileInfo.New($"repo/{packageRoot.Key}.{packageRoot.Value}.zip"), tgzPackageFile);

    // get tgz info for assertion
    using GZipStream gzipStream = new GZipStream(tgzPackageFile.OpenRead(), CompressionMode.Decompress);
    using TarReader tarReader = new(gzipStream);
    dynamic packageJson = null;
    int tgzEntriesTotal = 0;
    while (tarReader.GetNextEntry() is { } entry)
    {
      if (entry.Name == "package/package.json")
      {
        using MemoryStream ms = new();
        entry.DataStream.CopyTo(ms);
        packageJson = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(ms.ToArray()));
      }
      tgzEntriesTotal++;
    }

    tgzEntriesTotal.Should().Be(2, "The tgz should contain only 2 entries, the package.json and the manifest.xml.");
    Assert.NotNull(packageJson);
    Assert.NotNull(packageJson.deployment);
    Assert.NotNull(packageJson.deployment.steps);
    Assert.Equal(2, (int)packageJson.deployment.steps.Count);
  }

  [Fact]
  public void ConvertZipToTgz_WithPackageJson()
  {
    KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.0.0");
    const string cliPackageType = "Root";

    string manifestContent =
        @$"{{
                ""name"": ""{packageRoot.Key}"",
                ""description"": """",
                ""packageName"": ""Critical Manufacturing Customization"",
                ""version"": ""{packageRoot.Value}"",
                ""author"": ""Critical Manufacturing"",
                ""keywords"": [
                  ""cmf-deployment-package""
                ],
                ""isToForceInstall"": false,
                ""isUniqueInstall"": true,
                ""forceRerunAfterDatabaseRestore"": false,
                ""deployment"": {{
                  ""manifestVersion"": 0,
                  ""isInstallable"": false,
                  ""packageType"": ""{cliPackageType}"",
                  ""targetDirectory"": """",
                  ""targetLayerDirectory"": """",
                  ""targetLayer"": """",
                  ""buildDate"": null,
                  ""steps"": [
                    {{
                      ""type"": ""RestoreDatabaseFromBackup"",
                      ""contentPath"": ""online.bak"",
                      ""elements"": [],
                      ""targetDatabase"": ""$(Product.Database.Online)""
                    }}
                  ],
                  ""packageDemands"": [],
                  ""metadata"": {{
                    ""originalPackageId"": ""Cmf.Database.Mes.Online""
                  }}
                }},
                ""dependencies"": {{}},
                ""mandatoryDependencies"": {{}},
                ""conditionalDependencies"": {{}}
              }}";

    var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
      {
          { $"repo/{packageRoot.Key}.{packageRoot.Value}.zip", new MockFileData(new DFPackageBuilder().CreateEntry("package.json", manifestContent).ToByteArray()) }
      });
    ExecutionContext.Initialize(fileSystem);

    IFileInfo tgzPackageFile = fileSystem.FileInfo.New($"repo/{packageRoot.Key}.{packageRoot.Value}.tgz");

    // test conversion zip->tgz
    CmfPackageController.ConvertZipToTarGz(fileSystem.FileInfo.New($"repo/{packageRoot.Key}.{packageRoot.Value}.zip"), tgzPackageFile, lowercase: true);

    // get tgz info for assertion
    using GZipStream gzipStream = new GZipStream(tgzPackageFile.OpenRead(), CompressionMode.Decompress);
    using TarReader tarReader = new(gzipStream);
    dynamic packageJson = null;
    int tgzEntriesTotal = 0;
    while (tarReader.GetNextEntry() is { } entry)
    {
      if (entry.Name == "package/package.json")
      {
        using MemoryStream ms = new();
        entry.DataStream.CopyTo(ms);
        packageJson = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(ms.ToArray()));
      }
      tgzEntriesTotal++;
    }

    tgzEntriesTotal.Should().Be(1, "The tgz should contain only 1 entry, the package.json.");
    Assert.NotNull(packageJson);
    Assert.NotNull(packageJson.deployment);
    Assert.NotNull(packageJson._originalPackageId);
    Assert.NotNull(packageJson.deployment.steps);
    Assert.Equal(1, (int)packageJson.deployment.steps.Count);
    Assert.Equal(packageRoot.Key.ToLowerInvariant(), packageJson.name.ToString());
    Assert.Equal(packageRoot.Key, packageJson._originalPackageId.ToString());
  }

  [Theory]
  [InlineData("true")]
  [InlineData("false")]
  public async Task NPMRepositoryClient_Get_PackageWithoutSomeProperties(string disableNpmPropertiesCheck)
  {
    Environment.SetEnvironmentVariable("cmf_cli_disable_npm_properties_check", disableNpmPropertiesCheck);
    var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
    repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));

    ExecutionContext.ServiceProvider = (new ServiceCollection())
        .AddSingleton<IVersionService, MockVersionService>()
        .AddSingleton(repositoryAuthStoreMock.Object)
        .BuildServiceProvider();
    var packageId = "Cmf.Custom.Data";
    var version = "1.0.0";

    var feed = "https://example.repo/";
    // Mock HttpMessageHandler to intercept the HttpClient request
    var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

    // Setup the protected method SendAsync
    mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri == $"{feed}Cmf.Custom.Data".ToLowerInvariant()),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
          Content = new StringContent(
                """
                    {
                    "_id": "cmf.custom.data",
                    "name": "cmf.custom.data",
                    "dist-tags": {
                      "latest": "1.0.0"
                    },
                    "versions": {
                      "1.0.0": {
                        "name": "cmf.custom.data",
                        "version": "1.0.0",
                        "author": "Critical Manufacturing",
                        "_id": "cmf.custom.data@1.0.0",
                        "dist": {
                          "shasum": "1234",
                          "tarball": "https://example.repo/cmf.custom.data/-/cmf.custom.data-1.0.0.tgz",
                          "fileCount": 5,
                          "integrity": "sha512-dummy",
                          "signatures": [
                            {
                              "sig": "dummy",
                              "keyid": "SHA256:dummy"
                            }
                          ],
                          "unpackedSize": 10345
                        },
                        "_cliVersion": "9.9.9",
                        "description": "Cmf Custom Data",
                        "directories": {}
                        },
                    }}
                    """
                , Encoding.UTF8, "application/json")
        });

    // Create HttpClient with the mocked handler
    var httpClient = new HttpClient(mockHandler.Object)
    {
      BaseAddress = new Uri(feed.TrimEnd('/'))
    };
    var npmClient = new NPMClient(baseUrl: feed, client: httpClient);

    var client = new NPMRepositoryClient(feed, new MockFileSystem(), npmClient);
    var pkg = await client.Find(packageId, version);
    pkg.Should().Be(disableNpmPropertiesCheck == "false" ? null : pkg);
  }
}
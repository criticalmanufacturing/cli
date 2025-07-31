using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using tests.Mocks;
using tests.Objects;
using Xunit;
using ExecutionContext = Cmf.CLI.Core.Objects.ExecutionContext;

namespace tests.Specs;

public class NPM
{
    protected void Setup()
    {
        var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
        repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton(repositoryAuthStoreMock.Object)
            .BuildServiceProvider();
    }

    [Fact]
    public async Task FindPackage()
    {
        Setup();

        var httpClient = new HttpClient();

        var npmClient = new NPMClient(client: httpClient);

        var pkgNames = await npmClient.SearchPackages("@criticalmanufacturing/cli");

        pkgNames.Should().Contain("@criticalmanufacturing/cli");
    }
    
    [Fact]
    public async Task GetPackageVersion()
    {
        Setup();

        var httpClient = new HttpClient();

        var npmClient = new NPMClient(client: httpClient);

        var pkgInfo = await npmClient.FetchPackageInfo("@criticalmanufacturing/cli", "5.1.0");

        pkgInfo.Name.Should().Be("@criticalmanufacturing/cli");
        pkgInfo.Version.Should().Be("5.1.0");
        pkgInfo.Dist.Tarball.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DownloadPackage()
    {
        Setup();

        var httpClient = new HttpClient();
        var npmClient = new NPMClient(client: httpClient);
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { "/tmp/.gitkeep", new MockFileData("")} // ensure output directory exists
        });
        // var fs = new FileSystem();
        var output = fs.FileInfo.New("/tmp/cli@5.1.0.tgz");
        var outputFile = await npmClient.DownloadPackage("@criticalmanufacturing/cli", "5.1.0", output);
        
        outputFile.Should().NotBeNull();
        outputFile.Exists.Should().BeTrue();
        outputFile.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PublishPackage()
    {
        var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
        repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton(repositoryAuthStoreMock.Object)
            .BuildServiceProvider();

        var feed = "https://example.repo/";
        var packageId = "Cmf.Custom.Baseline.TarExample";
        var version = "3.2.1";
        
        // Mock HttpMessageHandler to intercept the HttpClient request
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        
        // Setup the protected method SendAsync
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri == $"{feed}{packageId}".ToLowerInvariant() && req.Content.Headers.GetValues("content-type").FirstOrDefault() == "application/json"),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"success\"}", Encoding.UTF8, "application/json")
            });

        // Create HttpClient with the mocked handler
        var client = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(feed.TrimEnd('/'))
        };
        
        var npmClient = new NPMClient(baseUrl: feed, client: client);

        
        var repo = OperatingSystem.IsWindows() ? "\\\\share\\dir" : "/repoDir";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{repo}/{packageId}.{version}.tgz", new DFTGZPackageBuilder().CreateEntry("package.json",
                $$"""
                 {
                    "dummy, will use manifest.xml first if available"
                 }
                 """).CreateEntry("manifest.xml", 
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                                     <deploymentPackage>
                                       <packageId>{packageId}</packageId>
                                       <version>{version}</version>
                                       <dependencies>
                                         <dependency id="Inner.Package" version="0.0.1" mandatory="true" isMissing="true" />
                                       </dependencies>
                                     </deploymentPackage>
                 """).ToMockFileData() }
        });
        var pkg = fileSystem.FileInfo.New($"{repo}/{packageId}.{version}.tgz");
        pkg.Exists.Should().BeTrue();
        await npmClient.PublishPackage(pkg);
        
        // Verify that the mocked handler was called as expected
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(), // Ensure it was called once
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == $"{feed}{packageId}".ToLowerInvariant()),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    protected void SetupCredentials(ICredential credentials, out NPMClient npmClient, out Mock<HttpMessageHandler> mockHandler)
    {
        string baseUrl = "https://example.repo/";

        var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
        repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));
        repositoryAuthStoreMock.Setup(x => x.GetCredentialsFor<NPMRepositoryCredentials>(It.IsAny<CmfAuthFile>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(credentials);

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton(repositoryAuthStoreMock.Object)
            .BuildServiceProvider();

        mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("""
                 {
                     "dist-tags": {
                        "latest": "0.0.1"
                     }
                 }
                 """, Encoding.UTF8, "application/json")
            });

        npmClient = new NPMClient(baseUrl, new HttpClient(mockHandler.Object));
    }

    [Fact]
    public async Task Authentication_BasicCredentials()
    {
        // Arrange
        SetupCredentials(new BasicCredential { Username = "user", Password = "password" },
            out var npmClient,
            out var mockHandler);

        // Act
        var pkg = await npmClient.GetLatestVersion();

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Headers != null &&
                                                 req.Headers.Authorization != null &&
                                                 req.Headers.Authorization.Scheme == "Basic" &&
                                                 req.Headers.Authorization.Parameter == Convert.ToBase64String(Encoding.UTF8.GetBytes("user:password"))),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task Authentication_BearerCredentials()
    {
        // Arrange
        SetupCredentials(new BearerCredential { Token = "A.B.C" },
            out var npmClient,
            out var mockHandler);

        // Act
        var pkg = await npmClient.GetLatestVersion();

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Headers != null &&
                                                 req.Headers.Authorization != null &&
                                                 req.Headers.Authorization.Scheme == "Bearer" &&
                                                 req.Headers.Authorization.Parameter == "A.B.C"),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task Authentication_NoCredentials()
    {
        // Arrange
        SetupCredentials(credentials: null,
            out var npmClient,
            out var mockHandler);

        // Act
        var pkg = await npmClient.GetLatestVersion();

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Headers != null &&
                                                 req.Headers.Authorization == null),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
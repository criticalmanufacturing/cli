using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using tests.Mocks;
using tests.Objects;
using Xunit;
using ExecutionContext = Cmf.CLI.Core.Objects.ExecutionContext;

namespace tests.Specs;

public class NPM
{
    [Fact]
    public async Task ResolveDistTag_ReturnsExactVersionFromConfiguredRegistry()
    {
        Setup();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.RequestUri.ToString() == "https://registry.example/npm/@criticalmanufacturing/ngx-schematics"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"dist-tags\":{\"beta-1200\":\"12.0.0-beta.99\"}}", Encoding.UTF8, "application/json")
            });
        var client = new NPMClient("https://registry.example/npm/", new HttpClient(handler.Object));

        var result = await client.ResolveDistTag("@criticalmanufacturing/ngx-schematics", "beta-1200");

        result.Should().Be("12.0.0-beta.99");
        handler.VerifyAll();
    }

    [Theory]
    [InlineData("{\"dist-tags\":{}}")]
    [InlineData("{}")]
    [InlineData("{\"dist-tags\":{\"beta-1200\":\"\"}}")]
    public async Task ResolveDistTag_MissingTag_Throws(string responseBody)
    {
        Setup();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
        var client = new NPMClient("https://registry.example", new HttpClient(handler.Object));

        Func<Task> resolve = () => client.ResolveDistTag("@criticalmanufacturing/ngx-schematics", "beta-1200");

        await resolve.Should().ThrowAsync<CliException>().WithMessage("*beta-1200*ngx-schematics*");
    }

    [Fact]
    public async Task ResolveDistTag_RegistryError_Throws()
    {
        Setup();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized));
        var client = new NPMClient("https://registry.example", new HttpClient(handler.Object));

        Func<Task> resolve = () => client.ResolveDistTag("@criticalmanufacturing/ngx-schematics", "beta-1200");

        await resolve.Should().ThrowAsync<HttpRequestException>();
    }

    protected void Setup(bool streaming = false)
    {
        var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
        repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));

        var features = new Mock<IFeaturesService>();
        features.SetupGet(x => x.UseStreamingPublish).Returns(streaming);

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton(repositoryAuthStoreMock.Object)
            .AddSingleton(features.Object)
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Upload_SendsCompletePayloadAndDisposesContent(bool streaming)
    {
        Setup(streaming);
        var package = CreatePublishPackage();
        var bytes = package.FileSystem.File.ReadAllBytes(package.FullName);
        using var handler = new UploadHandler();
        using var httpClient = new HttpClient(handler);
        var client = new NPMClient("https://registry.example", httpClient);

        // Reuse the client just as publishing a directory of packages does.
        await client.PublishPackage(package);
        await client.PublishPackage(package);

        handler.RequestCount.Should().Be(2);
        var body = JObject.Parse(handler.Payload);
        var attachment = body["_attachments"]["cmf.custom.test-1.0.0.tgz"];
        Convert.FromBase64String(attachment["data"].Value<string>()).Should().Equal(bytes);
        attachment["length"].Value<long>().Should().Be(bytes.Length);
        body["versions"]["1.0.0"]["dist"]["integrity"].Value<string>()
            .Should().Be("sha512-" + Convert.ToBase64String(SHA512.HashData(bytes)));
        handler.ContentLength.Should().Be(System.Text.Encoding.UTF8.GetByteCount(handler.Payload));
        Func<Task> readDisposedContent = () => handler.Content.ReadAsStringAsync();
        await readDisposedContent.Should().ThrowAsync<ObjectDisposedException>();
        Func<Task> readDisposedResponse = () => handler.Response.Content.ReadAsStringAsync();
        await readDisposedResponse.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UploadFailure_PreservesUnderlyingExceptionAndDisposesContent(bool streaming)
    {
        Setup(streaming);
        var rootCause = new IOException("Connection reset by peer");
        var failure = new HttpRequestException("Error while copying content to a stream.", rootCause);
        using var handler = new UploadHandler { Failure = failure };
        using var httpClient = new HttpClient(handler);
        var client = new NPMClient("https://registry.example", httpClient);

        Func<Task> publish = () => client.PublishPackage(CreatePublishPackage());

        var exception = await publish.Should().ThrowAsync<CliException>()
            .WithMessage("*Cmf.Custom.Test*Connection reset by peer*");
        exception.Which.InnerException.Should().BeSameAs(failure);
        Func<Task> readDisposedContent = () => handler.Content.ReadAsStringAsync();
        await readDisposedContent.Should().ThrowAsync<ObjectDisposedException>();
    }

    private static System.IO.Abstractions.IFileInfo CreatePublishPackage()
    {
        using var builder = new DFTGZPackageBuilder();
        var data = builder.CreateManifest("Cmf.Custom.Test", "1.0.0")
            .CreateEntry("content.txt", new string('a', 100000)).ToMockFileData();
        var fs = new MockFileSystem();
        fs.AddFile("/repo/test.tgz", data);
        return fs.FileInfo.New("/repo/test.tgz");
    }

    private sealed class UploadHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public string Payload { get; private set; }
        public long? ContentLength { get; private set; }
        public HttpContent Content { get; private set; }
        public HttpResponseMessage Response { get; private set; }
        public Exception Failure { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            Content = request.Content;
            ContentLength = Content.Headers.ContentLength;
            // Unlike a handler that just returns OK, consume the actual upload stream.
            using var output = new MemoryStream();
            await Content.CopyToAsync(output, cancellationToken);
            Payload = System.Text.Encoding.UTF8.GetString(output.ToArray());
            if (Failure != null)
            {
                throw Failure;
            }
            Response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
            return Response;
        }
    }
}

using Cmf.CLI.Commands;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using Cmf.Common.Cli.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ExecutionContext = Cmf.CLI.Core.Objects.ExecutionContext;

namespace tests.Specs;

public class PortalPatExpiration : IDisposable
{
    private static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(1900000000);
    private readonly ServiceProvider _previousServiceProvider = ExecutionContext.ServiceProvider;

    /// <summary>
    /// Restores shared services so later command tests do not use a disposed provider.
    /// </summary>
    public void Dispose() => ExecutionContext.ServiceProvider = _previousServiceProvider;

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateCredentials_MissingToken_ThrowsActionableError(string token)
    {
        var portal = new FixedClockPortal(new MockFileSystem(), Mock.Of<IPortalLoginCommand>());

        var exception = Assert.Throws<CliException>(() => portal.ValidateCredentials([Credential(token)]));

        exception.Message.Should().Be(CoreMessages.PortalPatMissing);
    }

    [Theory]
    [InlineData("not-a-jwt")]
    [InlineData("header.@@@.signature")]
    [InlineData("header..signature")]
    [InlineData(".e30.signature")]
    [InlineData("header.e30.")]
    public void ValidateCredentials_MalformedToken_ThrowsActionableError(string token)
    {
        var portal = new FixedClockPortal(new MockFileSystem(), Mock.Of<IPortalLoginCommand>());

        var exception = Assert.Throws<CliException>(() => portal.ValidateCredentials([Credential(token)]));

        exception.Message.Should().Be(CoreMessages.PortalPatInvalid);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("not-json")]
    [InlineData("{\"exp\":null}")]
    [InlineData("{\"exp\":\"secret-invalid-expiration\"}")]
    [InlineData("{\"exp\":true}")]
    [InlineData("{\"exp\":1.5}")]
    [InlineData("{\"exp\":9223372036854775807}")]
    public void ValidateCredentials_InvalidExpiration_DoesNotExposeClaimValues(string payload)
    {
        var portal = new FixedClockPortal(new MockFileSystem(), Mock.Of<IPortalLoginCommand>());

        var exception = Assert.Throws<CliException>(() => portal.ValidateCredentials([Credential(Token(payload))]));

        exception.Message.Should().Be(CoreMessages.PortalPatInvalid);
        exception.InnerException.Should().BeNull();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateCredentials_ExpiredIncludingExactBoundary_Throws(int seconds)
    {
        var portal = new FixedClockPortal(new MockFileSystem(), Mock.Of<IPortalLoginCommand>());
        var token = Token($$"""{"exp":{{Now.ToUnixTimeSeconds() + seconds}}}""");

        var exception = Assert.Throws<CliException>(() => portal.ValidateCredentials([Credential(token)]));

        exception.Message.Should().Be(CoreMessages.PortalPatExpired);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(86400)]
    [InlineData(2592000)]
    public void ValidateCredentials_FutureExpiration_DoesNotApplyRenewalThreshold(int seconds)
    {
        var login = new Mock<IPortalLoginCommand>();
        var portal = new FixedClockPortal(new MockFileSystem(), login.Object);
        var token = Token($$"""{"exp":{{Now.ToUnixTimeSeconds() + seconds}},"sub":"user"}""");

        portal.ValidateCredentials([Credential(token)]);

        login.Verify(x => x.Execute(), Times.Never);
    }

    [Fact]
    public void ValidateCredentials_Base64UrlPayload_IsAccepted()
    {
        var portal = new FixedClockPortal(new MockFileSystem(), Mock.Of<IPortalLoginCommand>());
        var token = Token("{\"exp\":1900000001,\"sub\":\"user\",\"extra\":\"  \u083e\u083f\"}");
        token.Should().ContainAny("-", "_");

        portal.ValidateCredentials([Credential(token)]);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Login_ExpiredToken_DoesNotWriteCredentialsOrInvokeBrowser(bool storeOnly, bool existingFiles)
    {
        var fileSystem = new MockFileSystem();
        var authPath = fileSystem.Path.GetFullPath("pat-expiration/.cmf-auth.json");
        if (existingFiles)
        {
            fileSystem.AddFile(authPath, new MockFileData("existing auth file"));
            fileSystem.AddFile(PortalRepositoryCredentials.TokenFilePath, new MockFileData("existing Portal token"));
            fileSystem.AddFile(fileSystem.Path.GetFullPath(".npmrc"), new MockFileData("existing npm config"));
            fileSystem.AddFile(fileSystem.Path.GetFullPath("NuGet.Config"), new MockFileData("existing NuGet config"));
            fileSystem.AddFile(fileSystem.Path.GetFullPath(".docker/config.json"), new MockFileData("existing Docker config"));
        }
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path));
        var login = new Mock<IPortalLoginCommand>();
        var portal = new FixedClockPortal(fileSystem, login.Object);
        var store = new RepositoryAuthStore(fileSystem.FileInfo.New(authPath));
        using var services = new ServiceCollection()
            .AddSingleton<IRepositoryCredentials>(portal)
            .AddSingleton<IRepositoryAuthStore>(store)
            .BuildServiceProvider();
        ExecutionContext.ServiceProvider = services;
        var arguments = new[] { "--token", Token("{\"exp\":1899999999}"), "--no-prompt" };
        if (storeOnly)
        {
            arguments = [..arguments, "--store-only"];
        }

        var exception = await Record.ExceptionAsync(() =>
            TestUtilities.TestInvokeAsync(new LoginCommand(fileSystem), arguments));

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(CoreMessages.PortalPatExpired);
        fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllText(path))
            .Should().BeEquivalentTo(before);
        login.Verify(x => x.Execute(), Times.Never);
    }

    [Fact]
    public async Task Login_TokenExpiringTomorrow_StoreOnlySucceedsWithoutBrowser()
    {
        var fileSystem = new MockFileSystem();
        var authPath = fileSystem.Path.GetFullPath("pat-expiration/.cmf-auth.json");
        var login = new Mock<IPortalLoginCommand>();
        var portal = new FixedClockPortal(fileSystem, login.Object);
        var store = new RepositoryAuthStore(fileSystem.FileInfo.New(authPath));
        using var services = new ServiceCollection()
            .AddSingleton<IRepositoryCredentials>(portal)
            .AddSingleton<IRepositoryAuthStore>(store)
            .BuildServiceProvider();
        ExecutionContext.ServiceProvider = services;
        var token = Token("{\"exp\":1900086400,\"sub\":\"user\"}");

        await TestUtilities.TestInvokeAsync(new LoginCommand(fileSystem), ["--token", token, "--store-only"]);

        fileSystem.File.ReadAllText(authPath).Should().Contain(token);
        login.Verify(x => x.Execute(), Times.Never);
    }

    private static BearerCredential Credential(string token) =>
        new(RepositoryCredentialsType.Portal, CmfAuthConstants.PortalRepository, null, token);

    private static string Token(string payload) =>
        "header." + Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_') + ".signature";

    private sealed class FixedClockPortal(MockFileSystem fileSystem, IPortalLoginCommand login)
        : PortalRepositoryCredentials(fileSystem, login)
    {
        protected override TimeProvider TimeProvider => new FixedClock();
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }
}

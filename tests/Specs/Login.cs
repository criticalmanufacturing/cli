using Cmf.CLI.Commands;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using Cmf.Common.Cli.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace tests.Specs
{
    public class Login
    {
        [Fact]
        public async Task Login_AutomaticPortalLogin()
        {
            // Arrange
            var mockCredential = new BearerCredential
            {
                RepositoryType = RepositoryCredentialsType.Portal,
                Repository = CmfAuthConstants.PortalRepository,
                Token = "header.payload.signature",
            };

            var authStoreMock = new Mock<IRepositoryAuthStore>();
            var portalRepositoryMock = new Mock<IRepositoryCredentials>();
            var portalAutomaticLoginMock = portalRepositoryMock.As<IRepositoryAutomaticLogin>();
            portalRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.Portal);

            portalAutomaticLoginMock.Setup(x => x.AutomaticLogin())
                .Returns(Task.FromResult<ICredential>(mockCredential));

            authStoreMock.Setup(x => x.GetRepositoryType(RepositoryCredentialsType.Portal)).Returns(portalRepositoryMock.Object);

            ExecutionContext.ServiceProvider = new ServiceCollection().AddSingleton(authStoreMock.Object).BuildServiceProvider();

            // Act
            await TestUtilities.TestInvokeAsync(new LoginCommand(), []);

            // Assert
            portalAutomaticLoginMock.Verify(x => x.AutomaticLogin(), Times.Once);
            authStoreMock.Verify(x => x.Save(It.IsAny<IList<ICredential>>(), It.IsAny<bool>()), Times.Once);
            authStoreMock.Verify(x => x.Save(
                It.Is<IList<ICredential>>(cred => cred.Count == 1 && cred[0] == (ICredential)mockCredential),
                It.Is<bool>(sync => sync == true)
            ), Times.Once);

        }

        [Fact]
        public async Task Login_NPMExplicitAuthType()
        {
            // Arrange
            var token = "header.payload.signature";
            var repository = "http://custom-npm-registry.com/";

            var authStoreMock = new Mock<IRepositoryAuthStore>();
            var npmRepositoryMock = new Mock<IRepositoryCredentials>();
            npmRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NPM);
            npmRepositoryMock.Setup(x => x.SupportedAuthTypes).Returns([AuthType.Basic, AuthType.Bearer]);

            authStoreMock.Setup(x => x.GetRepositoryType(It.IsAny<RepositoryCredentialsType>())).Returns(npmRepositoryMock.Object);

            ExecutionContext.ServiceProvider = new ServiceCollection().AddSingleton(authStoreMock.Object).BuildServiceProvider();

            // Act
            await TestUtilities.TestInvokeAsync(new LoginCommand(), ["NPM", repository, "-T", "Bearer", "-t", token]);

            // Assert
            authStoreMock.Verify(x => x.Save(It.IsAny<IList<ICredential>>(), It.IsAny<bool>()), Times.Once);
            authStoreMock.Verify(x => x.Save(
                It.Is<IList<ICredential>>(cred => cred.Count == 1 && cred[0] is BearerCredential && ((BearerCredential)cred[0]).Token == token
                                                                                                 && ((BearerCredential)cred[0]).RepositoryType == RepositoryCredentialsType.NPM
                                                                                                 && ((BearerCredential)cred[0]).Repository == repository),
                It.Is<bool>(sync => sync == true)
            ), Times.Once);
        }

        [Fact]
        public async Task Login_NPMMissingAuthType_ShouldFail()
        {
            // Arrange
            var token = "header.payload.signature";
            var repository = "http://custom-npm-registry.com/";

            var authStoreMock = new Mock<IRepositoryAuthStore>();
            var npmRepositoryMock = new Mock<IRepositoryCredentials>();
            npmRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NPM);
            npmRepositoryMock.Setup(x => x.SupportedAuthTypes).Returns([AuthType.Basic, AuthType.Bearer]);

            authStoreMock.Setup(x => x.GetRepositoryType(It.IsAny<RepositoryCredentialsType>())).Returns(npmRepositoryMock.Object);

            ExecutionContext.ServiceProvider = new ServiceCollection().AddSingleton(authStoreMock.Object).BuildServiceProvider();

            // Act
            var exception = await Record.ExceptionAsync(() => TestUtilities.TestInvokeAsync(new LoginCommand(), ["NPM", repository, "-t", token]));

            // Assert
            exception.Message.Should().Contain("Missing mandatory auth type");
        }

        [Fact]
        public async Task Login_NuGetBasicAuth()
        {
            // Arrange
            var repository = "https://custom-nuget-registry.com/index.json";
            var username = "user@domain.com";
            var password = "qwerty";

            var authStoreMock = new Mock<IRepositoryAuthStore>();
            var nugetRepositoryMock = new Mock<IRepositoryCredentials>();
            nugetRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NuGet);
            nugetRepositoryMock.Setup(x => x.SupportedAuthTypes).Returns([AuthType.Basic]);

            authStoreMock.Setup(x => x.GetRepositoryType(It.IsAny<RepositoryCredentialsType>())).Returns(nugetRepositoryMock.Object);

            ExecutionContext.ServiceProvider = new ServiceCollection().AddSingleton(authStoreMock.Object).BuildServiceProvider();

            // Act
            await TestUtilities.TestInvokeAsync(new LoginCommand(), ["NuGet", repository, "-u", username, "-p", password, "--store-only"]);

            // Assert
            authStoreMock.Verify(x => x.Save(It.IsAny<IList<ICredential>>(), It.IsAny<bool>()), Times.Once);
            authStoreMock.Verify(x => x.Save(
                It.Is<IList<ICredential>>(cred => cred.Count == 1 && cred[0] is BasicCredential && ((BasicCredential)cred[0]).Username == username
                                                                                                && ((BasicCredential)cred[0]).Password == password
                                                                                                && ((BasicCredential)cred[0]).RepositoryType == RepositoryCredentialsType.NuGet
                                                                                                && ((BasicCredential)cred[0]).Repository == repository),
                It.Is<bool>(sync => sync == false) // because of flag --store-only
            ), Times.Once);
        }

        [Fact]
        public async Task Login_Sync()
        {
            // Arrange
            var npmCred1 = new BasicCredential();
            var npmCred2 = new BearerCredential();
            var nugetCred1 = new BasicCredential();

            var authFile = new CmfAuthFile
            {
                Repositories = new Dictionary<RepositoryCredentialsType, CmfAuthFileRepositoryType>()
                {
                    { RepositoryCredentialsType.NPM, new CmfAuthFileRepositoryType { Credentials = [npmCred1, npmCred2] } },
                    { RepositoryCredentialsType.NuGet, new CmfAuthFileRepositoryType { Credentials = [nugetCred1] } }
                }
            };

            var nugetRepositoryMock = new Mock<IRepositoryCredentials>();
            nugetRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NuGet);

            var npmRepositoryMock = new Mock<IRepositoryCredentials>();
            npmRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NPM);

            var authStoreMock = new Mock<IRepositoryAuthStore>();
            authStoreMock.Setup(x => x.Load()).Returns(Task.FromResult(authFile));
            authStoreMock.SetupSequence(x => x.GetRepositoryType(It.IsAny<RepositoryCredentialsType>()))
                .Returns(npmRepositoryMock.Object)
                .Returns(nugetRepositoryMock.Object);

            ExecutionContext.ServiceProvider = new ServiceCollection().AddSingleton(authStoreMock.Object).BuildServiceProvider();

            // Act
            await TestUtilities.TestInvokeAsync(new SyncCommand(), []);

            // Assert
            npmRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Once);
            npmRepositoryMock.Verify(x => x.SyncCredentials(
                It.Is<IList<ICredential>>(cred => cred.Count == 2 && cred[0] == (ICredential)npmCred1
                                                                  && cred[1] == (ICredential)npmCred2)
            ), Times.Once);
            nugetRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Once);
            nugetRepositoryMock.Verify(x => x.SyncCredentials(
                It.Is<IList<ICredential>>(cred => cred.Count == 1 && cred[0] == (ICredential)nugetCred1)
            ), Times.Once);
        }

        [Fact]
        public async Task Login_Sync_RepositoryType()
        {
            // Arrange
            var npmCred1 = new BasicCredential();
            var npmCred2 = new BearerCredential();
            var nugetCred1 = new BasicCredential();

            var authFile = new CmfAuthFile
            {
                Repositories = new Dictionary<RepositoryCredentialsType, CmfAuthFileRepositoryType>()
                {
                    { RepositoryCredentialsType.NPM, new CmfAuthFileRepositoryType { Credentials = [npmCred1, npmCred2] } },
                    { RepositoryCredentialsType.NuGet, new CmfAuthFileRepositoryType { Credentials = [nugetCred1] } }
                }
            };

            var nugetRepositoryMock = new Mock<IRepositoryCredentials>();
            nugetRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NuGet);

            var npmRepositoryMock = new Mock<IRepositoryCredentials>();
            npmRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NPM);

            var authStoreMock = new Mock<IRepositoryAuthStore>();
            authStoreMock.Setup(x => x.Load()).Returns(Task.FromResult(authFile));
            authStoreMock.SetupSequence(x => x.GetRepositoryType(It.IsAny<RepositoryCredentialsType>()))
                .Returns(nugetRepositoryMock.Object)
                .Returns(nugetRepositoryMock.Object);

            ExecutionContext.ServiceProvider = new ServiceCollection().AddSingleton(authStoreMock.Object).BuildServiceProvider();

            // Act
            await TestUtilities.TestInvokeAsync(new SyncCommand(), ["NuGet", "sync"], setupParents: true);

            // Assert
            npmRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Never);
            nugetRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Once);
            nugetRepositoryMock.Verify(x => x.SyncCredentials(
                It.Is<IList<ICredential>>(cred => cred.Count == 1 && cred[0] == (ICredential)nugetCred1)
            ), Times.Once);
        }

        [Fact]
        public async Task Login_Sync_RepositoryUrl()
        {
            // Arrange
            var npmCred1 = new BasicCredential() { Repository = "http://url-1.com" };
            var npmCred2 = new BearerCredential() { Repository = "http://url-2.com" };
            var nugetCred1 = new BasicCredential();

            var authFile = new CmfAuthFile
            {
                Repositories = new Dictionary<RepositoryCredentialsType, CmfAuthFileRepositoryType>()
                {
                    { RepositoryCredentialsType.NPM, new CmfAuthFileRepositoryType { Credentials = [npmCred1, npmCred2] } },
                    { RepositoryCredentialsType.NuGet, new CmfAuthFileRepositoryType { Credentials = [nugetCred1] } }
                }
            };

            var nugetRepositoryMock = new Mock<IRepositoryCredentials>();
            nugetRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NuGet);

            var npmRepositoryMock = new Mock<IRepositoryCredentials>();
            npmRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NPM);

            var authStoreMock = new Mock<IRepositoryAuthStore>();
            authStoreMock.Setup(x => x.Load()).Returns(Task.FromResult(authFile));
            authStoreMock.SetupSequence(x => x.GetRepositoryType(It.IsAny<RepositoryCredentialsType>()))
                .Returns(npmRepositoryMock.Object)
                .Returns(npmRepositoryMock.Object);

            ExecutionContext.ServiceProvider = new ServiceCollection().AddSingleton(authStoreMock.Object).BuildServiceProvider();

            // Act
            await TestUtilities.TestInvokeAsync(new SyncCommand(), ["npm", "http://url-2.com", "sync"], setupParents: true);

            // Assert
            npmRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Once);
            npmRepositoryMock.Verify(x => x.SyncCredentials(
                It.Is<IList<ICredential>>(cred => cred.Count == 1 && cred[0] == (ICredential)npmCred2)
            ), Times.Once);
            nugetRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Never);
        }
    }
}

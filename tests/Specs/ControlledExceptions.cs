using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cmf.CLI.Commands;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Services;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Moq;
using NuGet.Versioning;
using Xunit;
using Core.Objects;
using SMBLibrary;
using SMBLibrary.Client;

namespace tests.Specs;

public class ControlledExceptions
{
    [Theory]
    [InlineData("Username")]
    [InlineData("Password")]
    [InlineData("Token")]
    public void Login_MissingNonInteractiveArgument_ThrowsCliException(string label)
    {
        var command = new LoginCommand(new MockFileSystem());

        var exception = Assert.Throws<CliException>(() => command.Prompt(label, noPrompt: true));

        exception.Message.Should().Be($"Missing command argument for \"{label}\"");
        exception.ErrorCode.Should().Be(ErrorCode.Default);
    }

    [Fact]
    public void MissingMandatoryProperty_ThrowsCliException()
    {
        var exception = Assert.Throws<CliException>(() =>
            GenericUtilities.ValidatePropertyRequirement("TOKEN", null, PropertyRequirement.Mandatory));

        exception.Message.Should().Be("Missing mandatory TOKEN.");
        exception.ErrorCode.Should().Be(ErrorCode.Default);
    }

    [Fact]
    public void NuGet_MissingKey_ThrowsCliException()
    {
        var repository = new NuGetRepositoryCredentials(new MockFileSystem());

        var exception = Assert.Throws<CliException>(() => repository.ValidateCredentials(
            [new BasicCredential { Repository = "https://example.com/nuget" }]));

        exception.Message.Should().Contain("Missing mandatory \"key\" value");
    }

    [Fact]
    public void UnsupportedAngularVersion_ThrowsCliException()
    {
        var service = new DependencyVersionService();

        var exception = Assert.Throws<CliException>(() => service.Angular(new NuGetVersion("13.0.0")));

        exception.Message.Should().Be("No Angular dependencies defined for MES version 13.0.0");
    }

    [Fact]
    public void Workspace_MissingPackageJson_ThrowsCliException()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/workspace/angular.json"] = new("""
                { "projects": { "test": { "root": "projects/test", "projectType": "library" } } }
                """)
        });

        var exception = Assert.Throws<CliException>(() =>
            new AngularWorkspace(fileSystem.DirectoryInfo.New("/workspace")));

        exception.Message.Should().Be("No package.json found at projects/test/package.json");
    }

    [Fact]
    public void Package_DuplicateDependencies_ThrowsCliException()
    {
        var package = CmfPackageController.FromXml(XDocument.Parse("""
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <dependencies>
                <dependency id="Cmf.Custom.Business" version="1.0.0" />
                <dependency id="Cmf.Custom.Business" version="1.0.0" />
              </dependencies>
            </deploymentPackage>
            """));
        var controller = new CmfPackageController(package, new MockFileSystem());

        var exception = Assert.Throws<CliException>(() => controller.ToJson());

        exception.Message.Should().Contain("dependencies are declared more than once")
            .And.Contain("Cmf.Custom.Business@1.0.0");
    }

    [Fact]
    public async Task Portal_LoginWithoutToken_ThrowsCliException()
    {
        var repository = new PortalRepositoryCredentials(new MockFileSystem(), Mock.Of<IPortalLoginCommand>());

        var exception = await Assert.ThrowsAsync<CliException>(() => repository.AutomaticLogin());

        exception.Message.Should().Contain("no auth token was retrieved");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedFolder_UnavailableShare_ThrowsCliException(bool write)
    {
        var client = new Mock<ISMBClient>();
        var status = NTStatus.STATUS_ACCESS_DENIED;
        client.Setup(x => x.TreeConnect(It.IsAny<string>(), out status)).Returns((ISMBFileStore)null);
        var folder = new SharedFolder(new Uri("smb://server/share"), client.Object, new MockFileSystem());

        var exception = Assert.Throws<CliException>(() =>
        {
            if (write) folder.PutFile("/local/file.txt", "remote.txt");
            else folder.GetFile("remote.txt");
        });

        exception.Message.Should().Be("Cannot perform actions in a non-existent shared folder.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedFolder_FileOperationFailure_ThrowsCliException(bool write)
    {
        var client = new Mock<ISMBClient>();
        var store = new Mock<ISMBFileStore>();
        client.Setup(x => x.TreeConnect(It.IsAny<string>(), out It.Ref<NTStatus>.IsAny)).Returns(store.Object);
        store.Setup(x => x.CreateFile(out It.Ref<object>.IsAny, out It.Ref<FileStatus>.IsAny,
            It.IsAny<string>(), It.IsAny<AccessMask>(), It.IsAny<SMBLibrary.FileAttributes>(),
            It.IsAny<ShareAccess>(), It.IsAny<CreateDisposition>(), It.IsAny<CreateOptions>(), null))
            .Returns(write ? NTStatus.STATUS_ACCESS_DENIED : NTStatus.STATUS_SUCCESS);
        store.Setup(x => x.ReadFile(out It.Ref<byte[]>.IsAny, It.IsAny<object>(), It.IsAny<long>(), It.IsAny<int>()))
            .Returns(NTStatus.STATUS_ACCESS_DENIED);
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/local/file.txt"] = new("contents")
        });
        var folder = new SharedFolder(new Uri("smb://server/share"), client.Object, fileSystem);

        var exception = Assert.Throws<CliException>(() =>
        {
            if (write) folder.PutFile("/local/file.txt", "remote.txt");
            else folder.GetFile("remote.txt");
        });

        exception.Message.Should().StartWith("Failed to ");
    }

    [Theory]
    [InlineData("{", "Failed to load credentials from CMF Auth File*")]
    [InlineData("""
        { "repositories": { "npm": { "credentials": [ { "authType": "invalid" } ] } } }
        """, "Invalid \"authType\" property value*")]
    public async Task AuthFile_InvalidConfiguration_ThrowsCliException(string contents, string expectedMessage)
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/auth/.cmf-auth.json"] = new(contents)
        });
        var store = new RepositoryAuthStore(fileSystem.FileInfo.New("/auth/.cmf-auth.json"));

        var exception = await Assert.ThrowsAsync<CliException>(() => store.Load());

        exception.Message.Should().Match(expectedMessage);
        exception.ErrorCode.Should().Be(ErrorCode.Default);
    }

    [Fact]
    public async Task AuthFile_MissingParentFolder_RemainsControlled()
    {
        var fileInfo = new Mock<IFileInfo>();
        var store = new TestAuthStore(fileInfo.Object);

        var exception = await Assert.ThrowsAsync<CliException>(() => store.SaveForTest());

        exception.Message.Should().StartWith("Could not create CMF Authfile folder");
    }

    [Fact]
    public async Task NuGet_InvalidXml_IsControlled()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [RepositoryCredentials.MockNuGetConfigFilePath] = new("<configuration>")
        });
        var repository = new NuGetRepositoryCredentials(fileSystem);

        var exception = await Assert.ThrowsAsync<CliException>(() => repository.SyncCredentials([]));

        exception.Message.Should().StartWith("Failed to sync credentials into NuGet config file:");
        exception.InnerException.Should().BeOfType<System.Xml.XmlException>();
    }

    [Theory]
    [InlineData("load")]
    [InlineData("save")]
    [InlineData("npm")]
    [InlineData("nuget")]
    public async Task CredentialOperations_KnownFileFailure_PreservesInnerException(string operation)
    {
        var failure = new IOException("File is unavailable");

        var exception = await Assert.ThrowsAsync<CliException>(() => RunFailingOperation(operation, failure));

        exception.InnerException.Should().BeSameAs(failure);
        exception.Message.Should().StartWith("Failed to ");
        exception.ErrorCode.Should().Be(ErrorCode.Default);
    }

    [Theory]
    [InlineData("load")]
    [InlineData("save")]
    [InlineData("npm")]
    [InlineData("nuget")]
    public async Task CredentialOperations_AccessDenied_IsControlled(string operation)
    {
        var failure = new UnauthorizedAccessException("Access denied");

        var exception = await Assert.ThrowsAsync<CliException>(() => RunFailingOperation(operation, failure));

        exception.InnerException.Should().BeSameAs(failure);
    }

    [Theory]
    [InlineData("load")]
    [InlineData("save")]
    [InlineData("npm")]
    [InlineData("nuget")]
    public async Task CredentialOperations_ExistingCliException_PreservesInstanceAndExitCode(string operation)
    {
        var failure = new CliException("Invalid credentials", ErrorCode.InvalidArgument);

        var exception = await Assert.ThrowsAsync<CliException>(() => RunFailingOperation(operation, failure));

        exception.Should().BeSameAs(failure);
        exception.ErrorCode.Should().Be(ErrorCode.InvalidArgument);
    }

    [Theory]
    [InlineData("load")]
    [InlineData("save")]
    [InlineData("npm")]
    [InlineData("nuget")]
    public async Task CredentialOperations_UnexpectedFailure_IsNotConverted(string operation)
    {
        var failure = new InvalidOperationException("Programming error");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => RunFailingOperation(operation, failure));

        exception.Should().BeSameAs(failure);
    }

    [Theory]
    [InlineData("load")]
    [InlineData("save")]
    [InlineData("npm")]
    [InlineData("nuget")]
    public async Task CredentialOperations_Cancellation_IsNotConverted(string operation)
    {
        var failure = new OperationCanceledException();

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => RunFailingOperation(operation, failure));

        exception.Should().BeSameAs(failure);
    }

    [Theory]
    [InlineData("npm")]
    [InlineData("nuget")]
    public async Task CredentialSync_UnsupportedCredential_RemainsControlled(string operation)
    {
        var credential = new Mock<ICredential>();
        credential.SetupGet(x => x.Repository).Returns("https://example.com/packages");
        credential.SetupGet(x => x.RepositoryType).Returns(RepositoryCredentialsType.NPM);
        IRepositoryCredentials repository = operation == "npm"
            ? new NPMRepositoryCredentials(new MockFileSystem())
            : new NuGetRepositoryCredentials(new MockFileSystem());

        var exception = await Assert.ThrowsAnyAsync<CliException>(() => repository.SyncCredentials([credential.Object]));

        exception.Message.Should().StartWith("Unsupported Credential type");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("header.%%%.signature")]
    [InlineData("header.bm90LWpzb24=.signature")]
    [InlineData("header.bnVsbA==.signature")]
    public void Portal_InvalidToken_ThrowsCliException(string token)
    {
        var repository = new TestPortalCredentials();

        var exception = Assert.Throws<CliException>(() => repository.ParseToken(token));

        exception.Message.Should().StartWith("Invalid format JWT token");
    }

    [Fact]
    public void Portal_NullToken_RetainsArgumentNullException()
    {
        var repository = new TestPortalCredentials();

        var exception = Assert.Throws<ArgumentNullException>(() => repository.ParseToken(null));

        exception.ParamName.Should().Be("token");
    }

    private static Task RunFailingOperation(string operation, Exception failure)
    {
        var file = new Mock<IFile>();
        file.Setup(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(failure);
        file.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(failure);
        var directory = new Mock<IDirectoryInfo>();
        directory.SetupGet(x => x.Exists).Returns(true);
        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(x => x.FullName).Returns("/auth/config");
        fileInfo.SetupGet(x => x.Exists).Returns(operation != "save");
        fileInfo.SetupGet(x => x.Directory).Returns(directory.Object);
        var factory = new Mock<IFileInfoFactory>();
        factory.Setup(x => x.New(It.IsAny<string>())).Returns(fileInfo.Object);
        var fileSystem = new Mock<IFileSystem>();
        fileSystem.SetupGet(x => x.File).Returns(file.Object);
        fileSystem.SetupGet(x => x.FileInfo).Returns(factory.Object);
        fileSystem.SetupGet(x => x.Path).Returns(new MockFileSystem().Path);
        fileInfo.SetupGet(x => x.FileSystem).Returns(fileSystem.Object);

        return operation switch
        {
            "load" => new RepositoryAuthStore(fileInfo.Object).Load(),
            "save" => new TestAuthStore(fileInfo.Object).SaveForTest(),
            "npm" => new NPMRepositoryCredentials(fileSystem.Object).SyncCredentials([]),
            "nuget" => new NuGetRepositoryCredentials(fileSystem.Object).SyncCredentials([]),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    private sealed class TestAuthStore(IFileInfo file) : RepositoryAuthStore(file)
    {
        public Task<CmfAuthFile> SaveForTest() => SaveInternal([]);
    }

    private sealed class TestPortalCredentials() : PortalRepositoryCredentials(new MockFileSystem())
    {
        public void ParseToken(string token) => ParseJwt(token);
    }
}

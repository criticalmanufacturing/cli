using System;
using Xunit;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using Moq;
using Cmf.CLI.Commands;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.IO;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Objects;
using System.IO.Compression;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Repository;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Core.Repository.Credentials;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tests.Mocks;

namespace tests.Specs;

public class Publish
{
    [Theory]
    [InlineData("testhost", @"\\testhost\files\share")]
    [InlineData("example.com", "https://example.com/repository")]
    [InlineData("", "/local/path/to/repository")]
    public void Repository_Arg_ParsedCorrectly(string expectedHost, string inputRepository)
    {
        string inputFile = "/test/testPackage.zip";

        var publishCommand = new PublishCommand();
        var cmd = new Command("publish");
        publishCommand.Configure(cmd);

        IFileInfo _file = null;
        Uri _repository = null;
        cmd.Handler = CommandHandler.Create<IFileInfo, Uri>((
            file, repository) =>
        {
            _file = file;
            _repository = repository;
        }
        );

        var console = new TestConsole();
        cmd.Invoke(new[] {
            inputFile, "--repository", inputRepository
        }, console);

        Assert.Equal(inputFile, _file.FullName);
        Assert.Equal(inputRepository, _repository.OriginalString);
        Assert.Equal(expectedHost, _repository.Host);
    }

    [Fact]
    public void NonDeploymentFrameworkPackage()
    {
        // Arrange
        using var zipStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            using (var entryStream = zipArchive.CreateEntry("package.json").Open())
            {
                entryStream.Write(Encoding.UTF8.GetBytes("""
                {
                    "name": "Cmf.Custom.Tests",
                    "version": "1.1.0",
                    "description": "Custom Tests Package",
                    "author": "Critical Manufacturing",
                    "keywords": ["cmf-tests-package"]
                }
                """));
            }
        }
        zipStream.Position = 0;

        var archivePath = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Packages\Cmf.Custom.Test.zip");
        var archiveData = zipStream.ToArray();

        var repositoryUrl = new Uri("https://fake.criticalmanufacturing.io");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { archivePath, new MockFileData(archiveData) },
        }, MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test"));

        IFileInfo publishedFileInfo = null;
        
        // Set up a Mock NPM Client that saves the last file info that was published to it
        // Later we can validate that it was only called once, and that means this is the only file that was uploaded
        var npmClient = new Mock<INPMClientEx>();
        npmClient.Setup(x => x.PublishPackage(It.IsAny<IFileInfo>()))
            .Callback((IFileInfo fileInfo) => publishedFileInfo = fileInfo);
        
        var repositoryLocator = new Mock<IRepositoryLocator>();
        repositoryLocator
            .SetupSequence(m => m.GetRepositoryClient(It.IsAny<Uri>(), It.IsAny<IFileSystem>()))
            .Returns(new ArchiveRepositoryClient(archivePath, fileSystem))
            .Returns(new NPMRepositoryClient(repositoryUrl.AbsoluteUri, fileSystem, npmClient.Object));
        
        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IFileSystem>(fileSystem)
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton<IRepositoryAuthStore>(RepositoryAuthStore.FromEnvironmentConfig(fileSystem))
            .AddSingleton<IRepositoryLocator>(repositoryLocator.Object)
            .AddSingleton<IRepositoryCredentials, NPMRepositoryCredentials>()
            .BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);

        // Act
        var publishCommand = new PublishCommand(fileSystem);
        publishCommand.Execute(fileSystem.FileInfo.New(archivePath), repositoryUrl, false, false);

        // Assert
        npmClient.Verify(x => x.PublishPackage(It.IsAny<IFileInfo>()), Times.Once);
        publishedFileInfo.Should().NotBeNull();
        publishedFileInfo.Exists.Should().BeTrue();
        publishedFileInfo.Extension.Should().Be(".tgz");
        
        // Extract the "package.json" file from the .tgz that was "uploaded" to the mock NPM client
        using GZipStream gzipStream = new GZipStream(publishedFileInfo.OpenRead(), CompressionMode.Decompress);
        using TarReader tarReader = new(gzipStream);
        JObject json = null;
        while (tarReader.GetNextEntry() is { } entry)
        {
            // Check if this is the file you're looking for
            if ((entry.Name == "package/package.json") && entry.EntryType == TarEntryType.V7RegularFile)
            {
                if (entry.DataStream != null)
                {
                    // Read the content of the file inside the TAR
                    using var reader = new StreamReader(entry.DataStream);
                    var contents = reader.ReadToEnd();
                    json = JsonConvert.DeserializeObject<JObject>(contents);
                }

                break;
            }
        }
        
        // Make sure the file package.json exists
        Assert.NotNull(json);
        
        Assert.True(json.ContainsKey("name"));
        Assert.Equal("Cmf.Custom.Tests", json["name"]!.Value<string>());

        Assert.True(json.ContainsKey("version"));
        Assert.Equal("1.1.0", json["version"]!.Value<string>());
        
        Assert.False(json.ContainsKey("deployment"));
    }
    
    [Fact]
    public void PublishToContinuousIntegrationRepo()
    {
        // Arrange
        using var zipStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            using (var entryStream = zipArchive.CreateEntry("package.json").Open())
            {
                entryStream.Write(Encoding.UTF8.GetBytes("""
                {
                    "name": "Cmf.Custom.Tests",
                    "version": "1.1.0",
                    "description": "Custom Tests Package",
                    "author": "Critical Manufacturing",
                    "keywords": ["cmf-tests-package"]
                }
                """));
            }
        }
        zipStream.Position = 0;

        var archivePath = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Package\Cmf.Custom.Test.1.0.0.zip");
        var archiveData = zipStream.ToArray();
        
        var repositoryUrl = new Uri("https://fake.criticalmanufacturing.io");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { archivePath, new MockFileData(archiveData) },
            { MockUnixSupport.Path(@"C:\repo\repositories.json"), new MockFileData($$"""
            {
                "CIRepository": "https://fake.criticalmanufacturing.io",
                "Repositories": [
                    "https://fake-release.criticalmanufacturing.io"
                ]
            }
            """) },
        }, MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test"));

        var remoteClientMock = new Mock<IRepositoryClient>();
        remoteClientMock.Setup(x => x.Put(It.IsAny<CmfPackageV1>()));
        
        var repositoryLocator = new Mock<IRepositoryLocator>();
        repositoryLocator
            .SetupSequence(m => m.GetRepositoryClient(It.IsAny<Uri>(), It.IsAny<IFileSystem>()))
            .Returns(new ArchiveRepositoryClient(archivePath, fileSystem))
            .Returns(remoteClientMock.Object);
        
        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IFileSystem>(fileSystem)
            .AddSingleton(repositoryLocator.Object)
            .BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);

        // Act
        var publishCommand = new PublishCommand(fileSystem);
        publishCommand.Execute(fileSystem.FileInfo.New(archivePath), repository: null, ci: true, release: false);

        // Assert
        repositoryLocator.Verify(x => x.GetRepositoryClient(repositoryUrl, It.IsAny<IFileSystem>()), Times.Once);
    }
    
    [Fact]
    public void NonDeploymentFrameworkPackage_MissingKeyword()
    {
        // Arrange
        using var zipStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            using (var entryStream = zipArchive.CreateEntry("package.json").Open())
            {
                entryStream.Write(Encoding.UTF8.GetBytes("""
                {
                    "name": "Cmf.Custom.Tests",
                    "version": "1.1.0",
                    "description": "Custom Tests Package",
                    "author": "Critical Manufacturing"
                }
                """));
            }
        }
        zipStream.Position = 0;

        var archivePath = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Packages\Cmf.Custom.Test.zip");
        var archiveData = zipStream.ToArray();

        var repositoryUrl = new Uri("https://fake.criticalmanufacturing.io");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { archivePath, new MockFileData(archiveData) },
        }, MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test"));

        IFileInfo publishedFileInfo = null;
        
        // Set up a Mock NPM Client that saves the last file info that was published to it
        // Later we can validate that it was only called once, and that means this is the only file that was uploaded
        var npmClient = new Mock<INPMClientEx>();
        npmClient.Setup(x => x.PublishPackage(It.IsAny<IFileInfo>()))
            .Callback((IFileInfo fileInfo) => publishedFileInfo = fileInfo);
        
        var repositoryLocator = new Mock<IRepositoryLocator>();
        repositoryLocator
            .SetupSequence(m => m.GetRepositoryClient(It.IsAny<Uri>(), It.IsAny<IFileSystem>()))
            .Returns(new ArchiveRepositoryClient(archivePath, fileSystem))
            .Returns(new NPMRepositoryClient(repositoryUrl.AbsoluteUri, fileSystem, npmClient.Object));
        
        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IFileSystem>(fileSystem)
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton<IRepositoryAuthStore>(RepositoryAuthStore.FromEnvironmentConfig(fileSystem))
            .AddSingleton<IRepositoryLocator>(repositoryLocator.Object)
            .AddSingleton<IRepositoryCredentials, NPMRepositoryCredentials>()
            .BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);

        // Act
        var publishCommand = new PublishCommand(fileSystem);
        var exception = publishCommand.Invoking(x => x.Execute(fileSystem.FileInfo.New(archivePath), repositoryUrl, false, false));

        // Assert
        exception.Should().Throw<Exception>().WithMessage("*Invalid manifest file: one of the following keywords must be present*");
    }
}

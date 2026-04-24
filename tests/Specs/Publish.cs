using System;
using Xunit;
using System.CommandLine;
using System.IO.Abstractions;
using Moq;
using Cmf.CLI.Commands;
using System.Threading.Tasks;
using Cmf.CLI.Core;
using Cmf.CLI.Utilities;
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
using Spectre.Console;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tests.Mocks;
using System.Linq;

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
        string inputFolder = "/test/packages";

        var publishCommand = new PublishCommand();
        var cmd = new Command("publish");
        publishCommand.Configure(cmd);

        var packagesArg = cmd.Arguments.FirstOrDefault(a => a.Name == "packagePaths") as Argument<string[]>;
        var repositoryOpt = cmd.Options.OfType<Option<Uri>>().FirstOrDefault();

        string[] _packages = null;
        Uri _repository = null;

        cmd.SetAction((parseResult, cancellationToken) =>
        {
            _packages = parseResult.GetValue(packagesArg);
            _repository = parseResult.GetValue(repositoryOpt);
            return Task.FromResult(0);
        });

        var console = new TestConsole();
        var parseResult = cmd.Parse(new[] {
            inputFile, inputFolder, "--repository", inputRepository
        });
        parseResult.Invoke(console);

        _packages.Should().Equal(inputFile, inputFolder);
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
        publishCommand.Execute(archivePath, repositoryUrl, false, false);

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
        Assert.Equal("cmf.custom.tests", json["name"]!.Value<string>()); // publishing to NPM causes the package id to become lowercase

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
        publishCommand.Execute(archivePath, repository: null, ci: true, release: false);

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
        var exception = publishCommand.Invoking(x => x.Execute(archivePath, repositoryUrl, false, false));

        // Assert
        exception.Should().Throw<Exception>().WithMessage("*Invalid manifest file: one of the following keywords must be present*");
    }

    [Fact]
    public void DryRunArgument_DoesNotPublishPackages()
    {
        // Arrange
        var repositoryUrl = new Uri("https://fake.criticalmanufacturing.io");
        var archivePath = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Packages\Cmf.Custom.Tests.1.1.0.zip");
        var archiveData = CreatePackageArchiveData("1.1.0");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { archivePath, new MockFileData(archiveData) },
        }, MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test"));

        var remoteClientMock = new Mock<IRepositoryClient>();

        var repositoryLocator = new Mock<IRepositoryLocator>();
        repositoryLocator
            .SetupSequence(m => m.GetRepositoryClient(It.IsAny<Uri>(), It.IsAny<IFileSystem>()))
            .Returns(new ArchiveRepositoryClient(archivePath, fileSystem))
            .Returns(remoteClientMock.Object);

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IFileSystem>(fileSystem)
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton<IRepositoryAuthStore>(RepositoryAuthStore.FromEnvironmentConfig(fileSystem))
            .AddSingleton<IRepositoryLocator>(repositoryLocator.Object)
            .AddSingleton<IRepositoryCredentials, NPMRepositoryCredentials>()
            .BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);

        var publishCommand = new PublishCommand(fileSystem);
        var cmd = new Command("publish");
        publishCommand.Configure(cmd);

        var console = new TestConsole();

        // Act
        var result = cmd.Parse(new[]
        {
            archivePath,
            "--repository", repositoryUrl.AbsoluteUri,
            "--dry-run"
        }).Invoke(console);

        // Assert
        result.Should().Be(0);
        remoteClientMock.Verify(x => x.Put(It.IsAny<CmfPackageV1>()), Times.Never);
        repositoryLocator.Verify(x => x.GetRepositoryClient(repositoryUrl, It.IsAny<IFileSystem>()), Times.Once);
    }

    [Fact]
    public void PublishFromDirectory()
    {
        // Arrange
        var repositoryUrl = new Uri("https://fake.criticalmanufacturing.io");
        var packageDir = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Package");
        var topLevelArchive1 = MockUnixSupport.Path(packageDir + @"\Cmf.Custom.Tests.1.1.0.zip");
        var topLevelArchive2 = MockUnixSupport.Path(packageDir + @"\Cmf.Custom.Tests.2.0.0.zip");
        var nestedArchive = MockUnixSupport.Path(packageDir + @"\subfolder\Cmf.Custom.Tests.3.0.0.zip");
        var archiveData1 = CreatePackageArchiveData("1.1.0");
        var archiveData2 = CreatePackageArchiveData("2.0.0");
        var archiveData3 = CreatePackageArchiveData("3.0.0");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { topLevelArchive1, new MockFileData(archiveData1) },
            { topLevelArchive2, new MockFileData(archiveData2) },
            { nestedArchive, new MockFileData(archiveData3) },
        }, packageDir);

        var publishedFilesCount = 0;
        
        var npmClient = new Mock<INPMClientEx>();
        npmClient.Setup(x => x.PublishPackage(It.IsAny<IFileInfo>()))
            .Callback(() => publishedFilesCount++);
        
        var repositoryLocator = new Mock<IRepositoryLocator>();
        repositoryLocator
            .Setup(m => m.GetRepositoryClient(It.IsAny<Uri>(), It.IsAny<IFileSystem>()))
            .Returns((Uri uri, IFileSystem fs) =>
            {
                if (uri.IsFile)
                {
                    return new ArchiveRepositoryClient(uri.LocalPath, fs);
                }
                return new NPMRepositoryClient(repositoryUrl.AbsoluteUri, fs, npmClient.Object);
            });
        
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
        publishCommand.Execute(packageDir, repositoryUrl, false, false);

        // Assert
        npmClient.Verify(x => x.PublishPackage(It.IsAny<IFileInfo>()), Times.Exactly(2));
        publishedFilesCount.Should().Be(2);
    }

    [Fact]
    public void PublishMultipleFilesAndDirectories()
    {
        // Arrange
        var repositoryUrl = new Uri("https://fake.criticalmanufacturing.io");
        var packageDir = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Package");
        var directArchive = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Packages\Cmf.Custom.Tests.0.9.0.zip");
        var folderArchive = MockUnixSupport.Path(packageDir + @"\Cmf.Custom.Tests.1.1.0.zip");
        var directArchiveData = CreatePackageArchiveData("0.9.0");
        var folderArchiveData = CreatePackageArchiveData("1.1.0");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { directArchive, new MockFileData(directArchiveData) },
            { folderArchive, new MockFileData(folderArchiveData) },
        }, MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test"));

        var publishedFilesCount = 0;

        var npmClient = new Mock<INPMClientEx>();
        npmClient.Setup(x => x.PublishPackage(It.IsAny<IFileInfo>()))
            .Callback(() => publishedFilesCount++);

        var repositoryLocator = new Mock<IRepositoryLocator>();
        repositoryLocator
            .Setup(m => m.GetRepositoryClient(It.IsAny<Uri>(), It.IsAny<IFileSystem>()))
            .Returns((Uri uri, IFileSystem fs) =>
            {
                if (uri.IsFile)
                {
                    return new ArchiveRepositoryClient(uri.LocalPath, fs);
                }
                return new NPMRepositoryClient(repositoryUrl.AbsoluteUri, fs, npmClient.Object);
            });

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
        publishCommand.Execute([directArchive, packageDir], repositoryUrl, false, false);

        // Assert
        npmClient.Verify(x => x.PublishPackage(It.IsAny<IFileInfo>()), Times.Exactly(2));
        publishedFilesCount.Should().Be(2);
    }

    [Fact]
    public void PublishFailsWithoutPublishingAnythingWhenAnyInputPathDoesNotExist()
    {
        // Arrange
        var repositoryUrl = new Uri("https://fake.criticalmanufacturing.io");
        var existingArchive = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Packages\Cmf.Custom.Tests.1.1.0.zip");
        var missingArchive = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Packages\Missing.zip");
        var archiveData = CreatePackageArchiveData("1.1.0");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { existingArchive, new MockFileData(archiveData) },
        }, MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test"));

        var npmClient = new Mock<INPMClientEx>();
        var repositoryLocator = new Mock<IRepositoryLocator>();
        repositoryLocator
            .Setup(m => m.GetRepositoryClient(It.IsAny<Uri>(), It.IsAny<IFileSystem>()))
            .Returns((Uri uri, IFileSystem fs) =>
            {
                if (uri.IsFile)
                {
                    return new ArchiveRepositoryClient(uri.LocalPath, fs);
                }

                return new NPMRepositoryClient(repositoryUrl.AbsoluteUri, fs, npmClient.Object);
            });

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IFileSystem>(fileSystem)
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton<IRepositoryAuthStore>(RepositoryAuthStore.FromEnvironmentConfig(fileSystem))
            .AddSingleton<IRepositoryLocator>(repositoryLocator.Object)
            .AddSingleton<IRepositoryCredentials, NPMRepositoryCredentials>()
            .BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);

        var publishCommand = new PublishCommand(fileSystem);

        // Act
        var exception = publishCommand.Invoking(x =>
            x.Execute([existingArchive, missingArchive], repositoryUrl, false, false));

        // Assert
        exception.Should().Throw<CliException>()
            .WithMessage($"*Could not find package file or directory at {missingArchive}*");
        npmClient.Verify(x => x.PublishPackage(It.IsAny<IFileInfo>()), Times.Never);
    }

    [Fact]
    public void PublishSkipsEmptyFolderAndLogsMessage()
    {
        // Arrange
        var repositoryUrl = new Uri("https://fake.criticalmanufacturing.io");
        var emptyFolder = MockUnixSupport.Path(@"C:\repo\Cmf.Custom.Test\Empty");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), emptyFolder);
        var npmClient = new Mock<INPMClientEx>();
        var repositoryLocator = new Mock<IRepositoryLocator>();
        repositoryLocator
            .Setup(m => m.GetRepositoryClient(It.IsAny<Uri>(), It.IsAny<IFileSystem>()))
            .Returns((Uri uri, IFileSystem fs) =>
            {
                if (uri.IsFile)
                {
                    return new ArchiveRepositoryClient(uri.LocalPath, fs);
                }

                return new NPMRepositoryClient(repositoryUrl.AbsoluteUri, fs, npmClient.Object);
            });

        var writer = new StringWriter();
        Log.AnsiConsole = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = (ColorSystemSupport)ColorSystem.NoColors,
            Out = new AnsiConsoleOutput(writer),
            Interactive = InteractionSupport.No,
            Enrichment = new ProfileEnrichment
            {
                UseDefaultEnrichers = false,
            },
        });

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IFileSystem>(fileSystem)
            .AddSingleton<IVersionService, MockVersionService>()
            .AddSingleton<IRepositoryAuthStore>(RepositoryAuthStore.FromEnvironmentConfig(fileSystem))
            .AddSingleton<IRepositoryLocator>(repositoryLocator.Object)
            .AddSingleton<IRepositoryCredentials, NPMRepositoryCredentials>()
            .BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);

        var publishCommand = new PublishCommand(fileSystem);

        // Act
        publishCommand.Execute(emptyFolder, repositoryUrl, false, false);

        // Assert
        writer.ToString().Should().Contain($"No packages found in folder {emptyFolder}. Skipping.");
        npmClient.Verify(x => x.PublishPackage(It.IsAny<IFileInfo>()), Times.Never);
    }

    private static byte[] CreatePackageArchiveData(string version)
    {
        using var zipStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            using var entryStream = zipArchive.CreateEntry("package.json").Open();
            entryStream.Write(Encoding.UTF8.GetBytes($$"""
            {
                "name": "Cmf.Custom.Tests",
                "version": "{{version}}",
                "description": "Custom Tests Package",
                "author": "Critical Manufacturing",
                "keywords": ["cmf-tests-package"]
            }
            """));
        }

        return zipStream.ToArray();
    }
}

using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository;
using Cmf.CLI.Core.Repository.Credentials;
using Cmf.CLI.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PeanutButter.INI;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using tests.Extensions;
using tests.Mocks;
using Xunit;

namespace tests.Specs;

public class RepositoryCredentials
{
    public static string MockCmfAuthFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cmf", "cmfauth.json");
    public static string MockNPMConfigFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".npmrc");
    public static string MockNuGetConfigFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NuGet", "NuGet.Config");

    public const string mockCmfAuthJson =
        """
        {
            "repositories": {
                "npm": {},
                "nuget": {
                    "credentials": [{
                        "authType": "Basic",
                        "repository": "https://criticalmanufacturing.io/repository/nuget/index.json",
                        "key": "CMF",
                        "username": "User@criticalmanufacturing.com",
                        "password": "qwerty"
                    }, {
                        "authType": "Bearer",
                        "repository": "https://custom-nuget-registry.io/index.json",
                        "token": "header.payload.signature",
                        "key": "Customer",
                    }]
                },
                "cifs": null,
                "docker": { "credentials": null }
            }
        }
        """;

    public Mock<IRepositoryAuthStore> AuthStoreMock = new();
    public Mock<IRepositoryCredentials> PortalRepositoryMock = new();
    public Mock<IRepositoryCredentials> NPMRepositoryMock = new();
    public Mock<IRepositoryCredentials> NugetRepositoryMock = new();
    public Mock<IRepositoryCredentials> DockerRepositoryMock = new();
    public Mock<IRepositoryCredentials> CIFSRepositoryMock = new();
    public ServiceCollection ServiceCollection = new();
    public RepositoryCredentials()
    {
        PortalRepositoryMock.Setup(x => x.RepositoryType).Returns(CmfAuthConstants.PortalRepositoryType);
        NPMRepositoryMock.Setup(x => x.RepositoryType).Returns(CmfAuthConstants.NPMRepositoryType);
        NugetRepositoryMock.Setup(x => x.RepositoryType).Returns(CmfAuthConstants.NuGetRepositoryType);
        DockerRepositoryMock.Setup(x => x.RepositoryType).Returns(CmfAuthConstants.DockerRepositoryType);
        CIFSRepositoryMock.Setup(x => x.RepositoryType).Returns(CmfAuthConstants.CIFSRepositoryType);

        ServiceCollection
            .AddSingleton(PortalRepositoryMock.Object)
            .AddSingleton(NPMRepositoryMock.Object)
            .AddSingleton(NugetRepositoryMock.Object)
            .AddSingleton(DockerRepositoryMock.Object)
            .AddSingleton(CIFSRepositoryMock.Object)
            .AddSingleton(AuthStoreMock.Object);
    }

    [Fact]
    public async Task RepositoryAuthStore_Load_NoFileExists()
    {
        // Arrange
        MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });

        var authStore = RepositoryAuthStore.FromEnvironmentConfig(fileSystem);

        // Act
        var authFile = await authStore.Load();

        // Assert
        authFile.Should().NotBeNull();
        authFile.Repositories.Should().NotBeNull();
        authFile.Repositories.Count.Should().Be(0);
    }

    [Fact]
    public async Task RepositoryAuthStore_Load_ExistingFile()
    {
        // Arrange
        MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path(MockCmfAuthFilePath), new MockFileData(mockCmfAuthJson) }
        });

        var authStore = RepositoryAuthStore.FromEnvironmentConfig(fileSystem);

        // Act
        var authFile = await authStore.Load();

        // Assert
        authFile.Should().NotBeNull();
        authFile.Repositories.Should().NotBeNull();
        authFile.Repositories.Count.Should().Be(4);

        // All repository types should load a list of credentials (even if the list is empty for some)
        foreach (var (repoType, credsCount) in new (string, int)[] { ("npm", 0), ("nuget", 2), ("cifs", 0), ("docker", 0) })
        {
            authFile.Repositories.ContainsKey(repoType).Should().BeTrue();
            authFile.Repositories[repoType].Should().NotBeNull();
            authFile.Repositories[repoType].Credentials.Should().NotBeNull();
            authFile.Repositories[repoType].Credentials.Count.Should().Be(credsCount);
        }

        // Also, validate that the credentials inside the NuGet repository are all correct
        var credentials = authFile.Repositories["nuget"].Credentials;
        credentials.Should().BeEquivalentTo(new ICredential[]
        {
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.NuGetRepositoryType,
                Key = "CMF",
                Repository = "https://criticalmanufacturing.io/repository/nuget/index.json",
                Username = "User@criticalmanufacturing.com",
                Password = "saferpassword",
            },
            new BearerCredential
            {
                RepositoryType = CmfAuthConstants.NuGetRepositoryType,
                Key = "Customer",
                Repository = "https://custom-nuget-registry.io/index.json",
                Token = "header.payload.signature",
            }
        });
    }

    [Fact]
    public async Task RepositoryAuthStore_Store_NoFileExists()
    {
        // Arrange
        MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });

        ExecutionContext.ServiceProvider = ServiceCollection.BuildServiceProvider();

        var authStore = RepositoryAuthStore.FromEnvironmentConfig(fileSystem);

        // Act
        await authStore.Save([
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.NuGetRepositoryType,
                Key = "CMF",
                Repository = "https://criticalmanufacturing.io/repository/nuget/index.json",
                Username = "User@criticalmanufacturing.com",
                Password = "saferpassword",
            },
            new BearerCredential
            {
                RepositoryType = CmfAuthConstants.NPMRepositoryType,
                Repository = "https://criticalmanufacturing.io/repository/npm/",
                Token = "header.payload.signature",
            },
        ]);

        // Assert
        NugetRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Once);
        NPMRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Once);

        DockerRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Never);
        CIFSRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Never);
        PortalRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Never);

        fileSystem.FileExists(MockCmfAuthFilePath).Should().BeTrue();

        JToken contents = JsonConvert.DeserializeObject<JToken>(fileSystem.File.ReadAllText(MockCmfAuthFilePath));
        contents.SelectToken("$.repositories.nuget.credentials")?.As<JArray>()?.Count.Should().Be(1);
        contents.SelectToken("$.repositories.npm.credentials")?.As<JArray>()?.Count.Should().Be(1);

        contents.SelectToken("$.repositories.nuget.0.authType")?.Value<string>().Should().Be(AuthType.Basic.ToString());
        contents.SelectToken("$.repositories.nuget.0.repository")?.Value<string>().Should().Be("https://criticalmanufacturing.io/repository/nuget/index.json");
        contents.SelectToken("$.repositories.nuget.0.password")?.Value<string>().Should().Be("saferpassword");
        contents.SelectToken("$.repositories.npm.0.authType")?.Value<string>().Should().Be(AuthType.Bearer.ToString());
        contents.SelectToken("$.repositories.npm.0.repository")?.Value<string>().Should().Be("https://criticalmanufacturing.io/repository/npm/");
        contents.SelectToken("$.repositories.npm.0.token")?.Value<string>().Should().Be("header.payload.signature");
    }

    [Fact]
    public async Task RepositoryAuthStore_Store_ExistingFile()
    {
        // Arrange
        MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path(MockCmfAuthFilePath), new MockFileData(mockCmfAuthJson) }
        });

        ExecutionContext.ServiceProvider = ServiceCollection.BuildServiceProvider();

        var authStore = RepositoryAuthStore.FromEnvironmentConfig(fileSystem);

        // Act
        await authStore.Save([
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.NuGetRepositoryType,
                Key = "CMF",
                Repository = "https://criticalmanufacturing.io/repository/nuget/index.json",
                Username = "User@criticalmanufacturing.com",
                Password = "qwerty",
            },
            new BearerCredential
            {
                RepositoryType = CmfAuthConstants.NPMRepositoryType,
                Repository = "https://criticalmanufacturing.io/repository/npm/",
                Token = "header.payload.signature",
            },
        ]);

        // Assert
        NugetRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Once);
        NPMRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Once);

        DockerRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Never);
        CIFSRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Never);
        PortalRepositoryMock.Verify(x => x.SyncCredentials(It.IsAny<IList<ICredential>>()), Times.Never);

        fileSystem.FileExists(MockCmfAuthFilePath).Should().BeTrue();

        // Credentials must have been merged
        JToken contents = JsonConvert.DeserializeObject<JToken>(fileSystem.File.ReadAllText(MockCmfAuthFilePath));
        contents.SelectToken("$.repositories.nuget.credentials")?.As<JArray>()?.Count.Should().Be(2);
        contents.SelectToken("$.repositories.npm.credentials")?.As<JArray>()?.Count.Should().Be(1);

        contents.SelectToken("$.repositories.nuget.0.repository")?.Value<string>().Should().Be("https://criticalmanufacturing.io/repository/nuget/index.json");
        contents.SelectToken("$.repositories.npm.0.repository")?.Value<string>().Should().Be("https://criticalmanufacturing.io/repository/npm/");
    }

    [Theory]
    [InlineData(CmfAuthConstants.CIFSRepositoryType, @"\\serverA\folder1", 0)]
    [InlineData(CmfAuthConstants.CIFSRepositoryType, @"\\serverB\folder2\sub\folder", 1)]
    [InlineData(CmfAuthConstants.NPMRepositoryType, "https://feed.example", 2)]
    [InlineData(CmfAuthConstants.NPMRepositoryType, "https://env.feed.example/npm/", 3)]
    public void RepositoryAuthStore_GetCredentials(string repositoryType, string path, int expectedCredIndex)
    {
        // Arrange
        var credentialsList = new List<ICredential>
        {
            new BasicCredential { RepositoryType = CmfAuthConstants.CIFSRepositoryType, Repository = @"\\serverA\folder1" },
            new BasicCredential { RepositoryType = CmfAuthConstants.CIFSRepositoryType, Repository = @"\\serverB\folder2" },
            new BasicCredential { RepositoryType = CmfAuthConstants.NPMRepositoryType, Repository = "https://feed.example" },
            new BasicCredential { RepositoryType = CmfAuthConstants.NPMRepositoryType, Repository = "https://env.feed.example/npm/" },
        };

        var authFile = new CmfAuthFile
        {
            Repositories = credentialsList.GroupBy(cred => cred.RepositoryType).ToDictionary(group => group.Key, group => new CmfAuthFileRepositoryType { Credentials = group.ToList() })
        };

        MockFileSystem fileSystem = new MockFileSystem();

        ExecutionContext.ServiceProvider = ServiceCollection.BuildServiceProvider();

        var authStore = RepositoryAuthStore.FromEnvironmentConfig(fileSystem);

        // Act
        var credential = authStore.GetCredentialsFor(repositoryType, authFile, path, ignoreEnvVars: true);

        // Assert
        if (expectedCredIndex < 0)
        {
            // Scenarios where no credential was expected to be found
            credential.Should().BeNull();
        }
        else
        {
            // Validate we returned the expected credential
            credential.Should().NotBeNull();
            credentialsList.IndexOf(credential).Should().Be(expectedCredIndex);
        }
    }


    [Fact]
    public void PortalRepositoryCredentials_GetDerivedCredentials()
    {
        // Arrange
        var portal = new PortalRepositoryCredentials(new MockFileSystem());

        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            """
            {
                "clientId": "Applications",
                "tenantName": "CustomerPortal",
                "strategyId": "AzureActiveDirectory",
                "sub": "CMF\\UNAME",
                "scope": "",
                "iat": 1744019654,
                "exp": 1746438854,
                "aud": "AuthPortal",
                "iss": "AuthPortal"
            }
            """));

        // For this function, we only care about the structure of the token (having 2 dots, 3 parts)
        // and the value of the middle segment (payload). The values of the header and signature are ignored
        // so they can be whatever in the tests
        var token = $"header.{payload}.signature";

        // Act
        var credentials = portal.GetDerivedCredentials([
            new BearerCredential
            {
                Token = token,
                RepositoryType = CmfAuthConstants.PortalRepositoryType,
                Repository = CmfAuthConstants.PortalRepository,
            }
        ]).ToList();

        // Assert
        credentials.Should().NotBeNull();
        credentials.Should().BeEquivalentTo<ICredential>([
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.NuGetRepositoryType,
                Repository = CmfAuthConstants.NuGetRepository,
                Key = CmfAuthConstants.NuGetKey,
                Username = "CMF\\UNAME",
                Password = token,
            },
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.NPMRepositoryType,
                Repository = CmfAuthConstants.NPMRepository,
                Username = "CMF\\UNAME",
                Password = token,
            },
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.DockerRepositoryType,
                Repository = CmfAuthConstants.DockerRepository,
                Username = "CMF\\UNAME",
                Password = token,
            }
        ]);
    }

    [Fact]
    public async Task NPMRepositoryCredentials_SyncCredentials_NoFileExists()
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        var npm = new NPMRepositoryCredentials(fileSystem);

        // Act
        await npm.SyncCredentials([
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.NPMRepositoryType,
                Repository = CmfAuthConstants.NPMRepository,
                Username = "CMF\\UNAME",
                Password = "qwerty",
            },
            new BearerCredential
            {
                RepositoryType = CmfAuthConstants.NPMRepositoryType,
                Repository = "http://custom-registry.com/",
                Token = "A.B.C",
            }
        ]);

        // Assert
        fileSystem.FileExists(MockNPMConfigFilePath).Should().BeTrue();

        var ini = new INIFile();
        ini.Parse(fileSystem.File.ReadAllText(MockNPMConfigFilePath));

        var expectedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes("CMF\\UNAME:qwerty"));
        ini.GetValue("", "//criticalmanufacturing.io/repository/npm/:_auth").Should().Be(expectedAuth);
        ini.GetValue("", "//custom-registry.com/:_authToken").Should().Be("A.B.C");
    }

    [Fact]
    public async Task NPMRepositoryCredentials_SyncCredentials_ExistingFile()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path(MockNPMConfigFilePath), new MockFileData(
                """
                registry="https://criticalmanufacturing.io/repository/npm/"
                //registry.npmjs.org/:_auth="V2h5V291bGQ6WW91RG9UaGF0"
                //criticalmanufacturing.io/repository/npm/:_authToken="header.payload.signature"
                //criticalmanufacturing.io/repository/npm/:always_auth="true"
                """
            ) }
        });

        var npm = new NPMRepositoryCredentials(fileSystem);

        // Act
        await npm.SyncCredentials([
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.NPMRepositoryType,
                Repository = CmfAuthConstants.NPMRepository,
                Username = "CMF\\UNAME",
                Password = "qwerty",
            },
            new BearerCredential
            {
                RepositoryType = CmfAuthConstants.NPMRepositoryType,
                Repository = "http://custom-registry.com/",
                Token = "A.B.C",
            }
        ]);

        // Assert
        var expectedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes("CMF\\UNAME:qwerty"));

        fileSystem.FileExists(MockNPMConfigFilePath).Should().BeTrue();
        fileSystem.File.ReadAllText(MockNPMConfigFilePath).Should().BeEquivalentToIgnoringNewLines($"""
            registry="https://criticalmanufacturing.io/repository/npm/"
            //registry.npmjs.org/:_auth="V2h5V291bGQ6WW91RG9UaGF0"
            //custom-registry.com/:_authToken="A.B.C"
            //criticalmanufacturing.io/repository/npm/:always_auth="true"
            //criticalmanufacturing.io/repository/npm/:_auth="{expectedAuth}"
            //custom-registry.com/:always_auth="true"
            """
        );
    }

    [Fact]
    public async Task NuGetRepositoryCredentials_SyncCredentials_NoFileExists()
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        var nuget = new NuGetRepositoryCredentials(fileSystem);

        // Act
        await nuget.SyncCredentials([
           new BasicCredential
           {
                RepositoryType = CmfAuthConstants.NuGetRepositoryType,
                Repository = CmfAuthConstants.NuGetRepository,
                Key = CmfAuthConstants.NuGetKey,
                Username = "CMF\\UNAME",
                Password = "qwerty",
           }
        ]);

        // Assert
        fileSystem.FileExists(MockNuGetConfigFilePath).Should().BeTrue();

        var xml = XDocument.Load(fileSystem.File.OpenRead(MockNuGetConfigFilePath));

        string repository = xml.XPathSelectElement($"//configuration/packageSources/add[@key=\"{CmfAuthConstants.NuGetKey}\"]")?.Attribute("value")?.Value;
        string username = xml.XPathSelectElement($"//configuration/packageSourceCredentials/{CmfAuthConstants.NuGetKey}/add[@key=\"Username\"]")?.Attribute("value")?.Value;
        string password = xml.XPathSelectElement($"//configuration/packageSourceCredentials/{CmfAuthConstants.NuGetKey}/add[@key=\"ClearTextPassword\"]")?.Attribute("value")?.Value;

        repository.Should().Be(CmfAuthConstants.NuGetRepository);
        username.Should().Be("CMF\\UNAME");
        password.Should().Be("qwerty");
    }

    [Fact]
    public async Task NuGetRepositoryCredentials_SyncCredentials_ExistingFile()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path(MockNuGetConfigFilePath), new MockFileData(
                """
                <?xml version="1.0" encoding="utf-8"?>
                <configuration>
                  <packageSources>
                    <add key="Microsoft Visual Studio Offline Packages" value="C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\" />
                  </packageSources>
                  <packageSourceCredentials>
                    <CMF>
                      <add key="Username" value="CMF\UNAME" />
                      <add key="ClearTextPassword" value="oldpassword" />
                    </CMF>
                  </packageSourceCredentials>
                </configuration>
                """
            ) }
        });

        var nuget = new NuGetRepositoryCredentials(fileSystem);

        // Act
        await nuget.SyncCredentials([
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.NPMRepositoryType,
                Repository = CmfAuthConstants.NPMRepository,
                Key = CmfAuthConstants.NuGetKey,
                Username = "CMF\\UNAME",
                Password = "qwerty",
            },
            new BasicCredential
            {
                RepositoryType = CmfAuthConstants.NPMRepositoryType,
                Repository = "https://api.nuget.org/v3/index.json",
                Key = "NuGet",
                Username = "uname",
                Password = "personalpassword",
            }
        ]);

        // Assert
        var expectedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes("CMF\\UNAME:qwerty"));

        fileSystem.FileExists(MockNuGetConfigFilePath).Should().BeTrue();
        fileSystem.File.ReadAllText(MockNuGetConfigFilePath).Should().BeEquivalentToIgnoringNewLines($"""
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <add key="Microsoft Visual Studio Offline Packages" value="C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\" />
                <add key="CMF" value="https://criticalmanufacturing.io/repository/npm/" protocolVersion="3" />
                <add key="NuGet" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
              </packageSources>
              <packageSourceCredentials>
                <CMF>
                  <add key="Username" value="CMF\UNAME" />
                  <add key="ClearTextPassword" value="qwerty" />
                </CMF>
                <NuGet>
                  <add key="Username" value="uname" />
                  <add key="ClearTextPassword" value="personalpassword" />
                </NuGet>
              </packageSourceCredentials>
            </configuration>
            """
        );
    }
}
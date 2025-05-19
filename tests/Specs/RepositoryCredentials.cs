using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
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
    public static string MockCmfAuthFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cmf-auth.json");
    public static string MockNPMConfigFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".npmrc");
    public static string MockNuGetConfigFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NuGet", "NuGet.Config");
    public static string MockPortalTokenFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "cmfportal", "cmfportaltoken");

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
        PortalRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.Portal);
        PortalRepositoryMock.Setup(x => x.SupportedAuthTypes).Returns([AuthType.Bearer]);
        NPMRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NPM);
        NPMRepositoryMock.Setup(x => x.SupportedAuthTypes).Returns([AuthType.Basic, AuthType.Bearer]);
        NugetRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.NuGet);
        NugetRepositoryMock.Setup(x => x.SupportedAuthTypes).Returns([AuthType.Basic]);
        DockerRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.Docker);
        DockerRepositoryMock.Setup(x => x.SupportedAuthTypes).Returns([AuthType.Basic]);
        CIFSRepositoryMock.Setup(x => x.RepositoryType).Returns(RepositoryCredentialsType.CIFS);
        CIFSRepositoryMock.Setup(x => x.SupportedAuthTypes).Returns([AuthType.Basic]);

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
        foreach (var (repoType, credsCount) in new (RepositoryCredentialsType, int)[] { (RepositoryCredentialsType.NPM, 0), (RepositoryCredentialsType.NuGet, 2), (RepositoryCredentialsType.CIFS, 0), (RepositoryCredentialsType.Docker, 0) })
        {
            authFile.Repositories.ContainsKey(repoType).Should().BeTrue();
            authFile.Repositories[repoType].Should().NotBeNull();
            authFile.Repositories[repoType].Credentials.Should().NotBeNull();
            authFile.Repositories[repoType].Credentials.Count.Should().Be(credsCount);
        }

        // Also, validate that the credentials inside the NuGet repository are all correct
        var credentials = authFile.Repositories[RepositoryCredentialsType.NuGet].Credentials;
        credentials.Should().BeEquivalentTo(new ICredential[]
        {
            new BasicCredential
            {
                RepositoryType = RepositoryCredentialsType.NuGet,
                Key = "CMF",
                Repository = "https://criticalmanufacturing.io/repository/nuget/index.json",
                Username = "User@criticalmanufacturing.com",
                Password = "saferpassword",
            },
            new BearerCredential
            {
                RepositoryType = RepositoryCredentialsType.NuGet,
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
                RepositoryType = RepositoryCredentialsType.NuGet,
                Key = "CMF",
                Repository = "https://criticalmanufacturing.io/repository/nuget/index.json",
                Username = "User@criticalmanufacturing.com",
                Password = "saferpassword",
            },
            new BearerCredential
            {
                RepositoryType = RepositoryCredentialsType.NPM,
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
                RepositoryType = RepositoryCredentialsType.NuGet,
                Key = "CMF",
                Repository = "https://criticalmanufacturing.io/repository/nuget/index.json",
                Username = "User@criticalmanufacturing.com",
                Password = "qwerty",
            },
            new BearerCredential
            {
                RepositoryType = RepositoryCredentialsType.NPM,
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
    [InlineData(RepositoryCredentialsType.CIFS, @"\\serverA\folder1", 0)]
    [InlineData(RepositoryCredentialsType.CIFS, @"\\serverB\folder2\sub\folder", 1)]
    [InlineData(RepositoryCredentialsType.NPM, "https://feed.example", 2)]
    [InlineData(RepositoryCredentialsType.NPM, "https://env.feed.example/npm/", 3)]
    public void RepositoryAuthStore_GetCredentials(RepositoryCredentialsType repositoryType, string path, int expectedCredIndex)
    {
        // Arrange
        var credentialsList = new List<ICredential>
        {
            new BasicCredential { RepositoryType = RepositoryCredentialsType.CIFS, Repository = @"\\serverA\folder1" },
            new BasicCredential { RepositoryType = RepositoryCredentialsType.CIFS, Repository = @"\\serverB\folder2" },
            new BasicCredential { RepositoryType = RepositoryCredentialsType.NPM, Repository = "https://feed.example" },
            new BasicCredential { RepositoryType = RepositoryCredentialsType.NPM, Repository = "https://env.feed.example/npm/" },
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
    public void RepositoryAuthStore_GetBearerEnvVarCredentials()
    {
        MockEnvironment environment = null;

        try
        {
            // Arrange
            environment = new MockEnvironment(new Dictionary<string, string>()
            {
                { "NPM__REPOURL__AUTH_TYPE", "bearer" },
                { "NPM__REPOURL__TOKEN", "a.b.c" },
            });

            NPMRepositoryMock.Setup(x => x.GetEnvironmentVariablePrefix(It.IsAny<string>())).Returns("NPM__REPOURL");

            ExecutionContext.ServiceProvider = ServiceCollection.BuildServiceProvider();

            var authStore = RepositoryAuthStore.FromEnvironmentConfig(new MockFileSystem());

            // Act
            var credential = authStore.GetCredentialsFor(RepositoryCredentialsType.NPM, new CmfAuthFile(), CmfAuthConstants.NPMRepository);

            // Assert
            credential.Should().BeEquivalentTo(new BearerCredential
            {
                RepositoryType = RepositoryCredentialsType.NPM,
                Repository = CmfAuthConstants.NPMRepository,
                Token = "a.b.c"
            });
        }
        finally
        {
            environment.Restore();
        }
    }

    [Fact]
    public void RepositoryAuthStore_GetBasicEnvVarCredentials()
    {
        MockEnvironment environment = null;

        try
        {
            // Arrange
            environment = new MockEnvironment(new Dictionary<string, string>()
            {
                { "CIFS__REPOURL__AUTH_TYPE", "BASIC" },
                { "CIFS__REPOURL__DOMAIN", "CMF" },
                { "CIFS__REPOURL__USERNAME", "user" },
                { "CIFS__REPOURL__PASSWORD", "querty" },
            });

            CIFSRepositoryMock.Setup(x => x.GetEnvironmentVariablePrefix(It.IsAny<string>())).Returns("CIFS__REPOURL");

            ExecutionContext.ServiceProvider = ServiceCollection.BuildServiceProvider();

            var authStore = RepositoryAuthStore.FromEnvironmentConfig(new MockFileSystem());

            // Act
            var credential = authStore.GetCredentialsFor(RepositoryCredentialsType.CIFS, new CmfAuthFile(), @"\\share.com\packages\CI\");

            // Assert
            credential.Should().BeEquivalentTo(new BasicCredential
            {
                RepositoryType = RepositoryCredentialsType.CIFS,
                Repository = @"\\share.com\packages\CI\",
                Domain = "CMF",
                Username = "user",
                Password = "querty"
            });
        }
        finally
        {
            environment.Restore();
        }
    }

    [Fact]
    public void RepositoryAuthStore_GetEnvVarCredentials_WrongAuthType()
    {
        MockEnvironment environment = null;

        try
        {
            // Arrange
            environment = new MockEnvironment(new Dictionary<string, string>()
            {
                { "DOCKER__REPOURL__AUTH_TYPE", "bearer" },
                { "DOCKER__REPOURL__TOKEN", "a.b.c" },
            });

            DockerRepositoryMock.Setup(x => x.GetEnvironmentVariablePrefix(It.IsAny<string>())).Returns("DOCKER__REPOURL");

            ExecutionContext.ServiceProvider = ServiceCollection.BuildServiceProvider();

            var authStore = RepositoryAuthStore.FromEnvironmentConfig(new MockFileSystem());

            // Act
            var exception = authStore.Invoking(x => x.GetCredentialsFor(RepositoryCredentialsType.Docker, new CmfAuthFile(), "registry.docker.io"));

            // Assert
            exception.Should().Throw<Exception>().WithMessage("Invalid auth type*");
        }
        finally
        {
            environment.Restore();
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
                RepositoryType = RepositoryCredentialsType.Portal,
                Repository = CmfAuthConstants.PortalRepository,
            }
        ]).ToList();

        // Assert
        credentials.Should().NotBeNull();
        credentials.Should().BeEquivalentTo<ICredential>([
            new BasicCredential
            {
                RepositoryType = RepositoryCredentialsType.NuGet,
                Repository = CmfAuthConstants.NuGetRepository,
                Key = CmfAuthConstants.NuGetKey,
                Username = "CMF\\UNAME",
                Password = token,
            },
            new BasicCredential
            {
                RepositoryType = RepositoryCredentialsType.NPM,
                Repository = CmfAuthConstants.NPMRepository,
                Username = "CMF\\UNAME",
                Password = token,
            },
            new BasicCredential
            {
                RepositoryType = RepositoryCredentialsType.Docker,
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
                RepositoryType = RepositoryCredentialsType.NPM,
                Repository = CmfAuthConstants.NPMRepository,
                Username = "CMF\\UNAME",
                Password = "qwerty",
            },
            new BearerCredential
            {
                RepositoryType = RepositoryCredentialsType.NPM,
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
                RepositoryType = RepositoryCredentialsType.NPM,
                Repository = CmfAuthConstants.NPMRepository,
                Username = "CMF\\UNAME",
                Password = "qwerty",
            },
            new BearerCredential
            {
                RepositoryType = RepositoryCredentialsType.NPM,
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

    [Theory]
    [InlineData("http://registry.npmjs.org/", "npm__registry_npmjs_org")]
    [InlineData("http://custom.registry.com/npm-group/", "npm__custom_registry_com_npm_group")]
    public void NPMRepositoryCredentials_GetEnvironmentVariablePrefix(string repository, string envVarPrefix)
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        var npm = new NPMRepositoryCredentials(fileSystem);

        // Act
        var prefix = npm.GetEnvironmentVariablePrefix(repository);

        // Assert
        prefix.Should().Be(envVarPrefix);
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
                RepositoryType = RepositoryCredentialsType.NuGet,
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
                RepositoryType = RepositoryCredentialsType.NPM,
                Repository = CmfAuthConstants.NPMRepository,
                Key = CmfAuthConstants.NuGetKey,
                Username = "CMF\\UNAME",
                Password = "qwerty",
            },
            new BasicCredential
            {
                RepositoryType = RepositoryCredentialsType.NPM,
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

    [Theory]
    [InlineData("https://api.nuget.org/v3/index.json", "nuget__api_nuget_org_v3_index_json")]
    [InlineData("https://custom.io/repository/nuget/index.json", "nuget__custom_io_repository_nuget_index_json")]
    public void NuGetRepositoryCredentials_GetEnvironmentVariablePrefix(string repository, string envVarPrefix)
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        var nuget = new NuGetRepositoryCredentials(fileSystem);

        // Act
        var prefix = nuget.GetEnvironmentVariablePrefix(repository);

        // Assert
        prefix.Should().Be(envVarPrefix);
    }

    [Theory]
    [InlineData(@"\\server.com", "cifs__server_com")]
    [InlineData(@"\\server.com\share", "cifs__server_com_share")]
    [InlineData(@"\\server.com\share\sub\folder\path", "cifs__server_com_share")]
    public void CIFSRepositoryCredentials_GetEnvironmentVariablePrefix(string repository, string envVarPrefix)
    {
        // Arrange
        var cifs = new CIFSRepositoryCredentials();

        // Act
        var prefix = cifs.GetEnvironmentVariablePrefix(repository);

        // Assert
        prefix.Should().Be(envVarPrefix);
    }

    [Fact]
    public async Task PortalRepositoryCredentials_SyncCredentials_NoFileExists()
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        var portal = new PortalRepositoryCredentials(fileSystem);

        // Act
        await portal.SyncCredentials([
           new BearerCredential
           {
                RepositoryType = RepositoryCredentialsType.Portal,
                Repository = CmfAuthConstants.PortalRepository,
                Token = "a.b.c"
           }
        ]);

        // Assert
        fileSystem.FileExists(MockPortalTokenFilePath).Should().BeTrue();

        var token = fileSystem.File.ReadAllText(MockPortalTokenFilePath);

        token.Trim().Should().Be("a.b.c");
    }

    [Fact]
    public async Task PortalRepositoryCredentials_SyncCredentials_ExistingFile()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path(MockPortalTokenFilePath), new MockFileData(
                """
                d.e.f
                """
            ) }
        });

        var portal = new PortalRepositoryCredentials(fileSystem);

        // Act
        await portal.SyncCredentials([
            new BearerCredential
            {
                RepositoryType = RepositoryCredentialsType.Portal,
                Repository = CmfAuthConstants.PortalRepository,
                Token = "a.b.c"
            }
        ]);

        // Assert
        fileSystem.FileExists(MockPortalTokenFilePath).Should().BeTrue();

        var token = fileSystem.File.ReadAllText(MockPortalTokenFilePath);

        token.Trim().Should().Be("a.b.c");
    }
}
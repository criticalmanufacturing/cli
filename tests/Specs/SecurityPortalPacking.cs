using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Handlers;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using PackageHandler = Cmf.CLI.Handlers.PackageTypeHandler;

namespace tests.Specs;

public class SecurityPortalPacking
{
    internal const string Configuration = """
        {
          "@jdt.merge": [
            {
              "@jdt.path": "$.tenants.config.SMT.strategies",
              "@jdt.value": [
                {
                  "id": "Cmf.Custom.SecurityPortal",
                  "type": "open-id-connect",
                  "config": {
                    "metadataUrl": "https://{{SSOApplicationServerURL}}/.well-known/openid-configuration",
                    "redirectUrl": "https://{{SecurityPortalBaseURL}}/tenant/SMT/login/callback",
                    "clientId": "{{SSOApplicationGeneratedClientId}}",
                    "scope": "openid profile email",
                    "customOption": "{{CustomToken}}"
                  }
                }
              ]
            }
          ]
        }
        """;

    public SecurityPortalPacking()
    {
        ExecutionContext.ServiceProvider = new ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>()
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(99)]
    public void LegacyVersions_KeepGeneratingTheV2Transformation(int? handlerVersion)
    {
        const string strategy = """
            {
              "id": "Cmf.Custom.SecurityPortal",
              "type": "open-id-connect",
              "config": {
                "metadataUrl": "{{MetadataUrl}}",
                "redirectUrl": "{{RedirectUrl}}",
                "clientId": "{{ClientId}}"
              }
            }
            """;
        var (fileSystem, handler) = CreateHandler(handlerVersion, configuration: strategy);
        handler.Should().BeOfType<SecurityPortalPackageTypeHandlerV2>();

        handler.Pack(fileSystem.DirectoryInfo.New("/repo/packed"), fileSystem.DirectoryInfo.New("/repo/output"));

        using var archive = new ZipArchive(fileSystem.File.OpenRead($"/repo/output/{handler.CmfPackage.ZipPackageName}"));
        using var reader = new StreamReader(archive.GetEntry("config.json").Open());
        var config = reader.ReadToEnd();
        config.Should().Contain("$.tenants.config.SMT.strategies")
            .And.Contain("{{MetadataUrl}}")
            .And.Contain("{{RedirectUrl}}")
            .And.Contain("{{ClientId}}");
        fileSystem.File.ReadAllText("/repo/config.json").Should().Be(strategy);
    }

    [Theory]
    [InlineData("10.2.0", "./src/", true)]
    [InlineData("11.2.5", "./dist/", true)]
    [InlineData("12.0.0", "./dist/", true)]
    [InlineData("10.2.0", "./src/", false)]
    [InlineData("11.2.5", "./dist/", false)]
    [InlineData("12.0.0", "./dist/", false)]
    public void V3_PreservesTheCompleteTransformation(string mesVersion, string configDirectory, bool omitTarget)
    {
        var (fileSystem, handler) = CreateHandler(3, mesVersion, omitTarget);
        handler.Should().BeOfType<SecurityPortalPackageTypeHandlerV3>();
        var source = fileSystem.File.ReadAllBytes("/repo/config.json");

        handler.Pack(fileSystem.DirectoryInfo.New("/repo/packed"), fileSystem.DirectoryInfo.New("/repo/output"));

        using var archive = new ZipArchive(fileSystem.File.OpenRead($"/repo/output/{handler.CmfPackage.ZipPackageName}"));
        using var packedConfig = new MemoryStream();
        using (var stream = archive.GetEntry("config.json").Open())
        {
            stream.CopyTo(packedConfig);
        }
        packedConfig.ToArray().Should().Equal(source);
        using var reader = new StreamReader(archive.GetEntry("manifest.xml").Open());
        var manifest = XDocument.Parse(reader.ReadToEnd());
        var step = manifest.Descendants("step").Should().ContainSingle().Subject;
        ((string)step.Attribute("type")).Should().Be("TransformFile");
        ((string)step.Attribute("file")).Should().Be("config.json");
        ((string)step.Attribute("tagFile")).Should().Be("true");
        ((string)step.Attribute("relativePath")).Should().Be(configDirectory);
        fileSystem.File.ReadAllBytes("/repo/config.json").Should().Equal(source);
    }

    [Fact]
    public void V3_DryRun_DoesNotCreateFilesOrDirectories()
    {
        var (fileSystem, handler) = CreateHandler(3);
        var before = fileSystem.AllFiles.ToDictionary(path => path, path => fileSystem.File.ReadAllBytes(path));
        var directories = fileSystem.AllDirectories.ToArray();

        handler.Pack(fileSystem.DirectoryInfo.New("/repo/dry-run"), fileSystem.DirectoryInfo.New("/repo/dry-run-output"), dryRun: true);

        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        fileSystem.AllDirectories.Should().BeEquivalentTo(directories);
        foreach (var entry in before)
        {
            fileSystem.File.ReadAllBytes(entry.Key).Should().Equal(entry.Value);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void V3_MissingConfig_ReportsAnError(bool dryRun)
    {
        var (fileSystem, handler) = CreateHandler(3);
        fileSystem.File.Delete("/repo/config.json");

        var pack = () => handler.Pack(fileSystem.DirectoryInfo.New("/repo/packed"), fileSystem.DirectoryInfo.New("/repo/output"), dryRun);

        pack.Should().Throw<CliException>().WithMessage("No config.json was provided");
        fileSystem.Directory.GetFiles("/repo/output").Should().BeEmpty();
    }

    private static (MockFileSystem FileSystem, PackageHandler Handler) CreateHandler(
        int? handlerVersion, string mesVersion = "11.2.5", bool omitTarget = true, string configuration = Configuration)
    {
        var version = handlerVersion.HasValue ? $"\"handlerVersion\": {handlerVersion.Value}," : string.Empty;
        var target = omitTarget ? string.Empty : ", \"target\": \"\"";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/repo/.project-config.json"] = new MockFileData($$"""
                { "MESVersion": "{{mesVersion}}", "Tenant": "SMT", "RepositoryType": "Customization" }
                """),
            ["/repo/cmfpackage.json"] = new MockFileData($$"""
                {
                  "packageId": "Cmf.Custom.SecurityPortal",
                  "version": "1.0.0",
                  "packageType": "SecurityPortal",
                  {{version}}
                  "isInstallable": true,
                  "contentToPack": [{ "source": "config.json"{{target}} }]
                }
                """),
            ["/repo/config.json"] = new MockFileData(Encoding.UTF8.GetBytes(configuration))
        }, "/repo");
        fileSystem.Directory.CreateDirectory("/repo/packed");
        fileSystem.Directory.CreateDirectory("/repo/output");
        ExecutionContext.Initialize(fileSystem);
        var handler = (PackageHandler)PackageTypeFactory.GetPackageTypeHandler(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
        return (fileSystem, handler);
    }
}

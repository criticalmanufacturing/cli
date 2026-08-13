using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Services;
using FluentAssertions;
using tests.Objects;
using Xunit;

namespace tests.Specs;

public class CmfPackageController_FromTgz
{
    [Fact]
    public void Constructor_ShouldReadXmlManifest_FromPackageFolderInTgz()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.Directory.CreateDirectory("/repo");
        var packageFile = fileSystem.FileInfo.New("/repo/package.tgz");
        var manifestXml = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.2.3</version>
            </deploymentPackage>
            """;

        var archiveBytes = new DFTGZPackageBuilder()
            .CreateEntry($"package/{CoreConstants.DeploymentFrameworkManifestFileName}", manifestXml)
            .ToByteArray();

        using (var stream = packageFile.Create())
        {
            stream.Write(archiveBytes, 0, archiveBytes.Length);
            stream.Flush();
        }

        var controller = new CmfPackageController(packageFile, fileSystem);

        controller.CmfPackage.Should().NotBeNull();
        controller.CmfPackage.PackageId.Should().Be("Cmf.Custom.Data");
        controller.CmfPackage.Version.Should().Be("1.2.3");
    }

    [Fact]
    public void Constructor_ShouldReadJsonManifest_FromPackageFolderInTgz()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.Directory.CreateDirectory("/repo");
        var packageFile = fileSystem.FileInfo.New("/repo/package.tgz");
        var manifestJson = """
            {
              "name": "Cmf.Custom.Data",
              "version": "1.2.3",
              "packageName": "Cmf.Custom.Data",
              "description": "Sample package",
              "packageType": "Generic",
              "keywords": ["cmf-deployment-package"],
              "deployment": {
                "packageId": "Cmf.Custom.Data",
                "version": "1.2.3"
              }
            }
            """;

        var archiveBytes = new DFTGZPackageBuilder()
            .CreateEntry($"package/{CoreConstants.PackageJson}", manifestJson)
            .ToByteArray();

        using (var stream = packageFile.Create())
        {
            stream.Write(archiveBytes, 0, archiveBytes.Length);
            stream.Flush();
        }

        var controller = new CmfPackageController(packageFile, fileSystem);

        controller.CmfPackage.Should().NotBeNull();
        controller.CmfPackage.PackageId.Should().Be("Cmf.Custom.Data");
        controller.CmfPackage.Version.Should().Be("1.2.3");
    }

    [Fact]
    public void Constructor_ShouldReadXmlManifest_FromRootOfTgz()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.Directory.CreateDirectory("/repo");
        var packageFile = fileSystem.FileInfo.New("/repo/package.tgz");
        var manifestXml = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <deploymentPackage>
              <packageId>Cmf.Custom.Root</packageId>
              <version>2.0.0</version>
            </deploymentPackage>
            """;

        var archiveBytes = new DFTGZPackageBuilder()
            .CreateEntry(CoreConstants.DeploymentFrameworkManifestFileName, manifestXml)
            .ToByteArray();

        using (var stream = packageFile.Create())
        {
            stream.Write(archiveBytes, 0, archiveBytes.Length);
            stream.Flush();
        }

        var controller = new CmfPackageController(packageFile, fileSystem);

        controller.CmfPackage.Should().NotBeNull();
        controller.CmfPackage.PackageId.Should().Be("Cmf.Custom.Root");
        controller.CmfPackage.Version.Should().Be("2.0.0");
    }

    [Fact]
    public void Constructor_ShouldReadJsonManifest_WithoutSteps_FromRootOfTgz()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.Directory.CreateDirectory("/repo");
        var packageFile = fileSystem.FileInfo.New("/repo/package.tgz");
        var manifestJson = """
            {
              "name": "Cmf.Custom.Root",
              "version": "2.0.0",
              "packageName": "Cmf.Custom.Root",
              "description": "Sample package without steps",
              "keywords": ["cmf-deployment-package"],
              "deployment": {
                "packageType": "Generic",
                "packageId": "Cmf.Custom.Root",
                "version": "2.0.0"
              }
            }
            """;

        var archiveBytes = new DFTGZPackageBuilder()
            .CreateEntry(CoreConstants.PackageJson, manifestJson)
            .ToByteArray();

        using (var stream = packageFile.Create())
        {
            stream.Write(archiveBytes, 0, archiveBytes.Length);
            stream.Flush();
        }

        var controller = new CmfPackageController(packageFile, fileSystem);

        controller.CmfPackage.Should().NotBeNull();
        controller.CmfPackage.PackageId.Should().Be("Cmf.Custom.Root");
        controller.CmfPackage.Version.Should().Be("2.0.0");
    }
}

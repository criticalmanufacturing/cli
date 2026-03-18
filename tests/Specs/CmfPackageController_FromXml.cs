using System.Linq;
using System.Xml.Linq;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Services;
using FluentAssertions;
using Xunit;

namespace tests.Specs;

public class CmfPackageController_FromXml
{
    [Fact]
    public void FromXml_ShouldParseStepMessageType_FromManifestAttribute()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="DeployFiles" contentPath="*.example" messageType="ImportObject" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps!.Single().MessageType.Should().Be(MessageType.ImportObject);
    }

    [Fact]
    public void FromXml_ShouldKeepStepMessageTypeNull_WhenAttributeIsMissing()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="DeployFiles" contentPath="*.example" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps!.Single().MessageType.Should().BeNull();
    }

    [Fact]
    public void FromXml_ShouldParseDeeBasePathAndImportXmlObjectPath_FromManifestAttributes()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="MasterData" contentPath="*.xlsx" deeBasePath="./dees" importXMLObjectPath="./xml" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps!.Single().DeeBasePath.Should().Be("./dees");
        pkg.Steps!.Single().ImportXMLObjectPath.Should().Be("./xml");
    }
}

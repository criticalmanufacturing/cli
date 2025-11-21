using Cmf.CLI.Constants;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Objects;
using Xunit;
using Cmf.CLI.Utilities;
using Cmf.CLI.Core.Services;
using System.Xml.Linq;
using System.ComponentModel;

namespace tests.Specs
{
    public class CmfPackageController_FromXml
    {
        [Fact]
        [Description("Default FromXml call removes default dependencies.")]
        public void FromXml_RemoveDefaultDependencies_ByDefault()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> defaultDep1 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> otherDep1 = new("Cmf.Custom.Package.FeatureA", "0.0.1");
            const string cliPackageType = "Root";

            var fileSystemManifestMockFileData = new MockFileData(
                @$"<deploymentPackage>
                      <packageId>{packageRoot.Key}</packageId>
                      <name>Critical Manufacturing Customization</name>
                      <packageType>Generic</packageType>
                      <cliPackageType>{cliPackageType}</cliPackageType>
                      <description>This package deploys Critical Manufacturing Customization</description>
                      <version>{packageRoot.Value}</version>
                      <isInstallable>True</isInstallable>
                      <isUniqueInstall>False</isUniqueInstall>
                      <keywords>cmf-root-package</keywords>
                      <dependencies>
                        <dependency id=""{defaultDep1.Key}"" version=""{defaultDep1.Value}"" mandatory=""false"" isIgnorable=""true"" />
                        <dependency id=""{otherDep1.Key}"" version=""{otherDep1.Value}"" mandatory=""false"" isIgnorable=""true"" />
                      </dependencies>
                    </deploymentPackage>");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/manifest.xml", fileSystemManifestMockFileData }
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo manifestFile = fileSystem.FileInfo.New($"repo/{CliConstants.DeploymentFrameworkManifestFileName}");

            var xml = XDocument.Parse(manifestFile.ReadToString());
            CmfPackageV1 cmfPackage = CmfPackageController.FromXml(xml);
            
            string json = new CmfPackageController(cmfPackage, fileSystem).ToJson();

            Assert.NotNull(cmfPackage);
            Assert.Equal(cliPackageType, cmfPackage.PackageType.ToString());
            
            Assert.Single (cmfPackage.Dependencies);
            Assert.Equal(otherDep1.Key, cmfPackage.Dependencies[0].Id);
            Assert.Equal(otherDep1.Value, cmfPackage.Dependencies[0].Version);
            Assert.True(cmfPackage.Dependencies[0].IsMissing);
        }
        
        [Fact]
        [Description("FromXml method is able to keep the default dependencies (required for ConvertZipToTarGz).")]
        public void FromXml_KeepsDefaultDependencies_WhenSpecified()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> defaultDep1 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
            KeyValuePair<string, string> otherDep1 = new("Cmf.Custom.Package.FeatureA", "0.0.1");
            const string cliPackageType = "Root";

            var fileSystemManifestMockFileData = new MockFileData(
                @$"<deploymentPackage>
                      <packageId>{packageRoot.Key}</packageId>
                      <name>Critical Manufacturing Customization</name>
                      <packageType>Generic</packageType>
                      <cliPackageType>{cliPackageType}</cliPackageType>
                      <description>This package deploys Critical Manufacturing Customization</description>
                      <version>{packageRoot.Value}</version>
                      <isInstallable>True</isInstallable>
                      <isUniqueInstall>False</isUniqueInstall>
                      <keywords>cmf-root-package</keywords>
                      <dependencies>
                        <dependency id=""{defaultDep1.Key}"" version=""{defaultDep1.Value}"" mandatory=""false"" isIgnorable=""true"" />
                        <dependency id=""{otherDep1.Key}"" version=""{otherDep1.Value}"" mandatory=""false"" isIgnorable=""true"" />
                      </dependencies>
                    </deploymentPackage>");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/manifest.xml", fileSystemManifestMockFileData }
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo manifestFile = fileSystem.FileInfo.New($"repo/{CliConstants.DeploymentFrameworkManifestFileName}");

            var xml = XDocument.Parse(manifestFile.ReadToString());
            CmfPackageV1 cmfPackage = CmfPackageController.FromXml(xml, keepDefaultDependencies: true);
            
            Assert.NotNull(cmfPackage);
            Assert.Equal(cliPackageType, cmfPackage.PackageType.ToString());
            Assert.Equal(packageRoot.Value, cmfPackage.Version);

            Assert.Equal (2, cmfPackage.Dependencies.Count);
            Assert.Equal(defaultDep1.Key, cmfPackage.Dependencies[0].Id);
            Assert.Equal(defaultDep1.Value, cmfPackage.Dependencies[0].Version);
            Assert.True(cmfPackage.Dependencies[0].IsMissing);
        }
    }
}

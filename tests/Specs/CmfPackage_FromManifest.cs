using Cmf.CLI.Constants;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Objects;
using Xunit;
using Cmf.CLI.Utilities;

namespace tests.Specs
{
    public class CmfPackage_FromManifest
    {
        [Fact]
        public void Root_HappyPath()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageDep1 = new("CriticalManufacturing.DeploymentMetadata", "8.3.0");
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
                        <dependency id=""{packageDep1.Key}"" version=""{packageDep1.Value}"" mandatory=""false"" isIgnorable=""true"" />
                      </dependencies>
                    </deploymentPackage>");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/manifest.xml", fileSystemManifestMockFileData }
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo manifestFile = fileSystem.FileInfo.New($"repo/{CliConstants.DeploymentFrameworkManifestFileName}");

            string message = string.Empty;
            CmfPackage cmfPackage = null;
            try
            {
                // Reading cmfPackage
                cmfPackage = CmfPackage.FromManifest(fileSystemManifestMockFileData.TextContents, setDefaultValues: true);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            Assert.Equal(string.Empty, message);
            Assert.NotNull(cmfPackage);
            Assert.Equal(cliPackageType, cmfPackage.PackageType.ToString());
            Assert.Equal(packageRoot.Value, cmfPackage.Version);
            Assert.Equal(packageDep1.Value, cmfPackage.Dependencies[0].Version);
            Assert.True(cmfPackage.Dependencies[0].IsMissing);
        }
    }
}

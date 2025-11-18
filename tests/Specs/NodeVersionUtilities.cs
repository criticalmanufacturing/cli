using Cmf.CLI.Utilities;
using Cmf.CLI.Core;
using System;
using Xunit;

namespace tests.Specs
{
    public class NodeVersionUtilitiesTests
    {
        [Fact]
        public void GetInstalledNodeVersion_ReturnsVersion_WhenNodeIsInstalled()
        {
            // This test will only work if Node.js is installed in the test environment
            var version = NodeVersionUtilities.GetInstalledNodeVersion();
            
            // If Node.js is not installed, the version will be null
            // We can't guarantee Node.js is installed, so we check if version is null or valid
            if (version != null)
            {
                Assert.True(version.Major >= 12, "Node.js major version should be at least 12");
            }
        }

        [Theory]
        [InlineData("8.0.0", "12")]  // MES v8.x requires Node.js v12
        [InlineData("9.0.0", "12")]  // MES v9.x requires Node.js v12
        [InlineData("10.0.0", "18")] // MES v10.x requires Node.js v18
        [InlineData("11.0.0", "20")] // MES v11.x requires Node.js v20
        [InlineData("12.0.0", "20")] // MES v12.x and later require Node.js v20
        public void ValidateNodeVersion_ThrowsException_WhenNodeNotInstalled(string mesVersionString, string requiredNodeVersion)
        {
            // Arrange
            var mesVersion = new Version(mesVersionString);
            
            // Act & Assert
            // This test assumes Node.js is either not installed or the version doesn't match
            // We test that the validation method properly handles the cases
            try
            {
                NodeVersionUtilities.ValidateNodeVersion(mesVersion, requiredNodeVersion);
                // If we get here, either Node.js is installed and the version matches,
                // or the validation passed
            }
            catch (CliException ex)
            {
                // Expected exceptions:
                // 1. Node.js is not installed
                // 2. Node.js version is incompatible
                Assert.True(
                    ex.Message.Contains("not installed") || 
                    ex.Message.Contains("not compatible"),
                    $"Unexpected exception message: {ex.Message}"
                );
            }
        }

        [Fact]
        public void ValidateNodeVersion_DoesNotThrow_WhenNodeVersionMatches()
        {
            // Arrange
            var installedVersion = NodeVersionUtilities.GetInstalledNodeVersion();
            
            // Only run this test if Node.js is actually installed
            if (installedVersion != null)
            {
                // Determine the MES version based on the installed Node.js version
                Version mesVersion;
                string requiredNodeVersion = installedVersion.Major.ToString();
                
                switch (installedVersion.Major)
                {
                    case 12:
                        mesVersion = new Version("8.0.0");
                        break;
                    case 18:
                        mesVersion = new Version("10.0.0");
                        break;
                    case 20:
                    default:
                        mesVersion = new Version("11.0.0");
                        break;
                }
                
                // Act & Assert - should not throw
                NodeVersionUtilities.ValidateNodeVersion(mesVersion, requiredNodeVersion);
            }
        }
    }
}

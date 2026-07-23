using System;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Xunit;

namespace tests.Specs
{
    public class GenericUtilitiesTests
    {
        [Theory]
        [InlineData("12.0.0", 12, 0, 0)]
        [InlineData("11.1.5", 11, 1, 5)]
        [InlineData("10.0.0.0", 10, 0, 0)]
        public void ParseVersion_PlainVersion_ParsesCorrectly(string version, int major, int minor, int patch)
        {
            var result = GenericUtilities.ParseVersion(version);

            result.Major.Should().Be(major);
            result.Minor.Should().Be(minor);
            result.Patch.Should().Be(patch);
            result.IsPrerelease.Should().BeFalse();
        }

        [Theory]
        [InlineData("12.0.0-alpha.1", 12, 0, 0, "alpha.1")]
        [InlineData("12.0.0-beta", 12, 0, 0, "beta")]
        [InlineData("11.1.5-rc.2+build.123", 11, 1, 5, "rc.2")]
        public void ParseVersion_PreReleaseVersion_PreservesPreReleaseLabel(string version, int major, int minor, int patch, string expectedRelease)
        {
            var result = GenericUtilities.ParseVersion(version);

            result.Major.Should().Be(major);
            result.Minor.Should().Be(minor);
            result.Patch.Should().Be(patch);
            result.IsPrerelease.Should().BeTrue();
            result.Release.Should().Be(expectedRelease);
        }

        [Fact]
        public void ParseVersion_NullOrEmpty_Throws()
        {
            ((Action)(() => GenericUtilities.ParseVersion(null))).Should().Throw<ArgumentException>();
            ((Action)(() => GenericUtilities.ParseVersion(""))).Should().Throw<ArgumentException>();
            ((Action)(() => GenericUtilities.ParseVersion("   "))).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ParseVersion_Invalid_Throws()
        {
            ((Action)(() => GenericUtilities.ParseVersion("not-a-version"))).Should().Throw<Exception>();
        }

        [Theory]
        [InlineData("12.0.0-alpha.1")]
        [InlineData("12.0.0")]
        public void TryParseVersion_ValidVersions_ReturnsTrue(string version)
        {
            var success = GenericUtilities.TryParseVersion(version, out var result);

            success.Should().BeTrue();
            result.Major.Should().Be(12);
            result.Minor.Should().Be(0);
            result.Patch.Should().Be(0);
        }

        [Fact]
        public void TryParseVersion_Invalid_ReturnsFalse()
        {
            var success = GenericUtilities.TryParseVersion("not-a-version", out var result);

            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void ParseVersion_PreReleaseAndPlainVersions_AreComparableAndOrdered()
        {
            // this mirrors how MESVersion is used across the CLI: comparisons must keep working
            // regardless of a pre-release label being present in the original string
            var preRelease = GenericUtilities.ParseVersion("12.0.0-alpha.1");
            var release = GenericUtilities.ParseVersion("12.0.0");
            var older = GenericUtilities.ParseVersion("11.0.0");

            // a pre-release version sorts before its associated release version, per semver rules
            (preRelease < release).Should().BeTrue();
            (preRelease > older).Should().BeTrue();
        }

        [Theory]
        [InlineData("12.0.0", "12.0.0")]
        [InlineData("11.1.5", "11.1.5")]
        [InlineData("12.0.0-alpha.1", "12.0.0")]
        [InlineData("10.0.0.0", "10.0.0")]
        public void ToVersion_CollapsesToThreePartVersion_WhenRevisionIsZero(string version, string expected)
        {
            var nuGetVersion = GenericUtilities.ParseVersion(version);

            var result = GenericUtilities.ToVersion(nuGetVersion);

            result.ToString().Should().Be(expected);
        }

        [Theory]
        [InlineData("12.0.0", "release-1200")]
        [InlineData("11.1.5", "release-1115")]
        [InlineData("12.0.0-alpha.1", "alpha-1200")]
        [InlineData("12.0.0-next.2", "next-1200")]
        [InlineData("11.1.5-beta.2", "beta-1115")]
        public void GetNpmDistTag_NuGetVersion_ComputesExpectedDistTag(string version, string expectedDistTag)
        {
            var nuGetVersion = GenericUtilities.ParseVersion(version);

            var result = GenericUtilities.GetNpmDistTag(nuGetVersion);

            result.Should().Be(expectedDistTag);
        }

        [Fact]
        public void GetNpmDistTag_PlainVersion_AlwaysUsesReleaseTag()
        {
            var result = GenericUtilities.GetNpmDistTag(new Version(12, 0, 0));

            result.Should().Be("release-1200");
        }
    }
}

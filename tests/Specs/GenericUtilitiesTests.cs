using System;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using FluentAssertions;
using NuGet.Versioning;
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
            var result = NuGetVersion.Parse(version);

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
            var result = NuGetVersion.Parse(version);

            result.Major.Should().Be(major);
            result.Minor.Should().Be(minor);
            result.Patch.Should().Be(patch);
            result.IsPrerelease.Should().BeTrue();
            result.Release.Should().Be(expectedRelease);
        }

        [Fact]
        public void ParseVersion_NullOrEmpty_Throws()
        {
            ((Action)(() => NuGetVersion.Parse(null))).Should().Throw<ArgumentException>();
            ((Action)(() => NuGetVersion.Parse(""))).Should().Throw<ArgumentException>();
            ((Action)(() => NuGetVersion.Parse("   "))).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ParseVersion_Invalid_Throws()
        {
            ((Action)(() => NuGetVersion.Parse("not-a-version"))).Should().Throw<Exception>();
        }

        [Theory]
        [InlineData("12.0.0-alpha.1")]
        [InlineData("12.0.0")]
        public void TryParseVersion_ValidVersions_ReturnsTrue(string version)
        {
            var success = NuGetVersion.TryParse(version, out var result);

            success.Should().BeTrue();
            result.Major.Should().Be(12);
            result.Minor.Should().Be(0);
            result.Patch.Should().Be(0);
        }

        [Fact]
        public void TryParseVersion_Invalid_ReturnsFalse()
        {
            var success = NuGetVersion.TryParse("not-a-version", out var result);

            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void ParseVersion_PreReleaseAndPlainVersions_AreComparableAndOrdered()
        {
            // this mirrors how MESVersion is used across the CLI: comparisons must keep working
            // regardless of a pre-release label being present in the original string
            var preRelease = NuGetVersion.Parse("12.0.0-alpha.1");
            var release = NuGetVersion.Parse("12.0.0");
            var older = NuGetVersion.Parse("11.0.0");

            // a pre-release version sorts before its associated release version, per semver rules
            (preRelease < release).Should().BeTrue();
            (preRelease > older).Should().BeTrue();
        }

        [Theory]
        [InlineData("12.0.0", "release-1200")]
        [InlineData("11.1.5", "release-1115")]
        [InlineData("12.0.0-alpha.1", "alpha-1200")]
        [InlineData("12.0.0-next.2", "next-1200")]
        [InlineData("11.1.5-beta.2", "beta-1115")]
        public void GetNpmDistTag_NuGetVersion_ComputesExpectedDistTag(string version, string expectedDistTag)
        {
            var nuGetVersion = NuGetVersion.Parse(version);

            var result = GenericUtilities.GetNpmDistTag(nuGetVersion);

            result.Should().Be(expectedDistTag);
        }

        [Fact]
        public void GetNpmDistTag_PlainVersion_AlwaysUsesReleaseTag()
        {
            var result = GenericUtilities.GetNpmDistTag(new NuGetVersion(12, 0, 0));

            result.Should().Be("release-1200");
        }

        [Fact]
        public void GetNpmDistTag_PreReleaseVersion_ShouldUsePrereleaseDistTag()
        {
            // Prerelease MES versions should map to the corresponding prerelease npm dist-tag rather than the release tag.
            var result = GenericUtilities.GetNpmDistTag(NuGetVersion.Parse("12.0.0-beta.2"));

            result.Should().Be("beta-1200");
        }

        [Theory]
        [InlineData("12.0.0", "release-1200")]
        [InlineData("12.0.0-beta.2", "beta-1200")]
        public void GetNpmDistTag_NuGetVersion_UsesSemVerAwareDistTag(string version, string expectedDistTag)
        {
            var mesVersion = NuGetVersion.Parse(version);

            var result = GenericUtilities.GetNpmDistTag(mesVersion);

            result.Should().Be(expectedDistTag);
        }

        [Theory]
        [InlineData("12.0.0", "12.0.0", false)]
        [InlineData("12.0.0-beta.1", "12.0.0-beta.2", true)]
        [InlineData("12.0.0-beta.2", "12.0.0", true)]
        [InlineData("12.0.0", "12.0.0-beta.1", false)]
        public void NuGetVersion_Comparison_RespectsSemVerOrdering(string left, string right, bool leftIsLessThanRight)
        {
            var leftVersion = new NuGetVersion(left);
            var rightVersion = new NuGetVersion(right);

            (leftVersion < rightVersion).Should().Be(leftIsLessThanRight);
        }
    }
}

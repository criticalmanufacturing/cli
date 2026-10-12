using Cmf.CLI.Core.Commands;
using FluentAssertions;
using System;
using System.CommandLine;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace tests.Specs;

public class BaseCommandTests
{
    /// <summary>
    /// Minimal concrete subclass that exposes ParseUri and ParseUriArray
    /// through options, so they can be triggered via command parsing.
    /// </summary>
    private class TestableBaseCommand : BaseCommand
    {
        public Option<Uri> UriOption { get; } = new Option<Uri>("--uri") { Required = false };
        public Option<Uri[]> UriArrayOption { get; } = new Option<Uri[]>("--uris")
        {
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = true,
            Required = false
        };

        public TestableBaseCommand() : base(new MockFileSystem()) { }

        public override void Configure(Command cmd)
        {
            UriOption.CustomParser = argResult => ParseUri(argResult);
            UriArrayOption.CustomParser = argResult => ParseUriArray(argResult);
            cmd.Add(UriOption);
            cmd.Add(UriArrayOption);
        }

        /// <summary>Parse a single --uri value and return the result.</summary>
        public Uri InvokeParseUri(string value)
        {
            var root = new Command("test");
            Configure(root);
            var parseResult = root.Parse(value != null ? new[] { "--uri", value } : Array.Empty<string>());
            return parseResult.GetValue(UriOption);
        }

        /// <summary>Parse one or more --uris values and return the result.</summary>
        public Uri[] InvokeParseUriArray(params string[] values)
        {
            var root = new Command("test");
            Configure(root);
            var args = new System.Collections.Generic.List<string> { "--uris" };
            args.AddRange(values);
            var parseResult = root.Parse(args.ToArray());
            return parseResult.GetValue(UriArrayOption);
        }
    }

    // -------------------------------------------------------------------------
    // ParseUri — no token
    // -------------------------------------------------------------------------

    [Fact]
    public void ParseUri_NoToken_ReturnsNull()
    {
        var sut = new TestableBaseCommand();
        var result = sut.InvokeParseUri(null);
        result.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // ParseUri — standard absolute URIs
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("https://example.com", "https://example.com/")]
    [InlineData("https://example.com/path/to/feed", "https://example.com/path/to/feed")]
    [InlineData("http://registry.example.com:4873/", "http://registry.example.com:4873/")]
    public void ParseUri_AbsoluteUrl_ReturnsAbsoluteUri(string input, string expected)
    {
        var sut = new TestableBaseCommand();
        var result = sut.InvokeParseUri(input);
        result.Should().NotBeNull();
        result.IsAbsoluteUri.Should().BeTrue();
        result.ToString().Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // ParseUri — UNC paths
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(@"\\server\share", "file://server/share")]
    [InlineData(@"\\server\share\sub", "file://server/share/sub")]
    public void ParseUri_UncPath_ReturnsFileUri(string input, string expected)
    {
        var sut = new TestableBaseCommand();
        var result = sut.InvokeParseUri(input);
        result.Should().NotBeNull();
        result.IsAbsoluteUri.Should().BeTrue();
        result.Scheme.Should().Be("file");
        result.ToString().Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // ParseUri — Windows drive paths
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(@"C:\path\to\dir", "file:///C:/path/to/dir")]
    [InlineData(@"D:\packages", "file:///D:/packages")]
    public void ParseUri_WindowsDrivePath_ReturnsFileUri(string input, string expected)
    {
        var sut = new TestableBaseCommand();
        var result = sut.InvokeParseUri(input);
        result.Should().NotBeNull();
        result.IsAbsoluteUri.Should().BeTrue();
        result.Scheme.Should().Be("file");
        result.ToString().Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // ParseUri — relative paths
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("./relative/path")]
    [InlineData("relative/path")]
    public void ParseUri_RelativePath_ReturnsRelativeUri(string input)
    {
        var sut = new TestableBaseCommand();
        var result = sut.InvokeParseUri(input);
        result.Should().NotBeNull();
        result.IsAbsoluteUri.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // ParseUriArray — no tokens
    // -------------------------------------------------------------------------

    [Fact]
    public void ParseUriArray_OptionNotProvided_ReturnsEmpty()
    {
        // When the option is absent entirely, CustomParser is not invoked;
        // the framework returns the type default (empty array).
        var root = new Command("test");
        var sut = new TestableBaseCommand();
        sut.Configure(root);
        var parseResult = root.Parse(Array.Empty<string>());
        var result = parseResult.GetValue(sut.UriArrayOption);
        result.Should().BeNullOrEmpty();
    }

    // -------------------------------------------------------------------------
    // ParseUriArray — single token
    // -------------------------------------------------------------------------

    [Fact]
    public void ParseUriArray_SingleUrl_ReturnsSingleElementArray()
    {
        var sut = new TestableBaseCommand();
        var result = sut.InvokeParseUriArray("https://example.com");
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].ToString().Should().Be("https://example.com/");
    }

    // -------------------------------------------------------------------------
    // ParseUriArray — multiple tokens
    // -------------------------------------------------------------------------

    [Fact]
    public void ParseUriArray_MultipleUrls_ReturnsAllUris()
    {
        var sut = new TestableBaseCommand();
        var result = sut.InvokeParseUriArray("https://feed1.example.com", "https://feed2.example.com");
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].ToString().Should().Be("https://feed1.example.com/");
        result[1].ToString().Should().Be("https://feed2.example.com/");
    }

    // -------------------------------------------------------------------------
    // ParseUriArray — multiple tokens with some empty
    // -------------------------------------------------------------------------

    [Fact]
    public void ParseUriArray_MultipleUrlsWithEmptyToken_IsIgnored()
    {
        var sut = new TestableBaseCommand();
        var inputs = new[] { "https://example.com", "", "https://example.org" };
        var result = sut.InvokeParseUriArray(inputs);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].ToString().Should().Be("https://example.com/");
        result[1].ToString().Should().Be("https://example.org/");
    }


    // -------------------------------------------------------------------------
    // ParseUriArray — mixed absolute and UNC
    // -------------------------------------------------------------------------

    [Fact]
    public void ParseUriArray_MixedInputs_ParsesAll()
    {
        var sut = new TestableBaseCommand();
        var result = sut.InvokeParseUriArray("https://example.com", @"\\server\share");
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Scheme.Should().Be("https");
        result[1].Scheme.Should().Be("file");
    }
}

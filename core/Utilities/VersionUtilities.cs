using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text.RegularExpressions;
using Cmf.CLI.Core;
using NuGet.Versioning;

namespace Cmf.CLI.Utilities;

/// <summary>Parses and formats versions used by CLI commands and plugins.</summary>
public static class VersionUtilities
{
    // Translate input failures into command-line diagnostics before any handler runs.
    /// <summary>Parses a version argument, reporting validation failures as command-line errors.</summary>
    /// <typeparam name="T">The parsed version type.</typeparam>
    /// <param name="result">The argument being parsed.</param>
    /// <param name="parse">The version parser.</param>
    /// <returns>The parsed value, or the default value when validation fails.</returns>
    public static T ParseVersion<T>(ArgumentResult result, Func<string, T> parse)
    {
        try
        {
            return parse(result.Tokens.SingleOrDefault()?.Value);
        }
        catch (CliException exception)
        {
            result.AddError(exception.Message);
            return default;
        }
    }

    /// <summary>Parses a NuGet version while preserving its original text.</summary>
    /// <param name="text">The version text.</param>
    /// <param name="field">The option or configuration field used in diagnostics.</param>
    /// <returns>The parsed NuGet version.</returns>
    public static NuGetVersion ParseNuGetVersion(string text, string field)
    {
        if (!NuGetVersion.TryParse(text, out var version))
            throw InvalidVersion(text, field, CoreMessages.NuGetVersionExpectedFormat);
        // NuGet's parser trims whitespace; its native constructor can retain the original text.
        return new NuGetVersion(version.Version, version.ReleaseLabels, version.Metadata, text);
    }

    /// <summary>Parses a semantic version.</summary>
    /// <param name="text">The version text.</param>
    /// <param name="field">The option or configuration field used in diagnostics.</param>
    /// <returns>The parsed semantic version.</returns>
    public static SemanticVersion ParseSemanticVersion(string text, string field)
    {
        if (!SemanticVersion.TryParse(text, out var version))
            throw InvalidVersion(text, field, CoreMessages.SemanticVersionExpectedFormat);
        return version;
    }

    /// <summary>Parses a MES version and enforces the minimum supported major version of 10.</summary>
    /// <param name="text">The version text.</param>
    /// <param name="field">The option or configuration field used in diagnostics.</param>
    /// <returns>The supported MES version.</returns>
    public static NuGetVersion ParseMesVersion(string text, string field)
    {
        var version = ParseNuGetVersion(text, field);
        if (version.Major < 10)
            throw new CliException(CoreMessages.UnsupportedMesVersion);
        return version;
    }

    /// <summary>Creates a diagnostic for a missing or invalid version.</summary>
    /// <param name="value">The supplied version text.</param>
    /// <param name="field">The affected option or configuration field.</param>
    /// <param name="expected">The expected version format.</param>
    /// <param name="source">The optional source of the value.</param>
    /// <returns>The exception describing the validation failure.</returns>
    public static CliException InvalidVersion(string value, string field, string expected, string source = null)
    {
        var location = string.IsNullOrEmpty(source) ? string.Empty : string.Format(CoreMessages.VersionInputSource, source);
        var message = string.IsNullOrWhiteSpace(value) ? CoreMessages.MissingVersionInput : CoreMessages.InvalidVersionInput;
        return new CliException(string.Format(message, field, location, value ?? CoreMessages.MissingVersionValue, expected));
    }

    /// <summary>Parses a semantic version using strict npm version syntax.</summary>
    /// <param name="value">The version text.</param>
    /// <param name="field">The option or configuration field used in diagnostics.</param>
    /// <returns>The parsed npm version.</returns>
    public static SemanticVersion ParseNpmVersion(string value, string field)
    {
        var version = ParseSemanticVersion(value, field);
        // NuGet's parser accepts relaxed numeric syntax that npm rejects.
        const string numeric = @"(?:0|[1-9][0-9]*)";
        const string prereleaseIdentifier = @"(?:0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*)";
        var pattern = $@"\A{numeric}\.{numeric}\.{numeric}(?:-{prereleaseIdentifier}(?:\.{prereleaseIdentifier})*)?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?\z";
        if (value.Length > 256 || !Regex.IsMatch(value, pattern, RegexOptions.NonBacktracking))
            throw InvalidVersion(value, field, CoreMessages.SemanticVersionExpectedFormat);
        return version;
    }

    private static void ValidateVersionSuffix(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value) || !NuGetVersion.TryParse($"1.0.0-{value}", out _))
        {
            var message = string.IsNullOrWhiteSpace(value) ? CoreMessages.MissingVersionInput : CoreMessages.InvalidVersionInput;
            throw new CliException(string.Format(message, field, string.Empty,
                value ?? CoreMessages.MissingVersionValue, CoreMessages.VersionSuffixExpectedFormat));
        }
    }

    /// <summary>Parses an optional build or prerelease suffix.</summary>
    /// <param name="result">The argument being parsed.</param>
    /// <param name="field">The option name used in diagnostics.</param>
    /// <returns>The validated suffix, or the default value when validation fails.</returns>
    public static string ParseSuffix(ArgumentResult result, string field)
        => ParseVersion(result, text =>
        {
            if (!string.IsNullOrEmpty(text)) ValidateVersionSuffix(text, field);
            return text;
        });

    /// <summary>Formats a version, preserving original NuGet text when available.</summary>
    /// <param name="version">The version to format, or null.</param>
    /// <returns>The version text, or null when no version is supplied.</returns>
    public static string GetVersionText(SemanticVersion version)
        => version is NuGetVersion nuget ? nuget.OriginalVersion ?? nuget.ToFullString() : version?.ToFullString();

    // System.CommandLine cannot convert an explicitly empty token to a null native version.
    /// <summary>Reads an optional version, treating an explicitly empty token as null.</summary>
    /// <typeparam name="T">The semantic version type.</typeparam>
    /// <param name="result">The parsed symbol containing the option.</param>
    /// <param name="option">The version option.</param>
    /// <returns>The parsed version, or null for an explicitly empty token.</returns>
    public static T GetOptionalVersion<T>(SymbolResult result, Option<T> option) where T : SemanticVersion
        => result.GetResult(option)?.Tokens.SingleOrDefault()?.Value == string.Empty ? null : result.GetValue(option);
}

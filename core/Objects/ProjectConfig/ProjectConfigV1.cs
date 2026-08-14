using System;
using System.Text.Json.Serialization;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using NuGet.Versioning;

namespace Cmf.CLI.Core.Objects;

public class ProjectConfigV1
{
    public string ProjectName { get; set; }
    public RepositoryType? RepositoryType { get; set; }
    public BaseLayer? BaseLayer { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri NPMRegistry { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri NuGetRegistry { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri AzureDevopsCollectionURL { get; set; }
    public string AgentPool { get; set; }
    public AgentType AgentType { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri RepositoryURL { get; set; }
    public string EnvironmentName { get; set; }
    public string DefaultDomain { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? RESTPort { get; set; }
    public string Tenant { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(MesVersionConverter))]
    public MesVersion MESVersion { get; set; }
    public SemanticVersion DevTasksVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(MesVersionConverter))]
    public MesVersion HTMLStarterVersion { get; set; }
    public SemanticVersion YoGeneratorVersion { get; set; }
    public string NGXSchematicsVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(MesVersionConverter))]
    public MesVersion NugetVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(MesVersionConverter))]
    public MesVersion TestScenariosNugetVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(BooleanJsonConverter))]
    public bool IsSslEnabled { get; set; }
    public string vmHostname { get; set; }
    public string DBReplica1 { get; set; }
    public string DBReplica2 { get; set; }
    public string DBServerOnline { get; set; }
    public string DBServerODS { get; set; }
    public string DBServerDWH { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri ReportServerURI { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(BooleanJsonConverter))]
    public bool AlwaysOn { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri InstallationPath { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri DBBackupPath { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri TemporaryPath { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? HTMLPort { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? GatewayPort { get; set; }
    public string ReleaseEnvironmentConfig { get; set; }
    public string AppEnvironmentConfig { get; set; }
    public Uri ISOLocation { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri DeploymentDir { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri DeliveredRepo { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri CIRepo { get; set; }

    public string Organization { get; set; }
    public string Product { get; set; }
}

public class BooleanJsonConverter : Newtonsoft.Json.JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override bool CanConvert( Type objectType )
    {
        if (Nullable.GetUnderlyingType(objectType) != null)
        {
            return Nullable.GetUnderlyingType(objectType) == typeof(bool);
        }
        return objectType == typeof(bool);
    }

    public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
    {
        // handle True and False as old project configs have these values (remnant of powershell implementation)
        switch ( reader.Value.ToString().ToLower().Trim() )
        {
            case "true":
                return true;
            case "false":
                return false;
            case "":
                return null;
        }

        // If we reach here, we're pretty much going to throw an error so let's let Json.NET throw it's pretty-fied error message.
        return new JsonSerializer().Deserialize( reader, objectType );
    }

    public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
    {
    }

}

/// <summary>
/// Represents a MES version while preserving full semantic-version identity.
/// Unlike <see cref="Version"/>, a <see cref="NuGetVersion"/> can safely carry pre-release labels
/// and build metadata such as "12.0.0-beta.1" or "12.0.0-alpha.3+build.7".
/// <para>
/// This wrapper keeps compatibility with older numeric checks by exposing a <see cref="NumericVersion"/>
/// conversion and implicit conversions for <see cref="Version"/>, while retaining the original SemVer
/// value for comparisons and serialization.
/// </para>
/// </summary>
public readonly struct MesVersion : IComparable, IComparable<MesVersion>, IComparable<Version>, IEquatable<MesVersion>, IEquatable<Version>
{
    private readonly NuGetVersion value;

    private static int NormalizeBuild(int build)
    {
        return build < 0 ? 0 : build;
    }

    private static Version NormalizeLegacyVersion(Version value)
    {
        if (value is null)
        {
            return null;
        }

        return new Version(value.Major, value.Minor, NormalizeBuild(value.Build));
    }

    public MesVersion(string version)
        : this(GenericUtilities.ParseVersion(version))
    {
    }

    public MesVersion(Version version)
        : this(version is null ? throw new ArgumentNullException(nameof(version)) : new NuGetVersion(version.Major, version.Minor, NormalizeBuild(version.Build)))
    {
    }

    public MesVersion(NuGetVersion version)
    {
        this.value = version ?? throw new ArgumentNullException(nameof(version));
    }

    public int Major => value.Major;
    public int Minor => value.Minor;
    public int Patch => value.Patch;
    public int Revision => value.Version.Revision;
    public bool IsPrerelease => value.IsPrerelease;
    public string Release => value.IsPrerelease ? value.Release : string.Empty;
    public Version NumericVersion => GenericUtilities.ToVersion(value);
    public NuGetVersion NuGetVersion => value;

    public override string ToString() => value.OriginalVersion;
    public override int GetHashCode() => value.GetHashCode();

    public override bool Equals(object obj)
    {
        return obj switch
        {
            MesVersion other => Equals(other),
            Version version => Equals(version),
            _ => false
        };
    }

    public bool Equals(MesVersion other) => value.Equals(other.value);
    public bool Equals(Version other)
    {
        if (other is null)
        {
            return false;
        }

        return NumericVersion == NormalizeLegacyVersion(other);
    }

    public int CompareTo(object obj)
    {
        if (obj is null)
        {
            return 1;
        }

        return obj switch
        {
            MesVersion other => CompareTo(other),
            Version version => CompareTo(version),
            _ => throw new ArgumentException($"Object must be of type {nameof(MesVersion)} or {nameof(Version)}.", nameof(obj))
        };
    }

    public int CompareTo(MesVersion other) => value.CompareTo(other.value);

    public int CompareTo(Version other)
    {
        if (other is null)
        {
            return 1;
        }

        var otherVersion = new NuGetVersion(other.Major, other.Minor, NormalizeBuild(other.Build));
        return value.CompareTo(otherVersion);
    }

    public static bool operator <(MesVersion left, MesVersion right) => left.CompareTo(right) < 0;
    public static bool operator >(MesVersion left, MesVersion right) => left.CompareTo(right) > 0;
    public static bool operator <=(MesVersion left, MesVersion right) => left.CompareTo(right) <= 0;
    public static bool operator >=(MesVersion left, MesVersion right) => left.CompareTo(right) >= 0;
    public static bool operator <(MesVersion left, Version right) => left.CompareTo(right) < 0;
    public static bool operator >(MesVersion left, Version right) => left.CompareTo(right) > 0;
    public static bool operator <=(MesVersion left, Version right) => left.CompareTo(right) <= 0;
    public static bool operator >=(MesVersion left, Version right) => left.CompareTo(right) >= 0;
    public static bool operator <(Version left, MesVersion right) => right.CompareTo(left) > 0;
    public static bool operator >(Version left, MesVersion right) => right.CompareTo(left) < 0;
    public static bool operator <=(Version left, MesVersion right) => right.CompareTo(left) >= 0;
    public static bool operator >=(Version left, MesVersion right) => right.CompareTo(left) <= 0;

    public static implicit operator MesVersion(string value) => string.IsNullOrWhiteSpace(value) ? default : new MesVersion(value);
    public static implicit operator MesVersion(Version value) => value is null ? default : new MesVersion(value);
    public static implicit operator Version(MesVersion value) => value.NumericVersion;
    public static implicit operator NuGetVersion(MesVersion value) => value.value;
}

/// <summary>
/// Reads and writes MES versions using the full SemVer form so prerelease labels are preserved.
/// This is the canonical converter for <see cref="ProjectConfigV1.MESVersion"/> because losing the
/// prerelease suffix would change the meaning of versions such as "12.0.0-beta.1".
/// </summary>
public class MesVersionConverter : Newtonsoft.Json.JsonConverter<MesVersion>
{
    public override MesVersion ReadJson(JsonReader reader, Type objectType, MesVersion existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return default;
        }

        var value = reader.Value?.ToString();
        return string.IsNullOrWhiteSpace(value) ? default : new MesVersion(value);
    }

    public override void WriteJson(JsonWriter writer, MesVersion value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}

/// <summary>
/// Converts a JSON version to a plain <see cref="Version"/> while intentionally discarding any
/// prerelease or build metadata. This is kept only for legacy config fields that are known to be
/// numeric-only and therefore cannot represent SemVer labels.
/// </summary>
public class LenientVersionConverter : Newtonsoft.Json.JsonConverter<Version>
{
    public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = reader.Value?.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : GenericUtilities.ToVersion(GenericUtilities.ParseVersion(value));
    }

    public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }
}
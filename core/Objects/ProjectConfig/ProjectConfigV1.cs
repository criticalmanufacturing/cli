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
    [Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<NuGetVersion>))]
    public NuGetVersion MESVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<SemanticVersion>))]
    public SemanticVersion DevTasksVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<SemanticVersion>))]
    public SemanticVersion HTMLStarterVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<SemanticVersion>))]
    public SemanticVersion YoGeneratorVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<SemanticVersion>))]
    public SemanticVersion NGXSchematicsVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<NuGetVersion>))]
    public NuGetVersion NugetVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<NuGetVersion>))]
    public NuGetVersion TestScenariosNugetVersion { get; set; }
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

/***
 * Converts project-config version strings into the concrete CLR types expected by the model.
 * This is required because .project-config.json stores values as strings while fields such as
 * MESVersion and NugetVersion are typed as NuGetVersion/SemanticVersion instances.
 * Without this conversion, Json.NET can try to assign a SemanticVersion to a NuGetVersion and throw
 * an invalid cast when prerelease values such as "12.0.0-beta.1" are deserialized.
 */
public class VersionStringConverter<T> : Newtonsoft.Json.JsonConverter<T>
    where T : class
{
    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null || reader.Value is null)
        {
            return null;
        }

        var value = reader.Value?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (typeof(T) == typeof(NuGetVersion))
        {
            return (T)(object)NuGetVersion.Parse(value);
        }

        if (typeof(T) == typeof(SemanticVersion))
        {
            return (T)(object)SemanticVersion.Parse(value);
        }

        throw new NotSupportedException($"Unsupported version type: {typeof(T).FullName}");
    }

    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }
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
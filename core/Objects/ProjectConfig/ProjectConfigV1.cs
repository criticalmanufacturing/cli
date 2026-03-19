using System;
using System.Text.Json.Serialization;
using Cmf.CLI.Core.Enums;
using Newtonsoft.Json;
using NuGet.Versioning;

namespace Cmf.CLI.Core.Objects;

public class ProjectConfigV1
{
    public required string ProjectName { get; set; }
    public required RepositoryType RepositoryType { get; set; }
    public required BaseLayer BaseLayer { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public required Uri NPMRegistry { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public required Uri NuGetRegistry { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri? AzureDevopsCollectionURL { get; set; }
    public string? AgentPool { get; set; }
    public AgentType AgentType { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri? RepositoryURL { get; set; }
    public required string EnvironmentName { get; set; }
    public string? DefaultDomain { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public required int RESTPort { get; set; }
    public required string Tenant { get; set; }
    public required Version MESVersion { get; set; }
    public required SemanticVersion DevTasksVersion { get; set; }
    public required Version HTMLStarterVersion { get; set; }
    public required SemanticVersion YoGeneratorVersion { get; set; }
    public required SemanticVersion NGXSchematicsVersion { get; set; }
    public required Version NugetVersion { get; set; }
    public required Version TestScenariosNugetVersion { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(BooleanJsonConverter))]
    public required bool IsSslEnabled { get; set; }
    public required string vmHostname { get; set; }
    public required string DBReplica1 { get; set; }
    public required string DBReplica2 { get; set; }
    public required string DBServerOnline { get; set; }
    public required string DBServerODS { get; set; }
    public required string DBServerDWH { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public required Uri ReportServerURI { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(BooleanJsonConverter))]
    public required bool AlwaysOn { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public required Uri InstallationPath { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public required Uri DBBackupPath { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public required Uri TemporaryPath { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public required int HTMLPort { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public required int GatewayPort { get; set; }
    public required string ReleaseEnvironmentConfig { get; set; }
    public string? AppEnvironmentConfig { get; set; }
    public Uri? ISOLocation { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri? DeploymentDir { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public Uri? DeliveredRepo { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
    public required Uri CIRepo { get; set; }

    public string? Organization { get; set; }
    public string? Product { get; set; }
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

    public override object? ReadJson( JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer )
    {
        // handle True and False as old project configs have these values (remnant of powershell implementation)
        switch (reader.Value?.ToString()?.ToLower().Trim())
        {
            case "true":
                return true;
            case "false":
                return false;
            case "":
                return null;
            default:
                // If it's not a string representation of a boolean, try the default deserialization (which can handle actual booleans and nulls)
                 return new JsonSerializer().Deserialize(reader, objectType);
        }
    }

    public override void WriteJson( JsonWriter writer, object? value, JsonSerializer serializer )
    {
    }

}

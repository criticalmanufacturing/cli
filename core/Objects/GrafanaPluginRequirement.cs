using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cmf.CLI.Core.Objects;

/// <summary>
/// A required Grafana plugin artifact declaration for a Grafana package.
/// </summary>
[JsonObject]
public class GrafanaPluginRequirement
{
    /// <summary>
    /// Gets or sets the plugin identifier in Grafana plugin registry.
    /// </summary>
    [JsonProperty(Order = 0)]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the exact plugin version.
    /// </summary>
    [JsonProperty(Order = 1)]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the optional plugin source (absolute/relative path or HTTPS url).
    /// </summary>
    [JsonProperty(Order = 2)]
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the optional SHA256 checksum for the plugin archive.
    /// </summary>
    [JsonProperty(Order = 3)]
    public string Sha256 { get; set; }

    /// <summary>
    /// Gets or sets an optional platform target for platform-specific plugin archives.
    /// </summary>
    [JsonProperty(Order = 4)]
    public string Platform { get; set; }
}

/// <summary>
/// A collection of Grafana plugin requirements.
/// </summary>
public class GrafanaPluginRequirementCollection : List<GrafanaPluginRequirement>
{
}

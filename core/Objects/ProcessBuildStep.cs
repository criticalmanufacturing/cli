using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Core.Utilities;
using Newtonsoft.Json;

namespace Cmf.CLI.Core.Objects;

/// <summary>
/// An atomic (from tool perspective) build step
/// </summary>
[JsonObject]
public class ProcessBuildStep : IEquatable<ProcessBuildStep>
{
    /// <summary>
    /// Gets or sets the arguments.
    /// </summary>
    /// <value>
    /// The arguments.
    /// </value>
    [JsonProperty(Order = 1)]
    public string[] Args { get; set; }

    /// <summary>
    /// Gets or sets the command.
    /// </summary>
    /// <value>
    /// The command.
    /// </value>
    [JsonProperty(Order = 2)]
    public string Command { get; set; }

    /// <summary>
    /// Gets or sets the working directory.
    /// </summary>
    /// <value>
    /// The working directory.
    /// </value>
    [JsonConverter(typeof(AbstractionsDirectoryConverter))]
    [JsonProperty(Order = 3)]
    public IDirectoryInfo WorkingDirectory { get; set; }

    /// <summary>
    /// Gets or sets the environment variables.
    /// </summary>
    /// <value>
    /// The environment variables to be used on a given ProcessBuildStep
    /// </value>
    [JsonProperty(Order = 4)]
    public Dictionary<string, string> EnvironmentVariables { get; set; }

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(ProcessBuildStep other)
    {
        if (ReferenceEquals(null, other)) return false;
        return string.Equals(this.Command, other.Command) &&
               this.Args?.Length == other.Args?.Length &&
               (
                   other.Args == null ||
                   (this.Args?.OrderBy(x => x).SequenceEqual(other.Args?.OrderBy(x => x)) ?? true)
               ) &&
               this.WorkingDirectory.FullName.Equals(other.WorkingDirectory.FullName);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ProcessBuildStep)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return (this.Command?.GetHashCode() ?? 0)
            + (this.Args?.GetHashCode() ?? 0)
            + (this.WorkingDirectory?.GetHashCode() ?? 0);
    }

    #endregion IEquatable
}
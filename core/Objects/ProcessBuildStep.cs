using System;
using System.IO.Abstractions;
using System.Linq;
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

/// <summary>
/// Converts a path string to an IDirectoryInfo and vice versa
/// </summary>
public class AbstractionsDirectoryConverter : JsonConverter
{
    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue((value as IDirectoryInfo).ToString());
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return reader.TokenType == JsonToken.String ? ExecutionContext.Instance.FileSystem.DirectoryInfo.FromDirectoryName(reader.Value?.ToString()) : null;
    }

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        Type t = (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>))
            ? Nullable.GetUnderlyingType(objectType)
            : objectType;

        return t?.FullName == typeof(ProcessBuildStep).FullName;
    }
}
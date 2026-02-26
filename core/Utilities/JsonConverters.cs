using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Cmf.CLI.Core.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cmf.CLI.Core.Utilities
{
    /// <summary>
    /// Converts a List of path string to an List of IDirectoryInfo and vice versa
    /// </summary>
    public class ListAbstractionsDirectoryConverter : JsonConverter
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            List<IDirectoryInfo> directories = new();
            foreach (var children in token.Children())
            {
                if (children.Type == JTokenType.String)
                {
                    directories?.Add((ExecutionContext.Instance ?? throw new InvalidOperationException("ExecutionContext not initialized")).FileSystem.DirectoryInfo.New(children.ToString()));
                }
            }
            return directories;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value is List<IDirectoryInfo> directories)
            {
                foreach (var directory in directories)
                {
                    writer.WriteValue(directory.ToString());
                }
            }
            else
            {
                throw new JsonSerializationException(
                    $"Expected {nameof(List<IDirectoryInfo>)}");
            }
            writer.WriteEndArray();
        }

        public override bool CanConvert(Type objectType)
        {
            // CanConvert is not called when [JsonConverter] attribute is used
            return false;
        }
    }

    /// <summary>
    /// Converts a path string to an IDirectoryInfo and vice versa
    /// </summary>
    public class AbstractionsDirectoryConverter : JsonConverter
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue((value as IDirectoryInfo)?.ToString());
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.String ? (ExecutionContext.Instance ?? throw new InvalidOperationException("ExecutionContext not initialized")).FileSystem.DirectoryInfo.New((string)reader.Value!) : null;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            Type? t = (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>))
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            return t?.FullName == typeof(ProcessBuildStep).FullName;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Newtonsoft.Json;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    /// The DF repositories used in this run
    /// </summary>
    [JsonObject]
    public class RepositoriesConfig
    {
        /// <summary>
        /// The CI repository folder: this is the place where Packages built by CI are stored, by branch
        /// </summary>
        [JsonConverter(typeof(UriConverter))]
        public Uri CIRepository { get; set; }

        /// <summary>
        /// The DF repositories: these contain package that we treat as official (i.e. upstream dependencies or already releases packages)
        /// </summary>
        [JsonConverter(typeof(UriListConverter))]
        public List<Uri> Repositories { get; set; }

        /// <summary>
        /// Initialize new RepositoriesConfig.
        /// This constructor is only used as fallback, if a config is found in the filesystem, the file will be deserialized into this object
        /// </summary>
        public RepositoriesConfig()
        {
            Repositories = new List<Uri>();
        }
    }

    class UriConverter : JsonConverter<Uri>
    {
        public override void WriteJson(JsonWriter writer, Uri value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override Uri ReadJson(JsonReader reader, Type objectType, Uri existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var value = serializer.Deserialize(reader, typeof(string)) as string;
                if (string.IsNullOrEmpty(value))
                    return null;

                try
                {
                    // First, try to create as absolute URI (handles http://, https://, file:// schemes)
                    return new Uri(value, UriKind.Absolute);
                }
                catch (UriFormatException)
                {
                    // If absolute URI creation fails, handle different path formats
                    try
                    {
                        // Check if it's a UNC path (starts with \\)
                        if (value.StartsWith("\\\\", StringComparison.Ordinal))
                        {
                            return new Uri(value);
                        }

                        // Check if it's an absolute local path (starts with / on Unix or drive letter on Windows)
                        if (Path.IsPathRooted(value))
                        {
                            return new Uri(Path.GetFullPath(value));
                        }

                        // Handle relative paths by converting to absolute file URI
                        // Get the current working directory and combine with the relative path
                        var absolutePath = Path.GetFullPath(value);
                        return new Uri(absolutePath);
                    }
                    catch (Exception)
                    {
                        // If all URI creation attempts fail, return a file URI with the original value
                        // This maintains backward compatibility while allowing the system to handle the path
                        return new Uri($"file:///{value.Replace('\\', '/')}", UriKind.Absolute);
                    }
                }
            }

            return hasExistingValue ? existingValue : null;
        }

        public override bool CanWrite => false;
    }
    class UriListConverter : JsonConverter<List<Uri>>
    {
        public override void WriteJson(JsonWriter writer, List<Uri> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override List<Uri> ReadJson(JsonReader reader, Type objectType, List<Uri> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // First deserialize as a list of strings
            var stringArray = serializer.Deserialize(reader, typeof(List<string>)) as List<string>;
            
            if (stringArray == null)
                return existingValue ?? new List<Uri>();

            var result = new List<Uri>();
            var uriConverter = new UriConverter();
            
            foreach (var uriString in stringArray)
            {
                if (!string.IsNullOrEmpty(uriString))
                {
                    try
                    {
                        // Use the same logic as UriConverter
                        // First, try to create as absolute URI (handles http://, https://, file:// schemes)
                        result.Add(new Uri(uriString, UriKind.Absolute));
                    }
                    catch (UriFormatException)
                    {
                        // If absolute URI creation fails, handle different path formats
                        try
                        {
                            // Check if it's a UNC path (starts with \\)
                            if (uriString.StartsWith("\\\\", StringComparison.Ordinal))
                            {
                                result.Add(new Uri(uriString));
                            }
                            // Check if it's an absolute local path (starts with / on Unix or drive letter on Windows)
                            else if (Path.IsPathRooted(uriString))
                            {
                                result.Add(new Uri(Path.GetFullPath(uriString)));
                            }
                            // Handle relative paths by converting to absolute file URI
                            else
                            {
                                var absolutePath = Path.GetFullPath(uriString);
                                result.Add(new Uri(absolutePath));
                            }
                        }
                        catch (Exception)
                        {
                            // If all URI creation attempts fail, return a file URI with the original value
                            // This maintains backward compatibility while allowing the system to handle the path
                            result.Add(new Uri($"file:///{uriString.Replace('\\', '/')}", UriKind.Absolute));
                        }
                    }
                }
            }

            return result;
        }

        public override bool CanWrite => false;
    }
}
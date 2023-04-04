using System;
using System.Collections.Generic;
using System.Linq;
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
                var value = (serializer.Deserialize(reader, typeof(string)) as string);
                if (string.IsNullOrEmpty(value))
                    return null;
                return new Uri(value, UriKind.Absolute);
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
            var array = serializer.Deserialize(reader, objectType) as List<Uri>;
            
            array = array?.Select(u => new Uri(u.OriginalString, UriKind.Absolute)).ToList();

            return array ?? existingValue;
        }

        public override bool CanWrite => false;
    }
}
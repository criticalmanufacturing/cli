using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Cmf.CLI.Utilities
{
    public class IoTParametersConverter : JsonConverter<Dictionary<string, IoTValueType>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<string, IoTValueType> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);
                if (kvp.Value == IoTValueType.Enum)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("dataType");
                    writer.WriteValue(kvp.Value.ToString());
                    writer.WritePropertyName("enumValues");
                    writer.WriteStartArray();
                    writer.WriteValue("First Option");
                    writer.WriteValue("Second Option");
                    writer.WriteValue("etc");
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteValue(kvp.Value.ToString());
                }
            }
            writer.WriteEndObject();
        }

        public override Dictionary<string, IoTValueType> ReadJson(JsonReader reader, Type objectType, Dictionary<string, IoTValueType> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<string, IoTValueType>();
            if (reader.TokenType == JsonToken.StartObject)
            {
                reader.Read();
                while (reader.TokenType == JsonToken.PropertyName)
                {
                    var key = reader.Value.ToString();
                    reader.Read();
                    if (reader.TokenType == JsonToken.String)
                    {
                        result[key] = Enum.Parse<IoTValueType>(reader.Value.ToString());
                    }
                    else if (reader.TokenType == JsonToken.StartObject)
                    {
                        string dataType = null;
                        while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                        {
                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                var propertyName = reader.Value.ToString();
                                reader.Read();
                                if (propertyName == "dataType")
                                {
                                    dataType = reader.Value.ToString();
                                }
                                else if (propertyName == "enumValues")
                                {
                                    while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                                    {
                                        // Read the enumValues array (but we don't store it in this example).
                                    }
                                }
                            }
                        }
                        if (dataType == IoTValueType.Enum.ToString())
                        {
                            result[key] = IoTValueType.Enum;
                        }
                    }
                    reader.Read();
                }
            }
            return result;
        }
    }
}

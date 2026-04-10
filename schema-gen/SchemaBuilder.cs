using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Builds JSON Schema nodes using <see cref="JsonSchemaExporter"/> with Newtonsoft.Json attribute support.
/// </summary>
internal static class SchemaBuilder
{
    /// <summary>
    /// Builds a complete JSON Schema for <paramref name="rootType"/>, respecting
    /// <see cref="JsonPropertyAttribute"/> and <see cref="JsonIgnoreAttribute"/> annotations
    /// and attaching <c>description</c> fields from <paramref name="docs"/>.
    /// </summary>
    public static JsonObject BuildRootSchema(Type rootType, DocumentationIndex docs)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { ApplyNewtonsoftAttributes }
            }
        };

        JsonSchemaExporterOptions exporterOptions = new()
        {
            TreatNullObliviousAsNonNullable = true,
            TransformSchemaNode = (context, schema) => TransformNode(context, schema, docs)
        };

        return JsonSchemaExporter.GetJsonSchemaAsNode(options, rootType, exporterOptions).AsObject();
    }

    /// <summary>
    /// Modifier applied to each <see cref="JsonTypeInfo"/>: removes properties that lack
    /// <see cref="JsonPropertyAttribute"/> or carry <see cref="JsonIgnoreAttribute"/>,
    /// applies explicit property names, and sorts by <see cref="JsonPropertyAttribute.Order"/>.
    /// </summary>
    private static void ApplyNewtonsoftAttributes(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        for (int i = typeInfo.Properties.Count - 1; i >= 0; i--)
        {
            JsonPropertyInfo prop = typeInfo.Properties[i];
            MemberInfo? member = prop.AttributeProvider as MemberInfo;

            if (member == null
                || member.GetCustomAttribute<JsonPropertyAttribute>() == null
                || member.GetCustomAttribute<JsonIgnoreAttribute>() != null)
            {
                typeInfo.Properties.RemoveAt(i);
                continue;
            }

            string? customName = member.GetCustomAttribute<JsonPropertyAttribute>()!.PropertyName;
            if (!string.IsNullOrWhiteSpace(customName))
            {
                prop.Name = customName;
            }
        }

        List<JsonPropertyInfo> sorted = typeInfo.Properties
            .OrderBy(p => (p.AttributeProvider as MemberInfo)?.GetCustomAttribute<JsonPropertyAttribute>()?.Order ?? 0)
            .ThenBy(p => p.Name)
            .ToList();

        typeInfo.Properties.Clear();
        foreach (JsonPropertyInfo prop in sorted)
        {
            typeInfo.Properties.Add(prop);
        }
    }

    /// <summary>
    /// Transforms each generated schema node: adds <c>additionalProperties: false</c> to object
    /// schemas, overrides integer enum schemas with string enum schemas when
    /// <see cref="StringEnumConverter"/> is applied, and attaches <c>description</c> from
    /// <paramref name="docs"/>.
    /// </summary>
    private static JsonNode TransformNode(JsonSchemaExporterContext context, JsonNode schema, DocumentationIndex docs)
    {
        PropertyInfo? member = context.PropertyInfo?.AttributeProvider as PropertyInfo;

        // Override integer enum schema with string enum when StringEnumConverter is applied
        if (member != null)
        {
            JsonConverterAttribute? converterAttr = member.GetCustomAttribute<JsonConverterAttribute>();
            if (converterAttr?.ConverterType == typeof(StringEnumConverter))
            {
                Type enumType = Nullable.GetUnderlyingType(member.PropertyType) ?? member.PropertyType;
                if (enumType.IsEnum)
                {
                    JsonArray values = new();
                    foreach (string name in Enum.GetNames(enumType))
                    {
                        values.Add(name);
                    }

                    return new JsonObject { ["type"] = "string", ["enum"] = values };
                }
            }
        }

        if (schema is not JsonObject schemaObj)
        {
            return schema;
        }

        // Enforce no additional properties on object schemas
        if (schemaObj["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out string? typeStr) && typeStr == "object")
        {
            schemaObj["additionalProperties"] = false;
        }

        // Attach description from docs
        string? summary = member != null
            ? docs.GetPropertySummary(member.DeclaringType!, member.Name)
            : context.PropertyInfo == null
                ? docs.GetTypeSummary(context.TypeInfo.Type)
                : null;

        if (!string.IsNullOrWhiteSpace(summary))
        {
            schemaObj["description"] = summary;
        }

        return schemaObj;
    }
}

using System.Net;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Parses XML documentation comments from C# source files and indexes them by
/// fully-qualified type and property keys so the schema generator can emit
/// <c>description</c> fields from source <c>///</c> comments.
/// </summary>
sealed class DocumentationIndex
{
    private static readonly Regex NamespaceRegex = new(@"^\s*namespace\s+([\w\.]+)", RegexOptions.Compiled);
    private static readonly Regex TypeRegex = new(@"^\s*public\s+(?:partial\s+)?(?:class|record|struct)\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);
    private static readonly Regex PropertyRegex = new(@"^\s*public\s+[^\(]*?\s+([A-Za-z_][A-Za-z0-9_]*)\s*\{", RegexOptions.Compiled);

    private readonly Dictionary<string, string> typeSummaries;
    private readonly Dictionary<string, string> propertySummaries;

    private DocumentationIndex(Dictionary<string, string> typeSummaries, Dictionary<string, string> propertySummaries)
    {
        this.typeSummaries = typeSummaries;
        this.propertySummaries = propertySummaries;
    }

    /// <summary>
    /// Scans all <c>.cs</c> files under <paramref name="sourceRootDirectory"/> and builds an index.
    /// </summary>
    public static DocumentationIndex Build(string sourceRootDirectory)
    {
        Dictionary<string, string> typeSummaries = new(StringComparer.Ordinal);
        Dictionary<string, string> propertySummaries = new(StringComparer.Ordinal);

        if (!Directory.Exists(sourceRootDirectory))
        {
            return new DocumentationIndex(typeSummaries, propertySummaries);
        }

        foreach (string filePath in Directory.EnumerateFiles(sourceRootDirectory, "*.cs", SearchOption.AllDirectories))
        {
            if (filePath.Contains("/obj/") || filePath.Contains("/bin/"))
            {
                continue;
            }

            ParseFile(filePath, typeSummaries, propertySummaries);
        }

        return new DocumentationIndex(typeSummaries, propertySummaries);
    }

    /// <summary>
    /// Returns the <c>///</c> summary for a type, or <c>null</c> if none was found.
    /// </summary>
    public string? GetTypeSummary(Type type)
    {
        typeSummaries.TryGetValue(GetTypeKey(type), out string? summary);
        return summary;
    }

    /// <summary>
    /// Returns the <c>///</c> summary for a property, or <c>null</c> if none was found.
    /// </summary>
    public string? GetPropertySummary(Type type, string propertyName)
    {
        propertySummaries.TryGetValue(GetPropertyKey(type, propertyName), out string? summary);
        return summary;
    }

    /// <summary>
    /// Parses a single source file and populates <paramref name="typeSummaries"/> and
    /// <paramref name="propertySummaries"/> with any XML doc summaries found.
    /// </summary>
    private static void ParseFile(string filePath, Dictionary<string, string> typeSummaries, Dictionary<string, string> propertySummaries)
    {
        string[] lines = File.ReadAllLines(filePath);
        string? currentNamespace = null;
        string? currentType = null;
        StringBuilder? summaryBuilder = null;

        foreach (string line in lines)
        {
            Match namespaceMatch = NamespaceRegex.Match(line);
            if (namespaceMatch.Success)
            {
                currentNamespace = namespaceMatch.Groups[1].Value;
            }

            string trimmed = line.TrimStart();
            if (trimmed.StartsWith("///", StringComparison.Ordinal))
            {
                summaryBuilder ??= new StringBuilder();
                summaryBuilder.AppendLine(trimmed[3..].Trim());
                continue;
            }

            Match typeMatch = TypeRegex.Match(line);
            if (typeMatch.Success)
            {
                currentType = typeMatch.Groups[1].Value;
                string? summary = CleanXmlSummary(summaryBuilder?.ToString());
                if (!string.IsNullOrWhiteSpace(summary) && !string.IsNullOrWhiteSpace(currentNamespace))
                {
                    typeSummaries[$"{currentNamespace}.{currentType}"] = summary;
                }

                summaryBuilder = null;
                continue;
            }

            Match propertyMatch = PropertyRegex.Match(line);
            if (propertyMatch.Success && !string.IsNullOrWhiteSpace(currentNamespace) && !string.IsNullOrWhiteSpace(currentType))
            {
                string propertyName = propertyMatch.Groups[1].Value;
                string? summary = CleanXmlSummary(summaryBuilder?.ToString());
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    propertySummaries[$"{currentNamespace}.{currentType}.{propertyName}"] = summary;
                }

                summaryBuilder = null;
                continue;
            }

            // Any non-trivial line that is not an attribute, region marker, or opening brace
            // means the pending summary does not belong to the next declaration.
            bool isAttributeOrDirective = trimmed.StartsWith("[", StringComparison.Ordinal)
                || trimmed.StartsWith("#region", StringComparison.Ordinal)
                || trimmed.StartsWith("#endregion", StringComparison.Ordinal)
                || trimmed == "{";

            if (!string.IsNullOrWhiteSpace(trimmed) && !isAttributeOrDirective)
            {
                summaryBuilder = null;
            }
        }
    }

    /// <summary>
    /// Extracts the inner text of <c>&lt;summary&gt;</c> and <c>&lt;remarks&gt;</c> blocks from
    /// <paramref name="raw"/>, strips any nested XML tags within those blocks, and HTML-decodes the result.
    /// All other doc comment sections (e.g. <c>&lt;param&gt;</c>, <c>&lt;returns&gt;</c>,
    /// <c>&lt;value&gt;</c>) are ignored entirely.
    /// </summary>
    private static string? CleanXmlSummary(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        // Collect the inner content of <summary> and <remarks> blocks only; ignore everything else.
        StringBuilder sb = new();
        foreach (Match m in Regex.Matches(raw, "<summary>([\\s\\S]*?)<\\/summary>", RegexOptions.IgnoreCase))
        {
            sb.AppendLine(m.Groups[1].Value);
        }
        foreach (Match m in Regex.Matches(raw, "<remarks>([\\s\\S]*?)<\\/remarks>", RegexOptions.IgnoreCase))
        {
            sb.AppendLine(m.Groups[1].Value);
        }

        string text = sb.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // Strip any remaining inner tags (e.g. <see cref="..."/>, <c>, <paramref name="..."/>).
        text = Regex.Replace(text, "<[^>]+>", string.Empty);
        text = WebUtility.HtmlDecode(text);

        return text.Trim();
    }

    /// <summary>Returns the dictionary lookup key for a type: <c>Namespace.TypeName</c>.</summary>
    private static string GetTypeKey(Type type)
    {
        return $"{type.Namespace}.{GetSimpleTypeName(type)}";
    }

    /// <summary>Returns the dictionary lookup key for a property: <c>Namespace.TypeName.PropertyName</c>.</summary>
    private static string GetPropertyKey(Type type, string propertyName)
    {
        return $"{type.Namespace}.{GetSimpleTypeName(type)}.{propertyName}";
    }

    /// <summary>Returns the simple type name, stripping generic arity suffixes (e.g. <c>List`1</c> → <c>List</c>).</summary>
    private static string GetSimpleTypeName(Type type)
    {
        string name = type.Name;
        int tick = name.IndexOf('`');
        if (tick >= 0)
        {
            name = name[..tick];
        }

        return name;
    }
}

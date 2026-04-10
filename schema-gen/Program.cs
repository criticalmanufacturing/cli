using System.Text.Json;
using System.Text.Json.Nodes;

string outputDirectory = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
    ? args[0]
    : Path.Combine(Directory.GetCurrentDirectory(), "schemas");
string sourceRootDirectory = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1])
    ? args[1]
    : Path.Combine(Directory.GetCurrentDirectory(), "../core");

Directory.CreateDirectory(outputDirectory);

var docs = DocumentationIndex.Build(sourceRootDirectory);

JsonObject rootSchema = SchemaBuilder.BuildRootSchema(typeof(Cmf.CLI.Core.Objects.CmfPackage), docs);

string schemaJson = rootSchema.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
string schemaPath = Path.Combine(outputDirectory, "cmfpackage.schema.json");

if (!File.Exists(schemaPath) || File.ReadAllText(schemaPath) != schemaJson)
{
    File.WriteAllText(schemaPath, schemaJson);
}

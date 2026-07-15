using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Cmf.CLI.Core.Enums;
using System.Runtime.CompilerServices;
using Cmf.CLI.Builders;
using System;

[assembly: InternalsVisibleTo("tests")]
namespace Cmf.CLI.Utilities
{
    public static class UpgradeBaseUtilities
    {
        /// <summary>
        /// Updates the value of a given key in a JSON-formatted string using a regular expression.
        /// </summary>
        /// <param name="text">The original JSON content as a string.</param>
        /// <param name="key">The JSON key whose value should be updated.</param>
        /// <param name="newValue">The new value to assign to the key.</param>
        /// <returns>The modified JSON string with the updated key value.</returns>
        public static string UpdateJsonValue(string text, string key, string newValue)
        {
            return Regex.Replace(text, $"\"{key}\"" + @".*:.*"".+""", $"\"{key}\": \"{newValue}\"", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Removes a specified key and its associated value from a JSON-formatted string.
        /// </summary>
        /// <param name="text">The original JSON content as a string.</param>
        /// <param name="key">The JSON key to remove from the object.</param>
        /// <returns>The modified JSON string with the specified key removed. 
        /// If the key does not exist, the original JSON string is returned unchanged.</returns>
        public static string RemoveJsonValue(string text, string key)
        {
            var obj = JObject.Parse(text);
            obj.Property(key)?.Remove(); // Remove the property if it exists
            return obj.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Updates all relevant NPM project files by replacing release version tags in package.json
        /// and removing the package-lock.json files to force regeneration.
        /// </summary>
        /// <param name="fileSystem">The file system abstraction used to access and modify files.</param>
        /// <param name="cmfPackage">The CMF package object containing the root directory path.</param>
        /// <param name="version">The new Base version.</param>
        public static void UpdateNPMProject(IFileSystem fileSystem, CmfPackage cmfPackage, string version)
        {
            // package.json files
            string[] filesToUpdate = fileSystem.Directory.GetFiles(cmfPackage.GetFileInfo().DirectoryName, "package.json", SearchOption.AllDirectories);
            string pattern = @"release-\d+";

            foreach (string filePath in filesToUpdate.Where(path => !path.Contains("node_modules") && !path.Contains("dist")))
            {
                string text = fileSystem.File.ReadAllText(filePath);
                text = Regex.Replace(text, pattern, $"release-{version.Replace(".", "")}", RegexOptions.IgnoreCase);

                fileSystem.File.WriteAllText(filePath, text);
            }

            // package-lock.json files
            string[] filesToDelete = fileSystem.Directory.GetFiles(cmfPackage.GetFileInfo().DirectoryName, "package-lock.json", SearchOption.AllDirectories);
            foreach (string filePath in filesToDelete.Where(path => !path.Contains("node_modules") && !path.Contains("dist")))
            {
                Log.Warning($"Package lock {filePath} has been deleted. Please build the {cmfPackage.PackageId} package to regenerate this file");
                fileSystem.File.Delete(filePath);
            }
        }

        /// <summary>
        /// Updates version references to CMF NuGet packages in all .csproj files within the package directory.
        /// </summary>
        /// <param name="fileSystem">The file system abstraction used to access and modify files.</param>
        /// <param name="cmfPackage">The CMF package object containing the root directory path.</param>
        /// <param name="version">The new Base version.</param>
        /// <param name="strictMatching">
        ///     If true, only references to Cmf.Navigo, Cmf.Foundation, Cmf.MessageBus and Cmf.Common.CustomActionUtilities packages will be updated.
        ///     If false, all packages starting with Cmf. will be updated.
        /// </param>
        public static void UpdateCSharpProject(IFileSystem fileSystem, CmfPackage cmfPackage, string version, bool strictMatching)
        {
            string[] filesToUpdate = fileSystem.Directory.GetFiles(cmfPackage.GetFileInfo().DirectoryName, "*.csproj", SearchOption.AllDirectories);
            
            string pattern;
            if (strictMatching)
            {
                // Only update Cmf.Navigo, Cmf.Foundation, Cmf.MessageBus and Cmf.Common.CustomActionUtilities references
                pattern = @"(Include=""Cmf\.(?:Navigo|Foundation|MessageBus|Common\.CustomActionUtilities)[^""]*""\s+Version="")(.*?)(""[\s/>])";
            }
            else
            {
                // Only update Cmf.* references
                pattern = @"(Include=""Cmf\.[^""]*""\s+Version="")(.*?)(""[\s/>])";
            }

            foreach (string filePath in filesToUpdate)
            {
                string text = fileSystem.File.ReadAllText(filePath);
                text = Regex.Replace(text, pattern, match =>
                {
                    return match.Groups[1].Value + version + match.Groups[3].Value;
                }, RegexOptions.IgnoreCase);

                fileSystem.File.WriteAllText(filePath, text);
            }
        }

        /// <summary>
        /// Updates IoT Masterdata and Automation Workflows files executing the appropriate migration scripts.
        /// </summary>
        /// <param name="fileSystem">The file system abstraction used to access files.</param>
        /// <param name="cmfPackage">The CMF package being processed.</param>
        /// <param name="version">The new Base version.</param>
        /// <param name="manifest">Optional manifest file path for the migration tool.</param>
        public static void UpdateIoTMasterdataFiles(IFileSystem fileSystem, CmfPackage cmfPackage, string version, string manifest = null)
        {        
            List<string> args = [];
            string workflowsFolder = null; 
            foreach (ContentToPack contentToPack in cmfPackage.ContentToPack ?? [])
            {
                if (contentToPack.Source?.Contains(@"$(version)") ?? false)
                {
                    Log.Warning("Source paths with \"$(version)\" in cmf packages will be ignored");
                    continue;
                }

                if (contentToPack.ContentType == ContentType.MasterData)
                {
                    var mdFilePath = fileSystem.Path.GetFullPath(fileSystem.Path.Combine(cmfPackage.GetFileInfo().DirectoryName, contentToPack.Source)).Replace("*", "");
                    args.Add(mdFilePath);
                } else if (contentToPack.ContentType == ContentType.AutomationWorkFlows)
                {
                    workflowsFolder = fileSystem.Path.GetFullPath(fileSystem.Path.Combine(cmfPackage.GetFileInfo().DirectoryName, contentToPack.Source)).Replace("*", "");
                }
            }

            if (!string.IsNullOrEmpty(workflowsFolder))
            {
                args.AddRange(["--workflowsFolder", workflowsFolder]);
            }

            if (!string.IsNullOrEmpty(manifest))
            {
                args.AddRange(["--manifest", manifest]);
            }

            args.AddRange(["--registry", ExecutionContext.Instance.ProjectConfig.NPMRegistry.ToString()]);
            args.AddRange(["--version", version]);

            var npxCommand = new NPXCommand
            {
                Command = "workflow-migration-tool",
                Args = [.. args],
                ForceColorOutput = true,
                DisplayName = "Workflow Migration Tool",
                WorkingDirectory = fileSystem.Directory.GetParent(cmfPackage.GetFileInfo().FullName)
            };

            try
            {
                npxCommand.Exec();
                Log.Information("Migration complete.");
            }
            catch (Exception ex)
            {
                throw new CliException($"Workflow migration failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Write the JSON file while preserving the original indentation.
        /// </summary>
        /// <param name="jsonPath">Path of the JSON file.</param>
        /// <param name="jsonText">Contents of the JSON file in string form.</param>
        /// <param name="jsonObject">Contents of the JSON file in JObject form.</param>
        /// <param name="fileSystem">The file system abstraction used to access files.</param>
        internal static void SerializeWithOriginalIndentation(string jsonPath, string jsonText, JObject jsonObject, IFileSystem fileSystem)
        {
            // Get the leading whitespace of the second JSON line (it should have exactly one level of indentation)
            string secondLine = jsonText.Split('\n').ElementAtOrDefault(1);

            int indentationCount = 2;
            char indentationChar = ' ';

            if (!string.IsNullOrEmpty(secondLine))
            {
                if (secondLine.StartsWith('\t'))
                {
                    indentationCount = 1;
                    indentationChar = '\t';
                }
                else if (secondLine.StartsWith(' '))
                {
                    indentationCount = Regex.Match(secondLine, @"^\s*").Value.Length; // Get the number of leading white space characters

                    indentationCount = (indentationCount <= 2) ? 2 : 4; // Force indentation to either be 2 or 4 spaces
                }
            }

            StringWriter stringWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = indentationCount,
                IndentChar = indentationChar,
            };

            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(jsonWriter, jsonObject);

            fileSystem.File.WriteAllText(jsonPath, stringWriter.ToString());
        }
    }
}
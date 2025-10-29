using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Cmf.CLI.Core.Enums;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;

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
        ///     If false, all packages starting with Cmf. will be updated, (excluding Cmf.Common.TestUtilities and Cmf.Common.TestFramework.ConnectIoT).
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
                // Only update Cmf.* references (excluding Cmf.Common.TestUtilities and Cmf.Common.TestFramework.ConnectIoT)
                pattern = @"(Include=""Cmf\.(?!Common\.TestUtilities|Common\.TestFramework\.ConnectIoT)[^""]*""\s+Version="")(.*?)(""[\s/>])";
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
        /// Updates version references in IoT master data and automation workflow files
        /// within a given package, skipping specific packages if configured.
        /// </summary>
        /// <param name="fileSystem">The file system abstraction used to access files.</param>
        /// <param name="cmfPackage">The CMF package being processed.</param>
        /// <param name="version">The new Base version.</param>
        /// <param name="iotPackagesToIgnore">List of package names to ignore during the update (e.g., custom tasks).</param>
        public static void UpdateIoTMasterdatasAndWorkflows(IFileSystem fileSystem, CmfPackage cmfPackage, string version, List<string> iotPackagesToIgnore)
        {
            /// You might be wondering why the ignorePackages starts with these three packages by default:
            /// 
            /// When I was testing this command on a bunch of projects, 
            /// I noticed that the version of these template packages were being inadvertently changed on a lot of projects.
            /// 
            /// While one is able to use the --iotPackagesToIgnore flag to ignore these packages,
            /// by adding these packages to the ignore list by default we make the usage of this command more ergonomic.
            
            List<string> ignorePackages = new List<string>()
            {
                "@criticalmanufacturing/connect-iot-controller-engine-custom-utilities-tasks", // SMT Template
                "@criticalmanufacturing/connect-iot-controller-engine-custom-smt-utilities-tasks", // SMT Template
                "@criticalmanufacturing/connect-iot-utilities-semi-tasks", // Semi Template
            };
            ignorePackages.AddRange(iotPackagesToIgnore ?? []);

            // Useful debug info
            Log.Debug("Packages that will be ignored:");
            ignorePackages.ForEach(pkg => Log.Debug($"  - {pkg}"));

            List<string> mdlFiles = new List<string>();
            List<string> workflowFiles = new List<string>();

            foreach (ContentToPack contentToPack in cmfPackage.ContentToPack ?? [])
            {
                if (contentToPack.Source?.Contains(@"$(version)") ?? false)
                {
                    Log.Warning("Source paths with \"$(version)\" in cmf packages will be ignored");
                    continue;
                }

                if (contentToPack.ContentType == ContentType.MasterData)
                {
                    mdlFiles.AddRange(fileSystem.Directory.GetFiles(
                        cmfPackage.GetFileInfo().DirectoryName,
                        contentToPack.Source,
                        SearchOption.AllDirectories
                    ));
                }
                else if (contentToPack.ContentType == ContentType.AutomationWorkFlows)
                {
                    workflowFiles.AddRange(fileSystem.Directory.GetFiles(
                        cmfPackage.GetFileInfo().DirectoryName,
                        contentToPack.Source,
                        SearchOption.AllDirectories
                    ));
                }
            }

            if (mdlFiles.Where(mdl => !mdl.EndsWith(".json")).Any())
            {
                Log.Warning("Only .json masterdata files will be updated");
            }

            // Update the MES references that might be present in the mdl files
            foreach (string mdlPath in mdlFiles.Where(mdl => mdl.EndsWith(".json")))
            {
                UpdateIoTMasterdata(mdlPath, fileSystem, cmfPackage, version, ignorePackages);
            }

            Log.Debug("Processing workflows...");
            // Update the IoT workflows
            foreach (string wflPath in workflowFiles.Where(path => path.EndsWith(".json")))
            {
                UpdateIoTWorkflow(wflPath, fileSystem, cmfPackage, version, ignorePackages);
            }
        }

        /// <summary>
        /// Updates version values in a given IoT master data (.json) file.
        /// </summary>
        /// <param name="mdlPath">Path to the master data file.</param>
        /// <param name="fileSystem">The file system abstraction used to access files.</param>
        /// <param name="cmfPackage">The CMF package that owns the master data.</param>
        /// <param name="version">The new Base version.</param>
        /// <param name="ignorePackages">List of task package names to ignore.</param>
        private static void UpdateIoTMasterdata(string mdlPath, IFileSystem fileSystem, CmfPackage cmfPackage, string version, List<string> ignorePackages)
        {
            // Update some versions in several places in the masterdata
            string text = fileSystem.File.ReadAllText(mdlPath);
            foreach (string key in new string[] { "PackageVersion", "ControllerPackageVersion", "MonitorPackageVersion", "ManagerPackageVersion" })
            {
                text = UpgradeBaseUtilities.UpdateJsonValue(text, key, version);
            }

            // Updating the versions in <DM>AutomationController requires special handling
            JObject packageJsonObject = JsonConvert.DeserializeObject<JObject>(text);

            if (packageJsonObject.ContainsKey("<DM>AutomationController"))
            {
                JObject automationControllers = packageJsonObject["<DM>AutomationController"] as JObject;

                foreach (JProperty prop in automationControllers.Properties())
                {
                    UpdateTaskLibraryPackageJson(prop, version, ignorePackages);
                }
            }

            SerializeWithOriginalIndentation(mdlPath, text, packageJsonObject, fileSystem);
        }

        /// <summary>
        /// Updates the version of package strings listed in the "TasksLibraryPackages" field of a given <DM>AutomationController JSON property.
        /// </summary>
        /// <param name="prop">The JSON property representing a controller entry within the "<DM>AutomationController" object.</param>
        /// <param name="version">The new version string to apply to all eligible package entries.</param>
        /// <param name="ignorePackages"> A list of package name substrings to exclude from version updates.
        /// Any package string that contains a substring from this list will be skipped.
        /// </param>
        /// <remarks>
        /// The "TasksLibraryPackages" field may be stored either as a JSON array or as a stringified JSON array.
        /// This method detects the format and updates the version suffix (e.g., "@11.1.3") for each package string,
        /// while preserving the original format of the field (string or array). I have no idea why are two different
        /// formats are allowed in this field, but now we're stuck supporting them both
        /// </remarks>
        private static void UpdateTaskLibraryPackageJson(JProperty prop, string version, List<string> ignorePackages)
        {
            JObject controller = (JObject)prop.Value;
            JToken tasksLibraryPackagesToken = controller["TasksLibraryPackages"];

            if (tasksLibraryPackagesToken == null || tasksLibraryPackagesToken.Type == JTokenType.Null)
            {
                return;
            }

            JArray tasksLibraryPackages = null;
            bool wasStringFormat = false;

            if (tasksLibraryPackagesToken.Type == JTokenType.String)
            {
                string rawString = tasksLibraryPackagesToken.ToString();
                if (!string.IsNullOrWhiteSpace(rawString))
                {
                    tasksLibraryPackages = JsonConvert.DeserializeObject<JArray>(rawString);
                    wasStringFormat = true;
                }
            }
            else if (tasksLibraryPackagesToken.Type == JTokenType.Array)
            {
                tasksLibraryPackages = (JArray)tasksLibraryPackagesToken;
            }

            if (tasksLibraryPackages == null)
            {
                return;
            }

            for (int i = 0; i < tasksLibraryPackages.Count; i++)
            {
                string packageStr = tasksLibraryPackages[i]?.ToString();

                if (string.IsNullOrEmpty(packageStr) || ignorePackages.Any(ignore => packageStr.Contains(ignore)))
                {
                    continue;
                }
                tasksLibraryPackages[i] = Regex.Replace(packageStr, @"@\d+.*$", $"@{version}");
            }

            if (wasStringFormat)
            {
                // Write back as a compact JSON string
                controller["TasksLibraryPackages"] = JsonConvert.SerializeObject(tasksLibraryPackages, Formatting.None);
            }
            else
            {
                // Preserve original JArray structure
                controller["TasksLibraryPackages"] = tasksLibraryPackages;
            }
        }

        /// <summary>
        /// Updates the package version references in an IoT automation workflow file, 
        /// skipping any package names included in the ignore list.
        /// </summary>
        /// <param name="wflPath">Path to the workflow .json file.</param>
        /// <param name="fileSystem">The file system abstraction used to access files.</param>
        /// <param name="cmfPackage">The CMF package that owns the workflow.</param>
        /// <param name="version">The new Base version.</param>
        /// <param name="ignorePackages">List of task package names to skip when applying the version update.</param>
        private static void UpdateIoTWorkflow(string wflPath, IFileSystem fileSystem, CmfPackage cmfPackage, string version, List<string> ignorePackages)
        {
            Log.Debug($"  - {wflPath}");
            string packageJson = fileSystem.File.ReadAllText(wflPath);
            dynamic packageJsonObject = JsonConvert.DeserializeObject(packageJson);

            if (!packageJsonObject.ContainsKey("tasks"))
            {
                throw new CliException(string.Format(CoreMessages.MissingMandatoryPropertyInFile, "tasks", wflPath));
            }
            if (!packageJsonObject.ContainsKey("converters"))
            {
                throw new CliException(string.Format(CoreMessages.MissingMandatoryPropertyInFile, "converters", wflPath));
            }

            foreach (var task in packageJsonObject?["tasks"])
            {
                string name = (string)task["reference"]["package"]["name"];
                if (ignorePackages.Any(ignore => name.Contains(ignore)))
                {
                    continue; // If there's a match with a package in the ignorePackages, skip the version bump
                }

                task["reference"]["package"]["version"] = version;
            }

            foreach (var converter in packageJsonObject?["converters"])
            {
                string name = (string)converter["reference"]["package"]["name"];
                if (ignorePackages.Any(ignore => name.Contains(ignore)))
                {
                    continue; // If there's a match with a package in the ignorePackages, skip the version bump
                }

                converter["reference"]["package"]["version"] = version;
            }

            SerializeWithOriginalIndentation(wflPath, packageJson, packageJsonObject, fileSystem);
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
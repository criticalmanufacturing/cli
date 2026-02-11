using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Cmf.CLI.Core;
using Newtonsoft.Json.Linq;

namespace Cmf.CLI.Utilities
{
    /// <summary>
    ///
    /// </summary>
    public static class IoTUtilities
    {
        /// <summary>
        /// Bumps the workflow files.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version of the build (v-b).</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="packageNames">The package names.</param>
        /// <param name="fileSystem">the underlying file system</param>
        public static void BumpWorkflowFiles(string group, string version, string buildNr, string workflowName, string packageNames, IFileSystem fileSystem)
        {
            List<string> workflowFiles = fileSystem.Directory.GetFiles(group, "*.json").ToList();

            if (!string.IsNullOrEmpty(workflowName) && workflowFiles.Any(wf => wf.Contains(workflowName)))
            {
                workflowFiles = workflowFiles.Where(wf => wf.Contains(workflowName)).ToList();
            }

            foreach (var workflowFile in workflowFiles)
            {
                string packageJson = fileSystem.File.ReadAllText(workflowFile);
                JObject packageJsonObject = JsonConvert.DeserializeObject<JObject>(packageJson)
                    ?? throw new InvalidOperationException("Invalid JSON");

                var tasksArray = packageJsonObject["tasks"] as JArray ?? [];

                foreach (var tasks in tasksArray)
                {
                    string? packageName = tasks["reference"]?["package"]?["name"]?.ToString()?.Split("/")[1]?.Replace("connect-iot-", "");
                    if (packageName is not null && 
                        (packageNames == null || !string.IsNullOrEmpty(packageNames) && packageNames.Contains(packageName)))
                    {
                        string? currentVersion = tasks["reference"]?["package"]?["version"]?.ToString().Split("-")[0];
                        if (currentVersion is null)
                        {
                            Log.Warning($"Could not find version for package {packageName} in workflow {workflowFile}. Skipping version bump for this package.");
                            continue;
                        }
                        tasks["reference"]?["package"]?["version"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                    }
                }

                var convertersArray = packageJsonObject["converters"] as JArray ?? [];

                foreach (var converters in convertersArray)
                {
                    string? packageName = converters["reference"]?["package"]?["name"]?.ToString()?.Split("/")[1]?.Replace("connect-iot-", "");
                    if (packageName is not null && 
                        (packageNames == null || !string.IsNullOrEmpty(packageNames) && packageNames.Contains(packageName)))
                    {
                        string? currentVersion = converters["reference"]?["package"]?["version"]?.ToString().Split("-")[0];
                        if (currentVersion is null)
                        {
                            Log.Warning($"Could not find version for package {packageName} in workflow {workflowFile}. Skipping version bump for this package.");
                            continue;
                        }
                        converters["reference"]?["package"]?["version"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                    }
                }

                fileSystem.File.WriteAllText(workflowFile, JsonConvert.SerializeObject(packageJsonObject, Formatting.Indented));
            }
        }

        /// <summary>
        /// Bumps the io t master data.
        /// </summary>
        /// <param name="automationWorkflowFileGroup">The automation workflow file group.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version of the build (v-b).</param>
        /// <param name="packageNames">The package names.</param>
        /// <param name="onlyCustomization">if set to <c>true</c> [only customization].</param>
        /// <param name="fileSystem">the underlying file system</param>
        public static void BumpIoTMasterData(string automationWorkflowFileGroup, string version, string buildNr, IFileSystem fileSystem, string? packageNames = null, bool onlyCustomization = true)
        {
            IDirectoryInfo parentDirectory =
                fileSystem.Directory.GetParent(automationWorkflowFileGroup)
                ?? throw new ArgumentException(
                    $"Path '{automationWorkflowFileGroup}' has no parent directory",
                    nameof(automationWorkflowFileGroup));            
            List<string> jsonMasterDatas = fileSystem.Directory.GetFiles(parentDirectory.FullName, "*.json").ToList();

            foreach (var jsonMasterData in jsonMasterDatas)
            {
                string jsonFile = fileSystem.File.ReadAllText(jsonMasterData);
                JObject jsonObject =
                    JsonConvert.DeserializeObject<JObject>(jsonFile)
                    ?? throw new InvalidOperationException("Invalid JSON");
                if (onlyCustomization && !String.IsNullOrEmpty(packageNames))
                {
                    var automationProtocols =
                        jsonObject["<DM>AutomationProtocol"] as JArray
                        ?? [];
                    // Update Automation Protocol
                    
                    foreach (var token in automationProtocols)
                    {
                        if (token is not JObject protocol)
                            throw new InvalidOperationException("Invalid JSON structure for Automation Protocol");
                        
                        // Assumes @instance/packageName
                        var packageName = protocol["Package"]?.ToString()?.Split('/')?.ElementAtOrDefault(1);

                        var currentVersion = protocol["PackageVersion"]?.ToString()?.Split('-')?.FirstOrDefault();

                        if (!string.IsNullOrEmpty(packageName) &&
                            !string.IsNullOrEmpty(currentVersion) &&
                            packageNames?.Contains(packageName) == true)
                        {
                            protocol["PackageVersion"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                        }
                    }
                }
                else if (!onlyCustomization)
                {
                    // Update Automation Protocol
                    var automationProtocols =
                        jsonObject["<DM>AutomationProtocol"] as JArray ?? [];
                    foreach (var token in automationProtocols)
                    {
                        if (token is not JObject protocol)
                            throw new InvalidOperationException("Invalid JSON structure for AutomationProtocol");
                        // Assumes @instance/packageName
                        var packageName = protocol["Package"]?.ToString()?.Split('/').ElementAtOrDefault(1);                        
                        var currentVersion = protocol["PackageVersion"]?.ToString()?.Split('-').FirstOrDefault();

                        if (!string.IsNullOrEmpty(packageName) &&
                            !string.IsNullOrEmpty(currentVersion) &&
                            packageNames?.Contains(packageName) == true)
                        {
                            protocol["PackageVersion"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);                        }
                    }
                    // Update Automation Manager
                    var automationManagers = jsonObject["<DM>AutomationManager"] as JArray ?? [];
                    foreach (var token in automationManagers)
                    {
                        if (token is not JObject manager)
                            throw new InvalidOperationException("Invalid JSON structure for AutomationManager");

                        var currentVersion = manager["ManagerPackageVersion"]?.ToString()?.Split('-').FirstOrDefault();

                        if (!string.IsNullOrEmpty(currentVersion))
                        {
                            var newVersion = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);

                            manager["ManagerPackageVersion"] = newVersion;
                            manager["MonitorPackageVersion"] = newVersion;
                        }
                    }

                    // Update Automation Controller
                    var automationControllers = jsonObject["<DM>AutomationController"] as JArray ?? [];

                    foreach (var token in automationControllers)
                    {
                        if (token is not JObject controller)
                            throw new InvalidOperationException("Invalid JSON structure for AutomationController");

                        var currentVersion = controller["ControllerPackageVersion"]?.ToString()?.Split('-').FirstOrDefault();

                        if (!string.IsNullOrEmpty(currentVersion))
                        {
                            controller["ControllerPackageVersion"] = 
                                GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                        }
                    }
                }
            }

            string[] xmlMasterDatas = fileSystem.Directory.GetFiles(parentDirectory.FullName, "*.xml");
            string[] xlsxMasterDatas = fileSystem.Directory.GetFiles(parentDirectory.FullName, "*.xml");

            if (xmlMasterDatas == null || xmlMasterDatas.Length == 0)
            {
                Log.Warning($"Beware XML MasterData is not supported for IoT Bump");
            }
            if (xlsxMasterDatas == null || xlsxMasterDatas.Length == 0)
            {
                Log.Warning($"Beware XLSX (Excel) MasterData is not supported for IoT Bump");
            }
        }

        /// <summary>
        /// Bumps the iot custom packages.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version of the build (v-b).</param>
        /// <param name="packageNames">The package names.</param>
        /// <param name="fileSystem">the underlying file system</param>
        public static void BumpIoTCustomPackages(string packagePath, string version, string buildNr, string packageNames, IFileSystem fileSystem)
        {
            string[] iotPackages = fileSystem.Directory.GetDirectories(packagePath + "/src/", "*");

            foreach (string iotPackage in iotPackages)
            {
                string packageName = iotPackage.Split("/").Last();
                if (String.IsNullOrEmpty(packageNames) || packageNames.Contains(packageName))
                {
                    // Change Package Json
                    if (fileSystem.File.Exists(iotPackage + "/package.json"))
                    {
                        string packageJson = fileSystem.File.ReadAllText(iotPackage + "/package.json");
                        JObject packageJsonObject = 
                            JsonConvert.DeserializeObject<JObject>(packageJson) ?? 
                            throw new InvalidOperationException("Invalid JSON");
                        packageJsonObject["version"] = GenericUtilities.RetrieveNewVersion(packageJsonObject["version"]?.ToString(), version, buildNr);

                        packageJson = JsonConvert.SerializeObject(packageJsonObject, Formatting.Indented);
                        fileSystem.File.WriteAllText(iotPackage + "/package.json", packageJson);
                    }

                    // Change Metadata.ts
                    if (fileSystem.File.Exists(iotPackage + "/src/metadata.ts"))
                    {
                        string metadata = fileSystem.File.ReadAllText(iotPackage + "\\src\\metadata.ts");

                        var metadataVersion = Regex.Match(metadata, "version: \\\"(.*?)\\\",", RegexOptions.Singleline)?.Value?.Split("version: \"")[1]?.Split("\",")[0];
                            
                        metadata = Regex.Replace(metadata, "version: \\\"(.*?)\\\",", $"version: \"{GenericUtilities.RetrieveNewVersion(metadataVersion, version, buildNr)}\",");

                        fileSystem.File.WriteAllText(iotPackage + "/src/metadata.ts", metadata);
                    }
                }
            }
        }

    }
}
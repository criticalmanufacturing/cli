using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cmf.Common.Cli.Utilities
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
        public static void BumpWorkflowFiles(string group, string version, string buildNr, string workflowName, string packageNames)
        {
            List<string> workflowFiles = Directory.GetFiles(group, "*.json").ToList();

            if (!string.IsNullOrEmpty(workflowName) && workflowFiles.Any(wf => wf.Contains(workflowName)))
            {
                workflowFiles = workflowFiles.Where(wf => wf.Contains(workflowName))?.ToList();
            }

            foreach (var workflowFile in workflowFiles)
            {
                string packageJson = File.ReadAllText(workflowFile);
                dynamic packageJsonObject = JsonConvert.DeserializeObject(packageJson);

                foreach (var tasks in packageJsonObject?["tasks"])
                {
                    string packageName = tasks["reference"]["package"]["name"]?.ToString()?.Split("/")[1]?.Replace("connect-iot-", "");
                    if (packageNames == null || !string.IsNullOrEmpty(packageNames) && packageNames.Contains(packageName))
                    {
                        string currentVersion = tasks["reference"]["package"]["version"]?.ToString().Split("-")[0];

                        tasks["reference"]["package"]["version"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                    }
                }

                foreach (var converters in packageJsonObject?["converters"])
                {
                    string packageName = converters["reference"]["package"]["name"]?.ToString()?.Split("/")[1]?.Replace("connect-iot-", "");
                    if (packageNames == null || !string.IsNullOrEmpty(packageNames) && packageNames.Contains(packageName))
                    {
                        string currentVersion = converters["reference"]["package"]["version"]?.Split("-")[0];

                        converters["reference"]["package"]["version"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                    }
                }

                File.WriteAllText(workflowFile, JsonConvert.SerializeObject(packageJsonObject, Formatting.Indented));
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
        public static void BumpIoTMasterData(string automationWorkflowFileGroup, string version, string buildNr, string packageNames = null, bool onlyCustomization = true)
        {
            DirectoryInfo parentDirectory = Directory.GetParent(automationWorkflowFileGroup);
            List<string> jsonMasterDatas = Directory.GetFiles(parentDirectory.FullName, "*.json").ToList();

            foreach (var jsonMasterData in jsonMasterDatas)
            {
                string jsonFile = File.ReadAllText(jsonMasterData);
                dynamic jsonObject = JsonConvert.DeserializeObject(jsonFile);

                if (onlyCustomization && !String.IsNullOrEmpty(packageNames))
                {
                    var automationProtocols = jsonObject?["<DM>AutomationProtocol"] ?? new string[] { };

                    // Update Automation Protocol
                    for (int i = 1; i <= automationProtocols.Length; i++)
                    {
                        // Assumes @instance/packageName
                        string packageName = automationProtocols?[i.ToString()]?["Package"]?.ToString()?.Split("/")[1];
                        string currentVersion = automationProtocols?[i.ToString()]?["PackageVersion"]?.ToString()?.Split("-")[0];

                        if (!String.IsNullOrEmpty(automationProtocols?[i.ToString()]?["Package"]?.ToString()) &&
                                packageNames.Contains(packageName))
                        {
                            automationProtocols[i.ToString()]["PackageVersion"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                        }
                    }
                }
                else if (!onlyCustomization)
                {
                    // Update Automation Protocol
                    var automationProtocols = jsonObject?["<DM>AutomationProtocol"] ?? new string[] { };
                    for (int i = 1; i <= automationProtocols.Length; i++)
                    {
                        // Assumes @instance/packageName
                        string packageName = automationProtocols?[i.ToString()]?["Package"]?.ToString()?.Split("/")[1];
                        string currentVersion = automationProtocols?[i.ToString()]?["PackageVersion"]?.ToString()?.Split("-")[0];

                        if (!String.IsNullOrEmpty(automationProtocols?[i.ToString()]?["Package"]?.ToString()) &&
                                packageNames.Contains(packageName))
                        {
                            automationProtocols[i.ToString()]["PackageVersion"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                        }
                    }
                    // Update Automation Manager
                    var automationManagers = jsonObject?["<DM>AutomationManager"] ?? new string[] { };
                    for (int i = 1; i <= automationManagers.Length; i++)
                    {
                        string currentVersion = automationManagers?[i.ToString()]?["ManagerPackageVersion"]?.ToString()?.Split("-")[0];
                        if (!String.IsNullOrEmpty(automationManagers?[i.ToString()]?["Package"]?.ToString()))
                        {
                            automationManagers[i.ToString()]["ManagerPackageVersion"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                            automationManagers[i.ToString()]["MonitorPackageVersion"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                        }
                    }

                    // Update Automation Controller
                    var automationControllers = jsonObject["<DM>AutomationController"];
                    for (int i = 1; i <= automationControllers.Length; i++)
                    {
                        string currentVersion = automationManagers?[i.ToString()]?["ControllerPackageVersion"]?.ToString()?.Split("-")[0];
                        if (!String.IsNullOrEmpty(automationManagers?[i.ToString()]?["Package"]?.ToString()))
                        {
                            automationManagers[i.ToString()]["ControllerPackageVersion"] = GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr);
                        }
                    }
                }
            }

            string[] xmlMasterDatas = Directory.GetFiles(parentDirectory.FullName, "*.xml");
            string[] xlsxMasterDatas = Directory.GetFiles(parentDirectory.FullName, "*.xml");

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
        public static void BumpIoTCustomPackages(string packagePath, string version, string buildNr, string packageNames)
        {
            string[] iotPackages = Directory.GetDirectories(packagePath + "/src/", "*");

            foreach (string iotPackage in iotPackages)
            {
                string packageName = iotPackage.Split("/").Last();
                if (String.IsNullOrEmpty(packageNames) || packageNames.Contains(packageName))
                {
                    // Change Package Json
                    if (File.Exists(iotPackage + "/package.json"))
                    {
                        string packageJson = File.ReadAllText(iotPackage + "/package.json");
                        dynamic packageJsonObject = JsonConvert.DeserializeObject(packageJson);

                        packageJsonObject["version"] = GenericUtilities.RetrieveNewVersion(packageJsonObject["version"].ToString(), version, buildNr);

                        packageJson = JsonConvert.SerializeObject(packageJsonObject, Formatting.Indented);
                        File.WriteAllText(iotPackage + "/package.json", packageJson);
                    }

                    // Change Metadata.ts
                    if (File.Exists(iotPackage + "/src/metadata.ts"))
                    {
                        string metadata = File.ReadAllText(iotPackage + "\\src\\metadata.ts");

                        var metadataVersion = Regex.Match(metadata, "version: \\\"(.*?)\\\",", RegexOptions.Singleline)?.Value?.Split("version: \"")[1]?.Split("\",")[0];
                            
                        metadata = Regex.Replace(metadata, "version: \\\"(.*?)\\\",", $"version: \"{GenericUtilities.RetrieveNewVersion(metadataVersion, version, buildNr)}\",");

                        File.WriteAllText(iotPackage + "/src/metadata.ts", metadata);
                    }
                }
            }
        }

    }
}
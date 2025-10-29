
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="DataPackageTypeHandlerV2" />
    public class IoTDataPackageTypeHandlerV2 : DataPackageTypeHandlerV2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPackageTypeHandlerV2" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public IoTDataPackageTypeHandlerV2(CmfPackage cmfPackage) : base(cmfPackage)
        {
        }

        /// <summary>
        /// Bumps the specified CMF package.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="bumpInformation">The bump information.</param>
        public override void Bump(string version, string buildNr, Dictionary<string, object> bumpInformation = null)
        {
            base.Bump(version, buildNr, bumpInformation);
            // Get All AutomationWorkflowFiles Folders
            List<string> automationWorkflowDirectory = this.fileSystem.Directory.GetDirectories(CmfPackage.GetFileInfo().DirectoryName, "AutomationWorkflowFiles", SearchOption.AllDirectories).ToList();

            // Get Parent Root
            IDirectoryInfo parentRootDirectory = FileSystemUtilities.GetPackageRootByType(CmfPackage.GetFileInfo().DirectoryName, PackageType.Root, this.fileSystem);
            CmfPackageCollection cmfPackageIoT = parentRootDirectory.LoadCmfPackagesFromSubDirectories(packageType: PackageType.IoT);

            #region GetCustomPackages

            // Get Dev Tasks
            string packageNames = null;
            foreach (CmfPackage iotPackage in cmfPackageIoT)
            {
                packageNames += GetCustomPackages(iotPackage);
            }

            #endregion GetCustomPackages

            #region Filter by Root

            if (bumpInformation.ContainsKey("root") && !String.IsNullOrEmpty(bumpInformation["root"] as string))
            {
                string root = bumpInformation["root"] as string;
                if (!automationWorkflowDirectory.Any())
                {
                    Log.Warning($"No AutomationWorkflowFiles found in root {root}");
                }
                // Get All AutomationWorkflowFiles Folders that are under root
                automationWorkflowDirectory = automationWorkflowDirectory.Where(awf => awf.Contains(root))?.ToList() ?? new();
            }

            #endregion Filter by Root

            foreach (string automationWorkflowFileGroup in automationWorkflowDirectory)
            {
                #region Bump AutomationWorkflow

                // Get All Group Folders
                List<string> groups = this.fileSystem.Directory.GetDirectories(automationWorkflowFileGroup, "*").ToList();

                groups.ForEach(group => IoTUtilities.BumpWorkflowFiles(group, version, buildNr, null, packageNames, this.fileSystem));

                #endregion Bump AutomationWorkflow

                #region Bump IoT Masterdata

                IoTUtilities.BumpIoTMasterData(automationWorkflowFileGroup, version, buildNr, this.fileSystem, packageNames, onlyCustomization: true);

                #endregion Bump IoT Masterdata
            }
        }

        /// <summary>
        /// Bumps the Base version of the package
        /// </summary>
        /// <param name="version">The new Base version.</param>
        public override void UpgradeBase(string version, string iotVersion, List<string> iotPackagesToIgnore)
        {
            base.UpgradeBase(version, iotVersion, iotPackagesToIgnore);
            UpgradeBaseUtilities.UpdateCSharpProject(this.fileSystem, this.CmfPackage, version, true);

            if (iotVersion == null)
            {
                return;
            }

            UpgradeBaseUtilities.UpdateIoTMasterdatasAndWorkflows(this.fileSystem, this.CmfPackage, iotVersion, iotPackagesToIgnore);
        }

        /// <summary>
        /// Retrieves all custom iot package names
        /// </summary>
        /// <param name="iotPackage"></param>
        /// <returns></returns>
        private string GetCustomPackages(CmfPackage iotPackage)
        {
            string targetDirectory = ".dev-tasks.json";
            string targetProperties = "packages";

            if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major > 10)
            {
                targetDirectory = "package.json";
                targetProperties = "workspaces";
            }

            string packagesFile = this.fileSystem.Directory.GetFiles(iotPackage.GetFileInfo().DirectoryName, targetDirectory).FirstOrDefault();

            string contentJson = this.fileSystem.File.ReadAllText(packagesFile);
            dynamic contentObject = JsonConvert.DeserializeObject(contentJson);

            string packageNames = string.IsNullOrEmpty(contentObject["packagesBuildBump"]?.ToString()) ? contentObject[targetProperties]?.ToString() : contentObject["packagesBuildBump"]?.ToString();

            return packageNames;
        }
    }
}
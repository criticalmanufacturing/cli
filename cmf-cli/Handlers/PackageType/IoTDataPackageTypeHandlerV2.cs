using Cmf.Common.Cli.Builders;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Utils = Cmf.Common.Cli.Utilities.FileSystemUtilities;

namespace Cmf.Common.Cli.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Handlers.DataPackageTypeHandlerV2" />
    public class IoTDataPackageTypeHandlerV2 : DataPackageTypeHandlerV2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPackageTypeHandlerV2" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public IoTDataPackageTypeHandlerV2(CmfPackage cmfPackage) : base(cmfPackage)
        {
            IBuildCommand[] BuildStepsIoTData = new IBuildCommand[]
            {
                new ConsistencyCheckCommand()
                {
                    DisplayName = "Consistency Check Validator Command",
                    FileSystem = this.fileSystem,
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
        }
            }; 

            BuildSteps = BuildSteps.Concat(BuildStepsIoTData).ToArray();
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
            IDirectoryInfo parentRootDirectory = Utils.GetPackageRootByType(CmfPackage.GetFileInfo().DirectoryName, PackageType.Root, this.fileSystem);
            CmfPackageCollection cmfPackageIoT = parentRootDirectory.LoadCmfPackagesFromSubDirectories(packageType: PackageType.IoT);

            #region GetCustomPackages

            // Get Dev Tasks
            string packageNames = null;
            string devTasksJson = null;
            dynamic devTasksJsonObject;
            foreach (CmfPackage iotPackage in cmfPackageIoT)
            {
                string devTasksFile = this.fileSystem.Directory.GetFiles(iotPackage.GetFileInfo().DirectoryName, ".dev-tasks.json")[0];

                devTasksJson = this.fileSystem.File.ReadAllText(devTasksFile);
                devTasksJsonObject = JsonConvert.DeserializeObject(devTasksJson);

                packageNames += devTasksJsonObject["packagesBuildBump"]?.ToString();

                if (string.IsNullOrEmpty(packageNames))
                {
                    packageNames += devTasksJsonObject["packages"]?.ToString();
                }
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
    }
}
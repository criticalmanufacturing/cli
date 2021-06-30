using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BumpCommand" />
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
    [CmfCommand(name: "bumpIoTConfiguration")]
    public class BumpCommandIoTConfiguration : BumpCommand
    {
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "path",
                getDefaultValue: () => { return this.fileSystem.DirectoryInfo.FromDirectoryName("."); },
                description: "path"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-v", "--version" },
                description: "Will bump all versions to the version specified"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-b", "--buildNrVersion" },
                description: "Will add this version next to the version (v-b)"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "-md", "--masterData" },
                getDefaultValue: () => { return false; },
                description: "Will bump IoT MasterData version (only applies to .json)"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "-iot", "--iot" },
                getDefaultValue: () => { return true; },
                description: "Will bump IoT Automation Workflows"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-pckNames", "--packageNames" },
                description: "Packages to be bumped"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-r", "--root" },
                description: "Specify root to specify version where we want to apply the bump"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-g", "--group" },
                description: "Group of workflows to change, typically they are grouped by Automation Manager"));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-wkflName", "--workflowName" },
                description: "Specific workflow to be bumped"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "-isToTag", "--isToTag" },
                getDefaultValue: () => { return false; },
                description: "Instead of replacing the version will add -$version"));

            cmd.AddOption(new Option<bool>(
                aliases: new string[] { "-mdCustomization", "--mdCustomization" },
                getDefaultValue: () => { return false; },
                description: "Instead of replacing the version will add -$version"));

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, string, bool, bool, string, string, string, string, bool, bool>(Execute);
        }

        /// <summary>
        /// Executes the specified package directory.
        /// </summary>
        /// <param name="packageDirectory">The package directory.</param>
        /// <param name="version">The version.</param>
        /// <param name="buildNr"></param>
        /// <param name="isToBumpMasterdata">if set to <c>true</c> [is to bump masterdata].</param>
        /// <param name="isToBumpIoT">if set to <c>true</c> [is to bump io t].</param>
        /// <param name="packageNames">The package names.</param>
        /// <param name="root">The root.</param>
        /// <param name="group">The group.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="isToTag">if set to <c>true</c> [is to tag].</param>
        /// <param name="onlyMdCustomization">if set to <c>true</c> [only md customization].</param>
        /// <returns></returns>
        public void Execute(IDirectoryInfo packageDirectory, string version, string buildNr, bool isToBumpMasterdata, bool isToBumpIoT, string packageNames, string root, string group, string workflowName, bool isToTag, bool onlyMdCustomization)
        {
            // Get All AutomationWorkflowFiles Folders
            List<string> automationWorkflowDirectories = this.fileSystem.Directory.GetDirectories(packageDirectory.FullName, "AutomationWorkflowFiles").ToList();

            if (!String.IsNullOrEmpty(root))
            {
                if (!automationWorkflowDirectories.Any())
                {
                    Log.Warning($"No AutomationWorkflowFiles found in root {root}");
                }
                // Get All AutomationWorkflowFiles Folders that are under root
                automationWorkflowDirectories = automationWorkflowDirectories.Where(awf => awf.Contains(root))?.ToList();
            }

            foreach (string automationWorkflowDirectory in automationWorkflowDirectories)
            {
                #region Bump AutomationWorkflow

                if (isToBumpIoT)
                {
                    // Get All Group Folders
                    List<string> groups = this.fileSystem.Directory.GetDirectories(automationWorkflowDirectory, "*").ToList();
                    if (!String.IsNullOrEmpty(group) && groups.Any(gr => gr.Contains(group)))
                    {
                        // Get All Group Folders that are called group
                        groups = groups.Where(gr => gr.Contains(group)).ToList();
                    }

                    groups.ForEach(group => IoTUtilities.BumpWorkflowFiles(group, version, buildNr, workflowName, packageNames, this.fileSystem));
                }

                #endregion Bump AutomationWorkflow

                #region Bump IoT Masterdata

                if (isToBumpMasterdata)
                {
                    IoTUtilities.BumpIoTMasterData(automationWorkflowDirectory, version, buildNr, this.fileSystem, onlyCustomization: onlyMdCustomization);
                }

                #endregion Bump IoT Masterdata
            }
        }
    }
}
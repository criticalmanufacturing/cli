using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="BumpCommand" />
    /// <seealso cref="BaseCommand" />
    [CmfCommand(name: "configuration", ParentId = "bump_iot")]
    public class BumpIoTConfigurationCommand : BumpCommand
    {
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var pathArgument = new Argument<IDirectoryInfo>("path")
            {
                Description = "Working Directory"
            };
            pathArgument.CustomParser = argResult => Parse<IDirectoryInfo>(argResult, ".");
            pathArgument.DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, ".");
            cmd.Arguments.Add(pathArgument);

            var versionOption = new Option<string>("--version", "-v")
            {
                Description = "Will bump all versions to the version specified"
            };
            cmd.Options.Add(versionOption);

            var buildNrVersionOption = new Option<string>("--buildNrVersion", "-b")
            {
                Description = "Will add this version next to the version (v-b)"
            };
            cmd.Options.Add(buildNrVersionOption);

            var masterDataOption = new Option<bool>("--masterData", "-md")
            {
                Description = "Will bump IoT MasterData version (only applies to .json)",
                DefaultValueFactory = _ => false
            };
            cmd.Options.Add(masterDataOption);

            var iotOption = new Option<bool>("--iot", "-iot")
            {
                Description = "Will bump IoT Automation Workflows",
                DefaultValueFactory = _ => true
            };
            cmd.Options.Add(iotOption);

            var packageNamesOption = new Option<string>("--packageNames", "-pckNames")
            {
                Description = "Packages to be bumped"
            };
            cmd.Options.Add(packageNamesOption);

            var rootOption = new Option<string>("--root", "-r")
            {
                Description = "Specify root to specify version where we want to apply the bump"
            };
            cmd.Options.Add(rootOption);

            var groupOption = new Option<string>("--group", "-g")
            {
                Description = "Group of workflows to change, typically they are grouped by Automation Manager"
            };
            cmd.Options.Add(groupOption);

            var workflowNameOption = new Option<string>("--workflowName", "-wkflName")
            {
                Description = "Specific workflow to be bumped"
            };
            cmd.Options.Add(workflowNameOption);

            var isToTagOption = new Option<bool>("--isToTag", "-isToTag")
            {
                Description = "Instead of replacing the version will add -$version",
                DefaultValueFactory = _ => false
            };
            cmd.Options.Add(isToTagOption);

            var mdCustomizationOption = new Option<bool>("--mdCustomization", "-mdCustomization")
            {
                Description = "Instead of replacing the version will add -$version",
                DefaultValueFactory = _ => false
            };
            cmd.Options.Add(mdCustomizationOption);

            // Add the handler
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var path = parseResult.GetValue(pathArgument);
                var version = parseResult.GetValue(versionOption);
                var buildNr = parseResult.GetValue(buildNrVersionOption);
                var isToBumpMasterdata = parseResult.GetValue(masterDataOption);
                var isToBumpIoT = parseResult.GetValue(iotOption);
                var packageNames = parseResult.GetValue(packageNamesOption);
                var root = parseResult.GetValue(rootOption);
                var group = parseResult.GetValue(groupOption);
                var workflowName = parseResult.GetValue(workflowNameOption);
                var isToTag = parseResult.GetValue(isToTagOption);
                var onlyMdCustomization = parseResult.GetValue(mdCustomizationOption);

                Execute(path, version, buildNr, isToBumpMasterdata, isToBumpIoT, packageNames, root, group, workflowName, isToTag, onlyMdCustomization);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Executes the specified package directory.
        /// </summary>
        /// <param name="path">The package directory.</param>
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
        public void Execute(IDirectoryInfo path, string version, string buildNr, bool isToBumpMasterdata, bool isToBumpIoT, string packageNames, string root, string group, string workflowName, bool isToTag, bool onlyMdCustomization)
        {
            using var activity = Core.Objects.ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            
            // Get All AutomationWorkflowFiles Folders
            List<string> automationWorkflowDirectories = this.fileSystem.Directory.GetDirectories(path.FullName, "AutomationWorkflowFiles").ToList();

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
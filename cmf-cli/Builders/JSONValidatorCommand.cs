using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cmf.CLI.Builders
{
    /// <summary>
    /// Validator for json files
    /// </summary>
    /// <seealso cref="ProcessCommand" />
    /// <seealso cref="IBuildCommand" />
    public class JSONValidatorCommand : IBuildCommand
    {
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public List<FileToPack> FilesToValidate { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Only Executes on Test (--test)
        /// </summary>
        /// <value>
        /// boolean if to execute on Test should be true
        /// </value>
        public bool Test { get; set; } = false;

        /// <summary>
        /// Gets or sets the condition. 
        /// This will impact the Condition(), the Condtion will run the Func to determine if it should reply with true or false
        /// By Default it will return true
        /// </summary>
        /// <value>
        /// A Func that if it returns true it will allow the Execute to run.
        /// </value>
        /// <returns>Func<bool></returns>
        public Func<bool> ConditionForExecute = () => { return true; };

        /// <summary>
        /// This method will be used to do a run check before the Exec() is able to run.
        /// If Condition() is false, the Exec() will not be able to run
        /// If Condition() is true, the Exec() will run
        /// </summary>
        /// <returns></returns>
        public bool Condition()
        {
            return ConditionForExecute();
        }

        /// <summary>
        /// Search all the json files and validate them
        /// </summary>
        /// <returns></returns>
        public Task Exec()
        {
            if (Condition())
            {
                List<WorkflowsToValidate> workflowNamesAndPaths = [];
                List<string> subWorkflowsToValidate = [];
                foreach (var file in FilesToValidate.Where(file => file.Source.FullName.Contains(".json")))
                {
                    //Open file for Read\Write
                    Stream fs = file.Source.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

                    //Create object of StreamReader by passing FileStream object on which it needs to operates on
                    StreamReader sr = new StreamReader(fs);

                    //Use ReadToEnd method to read all the content from file
                    string fileContent = sr.ReadToEnd();

                    try
                    {
                        var json = JsonDocument.Parse(fileContent);

                        #region Collect JSON SubWorkflows to Validate
                        workflowNamesAndPaths.AddRange(ExtractWorkflowNames(json));
                        subWorkflowsToValidate.AddRange(ExtractSubWorkflowNamesFromAutomationWorkflow(json));
                        #endregion
                    }
                    catch (Exception)
                    {
                        throw new CliException($"File {file.Source.FullName} is not a valid json");
                    }

                    #region Validate IoT Workflow Paths

                    var matchCheckWorlflowFilePath = Regex.Matches(fileContent, @"Workflow"": ""(.*?)\\(.*?).json");

                    if (matchCheckWorlflowFilePath.Any())
                    {
                        throw new CliException($"JSON File {file.Source.FullName} is not a valid on '{matchCheckWorlflowFilePath.Select(x => x.Value + "/r/n").ToJson()}'. Please normalize all slashes to be forward slashes /");
                    }

                    #endregion
                }

                #region Validate all IoT JSON SubWorkflows exist
                if (subWorkflowsToValidate.Count > 0)
                {
                    AllSubWorkflowsExist(subWorkflowsToValidate, workflowNamesAndPaths, FilesToValidate.Select(file => file.Source.FullName).ToList());
                }
                #endregion
            }
            else
            {
                Log.Debug($"Command: {this.DisplayName} will not be executed as its condition was not met");
            }
            return null;
        }

        private static bool AllSubWorkflowsExist(List<string> isContainedList, List<WorkflowsToValidate> containsList, List<string> filesToValidate = null)
        {
            // Use a HashSet for fast lookups
            var set = new HashSet<string>(containsList.Select(item => item.Name));

            foreach (var item in isContainedList)
            {
                if (!set.Contains(item))
                {
                    throw new CliException($"The subworkflow {item} is mentioned but there is no workflow declared with that name.");
                }

                var workflowLocation = containsList.FirstOrDefault(clitem => clitem.Name == item);
                if (!string.IsNullOrEmpty(workflowLocation.Path) && filesToValidate != null)
                {
                    IsPartialPathPresent(workflowLocation.Path, filesToValidate, true);
                }
            }

            return true; // All items are present
        }

        private static bool IsPartialPathPresent(string partialPath, List<string> filePaths, bool throwOnFalse = true)
        {
            foreach (var fullPath in filePaths)
            {
                if (fullPath.Replace("\\", "/").Contains(partialPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true; // Return true as soon as a match is found
                }
            }

            if (throwOnFalse)
            {
                throw new CliException($"Could not find the path {partialPath} for the Workflow");
            }

            return false; // No match found
        }

        private static List<WorkflowsToValidate> ExtractWorkflowNames(JsonDocument json)
        {
            var names = new List<WorkflowsToValidate>();
            // Navigate to the "AutomationControllerWorkflow" object
            if (json.RootElement.TryGetProperty("AutomationControllerWorkflow", out JsonElement workflows) &&
                workflows.ValueKind == JsonValueKind.Object)
            {
                // Iterate through all properties in "AutomationControllerWorkflow"
                foreach (var property in workflows.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object &&
                        property.Value.TryGetProperty("Name", out JsonElement name))
                    {
                        // IsFile is deprecated in favor of `mdfile:` prefix
                        // It indicates on the property where to find the information 
                        property.Value.TryGetProperty("IsFile", out JsonElement isFile);
                        property.Value.TryGetProperty("Workflow", out JsonElement workflow);

                        // Check if the workflow has prefix
                        bool isWorkflowPath = workflow.ToString().StartsWith("mdfile:");
                        
                        if (isFile.ToString().ToBool() || isWorkflowPath)
                        {
                            string workflowValue = workflow.GetString();

                            // If has the prefix, it should strip the extention to get only the path
                            if (isWorkflowPath)
                            {
                                workflowValue = workflowValue.Replace("mdfile://", "");
                            }

                            names.Add(new WorkflowsToValidate(name.GetString(), workflowValue));
                        }
                        else
                        {
                            // In this case the workflow is already in the json file, so we don't need to keep the path
                            names.Add(new WorkflowsToValidate(name.GetString()));
                        }

                    }
                }
            }
            return names;
        }

        private static List<string> ExtractSubWorkflowNamesFromAutomationWorkflow(JsonDocument json)
        {
            var names = new List<string>();
            // Navigate to the "tasks" array
            if (json.RootElement.TryGetProperty("tasks", out JsonElement tasks) &&
                tasks.ValueKind == JsonValueKind.Array)
            {
                foreach (var task in tasks.EnumerateArray())
                {
                    // Check if the task contains "settings" and "automationWorkflow"
                    if (task.TryGetProperty("settings", out JsonElement settings) &&
                        settings.TryGetProperty("automationWorkflow", out JsonElement automationWorkflow) &&
                        automationWorkflow.ValueKind == JsonValueKind.Object)
                    {
                        // Check if "IsShared" is false and retrieve "Name"
                        if (automationWorkflow.TryGetProperty("IsShared", out JsonElement isShared) &&
                            isShared.ValueKind == JsonValueKind.False &&
                            automationWorkflow.TryGetProperty("Name", out JsonElement name))
                        {
                            names.Add(name.GetString());
                        }
                    }
                }
            }
            return names;
        }
    }

    public record WorkflowsToValidate
    {
        public string Path { get; set; }
        public string Name { get; set; }

        public WorkflowsToValidate(string name, string path = "")
        {
            this.Path = path;
            this.Name = name;
        }
    }
}

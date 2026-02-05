using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TemplateEngine.Utils;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands.New.IoT
{
    /// <summary>
    /// Generates IoT Task structure
    /// </summary>
    [CmfCommand("task", ParentId = "new_iot", Id = "iot_task")]
    public class GenerateTaskCommand : TemplateCommand
    {
        private Dictionary<string, TaskInputOutputType> Inputs = [];
        private Dictionary<string, TaskInputOutputType> Outputs = [];
        private Dictionary<string, Dictionary<string, HashSet<TaskSetting>>> Settings = [];

        /// <summary>
        /// constructor
        /// </summary>
        public GenerateTaskCommand() : base("task")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem">the filesystem implementation</param>
        public GenerateTaskCommand(IFileSystem fileSystem) : base("task", fileSystem)
        {
        }

        public override void Configure(Command cmd)
        {
            var nearestIoTPackage = FileSystemUtilities.GetPackageRootByType(
                this.fileSystem.Directory.GetCurrentDirectory(),
                PackageType.IoT,
                this.fileSystem
            );

            var workingDirArgument = new Argument<IDirectoryInfo>("workingDir")
            {
                Description = "Working Directory"
            };
            workingDirArgument.CustomParser = argResult => Parse<IDirectoryInfo>(argResult, nearestIoTPackage?.FullName);
            workingDirArgument.DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, nearestIoTPackage?.FullName);
            cmd.Arguments.Add(workingDirArgument);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var workingDir = parseResult.GetValue(workingDirArgument);
                Execute(workingDir);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir">nearest root package</param>
        /// <param name="version">package version</param>
        /// <param name="htmlPackageLocation">location of html package</param>
        public void Execute(IDirectoryInfo workingDir)
        {
            if (workingDir == null)
            {
                throw new CliException("This command needs to run inside an iot project. Run `cmf new iot` to create a new project.");
            }

            if (Core.Objects.ExecutionContext.Instance.ProjectConfig.MESVersion.Major < 11)
            {
                throw new CliException("This command is only valid for versions above 11.0.0");
            }

            using var activity = Core.Objects.ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

            var task = HandleTask(new TaskValues());

            this.HandleInputs();
            task.InputsInterface = this.HandleInputsInterface(this.Inputs);
            this.HandleOutputs();
            task.OutputsInterface = this.HandleOutputsInterface(this.Outputs);

            this.HandleSettings();
            task.Settings = this.Settings.Count > 0 ? this.Settings : task.Settings;
            this.HandleSettingsInterface(task);

            task.Inputs = this.Inputs.Concat(task.Inputs).ToDictionary(pair => pair.Key, pair => pair.Value);
            task.Outputs = this.Outputs.Concat(task.Outputs).ToDictionary(pair => pair.Key, pair => pair.Value);

            var args = this.GenerateArgs(workingDir, this.fileSystem.Directory.GetCurrentDirectory(), task.Name, task.ClassName, task.SettingsDefaults, task.TestSettingsDefaults, task.InputsInterface, task.OutputsInterface, task.SettingsInterface, task.IsProtocol.ToString());
            this.CommandName = "iot-task";
            base.RunCommand(args);

            var indexPath = this.fileSystem.Path.Join(this.fileSystem.Directory.GetCurrentDirectory(), "src/index.ts");
            this.fileSystem.File.AppendAllLines(indexPath, ["export { " + task.ClassName + "Task } from \"./tasks/" + task.Name + "/" + task.Name + ".task\";\r\n"]);

            var template = new TemplateTaskLibrary()
            {
                Tasks = [task]
            };
            var templatePath = this.fileSystem.Path.Join(this.fileSystem.Directory.GetCurrentDirectory(), $"templates/task_{task.Name}.json");
            this.fileSystem.File.WriteAllText(templatePath,
                JsonConvert.SerializeObject(template,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));
        }

        private TaskValues HandleTask(TaskValues task)
        {
            task.Name = AnsiConsole.Ask("What is the task name?", task.Name).ToCamelCase();
            task.ClassName = task.Name.ToPascalCase();
            task.Title = AnsiConsole.Ask("What is the task Title?", task.Title);
            task.Icon = AnsiConsole.Ask("What is the icon class name (if empty will use default icon icon-core-tasks-connect-iot-lg-logmessage)?", "icon-core-tasks-connect-iot-lg-logmessage");
            task.IsProtocol = AnsiConsole.Prompt(new ConfirmationPrompt("Is this task used by the protocol driver?") { DefaultValue = task.IsProtocol });
            task.IsController = true;
            if (task.IsProtocol)
            {
                task.IsController = AnsiConsole.Prompt(new ConfirmationPrompt("Will this task be also usable without the driver connection?") { DefaultValue = task.IsController });
            }

            task.Lifecycle = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What is the lifecycle of the task?")
                    .AddChoices(System.Enum.GetNames(typeof(Lifecycle))));
            if (task.Lifecycle != Lifecycle.Productive.ToString())
            {
                task.LifecycleMessage = AnsiConsole.Ask("What message should the user see regarding lifecycle?", "");
            }

            var taskLibraryPkgJsonPath = this.fileSystem.Path.Join(this.fileSystem.Directory.GetCurrentDirectory(), "package.json");
            var json = fileSystem.File.ReadAllText(taskLibraryPkgJsonPath);
            dynamic taskLibraryPkgJson = JsonConvert.DeserializeObject(json);

            string[] taskLibraryDependsOnProtocols = taskLibraryPkgJson.criticalManufacturing.tasksLibrary.dependsOnProtocol.ToObject<string[]>();
            string[] taskLibraryDependsOnScope = taskLibraryPkgJson.criticalManufacturing.tasksLibrary.dependsOnScope.ToObject<string[]>();

            task.DependsOnProtocol = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Is this task specific for any protocol?")
                    .NotRequired()
                    .AddChoices(taskLibraryDependsOnProtocols));

            task.DependsOnScope = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("On which scopes this library can be used")
                    .NotRequired()
                    .AddChoices(taskLibraryDependsOnScope));
            return task;
        }

        private string HandleInputsInterface(Dictionary<string, TaskInputOutputType> inputs)
        {
            string inputsInterface = string.Empty;
            foreach (var input in inputs)
            {
                var inputValue = input.Value;
                var defaultValue = inputValue.DefaultValue is string ? "\"\"" : JsonConvert.SerializeObject(inputValue.DefaultValue);

                inputsInterface += $"\t/** {inputValue.DisplayName} */\r\n";
                inputsInterface += $"\tpublic {input.Key}: {IoTStructures.ConvertIoTTypesToJSTypes<DataTypeInputOutput>(inputValue.DataType)} = {defaultValue};\r\n";
            }
            return inputsInterface.TrimEnd();
        }

        private string HandleOutputsInterface(Dictionary<string, TaskInputOutputType> outputs)
        {
            string outputsInterface = string.Empty;
            foreach (var output in outputs)
            {
                var outputValue = output.Value;
                var convertedDataType = IoTStructures.ConvertIoTTypesToJSTypes<DataTypeInputOutput>(outputValue.DataType);

                outputsInterface += $"\t/** {outputValue.DisplayName} */\r\n";
                outputsInterface += $"\tpublic {output.Key}: Task.Output<{convertedDataType}> = new Task.Output<{convertedDataType}>();\r\n";
            }
            return outputsInterface.TrimEnd();
        }

        private void HandleSettingsInterface(TaskValues task)
        {
            string settingsInterface = string.Empty;
            string settingsDefaults = string.Empty;
            string testSettingsDefaults = string.Empty;

            foreach (var tab in task.Settings)
            {
                foreach (var section in tab.Value)
                {
                    foreach (var setting in section.Value)
                    {
                        string comment = string.IsNullOrEmpty(setting.InfoMessage) ? string.IsNullOrEmpty(setting.DisplayName) ? string.IsNullOrEmpty(setting.Name) ? "" : setting.Name : setting.DisplayName : setting.InfoMessage;

                        settingsInterface += $"\t/** {comment} */\r\n";
                        settingsInterface += $"\t{setting.SettingKey}: {IoTStructures.ConvertIoTTypesToJSTypes<DataTypeSetting>(setting.DataType)};\r\n";

                        settingsDefaults += $"\t{setting.SettingKey}: {JsonConvert.SerializeObject(setting.DefaultValue)},\r\n";
                        testSettingsDefaults += $"\t\t\t\t{setting.SettingKey}: {JsonConvert.SerializeObject(setting.DefaultValue)},\r\n";
                    }
                }
            }

            task.SettingsInterface = settingsInterface.TrimEnd();
            task.SettingsDefaults = settingsDefaults.TrimEnd();
            task.TestSettingsDefaults = testSettingsDefaults.TrimEnd();
        }

        private void HandleInputs()
        {
            AnsiConsole.WriteLine("** Input Menu **");
            var choice = AskChoiceAndLogInputsOutputs(this.Inputs);

            while (choice != Choices.Done)
            {
                switch (choice)
                {
                    case Choices.Add:
                        {
                            AddInput(this.Inputs, new TaskInputOutputType());
                            break;
                        }
                    case Choices.Remove:
                        {
                            var inputToRemove = AnsiConsole.Prompt(
                                                new SelectionPrompt<string>()
                                                    .Title("Input to Remove")
                                                    .AddChoices(this.Inputs.Keys.Concat(["**NONE**"]).ToList()));
                            if (inputToRemove != "**NONE**")
                            {
                                this.Inputs.Remove(inputToRemove);
                            }
                            break;
                        }
                    case Choices.Edit:
                        {
                            var inputToEdit = AnsiConsole.Prompt(
                                                new SelectionPrompt<string>()
                                                    .Title("Input to Edit")
                                                    .AddChoices(this.Inputs.Keys.Concat(["**NONE**"]).ToList()));
                            if (inputToEdit != "**NONE**")
                            {
                                AddInput(this.Inputs, this.Inputs[inputToEdit]);
                            }
                            break;
                        }
                    default:
                        throw new Exception("Unknown Choice");
                }

                choice = AskChoiceAndLogInputsOutputs(this.Inputs);
            }
        }

        private void HandleOutputs()
        {
            AnsiConsole.WriteLine("** Output Menu **");
            var choice = AskChoiceAndLogInputsOutputs(this.Outputs, "Outputs");

            while (choice != Choices.Done)
            {
                switch (choice)
                {
                    case Choices.Add:
                        {
                            AddOutput(this.Outputs, new TaskInputOutputType());
                            break;
                        }
                    case Choices.Remove:
                        {
                            var outputToRemove = AnsiConsole.Prompt(
                                                new SelectionPrompt<string>()
                                                    .Title("Output to Remove")
                                                    .AddChoices(this.Outputs.Keys.Concat(["**NONE**"]).ToList()));
                            if (outputToRemove != "**NONE**")
                            {
                                this.Outputs.Remove(outputToRemove);
                            }
                            break;
                        }
                    case Choices.Edit:
                        {
                            var outputToEdit = AnsiConsole.Prompt(
                                                new SelectionPrompt<string>()
                                                    .Title("Input to Edit")
                                                    .AddChoices(this.Outputs.Keys.Concat(["**NONE**"]).ToList()));
                            if (outputToEdit != "**NONE**")
                            {
                                AddInput(this.Outputs, this.Outputs[outputToEdit]);
                            }
                            break;
                        }
                    default:
                        throw new Exception("Unknown Choice");
                }

                choice = AskChoiceAndLogInputsOutputs(this.Outputs, "Outputs");
            }
        }

        private void HandleSettings()
        {
            AnsiConsole.WriteLine("** Settings Menu **");
            var settingList = new List<string>();
            var choice = AskChoiceAndLogSettings(this.Settings);

            while (choice != Choices.Done)
            {
                switch (choice)
                {
                    case Choices.Add:
                        {
                            settingList.Add(AddSetting(this.Settings, new TaskSetting()).Name);
                            break;
                        }
                    case Choices.Remove:
                        {
                            var settingToRemove = AnsiConsole.Prompt(
                                                new SelectionPrompt<string>()
                                                    .Title("Setting to Remove")
                                                    .AddChoices(settingList.Concat(["**NONE**"]).ToList()));
                            if (settingToRemove != "**NONE**")
                            {
                                // This will remove the first setting found, if there are name collisions it may not work as expected
                                void removeSetting(Dictionary<string, Dictionary<string, HashSet<TaskSetting>>> settings)
                                {
                                    settings.ForEach(tab =>
                                    {
                                        tab.Value.ForEach(section =>
                                        {
                                            if (section.Value.Any(setting => setting.Name == settingToRemove))
                                            {
                                                section.Value.RemoveWhere(setting => setting.Name == settingToRemove);
                                                return;
                                            }
                                        });
                                    });
                                }

                                removeSetting(this.Settings);
                            }
                            break;
                        }
                    case Choices.Edit:
                        {
                            var settingToEdit = AnsiConsole.Prompt(
                                                new SelectionPrompt<string>()
                                                    .Title("Setting to Edit")
                                                    .AddChoices(settingList.Concat(["**NONE**"]).ToList()));
                            if (settingToEdit != "**NONE**")
                            {
                                // This will find the first setting, if there are name collisions it may not work as expected
                                AddSetting(this.Settings, FindSetting(this.Settings, settingToEdit));
                            }
                            break;
                        }
                    default:
                        throw new Exception("Unknown Choice");
                }

                choice = AskChoiceAndLogInputsOutputs(this.Outputs, "Outputs");
            }
        }

        private static void AddInput(Dictionary<string, TaskInputOutputType> inputs, TaskInputOutputType defaultInput)
        {
            var inputName = AnsiConsole.Ask("Input Name:", defaultInput?.DisplayName?.ToCamelCase() ?? "newInput");
            defaultInput.DisplayName = AnsiConsole.Ask("Input Display Name:", inputName.ToPascalCase());
            AnsiConsole.Write("Scaffolding only supports Static inputs, for other kinds, refer to the documentation and edit the template later on.");

            if (defaultInput.Type == TaskInputTypeType.Static)
            {
                var lastKnownDataType = defaultInput.DataType;
                defaultInput.DataType = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                    .Title("Input Data Type:")
                                    .AddChoices(Enum.GetNames(typeof(DataTypeInputOutput))));
                if (defaultInput.DataType != lastKnownDataType)
                {
                    defaultInput.DefaultValue = null;
                }
                defaultInput.DefaultValue = IoTStructures.AskDynamicType<DataTypeInputOutput>(defaultInput.DataType, "Input", defaultInput.DefaultValue);
            }

            if (inputs.ContainsKey(inputName))
            {
                AnsiConsole.Write($"Will override input {inputName}");
                inputs[inputName] = defaultInput;
            }
            else
            {
                inputs.Add(inputName, defaultInput);
            }
        }

        private static void AddOutput(Dictionary<string, TaskInputOutputType> outputs, TaskInputOutputType defaultOutput)
        {
            var outputName = AnsiConsole.Ask("Output Name:", "newOutput");
            defaultOutput.DisplayName = AnsiConsole.Ask("Output Display Name:", outputName.ToPascalCase());
            AnsiConsole.Write("Scaffolding only supports Static Outputs, for other kinds, refer to the documentation and edit the template later on.");

            if (defaultOutput.Type == TaskInputTypeType.Static)
            {
                defaultOutput.DataType = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                    .Title("Output Data Type:")
                                    .AddChoices(Enum.GetNames(typeof(DataTypeInputOutput))));
            }

            if (outputs.ContainsKey(outputName))
            {
                AnsiConsole.Write($"Will override output {outputName}");
                outputs[outputName] = defaultOutput;
            }
            else
            {
                outputs.Add(outputName, defaultOutput);
            }
        }

        private static TaskSetting AddSetting(Dictionary<string, Dictionary<string, HashSet<TaskSetting>>> settings, TaskSetting defaultSetting)
        {
            var location = AnsiConsole.Ask("Setting location (<Tab>/<Section>):", "General/Settings").Split("/");
            var tab = location[0];
            var section = location?[1] ?? location[0];

            defaultSetting.Name = AnsiConsole.Ask("Setting Name:", "newSetting");
            defaultSetting.DisplayName = AnsiConsole.Ask("Setting Display Name:", defaultSetting.Name.ToPascalCase());
            defaultSetting.SettingKey = AnsiConsole.Ask("Setting Workflow Json Key:", defaultSetting.SettingKey ?? defaultSetting.Name);
            defaultSetting.DataType = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                                        .Title("Setting Data Type:")
                                                        .AddChoices(System.Enum.GetNames(typeof(DataTypeSetting))));

            var lastKnownDataType = defaultSetting.DataType;
            if (defaultSetting.DataType != lastKnownDataType)
            {
                defaultSetting.DefaultValue = null;
            }
            if (defaultSetting.DataType == DataTypeSetting.Enum.ToString())
            {
                var enumValues = AnsiConsole.Ask("Setting Enum Values (use ',' as separator): ", string.Join(",", defaultSetting?.EnumValues ?? new()));
                defaultSetting.EnumValues = [.. enumValues.Split(",")];
            }

            defaultSetting.DefaultValue = IoTStructures.AskDynamicType<DataTypeSetting>(defaultSetting.DataType, "Setting", defaultSetting.DefaultValue);
            defaultSetting.InfoMessage = AnsiConsole.Ask("Setting Information Message (tooltip):", defaultSetting.InfoMessage ?? "");

            if (settings.ContainsKey(tab) && settings[tab].ContainsKey(section))
            {
                AnsiConsole.Write($"Will override setting {defaultSetting.Name}");
                settings[tab][section].Add(defaultSetting);
            }
            else
            {
                settings.Add(tab, new Dictionary<string, HashSet<TaskSetting>>() { { section, new HashSet<TaskSetting>() { defaultSetting } } });
            }
            return defaultSetting;
        }

        private static TaskSetting FindSetting(Dictionary<string, Dictionary<string, HashSet<TaskSetting>>> settings, string settingName)
        {
            foreach (var tab in settings)
            {
                foreach (var section in tab.Value)
                {
                    if (section.Value.Any(setting => setting.Name == settingName))
                    {
                        return section.Value.FirstOrDefault(setting => setting.Name == settingName);
                    }
                }
            }
            return null;
        }

        private static Choices AskChoiceAndLogInputsOutputs(Dictionary<string, TaskInputOutputType> values, string identifier = "Inputs")
        {
            if (values.Count != 0)
            {
                string stringInputs = string.Empty;
                foreach (KeyValuePair<string, TaskInputOutputType> element in values)
                {
                    var value = element.Value;
                    stringInputs += $"{element.Key} -> {value.DisplayName ?? element.Key} ({value.DataType}, default={JsonConvert.SerializeObject(value.DefaultValue) ?? ""})\r\n";
                }
                var panel = new Panel(stringInputs)
                {
                    Header = new PanelHeader($"Current {identifier}"),
                    Border = BoxBorder.Double,
                    Expand = true
                };
                AnsiConsole.Write(panel);
            }

            return IoTStructures.AskChoice();
        }

        private static Choices AskChoiceAndLogSettings(Dictionary<string, Dictionary<string, HashSet<TaskSetting>>> settings)
        {
            if (settings.Count != 0)
            {
                AnsiConsole.Write("Current Settings");
                string stringSettings = string.Empty;
                foreach (var tab in settings)
                {
                    AnsiConsole.Write($"Tab {tab} Settings");
                    foreach (var section in tab.Value)
                    {
                        stringSettings = string.Empty;
                        foreach (var setting in section.Value)
                        {
                            stringSettings += $"{setting.SettingKey} -> {setting.DisplayName ?? setting.Name} ({setting.DataType}, default={JsonConvert.SerializeObject(setting.DefaultValue) ?? ""})";
                        }
                        var panel = new Panel(stringSettings)
                        {
                            Header = new PanelHeader($"Section {section.Key}"),
                            Border = BoxBorder.Double,
                            Expand = true
                        };
                        AnsiConsole.Write(panel);
                    }
                }
            }

            return IoTStructures.AskChoice();
        }

        /// <inheritdoc />
        private List<string> GenerateArgs(
            IDirectoryInfo workingDir,
            string packageLocation,
            string taskName,
            string className,
            string settingDefaults,
            string testSettingDefaults,
            string inputsInterface,
            string outputsInterface,
            string settingsInterface,
            string isProtocolSetting)
        {
            Log.Debug($"Creating IoT Task at {packageLocation}");

            var args = new List<string>();
            args.AddRange(new[]
            {
                "--taskName", taskName,
                "--className", className,
                "--settingDefaults", settingDefaults,
                "--testSettingDefaults", testSettingDefaults,
                "--inputsInterface", inputsInterface,
                "--outputsInterface", outputsInterface,
                "--settingsInterface", settingsInterface,
                "--isProtocol", isProtocolSetting
            });

            return args;
        }
    }
}
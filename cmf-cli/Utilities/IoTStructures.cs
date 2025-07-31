using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Spectre.Console;
using System;
using System.Collections.Generic;

namespace Cmf.CLI.Utilities
{
    public class IoTStructures
    {
        public static Choices AskChoice()
        {
            var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Action:")
                        .AddChoices(Enum.GetNames(typeof(Choices))));

            return (Choices)Enum.Parse(typeof(Choices), choice, true);
        }

        public static object AskDynamicType<T>(string type, string identifier, object defaultValue)
        {
            var typeEnum = (T)Enum.Parse(typeof(T), type, true);
            switch (typeEnum)
            {
                case DataTypeInputOutput.DateTime:
                case DataTypeInputOutput.String:
                case DataTypeSetting.String:
                case DataTypeSetting.Enum:
                    return AnsiConsole.Ask($"{identifier} Default Value:", defaultValue?.ToString() ?? "");
                case DataTypeInputOutput.Integer:
                case DataTypeInputOutput.Long:
                case DataTypeSetting.Integer:
                case DataTypeSetting.Long:
                    return AnsiConsole.Ask($"{identifier} Default Value:",
                            defaultValue?.ToString() != null ? int.Parse(defaultValue?.ToString()) : 0);
                case DataTypeInputOutput.Decimal:
                case DataTypeSetting.Decimal:
                    return AnsiConsole.Ask($"{identifier} Default Value:",
                        defaultValue?.ToString() != null ? decimal.Parse(defaultValue?.ToString()) : 0);
                case DataTypeInputOutput.Boolean:
                case DataTypeSetting.Boolean:
                    return AnsiConsole.Ask($"{identifier} Default Value:",
                        defaultValue?.ToString() != null ? bool.Parse(defaultValue.ToString()) : false);
                case DataTypeInputOutput.Any:
                case DataTypeInputOutput.Object:
                case DataTypeSetting.Object:
                    return AnsiConsole.Ask($"{identifier} Default Value:", defaultValue);
                case DataTypeInputOutput.Buffer:
                    return AnsiConsole.Ask($"{identifier} Default Value:", defaultValue?.ToString() ?? "");
                default:
                    throw new Exception("Invalid DataType");
            }
        }

        public static string ConvertIoTTypesToJSTypes<T>(T type)
        {
            switch (type)
            {
                case DataTypeInputOutput.Any:
                    return "any";
                case DataTypeInputOutput.DateTime:
                    return "Date";
                case DataTypeInputOutput.String:
                case DataTypeSetting.String:
                    return "string";
                case DataTypeSetting.Enum:
                    return "<Declare your enum>";
                case DataTypeInputOutput.Decimal:
                case DataTypeInputOutput.Long:
                case DataTypeInputOutput.Integer:
                case DataTypeSetting.Decimal:
                case DataTypeSetting.Long:
                case DataTypeSetting.Integer:
                    return "number";
                case DataTypeInputOutput.Object:
                case DataTypeSetting.Object:
                    return "object";
                case DataTypeInputOutput.Buffer:
                    return "Buffer";
                case DataTypeInputOutput.Boolean:
                case DataTypeSetting.Boolean:
                    return "boolean";
                default:
                    throw new Exception("Invalid JS Type conversion");
            }
        }

        public static string ConvertIoTTypesToJSTypes<T>(string type)
        {
            var typeEnum = (T)Enum.Parse(typeof(T), type, true);
            return ConvertIoTTypesToJSTypes<T>(typeEnum);
        }

        public static string ConvertIoTValueTypeToTaskValueType(DataTypeInputOutput type)
        {
            switch (type)
            {
                case DataTypeInputOutput.Any:
                    return "undefined";
                default:
                    return "Task.TaskValueType." + type.ToString();
            }
        }

        public static string ConvertIoTValueTypeToTaskValueType(IoTValueType type)
        {
            switch (type)
            {
                case IoTValueType.Any:
                    return "undefined";
                default:
                    return "Task.TaskValueType." + type.ToString();
            }
        }
    }

    [JsonObject]
    public class TemplateTaskLibrary
    {
        [JsonProperty("converters")]
        public List<ConverterValues> Converters { get; set; } = [];

        [JsonProperty("tasks")]
        public List<TaskValues> Tasks { get; set; } = [];
    }

    [JsonObject]
    public class DriverValues
    {
        [JsonProperty("directory")]
        public string Directory { get; set; } = "driver-sample";

        [JsonProperty("packageName")]
        public string PackageFullName { get; set; }

        [JsonProperty("packageVersion")]
        public string PackageVersion { get; set; } = "0.0.0";

        [JsonProperty("identifier")]
        public string Identifier { get; set; } = "SampleDriver";

        [JsonIgnore]
        public string PackageScope { get; set; } = "@criticalmanufacturing";

        [JsonIgnore]
        public string PackageName { get; set; } = "connect-iot-driver-sample";

        [JsonIgnore]
        public string IdentifierCamel { get; set; }

        [JsonIgnore]
        public bool HasCommands { get; set; }
    }

    [JsonObject]
    public class ConverterValues
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "somethingToSomething";

        [JsonIgnore]
        public string ClassName { get; set; } = "";

        [JsonProperty("displayName")]
        public string Title { get; set; } = "Something To Something";

        [JsonProperty("inputDataType")]
        public string Input { get; set; } = DataTypeInputOutput.Any.ToString();

        [JsonProperty("outputDataType")]
        public string Output { get; set; } = DataTypeInputOutput.Any.ToString();

        [JsonProperty("parameters")]
        [JsonConverter(typeof(IoTParametersConverter))]
        public Dictionary<string, IoTValueType> Parameters { get; set; } = [];

        [JsonIgnore]
        public string InputAsJS { get; set; } = "undefined";
        [JsonIgnore]
        public string OutputAsJS { get; set; } = "undefined";
        [JsonIgnore]
        public bool HasParameters { get; set; } = false;
        [JsonIgnore]
        public string ParametersAsJS { get; set; } = "";
    }

    [JsonObject]
    public class TaskValues
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "blackBox";

        [JsonIgnore]
        public string ClassName { get; set; } = "";

        [JsonProperty("displayName")]
        public string Title { get; set; } = "Black Box";

        [JsonProperty("iconClass")]
        public string Icon { get; set; } = "icon-core-tasks-connect-iot-lg-logmessage";

        [JsonProperty("isProtocol")]
        public bool IsProtocol { get; set; } = false;

        [JsonProperty("isController")]
        public bool IsController { get; set; } = true;

        [JsonProperty("lifecycle")]
        public string Lifecycle { get; set; } = "Productive";

        [JsonProperty("lifecycleMessage")]
        public string LifecycleMessage { get; set; } = "";

        [JsonProperty("dependsOnProtocol")]
        public List<string> DependsOnProtocol { get; set; }

        [JsonProperty("dependsOnScope")]
        public List<string> DependsOnScope { get; set; }

        [JsonProperty("inputs")]
        public Dictionary<string, TaskInputOutputType> Inputs { get; set; } = new()
        {
            {
                "activate", new TaskInputOutputType
                {
                    Type = TaskInputTypeType.Activate,
                    DisplayName = "Activate",
                    DefaultValue = ""
                }
            }
        };

        [JsonProperty("outputs")]
        public Dictionary<string, TaskInputOutputType> Outputs { get; set; } = new()
        {
            {
                "success", new TaskInputOutputType
                {
                    Type = TaskInputTypeType.Success,
                    DisplayName = "Success",
                    DataType = null
                }
            },
            {
                "error", new TaskInputOutputType
                {
                    Type = TaskInputTypeType.Error,
                    DisplayName = "Error",
                    DataType = null
                }
            }
        };

        [JsonProperty("settings")]
        public Dictionary<string, Dictionary<string, HashSet<TaskSetting>>> Settings { get; set; } = new()
        {
            {
                "General", new Dictionary<string, HashSet<TaskSetting>>
                {
                    {
                        "Example Section", new HashSet<TaskSetting>
                        {
                            new TaskSetting
                            {
                                Name = "Example",
                                DisplayName = "Example Setting",
                                SettingKey = "example",
                                DataType = "string",
                                DefaultValue = "Hello World",
                                InfoMessage = "Information about the example setting"
                            }
                        }
                    }
                }
            }
        };

        [JsonIgnore]
        public string InputsInterface { get; set; } = "";
        [JsonIgnore]
        public string OutputsInterface { get; set; } = "";
        [JsonIgnore]
        public string SettingsInterface { get; set; } = "";
        [JsonIgnore]
        public string SettingsDefaults { get; set; } = "";
        [JsonIgnore]
        public string TestSettingsDefaults { get; set; } = "";
    }

    [JsonObject]
    public class TaskSetting
    {
        /// <summary>
        /// Name of the setting.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Text to display in the GUI, defaults to Name if not defined.
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Key of the settings where the value will be stored. Must be unique within the entire set of settings in the task.
        /// </summary>
        [JsonProperty("settingKey")]
        public string SettingKey { get; set; }

        /// <summary>
        /// Data type of the field (used to render).
        /// </summary>
        [JsonProperty("dataType")]
        public string DataType { get; set; }

        /// <summary>
        /// Flag indicating if this setting must be filled.
        /// </summary>
        [JsonProperty("isMandatory")]
        public bool? IsMandatory { get; set; }

        /// <summary>
        /// List of possible values when the type is Enum.
        /// </summary>
        [JsonProperty("enumValues")]
        public List<string> EnumValues { get; set; }

        /// <summary>
        /// Text to display in the hint box when hovered.
        /// </summary>
        [JsonProperty("infoMessage")]
        public string InfoMessage { get; set; }

        /// <summary>
        /// Value to use as default.
        /// </summary>
        [JsonProperty("defaultValue")]
        public object DefaultValue { get; set; }

        /// <summary>
        /// Expression that will be parsed to identify if this setting is to be displayed.
        /// Example: timerType != null && timerType !== "CronJob".
        /// </summary>
        [JsonProperty("condition")]
        public string Condition { get; set; }

        /// <summary>
        /// Extra settings to use to model the behavior of the field. These settings are different per control to use.
        /// </summary>
        [JsonProperty("settings")]
        public object Settings { get; set; }

        public TaskSetting()
        {
            Name = "settingName";
            SettingKey = "settingKey";
            DataType = DataTypeInputOutput.String.ToString();
        }
    }

    [JsonObject]
    public class TaskInputOutputType
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TaskInputTypeType Type { get; set; }

        [JsonProperty("dataType")]
        public string DataType { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("defaultValue")]
        public object DefaultValue { get; set; }

        public TaskInputOutputType()
        {
            Type = TaskInputTypeType.Static;
            DataType = DataTypeInputOutput.String.ToString();
        }
    }

    public enum DataTypeSetting
    {
        String,
        Integer,
        Long,
        Decimal,
        Boolean,
        Object,
        Enum
    }

    public enum DataTypeInputOutput
    {
        String,
        Any,
        Integer,
        Long,
        Decimal,
        Boolean,
        DateTime,
        Object,
        Buffer
    }

    public enum IoTValueType
    {
        String,
        Any,
        Integer,
        Long,
        Decimal,
        Boolean,
        DateTime,
        Object,
        Buffer,
        Enum
    }

    public enum TaskInputTypeType
    {
        Static,
        AutoPort,
        Dynamic,
        Activate,
        Success,
        Error
    }

    public enum Lifecycle
    {
        Productive,
        Experimental,
        Deprecated
    }

    public enum Choices
    {
        Done,
        Add,
        Remove,
        Edit
    }

    public enum Scopes
    {
        ConnectIoT,
        FactoryAutomation,
        EnterpriseIntegration,
        DataPlatform
    }
}

using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands.New.IoT
{
    /// <summary>
    /// Generates IoT Converter structure
    /// </summary>
    [CmfCommand("converter", ParentId = "new_iot", Id = "iot_converter")]
    public class GenerateConverterCommand : TemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public GenerateConverterCommand() : base("converter")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem">the filesystem implementation</param>
        public GenerateConverterCommand(IFileSystem fileSystem) : base("converter", fileSystem)
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

            var converter = HandleConverter(new ConverterValues());

            var args = this.GenerateArgs(workingDir, this.fileSystem.Directory.GetCurrentDirectory(), converter.Name, converter.ClassName, converter.Title, converter.InputAsJS, converter.OutputAsJS);
            this.CommandName = "iot-converter";
            base.RunCommand(args);

            var indexPath = this.fileSystem.Path.Join(this.fileSystem.Directory.GetCurrentDirectory(), "src/index.ts");
            this.fileSystem.File.AppendAllLines(indexPath, ["export { " + converter.ClassName + "Converter } from \"./converters/" + converter.Name + "/" + converter.Name + ".converter\";\r\n"]);

            var template = new TemplateTaskLibrary()
            {
                Converters = [converter]
            };
            var templatePath = this.fileSystem.Path.Join(this.fileSystem.Directory.GetCurrentDirectory(), $"templates/converter_{converter.Name}.json");
            this.fileSystem.File.WriteAllText(templatePath,
                JsonConvert.SerializeObject(template,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));
        }

        private ConverterValues HandleConverter(ConverterValues converter)
        {
            converter.Name = AnsiConsole.Ask("What is the converter name?", converter.Name).ToCamelCase();
            converter.ClassName = converter.Name.ToPascalCase();
            converter.Title = AnsiConsole.Ask("What is the converter Title?", converter.Title);

            converter.Input = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                    .Title("What is the input type?")
                                    .AddChoices(Enum.GetNames(typeof(DataTypeInputOutput))));
            converter.Output = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                    .Title("What is the output type?")
                                    .AddChoices(Enum.GetNames(typeof(DataTypeInputOutput))));

            converter.HasParameters = AnsiConsole.Prompt(new ConfirmationPrompt("Do you require parameters?") { DefaultValue = converter.HasParameters });

            if (converter.HasParameters)
            {
                converter = this.HandleParameters(converter);
            }

            converter.InputAsJS = IoTStructures.ConvertIoTTypesToJSTypes<DataTypeInputOutput>(converter.Input);
            converter.OutputAsJS = IoTStructures.ConvertIoTTypesToJSTypes<DataTypeInputOutput>(converter.Output);

            return converter;
        }

        private ConverterValues HandleParameters(ConverterValues converter)
        {
            var addParameter = true;

            while (addParameter)
            {
                var name = AnsiConsole.Ask("Parameter Name:", "newParameter");
                var type = AnsiConsole.Prompt(
                                        new SelectionPrompt<string>()
                                        .Title("Parameter Type:")
                                        .AddChoices(Enum.GetNames(typeof(IoTValueType))));
                var typeEnum = (IoTValueType)Enum.Parse(typeof(IoTValueType), type, true);
                var value = IoTStructures.ConvertIoTValueTypeToTaskValueType(typeEnum);

                if (typeEnum == IoTValueType.Any || typeEnum == IoTValueType.Enum)
                {
                    value = """
                        [{
                            friendlyName: "First",
                            value: "1"
                          }, {
                            friendlyName: "Second",
                            value: "2"
                        }]
                        """;
                    converter.ParametersAsJS += $"\t{name}:{value}\r\n";
                }

                if (!converter.Parameters.TryAdd(name, typeEnum))
                {
                    converter.Parameters[name] = typeEnum;
                }

                converter.ParametersAsJS = converter.ParametersAsJS.TrimEnd();

                addParameter = AnsiConsole.Prompt(new ConfirmationPrompt("Do you require more parameters?") { DefaultValue = false });
            }

            return converter;
        }

        /// <inheritdoc />
        private List<string> GenerateArgs(
            IDirectoryInfo workingDir,
            string packageLocation,
            string converterName,
            string className,
            string title,
            string inputAsJS,
            string outputAsJS)
        {
            Log.Debug($"Creating IoT Converter at {packageLocation}");

            var args = new List<string>();
            args.AddRange(new[]
            {
                "--converterName", converterName,
                "--className", className,
                "--title", title,
                "--inputAsJS", inputAsJS,
                "--outputAsJS", outputAsJS
            });

            return args;
        }
    }
}
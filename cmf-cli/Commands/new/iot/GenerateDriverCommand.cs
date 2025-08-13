using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;

namespace Cmf.CLI.Commands.New.IoT
{
    /// <summary>
    /// Generates IoT Driver structure
    /// </summary>
    [CmfCommand("driver", ParentId = "new_iot", Id = "iot_driver")]
    public class GenerateDriverCommand : TemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public GenerateDriverCommand() : base("driver")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem">the filesystem implementation</param>
        public GenerateDriverCommand(IFileSystem fileSystem) : base("driver", fileSystem)
        {
        }

        public override void Configure(Command cmd)
        {
            var nearestIoTPackage = FileSystemUtilities.GetPackageRootByType(
                this.fileSystem.Directory.GetCurrentDirectory(),
                PackageType.IoT,
                this.fileSystem
            );

            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, nearestIoTPackage?.FullName),
                isDefault: true
            )
            {
                Description = "Working Directory"
            });

            cmd.Handler = CommandHandler.Create<IDirectoryInfo>(this.Execute);
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

            if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major < 11)
            {
                throw new CliException("This command is only valid for versions above 11.0.0");
            }

            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

            var driver = HandleDriver(new DriverValues());

            var args = this.GenerateArgs(
                workingDir,
                this.fileSystem.Directory.GetCurrentDirectory(),
                driver.Directory,
                driver.Identifier,
                driver.IdentifierCamel,
                driver.PackageFullName,
                driver.PackageVersion,
                driver.HasCommands,
                driver.HasTemplates);
            this.CommandName = "iot-driver";
            base.RunCommand(args);
        }

        private DriverValues HandleDriver(DriverValues driver)
        {
            driver.Directory = AnsiConsole.Ask("What is the identifier (directory name)?", driver.Directory);
            driver.PackageScope = AnsiConsole.Ask("What is the driver scope?", driver.PackageScope);
            driver.PackageName = AnsiConsole.Ask("What is the package name?", driver.PackageName);
            driver.PackageFullName = $"{driver.PackageScope}/{driver.PackageName}";
            driver.PackageVersion = AnsiConsole.Ask("What is the package version?", driver.PackageVersion);

            driver.Identifier = AnsiConsole.Ask("What is the identifier (Protocol name, no spaces)?", driver.Identifier).ToPascalCase();
            driver.IdentifierCamel = driver.Identifier.ToCamelCase();

            driver.HasCommands = AnsiConsole.Prompt(new ConfirmationPrompt("Does the protocol support commands?") { DefaultValue = driver.HasCommands });
            driver.HasTemplates = AnsiConsole.Prompt(new ConfirmationPrompt("Do you wish to use templates? (By saying no, you will only have events and commands from the driver definition)") { DefaultValue = driver.HasTemplates });

            return driver;
        }

        /// <inheritdoc />
        private List<string> GenerateArgs(
            IDirectoryInfo workingDir,
            string packageLocation,
            string directoryName,
            string identifier,
            string identifierCamel,
            string packageName,
            string packageVersion,
            bool hasCommands,
            bool hasTemplates)
        {
            var mesVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            Log.Debug($"Creating IoT Driver at {packageLocation}");

            var args = new List<string>();
            args.AddRange(new[]
            {
                "--targetSystemVersionProcessed", $"release-{mesVersion.Major}{mesVersion.Minor}{mesVersion.Build}",
                "--directoryName", directoryName,
                "--identifier", identifier,
                "--identifierCamel", identifierCamel,
                "--packageName", packageName,
                "--packageVersion", packageVersion,
                "--npmRegistry", ExecutionContext.Instance.ProjectConfig.NPMRegistry.ToString(),
                "--hasCommands", hasCommands.ToString(),
                "--hasTemplates", hasTemplates.ToString()
            });

            return args;
        }
    }
}
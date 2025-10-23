using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Spectre.Console;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Linq;

namespace Cmf.CLI.Commands.New.IoT
{
    /// <summary>
    /// Generates IoT Task Library structure
    /// </summary>
    [CmfCommand("taskLibrary", ParentId = "new_iot", Id = "iot_tasklibrary", MinimumMESVersion = "11.0.0")]
    public class GenerateTaskLibraryCommand : TemplateCommand
    {

        /// <summary>
        /// constructor
        /// </summary>
        public GenerateTaskLibraryCommand() : base("taskLibrary")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem">the filesystem implementation</param>
        public GenerateTaskLibraryCommand(IFileSystem fileSystem) : base("taskLibrary", fileSystem)
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

            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

            var dirName = AnsiConsole.Ask("What is the directory name?", "controller-engine-custom-tasks");
            var packageScope = AnsiConsole.Ask("What is the package scope?", "@criticalmanufacturing");
            var packageName = AnsiConsole.Ask("What is the package name?", "connect-iot-controller-engine-custom-tasks");
            var fullPackageName = $"{packageScope}/{packageName}";

            var packageVersion = AnsiConsole.Ask("What is the package version?", "0.0.0");
            var identifier = AnsiConsole.Ask("What is the library name?", "My Tasks Library");

            var dependsOnScope = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("On which scopes this library can be used")
                    .NotRequired()
                    .AddChoices(System.Enum.GetNames(typeof(Scopes))));

            var mandatoryForScope = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("On which scopes this library is *mandatory* (selected by default)")
                    .NotRequired()
                    .AddChoices(System.Enum.GetNames(typeof(Scopes))));

            var dependsOnProtocol = AnsiConsole.Prompt(
                new TextPrompt<string>("Is this library specific for any protocol? If so, list the names separated by comma").DefaultValue(null))?.Split(",").ToList() ?? [];

            var mandatoryForProtocol = AnsiConsole.Prompt(
                new TextPrompt<string>("Is this library *mandatory* for any protocol? If so, list the names separated by comma").DefaultValue(null))?.Split(",").ToList() ?? [];

            var args = this.GenerateArgs(workingDir, dirName, fullPackageName, packageVersion, identifier, dependsOnScope, mandatoryForScope, dependsOnProtocol, mandatoryForProtocol);
            this.CommandName = "iot-taskLibrary";
            base.RunCommand(args);
        }

        /// <inheritdoc />
        private List<string> GenerateArgs(
            IDirectoryInfo workingDir,
            string dirName,
            string fullPackageName,
            string packageVersion,
            string identifier,
            List<string> dependsOnScope, List<string> mandatoryForScope, List<string> dependsOnProtocol, List<string> mandatoryForProtocol)
        {
            var mesVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            Log.Debug($"Creating IoT Task Library Package at {workingDir}");

            var args = new List<string>();
            args.AddRange(new[]
            {
                "--directoryName", dirName,
                "--npmRegistry", ExecutionContext.Instance.ProjectConfig.NPMRegistry.ToString(),
                "--nodeVersion", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().Node(ExecutionContext.Instance.ProjectConfig.MESVersion),
                "--identifier", identifier,
                "--identifierLower", identifier.Replace(" ", "").ToLower().Trim(),
                "--packageName", fullPackageName,
                "--packageVersion", packageVersion,
                "--targetSystemVersionProcessed", $"release-{mesVersion.Major}{mesVersion.Minor}{mesVersion.Build}",
                "--dependsOnScope", JsonConvert.SerializeObject(dependsOnScope),
                "--mandatoryForScope", JsonConvert.SerializeObject(mandatoryForScope),
                "--dependsOnProtocol", JsonConvert.SerializeObject(dependsOnProtocol),
                "--mandatoryForProtocol", JsonConvert.SerializeObject(mandatoryForProtocol)
            });

            return args;
        }
    }
}
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
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands.New.IoT
{
    /// <summary>
    /// Generates IoT Business Scenario structure
    /// </summary>
    [CmfCommand("businessScenario", ParentId = "new_iot", Id = "iot_businessscenario")]
    public class GenerateBusinessScenarioCommand : TemplateCommand
    {

        /// <summary>
        /// constructor
        /// </summary>
        public GenerateBusinessScenarioCommand() : base("businessScenario")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem">the filesystem implementation</param>
        public GenerateBusinessScenarioCommand(IFileSystem fileSystem) : base("businessScenario", fileSystem)
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
                Description = "Working Directory",
                CustomParser = argResult => Parse<IDirectoryInfo>(argResult, nearestIoTPackage?.FullName),
                DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, nearestIoTPackage?.FullName)
            };
            cmd.Add(workingDirArgument);

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

            if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major < 11 || (ExecutionContext.Instance.ProjectConfig.MESVersion.Major == 11 && ExecutionContext.Instance.ProjectConfig.MESVersion.Minor < 1))
            {
                throw new CliException("This command is only valid for versions above 11.1.0");
            }

            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

            var dirName = AnsiConsole.Ask("What is the directory name?", "connect-iot-business-scenarios-custom");
            var packageScope = AnsiConsole.Ask("What is the package scope?", "@criticalmanufacturing");
            var packageName = AnsiConsole.Ask("What is the package name?", "connect-iot-business-scenarios-custom");
            var fullPackageName = $"{packageScope}/{packageName}";

            var packageVersion = AnsiConsole.Ask("What is the package version?", "0.0.0");
            var identifier = AnsiConsole.Ask("What is the library name?", "My Business Scenarios Library");

            var args = this.GenerateArgs(workingDir, dirName, fullPackageName, packageVersion, identifier);
            this.CommandName = "iot-businessScenariosLibrary";
            base.RunCommand(args);
        }

        /// <inheritdoc />
        private List<string> GenerateArgs(
            IDirectoryInfo workingDir,
            string dirName,
            string fullPackageName,
            string packageVersion,
            string identifier)
        {
            Log.Debug($"Creating IoT Task Library Package at {workingDir}");

            var args = new List<string>();
            args.AddRange(new[]
            {
                "--directoryName", dirName,
                "--npmRegistry", ExecutionContext.Instance.ProjectConfig.NPMRegistry.ToString(),
                "--identifier", identifier,
                "--identifierLower", identifier.Replace(" ", "").ToLower().Trim(),
                "--packageName", fullPackageName,
                "--packageVersion", packageVersion
            });

            return args;
        }
    }
}
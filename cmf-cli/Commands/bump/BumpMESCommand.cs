using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="BaseCommand" />
    [CmfCommand(name: "mes", Id = "bump_mes", ParentId = "bump")]
    public class BumpMESCommand : BaseCommand
    {
        
        /// <summary>
        /// constructor for System.IO filesystem
        /// </summary>
        public BumpMESCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public BumpMESCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "packagePath",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, "."),
                description: "Package path"));

            cmd.AddArgument(new Argument<string>(
                name: "MESVersion",
                description: "New MES Version"
            ));

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-iot", "--iotVersion" },
                description: "New MES version for the IoT workflows & masterdatas."
            ));

            cmd.AddOption(new Option<List<string>>(
                aliases: new string[] { "-ignore", "--iotPackagesToIgnore" },
                description: "IoT packages to ignore when updating the MES version of the tasks in IoT workflows"
            )
            {
                AllowMultipleArgumentsPerToken = true,
            });

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, string, List<string>>(Execute);
        }

        /// <summary>
        /// Executes the specified package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="MESVersion">The new MES version.</param>
        /// <param name="iotVersion">New MES version for the IoT workflows & masterdata</param>
        /// <param name="iotPackagesToIgnore">IoT packages to ignore when updating the MES version of the tasks in IoT workflows</param>
        /// <exception cref="CliException"></exception>
        public void Execute(IDirectoryInfo packagePath, string MESVersion, string iotVersion, List<string> iotPackagesToIgnore)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

            var cmfPackagePaths = packagePath.GetFiles("cmfpackage.json", SearchOption.AllDirectories);

            foreach (IFileInfo path in cmfPackagePaths)
            {
                Log.Debug($"Processing {path.FullName}");
                Execute(CmfPackage.Load(path), MESVersion, iotVersion, iotPackagesToIgnore);
            }

            UpdateProjectConfig(packagePath, MESVersion);
            UpdatePipelineFiles($"{packagePath}/Builds/.vars", MESVersion);
            UpdatePipelineFiles($"EnvironmentConfigs", MESVersion);
        }

        /// <summary>
        /// Executes the specified CMF package.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="version">The version.</param>
        /// <param name="iotVersion">New MES version for the IoT workflows & masterdata</param>
        /// <param name="iotPackagesToIgnore">IoT packages to ignore when updating the MES version of the tasks in IoT workflows</param>
        /// <exception cref="CliException"></exception>
        public void Execute(CmfPackage cmfPackage, string version, string iotVersion, List<string> iotPackagesToIgnore)
        {
            IDirectoryInfo packageDirectory = cmfPackage.GetFileInfo().Directory;
            IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);

            packageTypeHandler.MESBump(version, iotVersion, iotPackagesToIgnore);
        }

        #region Utilities

        /// <summary>
        /// Updates the `ProjectConfig.json` file if found.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="MESVersion">The new MES version.</param>
        private void UpdateProjectConfig(IDirectoryInfo packagePath, string MESVersion)
        {
            IFileInfo projectConfig = this.fileSystem.FileInfo.New($"{packagePath}/{CoreConstants.ProjectConfigFileName}");

            if (projectConfig.Exists)
            {
                Log.Information($"Updating {CoreConstants.ProjectConfigFileName} file");

                string text = fileSystem.File.ReadAllText(projectConfig.FullName);
                foreach (string key in new string[] { "MESVersion", "NugetVersion", "TestScenariosNugetVersion" })
                {
                    text = MESBumpUtilities.UpdateJsonValue(text, key, MESVersion);
                }

                text = MESBumpUtilities.UpdateJsonValue(text, "ISOLocation", generateIsoLocation(MESVersion).Replace(@"\", @"\\"));
                fileSystem.File.WriteAllText(projectConfig.FullName, text);
            }
        }

        /// <summary>
        /// Updates the pipeline variable YAML files if found.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="MESVersion">The new MES version.</param>
        private void UpdatePipelineFiles(string packagePath, string MESVersion)
        {
            IDirectoryInfo variablesDir = this.fileSystem.DirectoryInfo.New(packagePath);

            if (variablesDir.Exists)
            {
                string[] filesToUpdate = this.fileSystem.Directory.GetFiles(variablesDir.FullName, "*.yml", SearchOption.TopDirectoryOnly);

                foreach (string yamlFile in filesToUpdate)
                {
                    Log.Information($"Updating {yamlFile} file");
                    string text = this.fileSystem.File.ReadAllText(yamlFile);

                    text = Regex.Replace(
                        text, 
                        @"\\\\management\\Setups\\cmNavigo.+\.iso",
                        generateIsoLocation(MESVersion), 
                        RegexOptions.IgnoreCase
                    );

                    text = Regex.Replace(
                        text,
                        @"@criticalmanufacturing\\mes:\d+\.\d+\.\d+",
                        $"@criticalmanufacturing\\mes:{MESVersion}",
                        RegexOptions.IgnoreCase
                    );

                    text = Regex.Replace(
                        text,
                        @"@criticalmanufacturing\\connectiot-manager:\d+\.\d+\.\d+",
                        $"@criticalmanufacturing\\connectiot-manager:{MESVersion}",
                        RegexOptions.IgnoreCase
                    );

                    text = Regex.Replace(
                        text,
                        @"refs\/tags\/\d+\.\d+\.\d+",
                        $"refs/tags/{MESVersion}",
                        RegexOptions.IgnoreCase
                    );

                    this.fileSystem.File.WriteAllText(yamlFile, text);
                }
            }
        }

        private string generateIsoLocation(string MESVersion)
        {
            Version version = new Version(MESVersion);
            string majorMinor = $"{version.Major}.{version.Minor}";

            if (version.Major >= 11)
            {
                return $@"\\management\Setups\cmNavigo\v{majorMinor}.x\Critical Manufacturing {MESVersion}-Optional Services.iso";
            }
            else
            {
                return $@"\\management\Setups\cmNavigo\v{majorMinor}.x\Critical Manufacturing {MESVersion}.iso";
            }
        }
        #endregion
    }
}
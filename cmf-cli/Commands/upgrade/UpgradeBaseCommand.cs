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
    [CmfCommand(name: "base", Id = "upgrade_base", ParentId = "upgrade", Description = "Upgrade the baseline version of the MES.")]
    public class UpgradeBaseCommand : BaseCommand
    {
        
        /// <summary>
        /// constructor for System.IO filesystem
        /// </summary>
        public UpgradeBaseCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public UpgradeBaseCommand(IFileSystem fileSystem) : base(fileSystem)
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
                name: "BaseVersion",
                description: "New framework/MES Version"
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
        /// <param name="baseVersion">The new Base version.</param>
        /// <param name="iotVersion">New MES version for the IoT workflows & masterdata</param>
        /// <param name="iotPackagesToIgnore">IoT packages to ignore when updating the MES version of the tasks in IoT workflows</param>
        /// <exception cref="CliException"></exception>
        public void Execute(IDirectoryInfo packagePath, string baseVersion, string iotVersion, List<string> iotPackagesToIgnore)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

            var cmfPackagePaths = packagePath.GetFiles("cmfpackage.json", SearchOption.AllDirectories);

            foreach (IFileInfo path in cmfPackagePaths)
            {
                Log.Debug($"Processing {path.FullName}");
                Execute(CmfPackage.Load(path), baseVersion, iotVersion, iotPackagesToIgnore);
            }

            UpdateProjectConfig(packagePath, baseVersion);
            Log.Warning("Don't forget to update pipeline files");
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

            packageTypeHandler.UpgradeBase(version, iotVersion, iotPackagesToIgnore);
        }

        #region Utilities

        /// <summary>
        /// Updates the `ProjectConfig.json` file if found.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="baseVersion">The new Base version.</param>
        private void UpdateProjectConfig(IDirectoryInfo packagePath, string baseVersion)
        {
            IFileInfo projectConfig = this.fileSystem.FileInfo.New(Path.Combine(packagePath.FullName, CoreConstants.ProjectConfigFileName));

            if (projectConfig.Exists)
            {
                Log.Information($"Updating {CoreConstants.ProjectConfigFileName} file");

                string text = fileSystem.File.ReadAllText(projectConfig.FullName);
                foreach (string key in new string[] { "MESVersion", "NugetVersion", "TestScenariosNugetVersion" })
                {
                    text = UpgradeBaseUtilities.UpdateJsonValue(text, key, baseVersion);
                }

                if (new Version(baseVersion).Major >= 11)
                {
                    // TODO: find a more elegant way to apply these changes to files/packages when this command is executed.
                    // For the moment, sneaking this if-statement in will do the job but long-term we'll need an approach that
                    // doesn't polute the "UpgradeBase" functions with if-else logic everywhere.
                    text = UpgradeBaseUtilities.RemoveJsonValue(text, "ISOLocation");
                }
                fileSystem.File.WriteAllText(projectConfig.FullName, text);
            }
        }
        #endregion
    }
}
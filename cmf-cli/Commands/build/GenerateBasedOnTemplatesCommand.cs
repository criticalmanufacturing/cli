using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PowershellCommand" />
    [CmfCommand(name: "generateBasedOnTemplates", Parent = "help", ParentId = "build_help")]
    public class GenerateBasedOnTemplatesCommand : PowershellCommand
    {
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.Handler = CommandHandler.Create(this.Execute);
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            var regex = new Regex("\"?id\"?:\\s+[\"'](.*)[\"']"); // match for menu item IDs
            
            var helpRoot = FileSystemUtilities.GetPackageRootByType(Environment.CurrentDirectory, PackageType.Help, this.fileSystem).FullName;
            var project = FileSystemUtilities.ReadProjectConfig(this.fileSystem).RootElement.GetProperty("Tenant").GetString();
            var helpPackagesRoot = this.fileSystem.Path.Join(helpRoot, "src", "packages");
            var helpPackages = this.fileSystem.Directory.GetDirectories(helpPackagesRoot);
            var pkgName = CmfPackage.Load(this.fileSystem.FileInfo.FromFileName(this.fileSystem.Path.Join(helpRoot, CliConstants.CmfPackageFileName))).PackageId.ToLowerInvariant();
            foreach (var helpPackagePath in helpPackages)
            {
                var helpPackage = this.fileSystem.DirectoryInfo.FromDirectoryName(helpPackagePath);
                var metadataFile = helpPackage.GetFiles("src/*.metadata.ts").FirstOrDefault();
                var metadataContent = metadataFile.ReadToString();
                var matchedIds = regex.Matches(metadataContent);
                var useLegacyFormat = false;
                if (matchedIds.Any(m => m.Captures.Any(id => !id.Value.Contains("."))))
                {
                    Log.Warning($"Using legacy menu item IDs! This package will not be deployable with other packages using legacy IDs, as collisions will happen!");
                    useLegacyFormat = true;
                }
                var pars = new Dictionary<string, string>
                {
                    {"basePath", helpRoot},
                    {"path", helpPackagePath},
                    {"project", useLegacyFormat ? project : pkgName}
                };
                var result = this.ExecutePwshScriptSync(pars);
                Console.WriteLine(String.Join(Environment.NewLine, result));
            }
        }

        /// <summary>
        /// Gets the powershell script.
        /// </summary>
        /// <returns></returns>
        protected override string GetPowershellScript()
        {
            return ResourceUtilities.GetEmbeddedResourceContent("Tools/GenerateBasedOnTemplates.ps1");
        }
    }
}
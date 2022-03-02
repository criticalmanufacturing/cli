using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
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
    [CmfCommand(name: "generateBasedOnTemplates", Parent = "help")]
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
            var helpRoot = FileSystemUtilities.GetPackageRootByType(Environment.CurrentDirectory, PackageType.Help, this.fileSystem).FullName;
            var project = FileSystemUtilities.ReadProjectConfig(this.fileSystem).RootElement.GetProperty("Tenant").GetString();
            var helpPackagesRoot = this.fileSystem.Path.Join(helpRoot, "src", "packages");
            var helpPackages = this.fileSystem.Directory.GetDirectories(helpPackagesRoot);
            var pkgName = CmfPackage.Load(this.fileSystem.FileInfo.FromFileName(this.fileSystem.Path.Join(helpRoot, "cmfpackage.json"))).PackageId.ToLowerInvariant();
            foreach (var helpPackagePath in helpPackages)
            {
                var pars = new Dictionary<string, string>
                {
                    {"basePath", helpRoot},
                    {"path", helpPackagePath},
                    {"project", pkgName}
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
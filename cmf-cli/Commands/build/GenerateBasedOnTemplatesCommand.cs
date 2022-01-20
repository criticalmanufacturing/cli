using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.PowershellCommand" />
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
            var helpRoot = FileSystemUtilities.GetPackageRootByType(Environment.CurrentDirectory, PackageType.Help, ExecutionContext.Instance.FileSystem).FullName;
            var project = FileSystemUtilities.ReadProjectConfig(ExecutionContext.Instance.FileSystem).RootElement.GetProperty("Tenant").GetString();
            var helpPackagesRoot = ExecutionContext.Instance.FileSystem.Path.Join(helpRoot, "src", "packages");
            var helpPackages = ExecutionContext.Instance.FileSystem.Directory.GetDirectories(helpPackagesRoot);
            foreach (var helpPackagePath in helpPackages)
            {
                var pars = new Dictionary<string, string>
                {
                    {"basePath", helpRoot},
                    {"path", helpPackagePath},
                    {"project", project}
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
            return GenericUtilities.GetEmbeddedResourceContent("Tools/GenerateBasedOnTemplates.ps1");
        }
    }
}
using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

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
            var helpRoot = this.fileSystem.Path.Join(FileSystemUtilities.GetProjectRoot(this.fileSystem).FullName, "UI", "Help");
            var project = FileSystemUtilities.ReadProjectConfig(this.fileSystem).RootElement.GetProperty("Tenant").GetString();
            var helpPackagesRoot = this.fileSystem.Path.Join(helpRoot, "src", "packages");
            var helpPackages = this.fileSystem.Directory.GetDirectories(helpPackagesRoot);
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
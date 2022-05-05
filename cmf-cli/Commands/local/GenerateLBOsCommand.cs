using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PowershellCommand" />
    [CmfCommand(name: "generateLBOs", Parent = "local")]
    public class GenerateLBOsCommand : PowershellCommand
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
            var toolsPath = this.fileSystem.Path.Join(Utilities.FileSystemUtilities.GetProjectRoot(this.fileSystem).FullName, "Tools");
            var pars = new Dictionary<string, string>
            {
                {"PSScriptRoot", toolsPath}
            };
            var result = this.ExecutePwshScriptSync(pars);
            Console.WriteLine(String.Join(Environment.NewLine, result));
        }

        /// <summary>
        /// Gets the powershell script.
        /// </summary>
        /// <returns></returns>
        protected override string GetPowershellScript()
        {
            return ResourceUtilities.GetEmbeddedResourceContent("Tools/Local_GenerateLBOs.ps1");
        }
    }
}
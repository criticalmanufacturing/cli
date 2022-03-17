using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Utilities;
using Utils = Cmf.CLI.Utilities.FileSystemUtilities;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PowershellCommand" />
    [CmfCommand(name: "getLocalEnvironment", Parent = "local")]
    public class GetLocalEnvironmentCommand : PowershellCommand
    {
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var root = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            var arg = new Argument<DirectoryInfo>(
                name: "target",
                description: "Where to place to fetched environment");
            cmd.AddArgument(arg);
            if (root != null)
            {
                var localEnvDefault = Path.GetRelativePath(Directory.GetCurrentDirectory(),
                    Path.Join(root.FullName, "LocalEnvironment"));
                arg.SetDefaultValue(new DirectoryInfo(localEnvDefault));
            }

            cmd.Handler = CommandHandler.Create<DirectoryInfo>(this.Execute);
        }

        /// <summary>
        /// Executes the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        public void Execute(DirectoryInfo target)
        {
            var config = FileSystemUtilities.ReadProjectConfig(this.fileSystem);
            var x = config.RootElement.EnumerateObject();
            var hostname = x.FirstOrDefault(y => y.NameEquals("vmHostname"));
            var installationPath = x.FirstOrDefault(y => y.NameEquals("InstallationPath"));
            var pars = new Dictionary<string, string>
            {
                {"hostname", hostname.Value.GetString()},
                {"SOURCE", installationPath.Value.GetString()},
                {"TARGET", target.FullName},
                {"ScriptPath", Directory.GetCurrentDirectory()}
            };

            // unprotect
            var unprotect =
                $@"$protectUnprotectConfigFilePath = ""{installationPath.Value.GetString()}\ProtectUnprotectConfigFile""
                $arguments = ""/Mode:2 /InstallPath:""""{installationPath.Value.GetString()}\BusinessTier""""""
                Set-Location -Path $protectUnprotectConfigFilePath
                Start-Process Cmf.Tools.ProtectUnprotectConfigFile.exe $arguments -Wait";
            Console.WriteLine("[V] Deal with host config file");
            var resultUnprotect = this.ExecutePwshScriptSync(pars, unprotect, hostname.Value.GetString());
            // Console.WriteLine(String.Join(Environment.NewLine, resultUnprotect));

            var result = this.ExecutePwshScriptSync(pars);
            Console.WriteLine(String.Join(Environment.NewLine, result));
        }

        /// <summary>
        /// Gets the powershell script.
        /// </summary>
        /// <returns></returns>
        protected override string GetPowershellScript()
        {
            return ResourceUtilities.GetEmbeddedResourceContent("Tools/Local_GetLocalEnvironment.ps1");
        }
    }
}
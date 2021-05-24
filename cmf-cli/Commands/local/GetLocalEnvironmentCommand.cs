using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using Utils = Cmf.Common.Cli.Utilities.FileSystemUtilities;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.PowershellCommand" />
    [CmfCommand(name: "getLocalEnvironment", Parent = "local")]
    public class GetLocalEnvironmentCommand : PowershellCommand
    {
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var root = Utils.GetProjectRoot();
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
            var config = Utils.ReadProjectConfig();
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
            return GenericUtilities.GetEmbeddedResourceContent("Tools/Local_GetLocalEnvironment.ps1");
        }
    }
}
using Cmf.CLI.Builders;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cmf.CLI.Commands.Generate
{
    /// <summary>
    ///     GenerateLBOsCommand
    /// </summary>
    [CmfCommand("lbos", Id = "generate_lbos", ParentId = "generate")]
    public class GenerateLBOsCommand : BaseCommand
    {
        /// <summary>
        ///     Configure command
        /// </summary>
        /// <param name="cmd">
        ///     Command
        /// </param>
        public override void Configure(Command cmd)
        {
            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        public void Execute()
        {
            string powershellExe = GenericUtilities.GetPowerShellExecutable();
            string generateLBOsScriptPath = fileSystem.Path.Join("Libs", "LBOs", "generateLBOs.ps1");

            CmfGenericUtilities.StartProjectDbContainer(fileSystem);
            CmfGenericUtilities.BuildAllPackagesOfType(PackageType.Business, fileSystem);

            Log.Information("GenerateLBOs PowerShell Script");
            new CmdCommand()
            {
                DisplayName = $"{powershellExe} .\\{generateLBOsScriptPath}",
                Command = $"{powershellExe} .\\{generateLBOsScriptPath}",
                Args = Array.Empty<string>(),
                WorkingDirectory = FileSystemUtilities.GetProjectRoot(fileSystem)
            }.Exec();
        }
    }
}
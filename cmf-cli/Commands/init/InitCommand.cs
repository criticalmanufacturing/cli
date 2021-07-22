using Cmf.Common.Cli.Attributes;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Config;
using Microsoft.TemplateEngine.Utils;
using Microsoft.TemplateSearch.Common.TemplateUpdate;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// Init command
    /// </summary>
    [CmfCommand("init")]
    public class InitCommand : TemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public InitCommand() : base("init")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public InitCommand(IFileSystem fileSystem) : base("init", fileSystem)
        {
        }

        /// <summary>
        /// configure command signature
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, "."),
                isDefault: true
            )
            {
                Description = "Working Directory"
            });

            cmd.AddArgument(new Argument<string>(
                name: "rootPackageName",
                getDefaultValue: () => "Cmf.Custom.Package"
            ));
            
            // passwords for user accounts
            
            cmd.AddOption(new Option<IFileInfo>(
                aliases: new string[] { "-c", "--config" },
                parseArgument: argResult => Parse<IFileInfo>(argResult),
                isDefault: true,
                description: "Configuration file exported from Setup"));
            
            cmd.AddOption(new Option<Uri>(
                aliases: new string[] { "--nugetRegistry" },
                description: "NuGet registry that contains the MES packages"
                ));

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, IFileInfo, Uri>(this.Execute);
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir"></param>
        /// <param name="rootPackageName"></param>
        /// <param name="config"></param>
        /// <param name="nugetRegistry"></param>
        public void Execute(IDirectoryInfo workingDir, string rootPackageName, IFileInfo config, Uri nugetRegistry)
        {
            var args = new List<string>()
            {
                // engine options
                "--output", workingDir.FullName,
                
                // template symbols
                "--customPackageName", rootPackageName
            };

            if (nugetRegistry != null)
            {
                args.AddRange(new [] {"--nugetRegistry", nugetRegistry.AbsoluteUri});
            }

            if (config != null)
            {
                args.AddRange(ParseConfigFile(config));
            }
            
            this.RunCommand(args);
        }

        private IEnumerable<string> ParseConfigFile(IFileInfo configFile)
        {
            var args = new List<string>();
            var configTxt = this.fileSystem.File.ReadAllText(configFile.FullName);
            dynamic configJson = JsonConvert.DeserializeObject(configTxt);
            if (configJson != null)
            {
                args.AddRange(new string[] { "--EnvironmentName", configJson["Product.SystemName"]?.Value });
                args.AddRange(new string[] { "--RESTPort", configJson["Product.ApplicationServer.Port"]?.Value });
                args.AddRange(new string[] { "--Tenant", configJson["Product.Tenant.Name"]?.Value });

                args.AddRange(new string[] { "--vmHostname", configJson["Product.ApplicationServer.Address"]?.Value });
                args.AddRange(new string[] { "--DBReplica1", configJson["Package[Product.Database.Online].Database.Server"]?.Value });
                args.AddRange(new string[] { "--DBReplica2", configJson["Package[Product.Database.Ods].Database.Server"]?.Value });
                args.AddRange(new string[] { "--DBServerOnline", configJson["Package[Product.Database.Online].Database.Server"]?.Value });
                args.AddRange(new string[] { "--DBServerODS", configJson["Package[Product.Database.Ods].Database.Server"]?.Value });
                args.AddRange(new string[] { "--DBServerDWH", configJson["Package[Product.Database.Dwh].Database.Server"]?.Value });
                args.AddRange(new string[] { "--ReportServerURI", configJson["Package.ReportingServices.Address"]?.Value });
                if (configJson["Product.Database.IsAlwaysOn"]?.Value)
                {
                    args.AddRange(new string[] { "--AlwaysOn" });
                }

                args.AddRange(new string[] { "--InstallationPath", configJson["Packages.Root.TargetDirectory"]?.Value });
                args.AddRange(new string[] { "--DBBackupPath", configJson["Product.Database.BackupShare"]?.Value });
                args.AddRange(new string[] { "--TemporaryPath", configJson["Product.DocumentManagement.TemporaryFolder"]?.Value });
                args.AddRange(new string[] { "--HTMLPort", configJson["Product.Presentation.IisConfiguration.Binding.Port"]?.Value });
                if (configJson["Product.Presentation.IisConfiguration.Binding.IsSslEnabled"]?.Value)
                {
                    args.AddRange(new string[] {"--IsSslEnabled"});    
                }
                

                args.AddRange(new string[] {"--GatewayPort", configJson["Product.Gateway.Port"]?.Value });

		        // args.Add("SQLUsername", "cmNavigoUser")
		        // # TODO: this needs to go
		        // args.Add("SQLPassword", "76492d1116743f0423413b16050a5345MgB8AGMAcABXAE8AcABsAG0AMQBQAEUAdgBrAHEAYgBnAG4ATgBOADkAUgBrAGcAPQA9AHwAOABjADYAZAAxAGMANgBjADcAYwBiADYAZAAzAGEAOQBhAGYAZAAzADEANAAwAGMAOABmADgANQA2AGYANQBhADYAOQA2ADgAYgA1AGQAMQBiAGUANQA4AGMAMAA0ADEAMQA0AGQANwBkAGEANQBhADAAOQA3AGMAYgAwAGQAZgA=")
		        // args.Add("AdminUsername", configJson[""Product.Users[1].Account")
		        // args.Add("AdminPassword", "76492d1116743f0423413b16050a5345MgB8ADAAVABRAGUAdQAzAHoAMgAwAHIAdQBrAEwASABxAHAASQBkAEoAWQBEAEEAPQA9AHwAOAAzADUAMQA5ADIANwBhADIAYgA1AGUAMgA3ADQAOQA5ADIAYwBlAGIAMQBiADkAOAAyAGQAMQBlAGMAMgA5ADIAMgBhADIAMgA1AGQAMQA5ADMAOQBkAGUANABjAGEAOAA0AGMANgAzADYAZQBmAGEAZAA1ADcAYwA5ADQAMAA=")
                args.AddRange(new string[] {"--ReleaseEnvironmentConfig", configFile.Name});
            } 
            return args;
        }
    }
}

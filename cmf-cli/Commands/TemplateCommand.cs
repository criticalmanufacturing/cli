using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Cmf.Common.Cli.Utilities;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Config;
using Microsoft.TemplateEngine.Utils;
using Newtonsoft.Json;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TemplateCommand : BaseCommand
    {
        private string commandName;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="commandName"></param>
        protected TemplateCommand(string commandName) : this(commandName, new FileSystem())
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="fileSystem"></param>
        protected TemplateCommand(string commandName, IFileSystem fileSystem) : base(fileSystem) => this.commandName = commandName;

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="args">the template engine arguments</param>
        public void RunCommand(IReadOnlyCollection<string> args)
        {
            var logger = new TelemetryLogger(null);

            New3Callbacks callbacks = new New3Callbacks()
            {
                OnFirstRun = FirstRun,
                //RestoreProject = RestoreProject
            };

            var version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            var commands = new List<string>
                {
                    this.commandName
                }
                .Concat(args.ToList())
                .Concat(new[] { "--debug:custom-hive", $"{System.Environment.GetEnvironmentVariable("USERPROFILE")}/.templateengine/cmf-cli/{version}" })
                .ToList();

            Log.Debug(string.Join(' ', commands));
            New3Command.Run(this.commandName, CreateHost(version), logger, callbacks, 
                commands.ToArray(), null);
        }

        private static ITemplateEngineHost CreateHost(string version)
        {
            var builtIns = new AssemblyComponentCatalog(new Assembly[]
            {
                typeof(RunnableProjectGenerator).GetTypeInfo().Assembly,
                typeof(ConditionalConfig).GetTypeInfo().Assembly,
                //typeof(NupkgUpdater).GetTypeInfo().Assembly
            });

            var preferences = new Dictionary<string, string>();

            return new DefaultTemplateEngineHost("cmf-cli", 
                version,
                System.Threading.Thread.CurrentThread.CurrentCulture.Name, preferences, builtIns);
}

        private static void FirstRun(IEngineEnvironmentSettings environmentSettings, IInstaller installer)
        {
            var paths = new Paths(environmentSettings);
            var templateFeed = new System.IO.DirectoryInfo(System.IO.Path.Join(paths.Global.BaseDir, "resources", "template_feed"));

            installer.InstallPackages(templateFeed.GetDirectories().Select(x => x.FullName));

            // install dotnet packages (for testing)
            //installer.InstallPackages(environmentSettings.Host.FileSystem.EnumerateFiles(@"C:\Program Files\dotnet\templates\5.0.7", "*.nupkg", System.IO.SearchOption.TopDirectoryOnly));
        }
        
        /// <summary>
        /// Parse a DF exported config file
        /// </summary>
        /// <param name="configFile">the path to the config file</param>
        /// <returns>template engine compatible switches array</returns>
        protected IEnumerable<string> ParseConfigFile(IFileInfo configFile)
        {
            var args = new List<string>();
            var configTxt = this.fileSystem.File.ReadAllText(configFile.FullName);
            dynamic configJson = JsonConvert.DeserializeObject(configTxt);
            if (configJson != null)
            {
                // here we retrieve only the entries from config that are actually useful for a template
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

                args.AddRange(new string[] {"--ReleaseEnvironmentConfig", configFile.Name});
            } 
            return args;
        }
    }
}
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Cmf.CLI.Objects;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Config;
using Microsoft.TemplateEngine.Utils;
using Newtonsoft.Json;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TemplateCommand : BaseCommand
    {
        protected string CommandName { get; set; }

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
        protected TemplateCommand(string commandName, IFileSystem fileSystem) : base(fileSystem) => this.CommandName = commandName;

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="args">the template engine arguments</param>
        public virtual void RunCommand(IReadOnlyCollection<string> args)
        {
            this.ExecuteTemplate(this.CommandName, args);
        }

        /// <summary>
        /// execute a template
        /// </summary>
        /// <param name="templateName">the name of the template</param>
        /// <param name="args">the template engine arguments</param>
        protected void ExecuteTemplate(string templateName, IReadOnlyCollection<string> args)
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
                    templateName
                }
                .Concat(args.ToList())
                .Concat(new[] { "--debug:custom-hive", $"{System.Environment.GetEnvironmentVariable("HOME")}/.templateengine/cmf-cli/{version}" })
                .ToList();

            Log.Debug(string.Join(' ', commands));
            New3Command.Run(templateName, CreateHost(version), logger, callbacks, 
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
                args.AddRange(new string[] { "--EnvironmentName", configJson["Product.SystemName"]?.Value ?? configJson["SYSTEM_NAME"]?.Value });
                args.AddRange(new string[] { "--RESTPort", configJson["Product.ApplicationServer.Port"]?.Value ?? configJson["APPLICATION_PUBLIC_HTTP_PORT"]?.Value });
                args.AddRange(new string[] { "--Tenant", configJson["Product.Tenant.Name"]?.Value ?? configJson["TENANT_NAME"]?.Value });

                args.AddRange(new string[] { "--vmHostname", configJson["Product.ApplicationServer.Address"]?.Value ?? configJson["APPLICATION_PUBLIC_HTTP_ADDRESS"]?.Value });
                args.AddRange(new string[] { "--DBReplica1", configJson["Package[Product.Database.Online].Database.Server"]?.Value ?? configJson["DATABASE_ONLINE_MSSQL_ADDRESS"]?.Value });
                args.AddRange(new string[] { "--DBReplica2", configJson["Package[Product.Database.Ods].Database.Server"]?.Value ?? configJson["DATABASE_ODS_MSSQL_ADDRESS"]?.Value ?? "" });
                args.AddRange(new string[] { "--DBServerOnline", configJson["Package[Product.Database.Online].Database.Server"]?.Value ?? configJson["DATABASE_ONLINE_MSSQL_ADDRESS"]?.Value });
                args.AddRange(new string[] { "--DBServerODS", configJson["Package[Product.Database.Ods].Database.Server"]?.Value ?? configJson["DATABASE_ODS_MSSQL_ADDRESS"]?.Value ?? "" });
                args.AddRange(new string[] { "--DBServerDWH", configJson["Package[Product.Database.Dwh].Database.Server"]?.Value ?? configJson["DATABASE_DWH_MSSQL_ADDRESS"]?.Value ?? "" });
                args.AddRange(new string[] { "--ReportServerURI", configJson["Package.ReportingServices.Address"]?.Value ?? configJson["REPORTING_SSRS_WEB_PORTAL_URL"]?.Value ?? "" });
                if (configJson["Product.Database.IsAlwaysOn"]?.Value ?? bool.Parse(configJson["DATABASE_MSSQL_ALWAYS_ON_ENABLED"]?.Value ?? false))
                {
                    args.AddRange(new string[] { "--AlwaysOn" });
                }

                if (configJson["Packages.Root.TargetDirectory"]?.Value != null)
                {
                    args.AddRange(new string[] { "--InstallationPath", configJson["Packages.Root.TargetDirectory"]?.Value });
                }
                if (configJson["Product.Database.BackupShare"]?.Value != null)
                {
                    args.AddRange(new string[] { "--DBBackupPath", configJson["Product.Database.BackupShare"]?.Value });
                }
                if (configJson["Product.DocumentManagement.TemporaryFolder"]?.Value != null)
                {
                    args.AddRange(new string[] { "--TemporaryPath", configJson["Product.DocumentManagement.TemporaryFolder"]?.Value });
                }
                args.AddRange(new string[] { "--HTMLPort", configJson["Product.Presentation.IisConfiguration.Binding.Port"]?.Value ?? configJson["APPLICATION_PUBLIC_HTTP_PORT"]?.Value });
                if (configJson["Product.Presentation.IisConfiguration.Binding.IsSslEnabled"]?.Value ?? bool.Parse(configJson["APPLICATION_PUBLIC_HTTP_TLS_ENABLED"]?.Value ?? false))
                {
                    args.AddRange(new string[] {"--IsSslEnabled"});    
                }

                args.AddRange(new string[] {"--GatewayPort", configJson["Product.Gateway.Port"]?.Value ?? configJson["APPLICATION_PUBLIC_HTTP_PORT"]?.Value });

                args.AddRange(new string[] {"--DefaultDomain", configJson["Product.Security.Domain"]?.Value ?? configJson["SECURITY_PORTAL_STRATEGY_LOCAL_AD_DEFAULT_DOMAIN"]?.Value});

                args.AddRange(new string[] {"--ReleaseEnvironmentConfig", configFile.Name});
            } 
            return args;
        }

        /// <summary>
        /// register this package as a dependency in the parent package
        /// </summary>
        /// <param name="packageName">this package name</param>
        /// <param name="version">this package version</param>
        /// <param name="parentPath">the parent directory of the package</param>
        /// <param name="isTestPackage">is this new package a test package</param>
        protected void RegisterAsDependencyInParent(string packageName, string version, string parentPath, bool isTestPackage = false)
        {
            // add to upper level package
            // {
            //     "id": "Cmf.Custom.Business",
            //     "version": "4.33.0"
            // }
            var parentPackageDir = FileSystemUtilities.GetPackageRoot(this.fileSystem, parentPath);
            if (parentPackageDir != null)
            {
                var package = this.GetPackageInFolder(parentPackageDir.FullName);
                if (package != null)
                {
                    var deps = isTestPackage ? 
                        package.TestPackages ??= new DependencyCollection() : 
                        package.Dependencies;
                    var doesNotExist = deps.FirstOrDefault(dep => dep.Id == packageName && dep.Version == version) == null;
                    if (doesNotExist)
                    {
                        deps.Add(new Dependency(packageName, version));
                        package.SaveCmfPackage();
                    }
                }
            }
        }

        /// <summary>
        /// Get the CmfPackage in the folder
        /// </summary>
        /// <param name="path">the folder path</param>
        /// <returns>the CmfPackage in the folder</returns>
        protected CmfPackage GetPackageInFolder(string path)
        {
            var cmfPackage = this.fileSystem.Path.Join(path, CliConstants.CmfPackageFileName);
            if (this.fileSystem.File.Exists(cmfPackage))
            {
                return CmfPackage.Load(
                    this.fileSystem.FileInfo.FromFileName(
                        cmfPackage),
                    fileSystem: this.fileSystem);
            }
            return null;
        }
    }
}
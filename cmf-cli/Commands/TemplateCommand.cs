using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Abstractions.TemplatePackage;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.IDE;
using Microsoft.TemplateEngine.Utils;
using Newtonsoft.Json;
using DefaultTemplateEngineHost = Microsoft.TemplateEngine.Edge.DefaultTemplateEngineHost;
using ExecutionContext = Cmf.CLI.Core.Objects.ExecutionContext;

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
        protected async void ExecuteTemplate(string templateName, IReadOnlyCollection<string> args)
        {
            Log.Debug($"Going to generate template {templateName}");
            // TODO: args needs to become a dictionary
            var parameters = new Dictionary<string, string>();
            var argList = args.ToList().Aggregate(new List<List<string>>(), (list, s) =>
            {
                if (s.StartsWith("--"))
                {
                    list.Add(new List<string>() { s });
                }
                else
                {
                    list.Last().Add(s);
                }

                return list;
            });
            foreach (var list in argList)
            {
                switch (list.Count)
                {
                    case 1:
                        parameters.Add(list[0].Replace("--", ""), null);
                        Log.Debug($"Adding template param with key {list[0].Replace("--", "")}");
                        break;
                    case 2:
                        parameters.Add(list[0].Replace("--", ""), list[1]);
                        Log.Debug($"Adding template param with key {list[0].Replace("--", "")} and value {list[1]}");
                        break;
                    default:
                        Log.Error($"Cannot deal with key pairs with length {list.Count}");
                        break;
                }
            }
            
            var name = parameters.ContainsKey("name") ? parameters["name"] : "package"; // package is a placeholder, if name isn't provided, the template doesn't use it
            var outputPath = parameters.ContainsKey("output") ? parameters["output"] : ExecutionContext.Instance.FileSystem.Directory.GetCurrentDirectory();

            var version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            var templateEngineHost = CreateHost(version);
            using var bootstrapper = new Bootstrapper(templateEngineHost, false);

            // this needs a refactor, right now we're minimizing the impact on the implemented commands
            if (templateName == "new" && parameters.ContainsKey("debug:reinit")) 
            {
                Log.Debug("Cleaning up the template engine store");
                // we have to re-instantiate the engine environment settings as they are private in the bootstrapper, but are exactly the same
                var engineEnvironmentSettings = new EngineEnvironmentSettings(templateEngineHost, virtualizeSettings: false);
                Log.Debug($"Cleaning up {engineEnvironmentSettings.Paths.HostVersionSettingsDir}");
                templateEngineHost.FileSystem.DirectoryDelete(engineEnvironmentSettings.Paths.HostVersionSettingsDir, true);
                templateEngineHost.FileSystem.CreateDirectory(engineEnvironmentSettings.Paths.HostVersionSettingsDir);
                Log.Debug("Done");
                return;
            }

            Log.Debug("Getting templates");
            // we want to use: var templates = await bootstrapper.GetTemplatesAsync(); 
            var t = bootstrapper.GetTemplatesAsync(default);
            t.Wait();
            var templates = t.Result;
            Log.Debug($"Found templates:\n{string.Join("\n",templates.Select(t => t.Name))}\n");
            var template = templates.FirstOrDefault(tpl => tpl.ShortNameList.Contains(templateName));
            if (template == null)
            {
                throw new CliException($"Cannot find template {templateName}");
            }
            Log.Debug($"Running template {template.Name} for generating {name} in {outputPath}");
            try
            {
                var result = await bootstrapper.CreateAsync(template!, name, outputPath, parameters);
                Log.Debug($"{result.Status.ToString()}: {result.ErrorMessage}");
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        
        internal class CmfCliPackageProviderFactory : ITemplatePackageProviderFactory
        {
            public Guid Id => new Guid("bacf2f68-3346-4097-a472-e093a2f2843a");

            public ITemplatePackageProvider CreateProvider(IEngineEnvironmentSettings settings)
            {
                return new CmfCliPackageProvider(this, settings);
            }
            
            private class CmfCliPackageProvider : ITemplatePackageProvider, IIdentifiedComponent
            {
                private readonly IEngineEnvironmentSettings settings;

                public CmfCliPackageProvider(ITemplatePackageProviderFactory builtInTemplatePackagesProviderFactory, IEngineEnvironmentSettings settings)
                {
                    this.Factory = builtInTemplatePackagesProviderFactory;
                    this.settings = settings;
                }

                public Task<IReadOnlyList<ITemplatePackage>> GetAllTemplatePackagesAsync(CancellationToken cancellationToken)
                {
                    List<ITemplatePackage> templatePackages = new List<ITemplatePackage>();
                    string assemblyLocation = ExecutionContext.Instance.FileSystem.Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    
                    IEnumerable<string> expandedPaths = InstallRequestPathResolution.ExpandMaskedPath(ExecutionContext.Instance.FileSystem.Path.Join(assemblyLocation, "resources", "template_feed"), settings);
                    foreach (string path in expandedPaths)
                    {
                        if (settings.Host.FileSystem.FileExists(path) || settings.Host.FileSystem.DirectoryExists(path))
                        {
                            templatePackages.Add(new TemplatePackage(this, path, settings.Host.FileSystem.GetLastWriteTimeUtc(path)));
                        }
                    }
                    
                    return Task.FromResult((IReadOnlyList<ITemplatePackage>)templatePackages);
                }

                public ITemplatePackageProviderFactory Factory { get; }
                // disable warning due to unused event (part of ITemplatePackageProvider)
                #pragma warning disable CS0067
                public event Action TemplatePackagesChanged;
                #pragma warning restore CS0067
                public Guid Id => new Guid("22a853c8-fae7-42fb-bc58-d858010becf5");
            }

            public string DisplayName => "CMF CLI templates";
        }

         private static ITemplateEngineHost CreateHost(string version)
         {
             var builtIns = new List<(Type InterfaceType, IIdentifiedComponent Instance)>();
             builtIns.AddRange(Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Components.AllComponents);
             builtIns.AddRange(Microsoft.TemplateEngine.Edge.Components.AllComponents);
             builtIns.Add((typeof(ITemplatePackageProviderFactory), new CmfCliPackageProviderFactory()));
             var preferences = new Dictionary<string, string>
             {
             };

             return new DefaultTemplateEngineHost(hostIdentifier: "cmf-cli", 
                 version: version,
                 defaults: preferences, 
                 builtIns: builtIns,
                 // loggerFactory: Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                 //     builder
                 //         .SetMinimumLevel(logLevel)
                 //         .AddConsole(config => config.FormatterName = nameof(CliConsoleFormatter))
                 //         .AddConsoleFormatter<CliConsoleFormatter, ConsoleFormatterOptions>(config =>
                 //         {
                 //             config.IncludeScopes = true;
                 //             config.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
                 //         }))
                 loggerFactory: null
             );
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
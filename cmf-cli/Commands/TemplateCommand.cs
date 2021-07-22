using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Config;
using Microsoft.TemplateEngine.Utils;

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
        /// <param name="args"></param>
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
                ;

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
    }
}
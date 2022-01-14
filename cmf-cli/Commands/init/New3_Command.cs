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

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// Command to test templates
    /// </summary>
    [CmfCommand("new3", IsHidden = true)]
    public class New3_Command : BaseCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public New3_Command() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public New3_Command(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        /// <summary>
        /// configure command signature
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            // Add the handler
            cmd.Handler = CommandHandler.Create((ParseResult parseResult, IConsole console) =>
            {
                // console.Out.WriteLine($"{parseResult}");
                Execute(parseResult.UnparsedTokens);
            });
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public static void Execute(IReadOnlyCollection<string> args)
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

            var commands = args.ToList().Concat(new[] { "--debug:custom-hive", $"{System.Environment.GetEnvironmentVariable("USERPROFILE")}/.templateengine/cmf-cli/{version}" });

            New3Command.Run("new3", CreateHost(version), logger, callbacks, 
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

            var preferences = new Dictionary<string, string>
            {
            };

            return new DefaultTemplateEngineHost("cmf-cli", 
                version,
                System.Threading.Thread.CurrentThread.CurrentCulture.Name, preferences, builtIns);
}

        private static void FirstRun(IEngineEnvironmentSettings environmentSettings, IInstaller installer)
        {
            var paths = new Paths(environmentSettings);
            var template_feed = new System.IO.DirectoryInfo(System.IO.Path.Join(paths.Global.BaseDir, "resources", "template_feed"));

            installer.InstallPackages(template_feed.GetDirectories().Select(x => x.FullName));

            // install dotnet packages (for testing)
            //installer.InstallPackages(environmentSettings.Host.FileSystem.EnumerateFiles(@"C:\Program Files\dotnet\templates\5.0.7", "*.nupkg", System.IO.SearchOption.TopDirectoryOnly));
        }
    }
}

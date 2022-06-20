using Cmf.CLI.Objects;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// List dependencies command
    /// </summary>
    [CmfCommand("ls")]
    public class ListDependenciesCommand : BaseCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ListDependenciesCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public ListDependenciesCommand(IFileSystem fileSystem) : base(fileSystem)
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

            cmd.AddOption(new Option<Uri[]>(
                aliases: new string[] { "-r", "--repos", "--repo" },
                description: "Repositories where dependencies are located (folder)"));

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, Uri[]>(Execute);
        }

        /// <summary>
        /// Determine and print a package dependency tree
        /// </summary>
        /// <param name="workingDir">the path of the package which dependency tree we want to obtain</param>
        /// <param name="repos">a set of repositories for remote packages</param>
        public void Execute(IDirectoryInfo workingDir, Uri[] repos)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{workingDir}/{CliConstants.CmfPackageFileName}");

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true, this.fileSystem);
            
            if (repos != null)
            {
                ExecutionContext.Instance.RepositoriesConfig.Repositories.InsertRange(0, repos);
            }

            Log.Status("Starting ls...", ctx => {
                cmfPackage.LoadDependencies(ExecutionContext.Instance.RepositoriesConfig.Repositories.ToArray(), ctx, true);
                ctx.Status("Finished ls");

                var tree = GenericUtilities.BuildTree(cmfPackage);
                Log.Render(tree);
            });
        }
    }
}

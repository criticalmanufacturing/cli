using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using ExecutionContext = Cmf.CLI.Core.Objects.ExecutionContext;
using Cmf.CLI.Core.Constants;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// List dependencies command
    /// </summary>
    [CmfCommand("ls", Id = "ls")]
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
            if (ExecutionContext.ServiceProvider?.GetService<IFeaturesService>()?.UseRepositoryClients ?? false)
            {
                // var ctrlr = new CmfPackageController(cmfpackageFile, this.fileSystem, setDefaultValues: true);
                var client = ExecutionContext.ServiceProvider?.GetService<IRepositoryLocator>()
                    .GetRepositoryClient(new Uri(workingDir.FullName), workingDir.FileSystem);
                var cmfPackage = client.Find(null, null).GetAwaiter().GetResult();
                var ctrlr = new CmfPackageController(cmfPackage, this.fileSystem);
                Log.Status("Starting ls...", ctx =>
                {
                    // TODO: we should await but the Status will exit without waiting, so we're blocking instead until we figure it out
                    ctrlr.LoadDependencies(null, ctx, true).GetAwaiter().GetResult(); 
                    ctx.Status("Finished ls");
                    Thread.Sleep(100);

                    try
                    {
                        var tree = GenericUtilities.BuildTree(ctrlr.CmfPackage);
                        Log.Render(tree);
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                        throw;
                    }
                });
            }
            else
            {
                IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{workingDir}/{CoreConstants.CmfPackageFileName}");

                // Reading cmfPackage
                CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true, this.fileSystem);
                
                if (repos != null)
                {
                    ExecutionContext.Instance.RepositoriesConfig.Repositories.InsertRange(0, repos);
                }
                
                Log.Status("Starting ls...", ctx => {
                    cmfPackage.LoadDependencies(ExecutionContext.Instance.RepositoriesConfig.Repositories.ToArray(), ctx, true);
                    ctx.Status("Finished ls");
                    Thread.Sleep(100);
                
                    var tree = GenericUtilities.BuildTree(cmfPackage);
                    Log.Render(tree);
                });
            }

            
            
        }
    }
}

using System;
using System.CommandLine;
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
            var workingDirArgument = new Argument<IDirectoryInfo>("workingDir")
            {
                Description = "Working Directory",
                CustomParser = argResult => Parse<IDirectoryInfo>(argResult, "."),
                DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, ".")
            };
            cmd.Add(workingDirArgument);

            var reposOption = new Option<Uri[]>("--repos", "-r", "--repo")
            {
                Description = "Repositories where dependencies are located (folder)",
                CustomParser = argResult => ParseUriArray(argResult)
            };
            cmd.Add(reposOption);

            // Add the handler
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var workingDir = parseResult.GetValue(workingDirArgument);
                var repos = parseResult.GetValue(reposOption);

                Execute(workingDir, repos);
                return Task.FromResult(0);
            });
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
                IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{workingDir}/{CliConstants.CmfPackageFileName}");

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
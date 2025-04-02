using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands;

[CmfCommand("publish", Id = "publish", Description = "Publishes a local package to the specified repository")]
public class PublishCommand : BaseCommand
{
    public override void Configure(Command cmd)
    {
        cmd.AddArgument(new Argument<IFileInfo>(
            name: "file",
            parse: (argResult) => Parse<IFileInfo>(argResult),
            isDefault: false)
        {
            Description = "Package file"
        });
        
        cmd.AddOption(new Option<Uri>(
            aliases: new string[] { "--repository" },
            description: "Repository the package should be published to"));

        cmd.IsHidden =
            !(ExecutionContext.ServiceProvider?.GetService<IFeaturesService>()?.UseRepositoryClients ?? false);
        
        // Add the handler
        cmd.Handler = CommandHandler.Create<IFileInfo, Uri>(Execute);
    }

    public void Execute(IFileInfo file, Uri repository)
    {
        using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

        if (!file.Exists || file.Directory == null)
        {
            throw new CliException(
                $"Could not find package file {file.FullName}, make sure the file exists and is valid");
        }

        if (file.Extension != ".zip" && file.Extension != ".tgz")
        {
            throw new CliException(
                $"The package needs to be in a zip or gzipped tar file (with .tgz extension). Use the `pack` command to get a valid file to publish.");
        }

        // request a client for the specific file, not its directory. This way the List call below always returns a single file.
        // NOTE: this is only guaranteed in the Local and Archive repo clients!
        //  It is possible to invoke this command from Linux with a share path which would return a CIFSRepositoryClient,
        //  which does not support file as root but also does not support List
        var client = ExecutionContext.ServiceProvider?.GetService<IRepositoryLocator>()
            .GetRepositoryClient(new Uri(file.FullName), file.FileSystem);
        if (client == null)
        {
            throw new CliException($"Could not determine repository type for {file.FullName}!");
        }
        Log.Debug($"Got client {client.GetType().Name} for package file {file.FullName}");
        var repoClient = ExecutionContext.ServiceProvider?.GetService<IRepositoryLocator>()
            .GetRepositoryClient(repository, file.FileSystem);
        if (repoClient == null)
        {
            throw new CliException($"Could not determine repository type for {repository.AbsoluteUri}!");
        }
        Log.Debug($"Got client {repoClient.GetType().Name} for repository URL {repository.AbsoluteUri}");
        var pkg = client.List().GetAwaiter().GetResult().FirstOrDefault();
        Log.Debug($"Got package {pkg!.PackageAtRef} from origin repository");
        var ctlr = new CmfPackageController(pkg, fileSystem);
        Log.Debug($"Publishing package with target repository client...");
        repoClient.Put(ctlr.CmfPackage).GetAwaiter().GetResult();
        Log.Debug("Publish completed!");
    }
}
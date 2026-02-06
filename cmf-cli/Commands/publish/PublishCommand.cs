using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands;

[CmfCommand("publish", Id = "publish", Description = "Publishes a local package to the specified repository")]
public class PublishCommand : BaseCommand
{
    #region Constructors

    /// <summary>
    /// Publish Command
    /// </summary>
    public PublishCommand() : base() { }

    /// <summary>
    /// Publish Command
    /// </summary>
    /// <param name="fileSystem"></param>
    public PublishCommand(IFileSystem fileSystem) : base(fileSystem) { }

    #endregion

    public override void Configure(Command cmd)
    {
        var fileArgument = new Argument<IFileInfo>("file")
        {
            Description = "Package file"
        };
        fileArgument.CustomParser = argResult => Parse<IFileInfo>(argResult);
        cmd.Arguments.Add(fileArgument);

        var ciOption = new Option<bool>("--ci")
        {
            Description = "Use the Continuous Integration repository URL from the repositories file"
        };
        cmd.Options.Add(ciOption);

        var releaseOption = new Option<bool>("--release")
        {
            Description = "Use the first non-CI repository URL from the repositories file"
        };
        cmd.Options.Add(releaseOption);

        var repositoryOption = new Option<Uri>("--repository")
        {
            Description = "Repository the package should be published to"
        };
        repositoryOption.CustomParser = argResult => ParseUri(argResult);
        cmd.Options.Add(repositoryOption);

        cmd.Hidden =
            !(Core.Objects.ExecutionContext.ServiceProvider?.GetService<IFeaturesService>()?.UseRepositoryClients ?? false);

        // Add the handler
        cmd.SetAction((parseResult, cancellationToken) =>
        {
            var file = parseResult.GetValue(fileArgument);
            var repository = parseResult.GetValue(repositoryOption);
            var ci = parseResult.GetValue(ciOption);
            var release = parseResult.GetValue(releaseOption);

            Execute(file, repository, ci, release);
            return Task.FromResult(0);
        });
    }

    public void Execute(IFileInfo file, Uri repository, bool ci, bool release)
    {
        using var activity = Core.Objects.ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

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

        if (ci && release)
        {
            throw new CliException(
                $"Cannot use both flags `--ci` and `--release` at the same time.");
        }

        if ((ci || release) && repository != null)
        {
            throw new CliException(
                $"The `--{(ci ? "ci" : "release")}` flag can only be used when no explicit `--repository is passed`.");
        }

        var repositoryLocator = Core.Objects.ExecutionContext.ServiceProvider?.GetService<IRepositoryLocator>();

        if (ci)
        {
            repository = Core.Objects.ExecutionContext.Instance.RepositoriesConfig?.CIRepository;

            if (repository == null)
            {
                throw new CliException(
                    $"No CIRepository was defined on the repositories configuration file, cannot use the `--ci` flag.");
            }
        }
        else if (release)
        {
            repository = Core.Objects.ExecutionContext.Instance.RepositoriesConfig?.Repositories?.FirstOrDefault();

            if (repository == null)
            {
                throw new CliException(
                    $"No Repositories were defined on the repositories configuration file, cannot use the `--release` flag.");
            }
        }

        if (repository == null)
        {
            throw new CliException(
                $"No repository URL to publish to was passed. Try using one of the following options: `--ci`, `--release` or `--repository`.");
        }

        // If it passes the above checks the only possible client for the requested file 
        // is a ArchiveRepositoryClient with only a single Package
        var client = Core.Objects.ExecutionContext.ServiceProvider?.GetService<IRepositoryLocator>()
            .GetRepositoryClient(new Uri(file.FullName), file.FileSystem) as ArchiveRepositoryClient;
        if (client == null)
        {
            throw new CliException($"Could not determine repository type for {file.FullName}!");
        }
        Log.Debug($"Got client {client.GetType().Name} for package file {file.FullName}");
        var repoClient = Core.Objects.ExecutionContext.ServiceProvider?.GetService<IRepositoryLocator>()
            .GetRepositoryClient(repository, file.FileSystem);
        if (repoClient == null)
        {
            throw new CliException($"Could not determine repository type for {repository.AbsoluteUri}!");
        }
        Log.Debug($"Got client {repoClient.GetType().Name} for repository URL {repository.AbsoluteUri}");
        Log.Debug($"Publishing package with target repository client...");
        repoClient.Put(client.List().Result.Single()).GetAwaiter().GetResult();
        Log.Debug("Publish completed!");
    }
}
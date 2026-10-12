using System;
using System.CommandLine;
using System.IO.Abstractions;
using System.Collections.Generic;
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
        var packagesArgument = new Argument<string[]>("packagePaths")
        {
            Description = "Package file(s) (.zip or .tgz) or folder(s) containing package files",
            Arity = ArgumentArity.OneOrMore
        };
        cmd.Add(packagesArgument);

        var ciOption = new Option<bool>("--ci")
        {
            Description = "Use the Continuous Integration repository URL from the repositories file"
        };
        cmd.Add(ciOption);

        var releaseOption = new Option<bool>("--release")
        {
            Description = "Use the first non-CI repository URL from the repositories file"
        };
        cmd.Add(releaseOption);

        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Resolve packages and print what would be published without uploading anything"
        };
        cmd.Add(dryRunOption);

        var repositoryOption = new Option<Uri>("--repository")
        {
            Description = "Repository the package should be published to",
            CustomParser = argResult => ParseUri(argResult)
        };
        cmd.Add(repositoryOption);

        cmd.Hidden =
            !(ExecutionContext.ServiceProvider?.GetService<IFeaturesService>()?.UseRepositoryClients ?? false);

        // Add the handler
        cmd.SetAction((parseResult, cancellationToken) =>
        {
            var packages = parseResult.GetValue(packagesArgument);
            var repository = parseResult.GetValue(repositoryOption);
            var ci = parseResult.GetValue(ciOption);
            var release = parseResult.GetValue(releaseOption);
            var dryRun = parseResult.GetValue(dryRunOption);

            Execute(packages, repository, ci, release, dryRun);
            return Task.FromResult(0);
        });
    }

    public void Execute(string packagePath, Uri repository, bool ci, bool release)
    {
        Execute([packagePath], repository, ci, release, false);
    }

    public void Execute(string[] packagePaths, Uri repository, bool ci, bool release)
    {
        Execute(packagePaths, repository, ci, release, false);
    }

    public void Execute(string[] packagePaths, Uri repository, bool ci, bool release, bool dryRun)
    {
        using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

        if (ci && release)
        {
            throw new CliException(
                "Cannot use both flags `--ci` and `--release` at the same time.");
        }

        if ((ci || release) && repository != null)
        {
            throw new CliException(
                $"The `--{(ci ? "ci" : "release")}` flag can only be used when no explicit `--repository is passed`.");
        }


        if (ci)
        {
            repository = ExecutionContext.Instance.RepositoriesConfig?.CIRepository;

            if (repository == null)
            {
                throw new CliException(
                    "No CIRepository was defined on the repositories configuration file, cannot use the `--ci` flag.");
            }
        }
        else if (release)
        {
            repository = ExecutionContext.Instance.RepositoriesConfig?.Repositories?.FirstOrDefault();

            if (repository == null)
            {
                throw new CliException(
                    "No Repositories were defined on the repositories configuration file, cannot use the `--release` flag.");
            }
        }

        if (repository == null)
        {
            throw new CliException(
                "No repository URL to publish to was passed. Try using one of the following options: `--ci`, `--release` or `--repository`.");
        }

        var repositoryLocator = ExecutionContext.ServiceProvider?.GetService<IRepositoryLocator>();
        var packagesToPublish = new List<CmfPackageV1>();

        foreach (var packagePath in packagePaths)
        {
            var directory = fileSystem.DirectoryInfo.New(packagePath);
            var fileInfo = fileSystem.FileInfo.New(packagePath);

            if (fileInfo.Exists)
            {
                if (fileInfo.Extension != ".zip" && fileInfo.Extension != ".tgz")
                {
                    throw new CliException(
                        "The package needs to be in a zip or gzipped tar file (with .tgz extension). Use the `pack` command to get a valid file to publish.");
                }

                var fileClient = repositoryLocator?.GetRepositoryClient(new Uri(fileInfo.FullName), fileInfo.FileSystem) as ArchiveRepositoryClient;
                if (fileClient == null)
                {
                    throw new CliException($"Could not determine repository type for {fileInfo.FullName}!");
                }

                Log.Debug($"Got client {fileClient.GetType().Name} for package file {fileInfo.FullName}");
                packagesToPublish.Add(fileClient.List().GetAwaiter().GetResult().Single());
                continue;
            }

            if (!directory.Exists)
            {
                throw new CliException(
                    $"Could not find package file or directory at {packagePath}, make sure the path exists and is valid");
            }

            var directoryClient = new ArchiveRepositoryClient(directory.FullName, directory.FileSystem);
            Log.Debug($"Got client {directoryClient.GetType().Name} for package directory {directory.FullName}");
            var folderPackages = directoryClient.List().GetAwaiter().GetResult();

            if (!folderPackages.Any())
            {
                Log.Information($"No packages found in folder {directory.FullName}. Skipping...");
                continue;
            }

            Log.Information($"Found {folderPackages.Count} package(s) in folder {directory.FullName}.");
            packagesToPublish.AddRange(folderPackages);
        }
        
        if (!packagesToPublish.Any())
        {
            Log.Information($"No packages found to publish.");
        }
        else
        {
            var repoClient = repositoryLocator?.GetRepositoryClient(repository, fileSystem);
            if (repoClient == null)
            {
                throw new CliException($"Could not determine repository type for {repository.AbsoluteUri}!");
            }
            Log.Debug($"Got client {repoClient.GetType().Name} for repository URL {repository.AbsoluteUri}");

            foreach (var package in packagesToPublish)
            {
                var action = dryRun ? "Would publish" : "Publishing";
                Log.Information($"{action} {package.PackageDotRef}...");

                if (!dryRun)
                {
                    repoClient.Put(package).GetAwaiter().GetResult();
                }
            }

            if (dryRun)
            {
                Log.Information($"Dry run completed. {packagesToPublish.Count} package(s) would be published.");
            }
            else
            {
                Log.Information($"Completed publishing {packagesToPublish.Count} package(s)!");
            }
        }
    }
}
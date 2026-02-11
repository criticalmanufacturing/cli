using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Core.Services;

public class RepositoryLocator : IRepositoryLocator
{
    private Dictionary<string, IRepositoryClient> clients = new();
    public IRepositoryClient GetRepositoryClient(Uri uri, IFileSystem fileSystem)
    {
        IRepositoryClient client;
        if (clients.TryGetValue(uri.AbsoluteUri, out var repositoryClient))
        {
            client = repositoryClient;
        }
        else
        {
            switch (uri.Scheme)
            {
                case "http":
                case "https":
                    client = new NPMRepositoryClient(uri.AbsoluteUri, fileSystem);
                    break;
                case "file":
                    if (uri.Host == "")
                    {
                        var file = fileSystem.FileInfo.New(uri.LocalPath);
                        if (file.Exists && (file.Extension == ".zip" || file.Extension == ".tgz"))
                        {
                            client = new ArchiveRepositoryClient(uri.LocalPath, fileSystem);
                        }
                        else
                        {
                            client = new LocalRepositoryClient(uri.LocalPath, fileSystem);
                        }
                    }
                    else
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            client = new ArchiveRepositoryClient(uri.AbsoluteUri, fileSystem);
                        }
                        else
                        {
                            client = new CIFSRepositoryClient(uri.AbsoluteUri, fileSystem);
                        }
                    }
                    break;
                    default:
                    throw new NotSupportedException(
                        $"Unsupported repository scheme '{uri.Scheme}'.");
            }
            clients.Add(uri.AbsoluteUri, client);
        }
        return client;
    }

    public IRepositoryClient GetSourceClient(IFileSystem fileSystem)
    {
        var root = FileSystemUtilities.GetProjectRoot(fileSystem) ?? throw new CliException("Not in a project repository");
        return this.GetRepositoryClient(new Uri(root.FullName), fileSystem);
    }

    public void InitializeClientsForRepositories(IFileSystem fileSystem, IEnumerable<Uri>? repoUris)
    {
        if (repoUris == null)
        {
            // load repositories from repositories.json
            var repositories = FileSystemUtilities.ReadRepositoriesConfig(fileSystem);
            repoUris = repositories.Repositories;
        }
        var clients = repoUris.Select(r => this.GetRepositoryClient(r,fileSystem)).Where(client => client != null);
        if (clients.Count() != repoUris.Count())
        {
            Log.Debug("Could not obtain clients for all repositories!");
        }
    }

    public void InitializeClientsForRepositories(IFileSystem fileSystem)
    {
        this.InitializeClientsForRepositories(fileSystem, null);
    }

    public async Task<CmfPackageV1?> FindPackage(string packageId, string packageVersion)
    {
        var x = this.clients.Where(pair => !pair.Value.Unreacheable).Select((pair) =>
        {
            var client = pair.Value;
            Log.Debug($"Looking for package {packageId}@{packageVersion} in repo {client.RepositoryRoot} handled by {client.GetType().Name}");
            return client.Find(packageId, packageVersion);
        }).ToList();
        try
        {
            // var y = await Task.WhenAll(x);
            // var z = y.FirstOrDefault(p => p != null);
            // return z;

            foreach (var taskPkg in x)
            {
                try 
                {
                    var pkg = await taskPkg;
                    if (pkg != null)
                    {
                        return pkg;
                    }
                } 
                catch (Exception e)
                {
                    Log.Debug(e.Message);
                }
            }
            
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
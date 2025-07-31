using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Services;

namespace Cmf.CLI.Core.Repository;

public class LocalRepositoryClient : IRepositoryClient
{
    private IDirectoryInfo root;
    private IFileInfo file = null;
    
    public LocalRepositoryClient(string rootPath) : this(rootPath, new FileSystem())
    {
    }
    
    public LocalRepositoryClient(string rootPath, IFileSystem fileSystem)
    {
        // remove file handler
        rootPath = new Uri(rootPath).AbsolutePath;
        Log.Debug("Creating LocalRepositoryClient for path " + rootPath);
        var checkFile = fileSystem.FileInfo.New(rootPath);
        if (checkFile.Exists)
        {
            file = checkFile;
            root = checkFile.Directory;
        }
        else
        {
            root = fileSystem.DirectoryInfo.New(rootPath);
        }
    }
    
    public async Task<CmfPackageV1> Find(string packageId, string version)
    {
        return (await this.List()).FirstOrDefault(p =>
        {
            var idMatch = string.IsNullOrEmpty(packageId) || p.PackageId == packageId;
            var versionMatch = string.IsNullOrEmpty(version) || p.Version == version;
            return idMatch && versionMatch;
        });
    }

    public Task<CmfPackageV1Collection> List()
    {
        CmfPackageV1Collection cmfPackages = [];
        IFileInfo[] cmfPackageFiles = this.file != null ? [this.file] : root.GetFiles(CoreConstants.CmfPackageFileName, SearchOption.AllDirectories);
        foreach (IFileInfo cmfPackageFile in cmfPackageFiles)
        {
            var cmfPackage = CmfPackageController.FromSourceManifest(cmfPackageFile);
            cmfPackage.Client = this;
            cmfPackages.Add(cmfPackage);
        }

        return Task.FromResult(cmfPackages);
    }

    public Task Put(CmfPackageV1 package)
    {
        throw new NotSupportedException("Cannot publish to the local source repository!");
    }

    public Task<IFileInfo> Get(CmfPackageV1 package, IDirectoryInfo targetDirectory)
    {
        throw new NotSupportedException();
    }

    public Task<IDirectoryInfo> Extract(CmfPackageV1 package, IDirectoryInfo targetDirectory)
    {
        throw new NotSupportedException("Cannot extract packages from the local source repository!");
    }

    public string RepositoryRoot => this.root.FullName;
    public bool Unreacheable => !this.root.Exists;
}
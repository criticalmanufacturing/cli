using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Core.Interfaces;

public interface IRepositoryLocator
{
    IRepositoryClient GetRepositoryClient(Uri uri, IFileSystem fileSystem, CmfAuthFile authFile);

    IRepositoryClient GetSourceClient(IFileSystem fileSystem);

    void InitializeClientsForRepositories(IFileSystem fileSystem, IEnumerable<Uri> repoUris, CmfAuthFile authFile);
    
    void InitializeClientsForRepositories(IFileSystem fileSystem, CmfAuthFile authFile);

    Task<CmfPackageV1> FindPackage(string packageId, string packageVersion);
}
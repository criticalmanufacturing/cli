using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Core.Interfaces;

public interface IRepositoryLocator
{
    IRepositoryClient GetRepositoryClient(Uri uri, IFileSystem fileSystem);

    IRepositoryClient GetSourceClient(IFileSystem fileSystem);

    void InitializeClientsForRepositories(IFileSystem fileSystem, IEnumerable<Uri> repoUris);
    
    void InitializeClientsForRepositories(IFileSystem fileSystem);

    Task<CmfPackageV1?> FindPackage(string packageId, string packageVersion);
}
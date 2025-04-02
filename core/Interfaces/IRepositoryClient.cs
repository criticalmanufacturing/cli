using System.IO.Abstractions;
using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Core.Interfaces;

public interface IRepositoryClient
{
    Task<CmfPackageV1> Find(string packageId, string version);
    Task<CmfPackageV1Collection> List();
    Task Put(CmfPackageV1 package);
    Task<IFileInfo> Get(CmfPackageV1 package, IDirectoryInfo targetDirectory);
    Task<IDirectoryInfo> Extract(CmfPackageV1 package, IDirectoryInfo targetDirectory);
    
    string RepositoryRoot { get; }
    
    bool Unreacheable { get; }
}
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Interfaces
{
    public interface IRepositoryAuthStore
    {
        IRepositoryCredentials GetRepositoryType(RepositoryCredentialsType repositoryType);

        IRepositoryCredentials GetRepositoryType<T>() where T : IRepositoryCredentials;

        ICredential GetEnvironmentCredentialsFor(RepositoryCredentialsType repositoryType, string repository);

        ICredential GetEnvironmentCredentialsFor<T>(string repository) where T : IRepositoryCredentials;

        ICredential GetCredentialsFor(RepositoryCredentialsType repositoryType, CmfAuthFile authFile, string absoluteUri, bool ignoreEnvVars = false);

        ICredential GetCredentialsFor<T>(CmfAuthFile authFile, string absoluteUri, bool ignoreEnvVars = false) where T : IRepositoryCredentials;

        Task<CmfAuthFile> Load();

        Task Save(IList<ICredential> credentials, bool sync = true);
    }
}

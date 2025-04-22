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

        /// <summary>
        /// Loads the auth file once, and caches it in memory, for any future calls of this same method.
        /// Not thread safe, if called concurrently, might load the file more than once.
        /// </summary>
        /// <returns></returns>
        Task<CmfAuthFile> GetOrLoad();

        Task Save(IList<ICredential> credentials, bool sync = true);
    }
}

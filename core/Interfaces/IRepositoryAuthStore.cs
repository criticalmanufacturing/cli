using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Interfaces
{
    public interface IRepositoryAuthStore
    {
        IRepositoryCredentials GetRepositoryType(RepositoryCredentialsType repositoryType);

        T GetRepositoryType<T>() where T : IRepositoryCredentials;

        ICredential? GetEnvironmentCredentialsFor(RepositoryCredentialsType repositoryType, string repository);

        ICredential? GetEnvironmentCredentialsFor<T>(string repository) where T : IRepositoryCredentials;

        ICredential? GetCredentialsFor(RepositoryCredentialsType repositoryType, CmfAuthFile authFile, string absoluteUri, bool ignoreEnvVars = false);

        ICredential? GetCredentialsFor<T>(CmfAuthFile authFile, string absoluteUri, bool ignoreEnvVars = false) where T : IRepositoryCredentials;

        /// <summary>
        /// Adds the derived credentials to the CmfAuthFile object
        /// </summary>
        /// <param name="authFile"></param>
        void AddDerivedCredentials(CmfAuthFile authFile);

        Task<CmfAuthFile> Load();

        /// <summary>
        /// Loads the auth file once, and caches it in memory, for any future calls of this same method.
        /// Not thread safe, if called concurrently, might load the file more than once.
        /// </summary>
        /// <returns></returns>
        Task<CmfAuthFile> GetOrLoad();

        /// <summary>
        /// Resets the auth file cached in memory, forcing the next call to <see cref="GetOrLoad"/>
        /// to re-load the credentials from disk.
        /// </summary>
        void Unload();

        Task<CmfAuthFile> Save(IList<ICredential> credentials, bool sync = true);
    }
}

using Cmf.CLI.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public interface IRepositoryCredentials
    {
        AuthType[] SupportedAuthTypes { get; }

        RepositoryCredentialsType RepositoryType { get; }

        PropertyRequirement KeyPropertyRequirement { get; }

        PropertyRequirement DomainPropertyRequirement { get; }

        /// <summary>
        /// Perform validations on the values of the credentials for this specific repository type.
        /// Fatal errors are thrown as an exception, but non-fatal issues might be logged as warnings.
        /// </summary>
        /// <param name="credentials"></param>
        void ValidateCredentials(IList<ICredential> credentials);

        /// <summary>
        /// Syncs (create/update only) into this repository's credentials file a list of credentials (coming from the CmfAuthFile)
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        Task SyncCredentials(IList<ICredential> credentials);

        /// <summary>
        /// Returns for a given Repository URL, what the env var prefix should be to override the credentials used for that repository
        /// </summary>
        /// <param name="repository">The Repository URL which will be used as the basis to load the credentials from the env vars.</param>
        /// <returns></returns>
        string GetEnvironmentVariablePrefix(string repository);
    }
}

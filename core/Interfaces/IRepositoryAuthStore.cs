using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Interfaces
{
    public interface IRepositoryAuthStore
    {
        IRepositoryCredentials GetRepositoryType(RepositoryCredentialsType repositoryType);

        Task<CmfAuthFile> Load();

        Task Save(IList<ICredential> credentials, bool sync = true);
    }
}

using Cmf.CLI.Core.Objects;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public interface IPortalRepositoryCredentials : IRepositoryCredentials
    {
        public Task<ICredential?> TryRenewToken(CmfAuthFile authFile);
    }
}

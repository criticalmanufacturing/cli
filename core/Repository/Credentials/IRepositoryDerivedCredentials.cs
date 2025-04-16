using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    /// <summary>
    /// Interface to be implemented by <see cref="IRepositoryCredentials"/> implementation classes
    /// that, when logged in, not only provide access for themselves, but with the same credentials,
    /// also provide access to additional repositories.
    /// 
    /// This was implemented primarily to support the use case internally where logging into the 
    /// Critical Manufacturing's Portal provides you with a token that is also valid for 
    /// Critical Manufacturing's NPM, NuGet and Docker registries.
    /// 
    /// In that scenario, the <see cref="PortalRepositoryCredentials" /> class also implements 
    /// <see cref="IRepositoryCredentialsSingleSignOn"/> 
    /// </summary>
    public interface IRepositoryCredentialsSingleSignOn
    {
        IEnumerable<ICredential> GetDerivedCredentials(IList<ICredential> originalCredentials);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public interface IRepositoryDerivedCredentials
    {
        IEnumerable<ICredential> GetDerivedCredentials(IList<ICredential> originalCredentials);
    }
}

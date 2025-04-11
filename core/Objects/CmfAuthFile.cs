using Cmf.CLI.Core.Repository.Credentials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Objects
{
    public class CmfAuthFile
    {
        public IDictionary<string, CmfAuthFileRepositoryType> Repositories { get; set; } = new Dictionary<string, CmfAuthFileRepositoryType>();
    }

    public class CmfAuthFileRepositoryType
    {
        public IList<ICredential> Credentials { get; set; } = new List<ICredential>();
    }
}

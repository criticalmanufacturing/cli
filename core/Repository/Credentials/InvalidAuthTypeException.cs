using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    internal class InvalidAuthTypeException : Exception
    {
        public InvalidAuthTypeException(ICredential cred)
            : this(cred.AuthType, cred.RepositoryType, cred.Repository) 
        { }

        public InvalidAuthTypeException(AuthType authType, string repositoryType, string repository, Exception innerException = null) 
            : base($"Unsupported Credential type {authType} for {repositoryType} repository \"{repository}\"", innerException) { }
    }
}

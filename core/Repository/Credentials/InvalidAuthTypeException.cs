using Cmf.CLI.Utilities;
using System;

namespace Cmf.CLI.Core.Repository.Credentials
{
    internal class InvalidAuthTypeException : CliException
    {
        public InvalidAuthTypeException(ICredential cred)
            : this(cred.AuthType, cred.RepositoryType, cred.Repository) 
        { }

        public InvalidAuthTypeException(AuthType authType, RepositoryCredentialsType repositoryType, string repository, Exception? innerException = null) 
            : base($"Unsupported Credential type {authType} for {repositoryType} repository \"{repository}\"", innerException!) { }
    }
}

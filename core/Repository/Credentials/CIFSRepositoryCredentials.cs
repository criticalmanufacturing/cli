using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public class CIFSRepositoryCredentials : IRepositoryCredentials
    {
        public CIFSRepositoryCredentials()
        { }

        public AuthType[] SupportedAuthTypes => [AuthType.Basic];

        public RepositoryCredentialsType RepositoryType => RepositoryCredentialsType.CIFS;

        public PropertyRequirement KeyPropertyRequirement => PropertyRequirement.Ignored;

        public PropertyRequirement DomainPropertyRequirement => PropertyRequirement.Mandatory;

        public void ValidateCredentials(IList<ICredential> credentials)
        { }

        public Task SyncCredentials(IList<ICredential> credentials)
        {
            return Task.CompletedTask;
        }

        public string GetEnvironmentVariablePrefix(string repository)
        {
            var uri = new Uri(repository);
            var share = uri.AbsolutePath.TrimStart('/').Split('/').FirstOrDefault();

            if (string.IsNullOrEmpty(share))
            {
                return GenericUtilities.BuildEnvVarPrefix(RepositoryType, $"{uri.Host}");
            }
            else
            {
                return GenericUtilities.BuildEnvVarPrefix(RepositoryType, $"{uri.Host}/{share}");
            }
        }
    }
}

using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public class DockerRepositoryCredentials : IRepositoryCredentials
    {
        public DockerRepositoryCredentials()
        { }

        public AuthType[] SupportedAuthTypes => [AuthType.Basic];

        public RepositoryCredentialsType RepositoryType => RepositoryCredentialsType.Docker;

        public PropertyRequirement KeyPropertyRequirement => PropertyRequirement.Ignored;

        public PropertyRequirement DomainPropertyRequirement => PropertyRequirement.Ignored;

        public void ValidateCredentials(IList<ICredential> credentials)
        { }

        public Task SyncCredentials(IList<ICredential> credentials)
        {
            foreach (var cred in credentials)
            {
                if (cred is BasicCredential basicCred)
                {
                    var process = Process.Start(new ProcessStartInfo("docker", ["login", basicCred.Repository, "-u", basicCred.Username, "--password-stdin"])
                    {
                        RedirectStandardInput = true,
                    });
                    process.StandardInput.WriteLine(basicCred.Password);
                    process.StandardInput.Close();

                    if (process != null)
                    {
                        process.WaitForExit();
                    }
                }
                else
                {
                    throw new InvalidAuthTypeException(cred);
                }
            }

            return Task.CompletedTask;
        }
    }
}

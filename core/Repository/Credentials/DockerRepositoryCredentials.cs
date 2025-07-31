using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                    bool dockerWasFound = false;

                    try
                    {
                        var process = Process.Start(new ProcessStartInfo("docker", ["login", basicCred.Repository, "-u", basicCred.Username, "--password-stdin"])
                        {
                            RedirectStandardInput = true,
                        });

                        if (process != null)
                        {
                            dockerWasFound = true;

                            process.StandardInput.WriteLine(basicCred.Password);
                            process.StandardInput.Close();
                            process.WaitForExit();
                        }
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode != 2 /* ERROR_FILE_NOT_FOUND */)
                        {
                            throw;
                        }
                    }

                    if (!dockerWasFound)
                    {
                        Log.Warning("Docker was not found installed on the system, so login credentials synchronization with docker was skipped.");
                    }
                }
                else
                {
                    throw new InvalidAuthTypeException(cred);
                }
            }

            return Task.CompletedTask;
        }

        public string GetEnvironmentVariablePrefix(string repository)
        {
            var uri = new Uri(repository);
            return GenericUtilities.BuildEnvVarPrefix(RepositoryType, $"{uri.Host}");
        }
    }
}

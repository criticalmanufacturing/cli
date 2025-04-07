using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using PeanutButter.INI;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public class NPMRepositoryCredentials : IRepositoryCredentials
    {
        protected IFileSystem _fileSystem;

        public NPMRepositoryCredentials(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public AuthType[] SupportedAuthTypes => [AuthType.Basic, AuthType.Bearer];

        public RepositoryCredentialsType RepositoryType => RepositoryCredentialsType.NPM;

        public PropertyRequirement KeyPropertyRequirement => PropertyRequirement.Ignored;

        public PropertyRequirement DomainPropertyRequirement => PropertyRequirement.Ignored;

        public void ValidateCredentials(IList<ICredential> credentials)
        {
            foreach (var credential in credentials)
            {
                if (credential is not BasicCredential && credential is not BearerCredential)
                {
                    throw new InvalidAuthTypeException(credential);
                }
            }
        }

        public async Task SyncCredentials(IList<ICredential> credentials)
        {
            IFileInfo npmrcFile = null;

            try
            {
                npmrcFile = GetConfigFile();

                var config = await LoadConfig(npmrcFile);

                foreach (var cred in credentials)
                {
                    // The format of the repo when used as a key in .npmrc does not contain protocol nor ports,
                    // so we need to make sure we strip them
                    var repoUriBuilder = new UriBuilder(cred.Repository);
                    repoUriBuilder.Scheme = null;
                    repoUriBuilder.Port = -1;
                    var repoUri = repoUriBuilder.ToString();

                    if (cred is BasicCredential basicCred)
                    {
                        var authBytes = Encoding.UTF8.GetBytes($"{basicCred.Username}:{basicCred.Password}");
                        var auth = Convert.ToBase64String(authBytes);

                        config.SetValue("", $"//{repoUri}:_auth", auth);
                        config.SetValue("", $"//{repoUri}:always_auth", "true");

                        // Make sure we unset any potential conflicting properties
                        config.RemoveValue("", $"//{repoUri}:_authToken");
                        config.RemoveValue("", $"//{repoUri}:username");
                        config.RemoveValue("", $"//{repoUri}:_password");
                    }
                    else if (cred is BearerCredential bearerCredential)
                    {
                        config.SetValue("", $"//{repoUri}:_authToken", bearerCredential.Token);
                        config.SetValue("", $"//{repoUri}:always_auth", "true");

                        // Make sure we unset any potential conflicting properties
                        config.RemoveValue("", $"//{repoUri}:_auth");
                        config.RemoveValue("", $"//{repoUri}:username");
                        config.RemoveValue("", $"//{repoUri}:_password");
                    }
                    else
                    {
                        throw new InvalidAuthTypeException(cred);
                    }
                }

                SaveConfig(npmrcFile, config);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to sync credentials into NPM config file: {npmrcFile?.FullName}", ex);
            }
        }

        #region Protected Methods

        protected IFileInfo GetConfigFile()
        {
            // NPM stores the per-user .npmrc file on the HOME directory
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return _fileSystem.FileInfo.New(_fileSystem.Path.Join(home, ".npmrc"));
        }

        protected async Task<INIFile> LoadConfig(IFileInfo npmrcFile)
        {
            var ini = new INIFile();

            npmrcFile = GetConfigFile();

            if (!npmrcFile.Exists)
            {
                return ini;
            }

            var contents = await _fileSystem.File.ReadAllTextAsync(npmrcFile.FullName);

            ini.Parse(contents);

            return ini;
        }

        protected void SaveConfig(IFileInfo npmrcFile, INIFile ini)
        {
            if (npmrcFile.DirectoryName != null)
            {
                _fileSystem.Directory.CreateDirectory(npmrcFile.DirectoryName);
            }

            // Write the INI back to the .npmrc file
            using (var stream = _fileSystem.File.OpenWrite(npmrcFile.FullName))
            {
                ini.Persist(stream, Encoding.UTF8);
                stream.SetLength(stream.Position);
            }
        }

        #endregion Protected Methods
    }
}

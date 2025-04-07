using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Services
{
    public class RepositoryAuthStore : IRepositoryAuthStore
    {
        protected IFileInfo _authFile;

        public RepositoryAuthStore(IFileInfo authFile)
        {
            _authFile = authFile;
        }

        #region Protected Methods

        protected void EnsureFolderExists()
        {
            var folder = _authFile.Directory;

            if (folder == null)
            {
                throw new Exception($"Could not create CMF Authfile folder to store file \"{_authFile}\"");
            }

            if (!folder.Exists)
            {
                folder.Create();
            }
        }

        protected JsonSerializerSettings CreateJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Converters = [new CredentialConverter()],
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                }
            };
        }

        protected async Task<CmfAuthFile> ReadFile()
        {
            Log.Debug($"Reading auth file {_authFile.FullName}...");
            var contents = await _authFile.FileSystem.File.ReadAllTextAsync(_authFile.FullName);

            if (string.IsNullOrWhiteSpace(contents))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<CmfAuthFile>(contents, CreateJsonSerializerSettings());
        }

        protected async Task WriteFile(CmfAuthFile authFile)
        {
            Log.Debug($"Serializing auth file into {_authFile.FullName}...");

            // Write the updated object back to the file
            var content = JsonConvert.SerializeObject(authFile, Formatting.Indented, CreateJsonSerializerSettings());

            await _authFile.FileSystem.File.WriteAllTextAsync(_authFile.FullName, content);
        }

        protected IDictionary<IRepositoryCredentials, IList<ICredential>> GroupWithRepository(IList<ICredential> credentials)
        {
            var allRepositoryCredentials = ExecutionContext.ServiceProvider.GetServices<IRepositoryCredentials>()
                .ToDictionary(repo => repo.RepositoryType);

            return credentials
                .GroupBy(cred => allRepositoryCredentials.GetValueOrDefault(cred.RepositoryType))
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList() as IList<ICredential>
                );
        }

        #endregion Protected Methods

        #region Public Methods

        public IRepositoryCredentials GetRepositoryType(RepositoryCredentialsType repositoryType)
        {
            var allRepositoryCredentials = ExecutionContext.ServiceProvider.GetServices<IRepositoryCredentials>();

            var repositoryCredentials = allRepositoryCredentials
                .FirstOrDefault(repo => repo.RepositoryType == repositoryType);

            if (repositoryCredentials == null)
            {
                var expectedRepositoryTypes = string.Join(", ", allRepositoryCredentials.Select(repo => repo.RepositoryType));

                throw new CliException($"Invalid repository type \"{repositoryType}\", expected one of: {expectedRepositoryTypes}.", ErrorCode.InvalidArgument);
            }

            return repositoryCredentials;
        }

        public async Task<CmfAuthFile> Load()
        {
            try
            {
                if (!_authFile.Exists)
                {
                    Log.Debug("Auth file does not exist, loading default empty authfile...");
                    return new CmfAuthFile();
                }

                var authFile = await ReadFile();

                if (authFile == null)
                {
                    authFile = new CmfAuthFile();
                }

                if (authFile.Repositories == null)
                {
                    authFile.Repositories = new Dictionary<RepositoryCredentialsType, CmfAuthFileRepositoryType>();
                }

                // Some properties are not serialized, because they are redundant when in the context of the file structure where they are located in
                // We need to set them when loading from disk
                foreach (var (repoType, repoData) in authFile.Repositories)
                {
                    if (repoData == null)
                    {
                        authFile.Repositories[repoType] = new CmfAuthFileRepositoryType();
                        continue;
                    }

                    if (repoData.Credentials == null)
                    {
                        repoData.Credentials = [];
                        continue;
                    }

                    foreach (var cred in repoData.Credentials)
                    {
                        cred.RepositoryType = repoType;
                    }
                }

                return authFile;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load credentials from CMF Auth File {_authFile.FullName}", ex);
            }
        }

        public async Task Save(IList<ICredential> credentials, bool sync = true)
        {
            if (!credentials.Any())
            {
                return;
            }

            var credentialsByRepo = GroupWithRepository(credentials);

            // Before anythin is saved, make sure all credentials are valid
            foreach (var (repoType, repoCredentials) in credentialsByRepo)
            {
                repoType.ValidateCredentials(repoCredentials);
            }

            // Save them in the .cmf-auth.json file
            await SaveInternal(credentials);

            // Sync the credentials with their respective tools (NPM, Docker, Nuget, etc...)
            if (sync)
            {
                foreach (var (repoType, repoCredentials) in credentialsByRepo)
                {
                    await repoType.SyncCredentials(repoCredentials);
                }
            }

            var derivedCredentials = new List<ICredential>();

            foreach (var (repoType, repoCredentials) in credentialsByRepo)
            {
                if (repoType is IRepositoryCredentialsSingleSignOn singleSignOnRepository)
                {
                    derivedCredentials.AddRange(singleSignOnRepository.GetDerivedCredentials(repoCredentials));
                }
            }

            Log.Debug($"Found {derivedCredentials.Count} derived credentials.");

            // Call this function recursively to save any derived credentials we gound
            if (derivedCredentials.Any())
            {
                await Save(derivedCredentials, sync: sync);
            }
        }

        protected async Task SaveInternal(IList<ICredential> credentials, bool sync = true)
        {
            try
            {
                EnsureFolderExists();

                CmfAuthFile authFile;

                // If the auth file already exists, merge the credentials, otherwise, create a file just with these
                if (_authFile.Exists)
                {
                    authFile = await Load();
                    // We need to merge the credentials for each repository type
                    //   - update records for repositories already on the file
                    //   - create records for new repositories
                    //   - this method does not handle removing credential data, that is handled by a different method for that explicit purpose
                    foreach (var cred in credentials)
                    {
                        if (authFile.Repositories.TryGetValue(cred.RepositoryType, out var repoData))
                        {
                            var repoCreds = repoData.Credentials;

                            bool found = false;

                            for (int i = 0; i < repoCreds.Count; i++)
                            {
                                // If we found a match, replace it in-place
                                if (repoCreds[i].Repository == cred.Repository)
                                {
                                    repoCreds[i] = cred;
                                    found = true;
                                    break;
                                }
                            }

                            // If this repository is not alread on the list, add a new one
                            if (!found)
                            {
                                repoCreds.Add(cred);
                            }
                        } 
                        else
                        {
                            authFile.Repositories[cred.RepositoryType] = new CmfAuthFileRepositoryType { Credentials = [cred] };
                        }
                    }
                }
                else
                {
                    authFile = new CmfAuthFile();
                    authFile.Repositories = credentials
                        .GroupBy(cred => cred.RepositoryType)
                        .ToDictionary(group => group.Key, group => new CmfAuthFileRepositoryType { Credentials = group.ToList() });
                }

                await WriteFile(authFile);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to store credentials into CMF Auth File {_authFile.FullName}", ex);
            }
        }

        #endregion Public Methods

        #region Public Static Methods

        public static RepositoryAuthStore FromEnvironmentConfig(IFileSystem fileSystem)
        {
            var authFile = Environment.GetEnvironmentVariable("cmf_cli_authfile");

            // If no custom file location is configured, use the default path
            if (string.IsNullOrEmpty(authFile))
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                authFile = fileSystem.Path.Join(userProfile, ".cmf-auth.json");
            }

            return new RepositoryAuthStore(fileSystem.FileInfo.New(authFile));
        }

        #endregion Public Static Methods
    }
}

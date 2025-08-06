using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Services
{
    public class RepositoryAuthStore : IRepositoryAuthStore
    {
        protected IFileInfo _authFile;

        protected CmfAuthFile _cachedAuthFile = null;

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
                    NamingStrategy = new CamelCaseNamingStrategy()
                    {
                        ProcessDictionaryKeys = true,
                    }
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

        protected ICredential GetEnvironmentCredentialsFor(IRepositoryCredentials repositoryType, string repository)
        {
            var envVarPrefix = repositoryType.GetEnvironmentVariablePrefix(repository);

            Log.Debug($"Checking if environment variable \"{envVarPrefix}__AUTH_TYPE\" is present, to override credentials for repository...");

            var type = GetEnvironmentVariable($"{envVarPrefix}__AUTH_TYPE", PropertyRequirement.Optional);

            ICredential credentials = type?.ToLowerInvariant() switch
            {
                "bearer" => new BearerCredential
                {
                    RepositoryType = repositoryType.RepositoryType,
                    Repository = repository,
                    Token = GetEnvironmentVariable($"{envVarPrefix}__TOKEN", PropertyRequirement.Mandatory),
                },
                "basic" => new BasicCredential
                {
                    RepositoryType = repositoryType.RepositoryType,
                    Repository = repository,
                    Domain = GetEnvironmentVariable($"{envVarPrefix}__DOMAIN", repositoryType.DomainPropertyRequirement),
                    Key = GetEnvironmentVariable($"{envVarPrefix}__KEY", repositoryType.KeyPropertyRequirement),
                    Username = GetEnvironmentVariable($"{envVarPrefix}__USERNAME", PropertyRequirement.Mandatory),
                    Password = GetEnvironmentVariable($"{envVarPrefix}__PASSWORD", PropertyRequirement.Mandatory),
                },
                "" or null => null,
                _ => throw new Exception($"Invalid authentication type \"{type}\" specified in environment variable \"{envVarPrefix}__AUTH_TYPE\"")
            };

            if (credentials != null && Array.IndexOf(repositoryType.SupportedAuthTypes, credentials.AuthType) == -1)
            {
                var supportedAuthTypeNames = string.Join(", ", repositoryType.SupportedAuthTypes);

                throw new Exception($"Invalid auth type \"{credentials.AuthType}\" for repository type \"{repositoryType.RepositoryType}\", supported values are: {supportedAuthTypeNames}.");
            }

            return credentials;
        }

        protected string GetEnvironmentVariable(string envVarName, PropertyRequirement requirement)
        {
            var value = Environment.GetEnvironmentVariable(envVarName);

            GenericUtilities.ValidatePropertyRequirement($"Environment Variable \"{envVarName}\"", value, requirement);

            return value;
        }

        protected ICredential GetCredentialsFor(IRepositoryCredentials repositoryType, CmfAuthFile authFile, string repository, bool ignoreEnvVars = false)
        {
            Log.Debug($"Get credentials for \"{repositoryType?.RepositoryType}\" \"{repository}\"...");

            ICredential credentials;

            if (!ignoreEnvVars)
            {
                credentials = GetEnvironmentCredentialsFor(repositoryType, repository);

                // if there are custom authentication variables, those take precedence over the regularly configured credentials
                if (credentials != null)
                {
                    Log.Debug($"  > found environment variables overriding credentials, using those...");
                    return credentials;
                }
            }

            if (authFile == null)
            {
                Log.Debug($"  > no auth file provided, returning no credentials...");
                return null;
            }

            if (!authFile.Repositories.TryGetValue(repositoryType.RepositoryType, out var repoData))
            {
                Log.Debug($"  > auth file does not contain credentials for type \"{repositoryType.RepositoryType}\", returning no credentials...");
                return null;
            }

            // TODO Support matching strategies per repository type
            credentials = repoData.Credentials
                .Where(cred => repository.StartsWith(cred.Repository))
                // We can defined a credential for repsoitory /host, and later one more specific, for repository /host/folder
                // This allows us to get always the most specific one (identified here by the length of the repository on the credential)
                .MaxBy(cred => cred.Repository.Length);

            Log.Debug($"  > {(credentials != null ? "found" : "did not find")} credentials for repository in auth file...");

            return credentials;
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

        public T GetRepositoryType<T>()
            where T : IRepositoryCredentials
        {
            var allRepositoryCredentials = ExecutionContext.ServiceProvider.GetServices<IRepositoryCredentials>();

            var repositoryCredentials = allRepositoryCredentials
                .OfType<T>()
                .FirstOrDefault();

            if (repositoryCredentials == null)
            {
                var expectedRepositoryTypes = string.Join(", ", allRepositoryCredentials.Select(repo => repo.RepositoryType));

                throw new CliException($"Invalid repository type \"{typeof(T).FullName}\" is not a registered Repository Credentials type.", ErrorCode.InvalidArgument);
            }

            return repositoryCredentials;
        }

        public ICredential GetEnvironmentCredentialsFor(RepositoryCredentialsType repositoryType, string repository)
        {
            return GetEnvironmentCredentialsFor(GetRepositoryType(repositoryType), repository);
        }

        public ICredential GetEnvironmentCredentialsFor<T>(string repository)
            where T : IRepositoryCredentials
        {
            return GetEnvironmentCredentialsFor(GetRepositoryType<T>(), repository);
        }

        public ICredential GetCredentialsFor(RepositoryCredentialsType repositoryType, CmfAuthFile authFile, string repository, bool ignoreEnvVars = false)
        {
            return GetCredentialsFor(GetRepositoryType(repositoryType), authFile, repository, ignoreEnvVars);
        }

        public ICredential GetCredentialsFor<T>(CmfAuthFile authFile, string repository, bool ignoreEnvVars = false)
            where T : IRepositoryCredentials
        {
            return GetCredentialsFor(GetRepositoryType<T>(), authFile, repository, ignoreEnvVars);
        }

        public void AddDerivedCredentials(CmfAuthFile authFile)
        {
            var derivedCredentials = new List<ICredential>();
            foreach (var kv in authFile.Repositories)
            {
                var repoType = GetRepositoryType(kv.Key);
                var repoCredentials = kv.Value.Credentials;

                if (repoType is IRepositoryCredentialsSingleSignOn singleSignOnRepository)
                {
                    derivedCredentials.AddRange(singleSignOnRepository.GetDerivedCredentials(repoCredentials));

                    Log.Debug($"Found {derivedCredentials.Count} derived credentials for {repoType}.");
                }
            }

            foreach (var derivedCred in derivedCredentials)
            {
                // Make sure the source is correctly set
                derivedCred.Source = CredentialSource.Derived;

                if (!authFile.Repositories.TryGetValue(derivedCred.RepositoryType, out var repoTypeCredentials))
                {
                    authFile.Repositories[derivedCred.RepositoryType] = repoTypeCredentials = new CmfAuthFileRepositoryType();
                }

                repoTypeCredentials.Credentials.Add(derivedCred);
            }
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

        public async Task<CmfAuthFile> GetOrLoad()
        {
            if (_cachedAuthFile == null)
            {
                _cachedAuthFile = await Load();
                AddDerivedCredentials(_cachedAuthFile);
            }

            return _cachedAuthFile;
        }

        public async Task<CmfAuthFile> Save(IList<ICredential> credentials, bool sync = true)
        {
            if (!credentials.Any())
            {
                return await Load();
            }

            var credentialsByRepo = GroupWithRepository(credentials);

            // Before anything is saved, make sure all credentials are valid
            foreach (var (repoType, repoCredentials) in credentialsByRepo)
            {
                repoType.ValidateCredentials(repoCredentials);
            }

            // Save them in the .cmf-auth.json file
            var savedAuthFile = await SaveInternal(credentials);

            // Sync the credentials with their respective tools (NPM, Docker, Nuget, etc...)
            if (sync)
            {
                // Create a "virtual" auth file with only the credentials to sync
                // (including derived credentials)
                var authFile = new CmfAuthFile();
                authFile.Repositories = credentialsByRepo
                    .ToDictionary(group => group.Key.RepositoryType, group => new CmfAuthFileRepositoryType { Credentials = group.Value });
                AddDerivedCredentials(authFile);
                
                foreach (var (repoType, repoCredentials) in authFile.Repositories)
                {
                    await GetRepositoryType(repoType).SyncCredentials(repoCredentials.Credentials);
                }
            }

            return savedAuthFile;
        }

        protected async Task<CmfAuthFile> SaveInternal(IList<ICredential> credentials, bool sync = true)
        {
            CmfAuthFile authFile;

            try
            {
                EnsureFolderExists();

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
                        if (cred.Source != CredentialSource.Manual)
                        {
                            Log.Debug($"Credential {cred.RepositoryType} {cred.Repository} will not be saved because its source is {cred.Source} and not {CredentialSource.Manual}");
                            continue;
                        }

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

            return authFile;
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

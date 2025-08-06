using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public class PortalRepositoryCredentials : IPortalRepositoryCredentials, IRepositoryAutomaticLogin, IRepositoryCredentialsSingleSignOn
    {
        #region Constants

        public const string TokenEnvVar = "CM_PORTAL_TOKEN";

        public static readonly List<string> CmfDomainList = new List<string>
        {
            CmfAuthConstants.DockerRepository,
            CmfAuthConstants.CollaborationHubRepository,
            CmfAuthConstants.MESRepository
        };

        public static readonly string TokenDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "cmfportal");
        public static readonly string TokenFilePath = Path.Combine(TokenDir, "cmfportaltoken");
        // How much time to consider, before the token actually expires, that we should already try to renovate it nonetheless
        public static readonly TimeSpan TokenValidityThreshold = TimeSpan.FromDays(5);

        #endregion Constants

        protected IFileSystem _fileSystem;
        protected IPortalLoginCommand _loginCommand;

        public PortalRepositoryCredentials(IFileSystem fileSystem, IPortalLoginCommand loginCommand)
        {
            _fileSystem = fileSystem;
            _loginCommand = loginCommand ?? new PortalLoginCommand();
        }

        public PortalRepositoryCredentials(IFileSystem fileSystem)
            : this(fileSystem, new PortalLoginCommand())
        { }

        public AuthType[] SupportedAuthTypes => [AuthType.Bearer];

        public RepositoryCredentialsType RepositoryType => RepositoryCredentialsType.Portal;

        public PropertyRequirement KeyPropertyRequirement => PropertyRequirement.Ignored;

        public PropertyRequirement DomainPropertyRequirement => PropertyRequirement.Ignored;

        public void ValidateCredentials(IList<ICredential> credentials)
        { }

        public async Task SyncCredentials(IList<ICredential> credentials)
        {
            if (credentials.Count > 1)
            {
                Log.Warning("Syncing multiple CM Portal credentials is not currently supported. Falling back and syncing only the first credential.");
            }

            var credential = credentials.FirstOrDefault();

            if (credential == null)
            {
                Log.Debug("No CM Portal credentials found to sync.");
                return;
            }

            if (!(credential is BearerCredential bearerCredential))
            {
                Log.Warning($"CM Portal credential type is {credential.AuthType}, only {AuthType.Bearer} credentials supported.");
                return;
            }

            _fileSystem.Directory.CreateDirectory(TokenDir);
            await _fileSystem.File.WriteAllTextAsync(TokenFilePath, bearerCredential.Token);
        }

        public async Task<ICredential> AutomaticLogin()
        {
            _loginCommand.Execute();

            // The command "cmf portal login" saves the token in this file
            var tokenFile = _fileSystem.FileInfo.New(TokenFilePath);

            if (!tokenFile.Exists)
            {
                throw new Exception($"Failed to login in CMF Customer Portal with \"cmf portal login\", no auth token was retrieved.");
            }

            var token = await _fileSystem.File.ReadAllTextAsync(tokenFile.FullName);

            return new BearerCredential
            {
                RepositoryType = RepositoryType,
                Repository = CmfAuthConstants.PortalRepository,
                Token = token,
            };
        }

        public IEnumerable<ICredential> GetDerivedCredentials(IList<ICredential> originalCredentials)
        {
            foreach (var cred in originalCredentials)
            {
                if (cred.Repository == CmfAuthConstants.PortalRepository)
                {
                    string[] cmfDomainListWithExtras = [..CmfDomainList, ..Environment.GetEnvironmentVariable("cmf_cli_derived_credentials_domain_list")?.Split(',') ?? []];
                    Uri[] repositoriesToCheck = [ExecutionContext.Instance?.RepositoriesConfig?.CIRepository, ..ExecutionContext.Instance?.RepositoriesConfig?.Repositories ?? []];
                    var derivedUrls = repositoriesToCheck.ToList().Where(url =>
                        cmfDomainListWithExtras.Contains(url?.Host)
                    );
                    
                    var token = ((BearerCredential)cred).Token;

                    var username = ParseJwt(token).Subject;

                    // NuGet
                    yield return new BasicCredential
                    {
                        RepositoryType = RepositoryCredentialsType.NuGet,
                        Repository = CmfAuthConstants.NuGetRepository,
                        Key = CmfAuthConstants.NuGetKey,
                        Username = username,
                        Password = token,
                    };
                    // NPM
                    yield return new BasicCredential
                    {
                        RepositoryType = RepositoryCredentialsType.NPM,
                        Repository = CmfAuthConstants.NPMRepository,
                        Username = username,
                        Password = token,
                    };
                    // Docker
                    yield return new BasicCredential
                    {
                        RepositoryType = RepositoryCredentialsType.Docker,
                        Repository = CmfAuthConstants.DockerRepository,
                        Username = username,
                        Password = token,
                    };

                    foreach (var url in derivedUrls)
                    {
                        yield return new BasicCredential
                        {
                            // Assume its an NPM repository
                            RepositoryType = RepositoryCredentialsType.NPM,
                            Repository = url.OriginalString,
                            Username = username,
                            Password = token,
                        };
                    }

                    break;
                }
            }
        }

        public string GetEnvironmentVariablePrefix(string repository)
        {
            var uri = new Uri(repository);
            return GenericUtilities.BuildEnvVarPrefix(RepositoryType, $"{uri.Host}{uri.PathAndQuery.TrimEnd('/')}");
        }

        public async Task<ICredential> TryRenewToken(CmfAuthFile authFile)
        {
            ICredential renewedCredential = null;

            // The token (might come from .cmf-auth.json or from cmfportaltoken files)
            string token = null;

            if (authFile.Repositories.TryGetValue(RepositoryType, out var portalCreds))
            {
                var credential = portalCreds.Credentials.FirstOrDefault();

                if (credential is BearerCredential bearerCredential)
                {
                    Log.Debug("Found a CM Portal token in the CM auth file");

                    token = bearerCredential.Token;
                }
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                if (_fileSystem.File.Exists(TokenFilePath))
                {
                    Log.Debug("Found a CM Portal token in the cmfportaltoken");

                    token = await _fileSystem.File.ReadAllTextAsync(TokenFilePath);

                    renewedCredential = new BearerCredential
                    {
                        RepositoryType = RepositoryType,
                        Repository = CmfAuthConstants.PortalRepository,
                        Token = token,
                    };
                }
            }

            if (string.IsNullOrWhiteSpace(token) || !IsTokenValid(token))
            {
                Log.Debug("Attempting to renew CM Portal token...");
                renewedCredential = await AutomaticLogin();
            }

            return renewedCredential;
        }

        protected bool IsTokenValid(string token)
        {
            bool isTokenValid = false;

            try
            {
                var payload = ParseJwt(token);

                Log.Debug($"Testing token for subject {payload.Subject} with expiration date {payload.ExpirationTime}");

                // If the token expiration date (minus the threshold) is still in the future
                isTokenValid = (payload.ExpirationTime - TokenValidityThreshold) > DateTime.Now;

                Log.Debug($" > Token is {(isTokenValid ? "valid" : "expired")}");
            }
            catch (Exception ex)
            {
                Log.Debug($"Parsing of payload failed with error: \"{ex.Message}\". Assuming token is invalid.");
            }

            return isTokenValid;
        }

        protected JWTPayload ParseJwt(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(token);
            }

            var parts = token.Split('.');

            if (parts.Length != 3)
            {
                throw new Exception($"Invalid format JWT token, expected 2 dots \".\", but found {parts.Length - 1} instead.");
            }

            // Add padding in case the string does not have the correct length (the padding equal signs are frequently ommitted in JWTs)
            var payloadBase64 = parts[1];
            while (payloadBase64.Length % 4 != 0) payloadBase64 += "=";

            var payloadBytes = Convert.FromBase64String(payloadBase64);
            var payloadString = Encoding.UTF8.GetString(payloadBytes);

            return JsonConvert.DeserializeObject<JWTPayload>(payloadString);
        }

        protected class JWTPayload
        {
            public string ClientId { get; set; }

            public string TenantName { get; set; }

            public string StrategyId { get; set; }

            [JsonProperty(PropertyName = "sub")]
            public string Subject { get; set; }

            public string Scope { get; set; }

            [JsonProperty(PropertyName = "iat")]
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTimeOffset IssuedAtTime { get; set; }

            [JsonProperty(PropertyName = "exp")]
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTimeOffset ExpirationTime { get; set; }

            [JsonProperty(PropertyName = "aud")]
            public string Audience { get; set; }

            [JsonProperty(PropertyName = "iss")]
            public string Issuer { get; set; }
        }
    }

    public interface IPortalLoginCommand
    {
        void Execute();
    }

    public class PortalLoginCommand : IPortalLoginCommand
    {
        public void Execute()
        {
            var process = Process.Start("cmf", ["portal", "login"]);

            if (process != null)
            {
                process.WaitForExit();
            }
        }
    }
}

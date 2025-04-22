using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public class PortalRepositoryCredentials : IRepositoryCredentials, IRepositoryAutomaticLogin, IRepositoryCredentialsSingleSignOn
    {
        #region Constants

        public const string TokenEnvVar = "CM_PORTAL_TOKEN";
        public static readonly string TokenDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "cmfportal");
        public static readonly string TokenFilePath = Path.Combine(TokenDir, "cmfportaltoken");

        #endregion Constants

        protected IFileSystem _fileSystem;

        public PortalRepositoryCredentials(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public AuthType[] SupportedAuthTypes => [AuthType.Bearer];

        public RepositoryCredentialsType RepositoryType => RepositoryCredentialsType.Portal;

        public PropertyRequirement KeyPropertyRequirement => PropertyRequirement.Ignored;

        public PropertyRequirement DomainPropertyRequirement => PropertyRequirement.Ignored;

        public void ValidateCredentials(IList<ICredential> credentials)
        { }

        public Task SyncCredentials(IList<ICredential> credentials)
        {
            return Task.CompletedTask;
        }

        public async Task<ICredential> AutomaticLogin()
        {
            var process = Process.Start("cmf", ["portal", "login"]);

            if (process != null)
            {
                process.WaitForExit();
            }

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
            var derived = new List<ICredential>();

            foreach (var cred in originalCredentials)
            {
                if (cred.Repository == CmfAuthConstants.PortalRepository)
                {
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
                }
            }
        }

        public string GetEnvironmentVariablePrefix(string repository)
        {
            var uri = new Uri(repository);
            return GenericUtilities.BuildEnvVarPrefix(RepositoryType, $"{uri.Host}{uri.PathAndQuery.TrimEnd('/')}");
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
            public DateTime IssuedAtTime { get; set; }

            [JsonProperty(PropertyName = "exp")]
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTime ExpirationTime { get; set; }

            [JsonProperty(PropertyName = "aud")]
            public string Audience { get; set; }

            [JsonProperty(PropertyName = "iss")]
            public string Issuer { get; set; }
        }
    }
}

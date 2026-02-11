using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public record class BasicCredential : ICredential
    {
        [JsonIgnore]
        public CredentialSource Source { get; set; } = CredentialSource.Manual;

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthType AuthType => AuthType.Basic;

        public RepositoryCredentialsType RepositoryType { get; set; }

        public string Repository { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Domain { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public BasicCredential() { }

        public BasicCredential(RepositoryCredentialsType repositoryType, string repository, string key, string domain, string username, string password)
        {
            RepositoryType = repositoryType;
            Repository = repository;
            Key = key;
            Domain = domain;
            Username = username;
            Password = password;
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public record class BearerCredential : ICredential
    {
        [JsonIgnore]
        public CredentialSource Source { get; set; } = CredentialSource.Manual;

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthType AuthType => AuthType.Bearer;

        public RepositoryCredentialsType RepositoryType { get; set; }

        public string Repository { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public BearerCredential() { }

        public BearerCredential(RepositoryCredentialsType repositoryType, string repository, string key, string token)
        {
            RepositoryType = repositoryType;
            Repository = repository;
            Token = token;
            Key = key;
        }
    }
}

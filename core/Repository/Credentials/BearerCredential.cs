using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public record class BearerCredential : ICredential
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AuthType AuthType => AuthType.Bearer;

        public RepositoryCredentialsType RepositoryType { get; set; }

        public string Repository { get; set; }

        public string Token { get; set; }

        public string Key { get; set; }

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

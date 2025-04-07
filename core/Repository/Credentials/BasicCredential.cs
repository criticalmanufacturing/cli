using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public record class BasicCredential : ICredential
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AuthType AuthType => AuthType.Basic;

        public RepositoryCredentialsType RepositoryType { get; set; }

        public string Repository { get; set; }

        public string Key { get; set; }

        public string Domain { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

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

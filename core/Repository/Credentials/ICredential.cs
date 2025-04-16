using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cmf.CLI.Core.Repository.Credentials
{
    [JsonObject]
    public interface ICredential
    {
        [JsonConverter(typeof(StringEnumConverter))]
        AuthType AuthType { get; }

        [JsonIgnore]
        public RepositoryCredentialsType RepositoryType { get; set; }

        public string Repository { get; set; }

        public string Key { get; set; }
    }
}

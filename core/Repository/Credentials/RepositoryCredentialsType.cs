using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public enum RepositoryCredentialsType
    {
        /// <summary>
        /// Critical Manufacturing's Customer Portal
        /// </summary>
        Portal,

        /// <summary>
        /// Node's NPM Package Registry
        /// </summary>
        NPM,

        /// <summary>
        /// Microsoft's NuGet Registry
        /// </summary>
        NuGet,

        /// <summary>
        /// Docker Container Images Registry
        /// </summary>
        Docker,

        /// <summary>
        /// CIFS Network file shares
        /// </summary>
        CIFS,
    }
}

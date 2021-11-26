using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cmf.Common.Cli.Objects
{
    /// <summary>
    /// The DF repositories used in this run
    /// </summary>
    [JsonObject]
    public class RepositoriesConfig
    {
        /// <summary>
        /// The CI repository folder: this is the place where Packages built by CI are stored, by branch
        /// </summary>
        public Uri CIRepository { get; set; }

        /// <summary>
        /// The DF repositories: these contain package that we treat as official (i.e. upstream dependencies or already releases packages)
        /// </summary>
        public List<Uri> Repositories { get; set; }

        /// <summary>
        /// Initialize new RepositoriesConfig.
        /// This constructor is only used as fallback, if a config is found in the filesystem, the file will be deserialized into this object
        /// </summary>
        public RepositoriesConfig()
        {
            Repositories = new List<Uri>();
        }
    }
}
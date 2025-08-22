using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Constants
{
    public static class CmfAuthConstants
    {
        #region Generic

        /// <summary>
        /// Repository URL for Critical Manufacturing's public Customer Portal
        /// </summary>
        public const string PortalRepository = "https://portal.criticalmanufacturing.com";

        /// <summary>
        /// Repository URL for Critical Manufacturing's public NuGet registry
        /// </summary>
        public const string NuGetRepository = "https://criticalmanufacturing.io/repository/nuget/index.json";

        /// <summary>
        /// Repository URL for Critical Manufacturing's public NPM registry
        /// </summary>
        public const string NPMRepository = "https://criticalmanufacturing.io/repository/npm/";
    
        /// <summary>
        /// Repository URL for Critical Manufacturing's public Docker registry
        /// </summary>
        public const string DockerRepository = "criticalmanufacturing.io";

        /// <summary>
        /// Repository URL for Critical Manufacturing's Collaboration Hub registry
        /// </summary>
        public const string CollaborationHubRepository = "cm-collaborationhub.io";

        /// <summary>
        /// Repository URL for Critical Manufacturing's MES registry
        /// </summary>
        public const string MESRepository = "cm-mes.io";

        /// <summary>
        /// Standard name for the NuGet Source for Critical Manufacturing's packages
        /// </summary>
        public const string NuGetKey = "CMF";

        #endregion
    }
}

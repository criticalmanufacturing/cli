using System.Runtime.Serialization;

namespace Cmf.CLI.Core.Enums
{
    public enum MasterDataTargetPlatformType
    {
        /// <summary>
        /// The current system (itself).
        /// </summary>
        Self = 0,
        /// <summary>
        /// App framework system (base platform).
        /// </summary>
        [EnumMember(Value = "Framework")]
        AppFramework = 1
    }
}

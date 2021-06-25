using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.Common.Cli.Enums
{
    /// <summary>
    /// Possible source for a CmfPackage
    /// </summary>
    public enum PackageLocation
    {
        /// <summary>
        /// Local filesystem
        /// </summary>
        Local,
        /// <summary>
        /// a specified repository
        /// </summary>
        Repository
    }
}

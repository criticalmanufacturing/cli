using Cmf.Common.Cli.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Cmf.Common.Cli.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{Objects.CmfPackage}" />
    public class CmfPackageCollection : List<CmfPackage>
    {
        /// <summary>
        /// Gets the dependency.
        /// </summary>
        /// <param name="dependency">The dependency.</param>
        /// <returns></returns>
        public CmfPackage GetDependency(Dependency dependency)
        {
            return this.FirstOrDefault(x => x.PackageId.IgnoreCaseEquals(dependency.Id) && x.Version.IgnoreCaseEquals(dependency.Version));
        }
    }
}
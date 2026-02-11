using System.Collections.Generic;
using System.Linq;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="CmfPackage" />
    public class CmfPackageCollection : List<CmfPackage>
    {
        /// <summary>
        /// Gets the dependency.
        /// </summary>
        /// <param name="dependency">The dependency.</param>
        /// <returns></returns>
        public CmfPackage? GetDependency(Dependency dependency)
        {
            return this.FirstOrDefault(x => x.PackageId.IgnoreCaseEquals(dependency.Id) && x.Version.IgnoreCaseEquals(dependency.Version));
        }

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="cmfPackage"></param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified dependency]; otherwise, <c>false</c>.
        /// </returns>
        public new bool Contains(CmfPackage cmfPackage)
        {
            return this.HasAny(x => x.PackageId.IgnoreCaseEquals(cmfPackage.PackageId) && x.Version.IgnoreCaseEquals(cmfPackage.Version));
        }
    }
}
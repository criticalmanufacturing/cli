using Cmf.Common.Cli.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Cmf.Common.Cli.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{Cmf.Common.Cli.Objects.Dependency}" />
    public class DependencyCollection : List<Dependency>
    {
        /// <summary>
        /// Gets the dependency.
        /// </summary>
        /// <param name="dependency">The dependency.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified dependency]; otherwise, <c>false</c>.
        /// </returns>
        public new bool Contains(Dependency dependency)
        {
            return Contains(dependency, false);
        }

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="dependency">The dependency.</param>
        /// <param name="ignoreVersion">if set to <c>true</c> [ignore version].</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified dependency]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Dependency dependency, bool ignoreVersion)
        {
            return this.HasAny() && this.Any(x => x.Id.IgnoreCaseEquals(dependency.Id) && (ignoreVersion || x.Version.IgnoreCaseEquals(dependency.Version)));
        }
    }
}
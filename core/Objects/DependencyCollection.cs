using System.Collections.Generic;
using System.Linq;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Dependency" />
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
            return this.HasAny(x => x.Id.IgnoreCaseEquals(dependency.Id) && (ignoreVersion || x.Version.IgnoreCaseEquals(dependency.Version)));
        }

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="dependencyId">The dependency.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified dependency]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string dependencyId)
        {
            return this.HasAny(x => x.Id.IgnoreCaseEquals(dependencyId));
        }

        /// <summary>
        /// Gets the specified dependency.
        /// </summary>
        /// <param name="dependency">The dependency.</param>
        /// <returns></returns>
        public Dependency? Get(Dependency dependency)
        {
            return this.FirstOrDefault(x => x.Id.IgnoreCaseEquals(dependency.Id) && x.Version.IgnoreCaseEquals(dependency.Version));
        }
    }
}

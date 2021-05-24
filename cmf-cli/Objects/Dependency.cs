using Cmf.Common.Cli.Utilities;
using Newtonsoft.Json;
using System;

namespace Cmf.Common.Cli.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="IEquatable{Dependency}" />
    public class Dependency : IEquatable<Dependency>
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty(Order = 0)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [JsonProperty(Order = 1)]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Dependency" /> is mandatory.
        /// </summary>
        /// <value>
        ///   <c>true</c> if mandatory; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(Order = 2)]
        [JsonIgnore]
        public bool Mandatory { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Dependency" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="version">The version.</param>
        /// <exception cref="ArgumentNullException">id
        /// or
        /// version</exception>
        public Dependency(string id, string version)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dependency" /> class.
        /// </summary>
        public Dependency()
        {
            Mandatory = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(Dependency other)
        {
            return other != null &&
                   Id.IgnoreCaseEquals(other.Id) &&
                   Version.IgnoreCaseEquals(other.Version);
        }

        #endregion
    }
}
﻿using Cmf.CLI.Core.Utilities;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using System;
using System.IO.Abstractions;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    /// Represents the content to be packed into a DF Package
    /// </summary>
    /// <seealso cref="System.IEquatable{Cmf.CLI.Core.Objects.RelatedPackage}" />
    public class RelatedPackage : IEquatable<RelatedPackage>
    {
        #region Public Properties

        /// <summary>
        /// Relative path to the package folder
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        [JsonConverter(typeof(AbstractionsDirectoryConverter))]
        public IDirectoryInfo Path { get; set; }

        /// <summary>
        /// Should trigger build before the root triggered package
        /// </summary>
        /// <value>
        /// The prebuild.
        /// </value>
        public bool PreBuild { get; set; }

        /// <summary>
        /// Should trigger build before the root triggered package
        /// </summary>
        /// <value>
        /// The prebuild.
        /// </value>
        public bool PostBuild { get; set; }

        /// <summary>
        /// Should trigger pack before the root triggered package
        /// </summary>
        /// <value>
        /// The prepack.
        /// </value>
        public bool PrePack { get; set; }

        /// <summary>
        /// Should trigger pack after the root triggered package
        /// </summary>
        /// <value>
        /// The prepack.
        /// </value>
        public bool PostPack { get; set; }

        [JsonIgnore]
        public bool IsSet { get; set; }
        #endregion

        #region Public Methods

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(RelatedPackage other)
        {
            return other != null &&
                   Path.FullName.IgnoreCaseEquals(other.Path.FullName);
        }

        #endregion
    }
}
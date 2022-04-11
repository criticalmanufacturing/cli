﻿using System;
using System.Collections.Generic;
using Cmf.CLI.Core.Enums;
using Cmf.Common.Cli.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.IEquatable{Cmf.Common.Cli.Objects.ContentToPack}" />
    public class ContentToPack : IEquatable<ContentToPack>
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets the ignore file.
        /// </summary>
        /// <value>
        /// The ignore file.
        /// </value>
        public List<string> IgnoreFiles { get; set; }

        /// <summary>
        /// Gets or sets the action to be applied to the content
        /// Default is "pack"
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PackAction? Action { get; set; }

        /// <summary>
        /// Gets or sets the type of the content.
        /// Default value = Generic
        /// </summary>
        /// <value>
        /// The type of the content.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentType? ContentType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(ContentToPack other)
        {
            return other != null &&
                   Source.IgnoreCaseEquals(other.Source) &&
                   Target.IgnoreCaseEquals(other.Target);
        }

        #endregion
    }
}
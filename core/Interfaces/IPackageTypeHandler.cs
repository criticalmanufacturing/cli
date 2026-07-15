using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace Cmf.CLI.Core.Interfaces
{
    /// <summary>
    ///
    /// </summary>
    public interface IPackageTypeHandler
    {
        /// <summary>
        /// Gets or sets the default content to ignore.
        /// </summary>
        /// <value>
        /// The default content to ignore.
        /// </value>
        public List<string> DefaultContentToIgnore { get; }

        /// <summary>
        /// Bumps the specified version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="versionSuffix">The version suffix.</param>
        /// <param name="bumpInformation">The bump information.</param>
        public abstract void Bump(string version, string versionSuffix, Dictionary<string, object> bumpInformation = null);

        /// <summary>
        /// Bumps the Base version of the package
        /// </summary>
        /// <param name="version">The new Base version.</param>
        /// <param name="manifest">The manifest file to use for the upgrade.</param>
        public abstract void Upgrade(string version, string manifest = null);

        /// <summary>
        /// Builds this instance.
        /// </summary>
        public void Build(bool test = false);

        /// <summary>
        /// Packs the specified package output dir.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="dryRun">if set to <c>true</c> list the package structure without creating files.</param>
        public void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false);

        /// <summary>
        /// Restore package dependencies (declared in cmfpackage.json) from repository packages
        /// </summary>
        public void RestoreDependencies(Uri[] repositories);
    }
}
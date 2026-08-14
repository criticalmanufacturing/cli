using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using NuGet.Versioning;
using Spectre.Console;

namespace Cmf.CLI.Utilities
{
    /// <summary>
    ///
    /// </summary>
    public static class GenericUtilities
    {
        #region Public Methods

        /// <summary>
        /// Will create a new version based on the old and new inputs
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <param name="version"></param>
        /// <param name="versionSuffix"></param>
        /// <returns>
        /// the new version
        /// </returns>
        public static string RetrieveNewVersion(string currentVersion, string version, string versionSuffix)
        {
            if (!string.IsNullOrEmpty(version))
            {
                currentVersion = version;
            }
            if (!string.IsNullOrEmpty(versionSuffix))
            {
                currentVersion += "-" + versionSuffix;
            }

            return currentVersion;
        }

        /// <summary>
        /// Will create a new version based on the old and new inputs
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <param name="version"></param>
        /// <param name="versionSuffix"></param>
        /// <returns>
        /// the new version
        /// </returns>
        public static string RetrieveNewPresentationVersion(string currentVersion, string version, string versionSuffix)
        {
            GenericUtilities.GetCurrentPresentationVersion(currentVersion, out string originalVersion, out string originalVersionSuffix);

            string newVersion = !string.IsNullOrEmpty(version) ? version : originalVersion;
            if (!string.IsNullOrEmpty(versionSuffix))
            {
                newVersion += "-" + versionSuffix;
            }

            return newVersion;
        }

        /// <summary>
        /// Get current version based on string, for
        /// the format 1.0.0-1234
        /// where 1.0.0 will be the version
        /// and the 1234 will be the version suffix
        /// </summary>
        /// <param name="source">Source information to be parsed</param>
        /// <param name="version">Version Number</param>
        /// <param name="versionSuffix">Version Suffix</param>
        public static void GetCurrentPresentationVersion(string source, out string version, out string versionSuffix)
        {
            version = string.Empty;
            versionSuffix = string.Empty;

            if (!string.IsNullOrWhiteSpace(source))
            {
                string[] sourceInfo = source.Split('-');
                version = sourceInfo[0];
                if (sourceInfo.Length > 1)
                {
                    versionSuffix = sourceInfo[1];
                }
            }
        }

        /// <summary>
        /// Get Package from Repository
        /// </summary>
        /// <param name="outputDir">Target directory for the package</param>
        /// <param name="repoUri">Repository Uri</param>
        /// <param name="force"></param>
        /// <param name="packageId">Package Identifier</param>
        /// <param name="packageVersion">Package Version</param>
        /// <param name="fileSystem">the underlying file system</param>
        /// <returns></returns>
        public static bool GetPackageFromRepository(IDirectoryInfo outputDir, Uri repoUri, bool force, string packageId, string packageVersion, IFileSystem fileSystem)
        {
            bool packageFound = false;

            // TODO: Support for nexus repository

            if (repoUri != null)
            {
                // If other repository types are supported they will be added here.

                if (repoUri.IsDirectory())
                {
                    // Create expected file name for the package to get
                    string _packageFileName = $"{packageId}.{packageVersion}.zip";
                    IDirectoryInfo repoDirectory = fileSystem.DirectoryInfo.New(repoUri.OriginalString);

                    if (repoDirectory.Exists)
                    {
                        // Search by Packages already Packed
                        IFileInfo[] dependencyFiles = repoDirectory.GetFiles(_packageFileName);
                        packageFound = dependencyFiles.HasAny();

                        if (packageFound)
                        {
                            foreach (IFileInfo dependencyFile in dependencyFiles)
                            {
                                string destDependencyFile = $"{outputDir.FullName}/{dependencyFile.Name}";
                                if (force && fileSystem.File.Exists(destDependencyFile))
                                {
                                    fileSystem.File.Delete(destDependencyFile);
                                }

                                dependencyFile.CopyTo(destDependencyFile);
                            }
                        }
                    }
                }
                else
                {
                    throw new CliException(CoreMessages.UrlsNotSupported);
                }
            }

            return packageFound;
        }

        /// <summary>
        /// Flatten a tree
        /// </summary>
        /// <param name="items">The top level tree items</param>
        /// <param name="getChildren">a function that for each tree node returns its children</param>
        /// <typeparam name="T">The tree node type</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(
            this IEnumerable<T> items,
            Func<T, IEnumerable<T>> getChildren)
        {
            var stack = new Stack<T>();
            foreach (var item in items)
                stack.Push(item);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                var children = getChildren(current);
                if (children == null) continue;

                foreach (var child in children)
                    stack.Push(child);
            }
        }

        /// <summary>
        /// Converts a JsonObject to an Uri
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
#nullable enable

        public static Uri? JsonObjectToUri(dynamic value)
        {
            return string.IsNullOrEmpty(value?.Value) ? null : new Uri(value!.Value);
        }
#nullable disable

        /// <summary>
        /// Parses a version string into a <see cref="NuGetVersion"/>, supporting semantic versioning
        /// pre-release labels and/or build metadata (e.g. "12.0.0-alpha.1+build"), unlike <see cref="Version"/>.
        /// </summary>
        /// <param name="version">the version string to parse</param>
        /// <returns>a <see cref="NuGetVersion"/> representing <paramref name="version"/></returns>
        /// <exception cref="ArgumentException">thrown when <paramref name="version"/> is null or empty</exception>
        /// <exception cref="FormatException">thrown when <paramref name="version"/> is not a valid version</exception>
        public static NuGetVersion ParseVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentException("Version cannot be null or empty", nameof(version));
            }

            if (NuGetVersion.TryParse(version, out var nuGetVersion))
            {
                return nuGetVersion;
            }

            // fallback to the standard parser, so a meaningful FormatException is still thrown for truly invalid values
            return new NuGetVersion(Version.Parse(version));
        }

        /// <summary>
        /// Same as <see cref="ParseVersion(string)"/> but returns <see langword="false"/> instead of throwing
        /// when <paramref name="version"/> cannot be parsed.
        /// </summary>
        /// <param name="version">the version string to parse</param>
        /// <param name="result">the parsed <see cref="NuGetVersion"/>, or <see langword="null"/> if parsing failed</param>
        /// <returns><see langword="true"/> if <paramref name="version"/> was successfully parsed</returns>
        public static bool TryParseVersion(string version, out NuGetVersion result)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                result = null;
                return false;
            }

            return NuGetVersion.TryParse(version, out result);
        }

        /// <summary>
        /// Converts a <see cref="NuGetVersion"/> to a <see cref="Version"/>, keeping the traditional
        /// 3-part "Major.Minor.Build" formatting (as opposed to <see cref="NuGetVersion.Version"/>, which
        /// always includes a 4th "Revision" component, e.g. "11.0.0.0" instead of "11.0.0"). Any
        /// pre-release label and/or build metadata is discarded, since <see cref="Version"/> cannot
        /// represent it.
        /// </summary>
        /// <param name="version">the version to convert</param>
        /// <returns>a <see cref="Version"/> built from the numeric release components of <paramref name="version"/></returns>
        public static Version ToVersion(NuGetVersion version)
        {
            return version.Version.Revision > 0
                ? version.Version
                : new Version(version.Major, version.Minor, version.Patch);
        }

        /// <summary>
        /// Computes the npm dist-tag conventionally used by CM packages (e.g. "@criticalmanufacturing/ngx-schematics")
        /// for a given product/MES version, e.g.:
        /// <list type="bullet">
        /// <item>"12.0.0" -&gt; "release-1200"</item>
        /// <item>"12.0.0-alpha.1" -&gt; "alpha-1200"</item>
        /// <item>"12.0.0-next.2" -&gt; "next-1200"</item>
        /// </list>
        /// </summary>
        /// <param name="version">the MES version whose prerelease label must be preserved when computing the dist-tag</param>
        /// <returns>the npm dist-tag for <paramref name="version"/></returns>
        public static string GetNpmDistTag(MesVersion version)
        {
            return GetNpmDistTag(version.NuGetVersion);
        }

        /// <summary>
        /// Computes the npm dist-tag for a full SemVer value, preserving the prerelease label when present.
        /// </summary>
        /// <param name="version">the parsed NuGet version to convert</param>
        /// <returns>the npm dist-tag for <paramref name="version"/></returns>
        public static string GetNpmDistTag(NuGetVersion version)
        {
            // ReleaseLabels are dot separated values from the pre-release part of the version, e.g. "alpha.1" or "beta.2".
            // We only want the first label (e.g. "alpha" or "beta") for the dist-tag.
            var label = version.IsPrerelease ? version.ReleaseLabels.FirstOrDefault() : null;
            return $"{(string.IsNullOrWhiteSpace(label) ? "release" : label)}-{version.Major}{version.Minor}{version.Patch}";
        }

        /// <summary>
        /// Same as <see cref="GetNpmDistTag(NuGetVersion)"/>, but for a plain <see cref="Version"/> (which
        /// cannot carry a pre-release label, so the result always uses the "release" dist-tag).
        /// </summary>
        /// <param name="version">the version to compute the dist-tag for</param>
        /// <returns>the npm dist-tag for <paramref name="version"/></returns>
        public static string GetNpmDistTag(Version version)
        {
            return GetNpmDistTag(new NuGetVersion(version.Major, version.Minor, version.Build));
        }

        /// <summary>
        /// Builds a tree representation of a CmfPackage dependency tree
        /// </summary>
        /// <param name="pkg">the root package</param>
        public static Tree BuildTree(CmfPackage pkg)
        {
            var tree = new Tree($"{pkg.PackageId}@{pkg.Version} [[{pkg.Location.ToString()}]]");
            if (pkg.Dependencies.HasAny())
            {
                for (int i = 0; i < pkg.Dependencies.Count; i++)
                {
                    Dependency dep = pkg.Dependencies[i];

                    if (!dep.IsMissing)
                    {
                        var curNode = tree.AddNode($"{dep.CmfPackage.PackageId}@{dep.CmfPackage.Version} [[{dep.CmfPackage.Location.ToString()}]]");
                        BuildTreeNodes(dep.CmfPackage, curNode);
                    }
                    else if (dep.IsMissing)
                    {
                        if (dep.Mandatory)
                        {
                            tree.AddNode($"[red]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                        else
                        {
                            tree.AddNode($"[yellow]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                    }
                }
            }
            return tree;
        }
        
        /// <summary>
        /// Builds a tree representation of a CmfPackage dependency tree
        /// </summary>
        /// <param name="pkg">the root package</param>
        public static Tree BuildTree(CmfPackageV1 pkg)
        {
            var tree = new Tree($"{pkg.PackageId}@{pkg.Version} [[{pkg.Client.RepositoryRoot}]]");
            if (pkg.Dependencies.HasAny())
            {
                for (int i = 0; i < pkg.Dependencies.Count; i++)
                {
                    Dependency dep = pkg.Dependencies[i];

                    if (!dep.IsMissing)
                    {
                        var curNode = tree.AddNode($"{dep.CmfPackageV1.PackageId}@{dep.CmfPackageV1.Version} [[{dep.CmfPackageV1.Client.RepositoryRoot}]]");
                        BuildTreeNodes(dep.CmfPackageV1, curNode);
                    }
                    else if (dep.IsMissing)
                    {
                        if (dep.Mandatory)
                        {
                            tree.AddNode($"[red]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                        else
                        {
                            tree.AddNode($"[yellow]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                    }
                }
            }
            return tree;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="envVarName"></param>
        /// <returns></returns>
        public static bool IsEnvVarTruthy(string envVarName)
        {
            var enableConsoleExporter = System.Environment.GetEnvironmentVariable(envVarName);
            return enableConsoleExporter is "1" or "true" or "TRUE" or "True";
        }

        public static void ValidatePropertyRequirement(string fieldName, string value, PropertyRequirement requirement)
        {
            if (requirement == PropertyRequirement.Ignored && !string.IsNullOrEmpty(value))
            {
                Log.Warning($"${fieldName} has been defined, but will be ignored because it is not needed.");
            }
            else if (requirement == PropertyRequirement.Mandatory && string.IsNullOrEmpty(value))
            {
                throw new Exception($"Missing mandatory {fieldName}.");
            }
        }

        public static string BuildEnvVarPrefix(RepositoryCredentialsType repositoryType, string baseUri)
        {
            char[] strip = ['/', '.', '-'];
            return repositoryType.ToString().ToLower() + "__" + new string(baseUri.Select(ch => strip.Contains(ch) ? '_' : ch).ToArray());
        }

        #endregion Public Methods

        #region Private Methods

        private static void BuildTreeNodes(CmfPackage pkg, TreeNode node)
        {
            if (pkg.Dependencies.HasAny())
            {
                for (int i = 0; i < pkg.Dependencies.Count; i++)
                {
                    Dependency dep = pkg.Dependencies[i];

                    if (!dep.IsMissing)
                    {
                        var curNode = node.AddNode($"{dep.CmfPackage.PackageId}@{dep.CmfPackage.Version} [[{dep.CmfPackage.Location.ToString()}]]");
                        BuildTreeNodes(dep.CmfPackage, curNode);
                    }
                    else if (dep.IsMissing)
                    {
                        if (dep.Mandatory)
                        {
                            node.AddNode($"[red]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                        else
                        {
                            node.AddNode($"[yellow]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                    }
                }
            }
        }
        
        private static void BuildTreeNodes(CmfPackageV1 pkg, TreeNode node)
        {
            if (pkg.Dependencies.HasAny())
            {
                for (int i = 0; i < pkg.Dependencies.Count; i++)
                {
                    Dependency dep = pkg.Dependencies[i];

                    if (!dep.IsMissing)
                    {
                        var curNode = node.AddNode($"{dep.CmfPackageV1.PackageId}@{dep.CmfPackageV1.Version} [[{dep.CmfPackageV1.Client.RepositoryRoot}]]");
                        BuildTreeNodes(dep.CmfPackageV1, curNode);
                    }
                    else if (dep.IsMissing)
                    {
                        if (dep.Mandatory)
                        {
                            node.AddNode($"[red]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                        else
                        {
                            node.AddNode($"[yellow]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                    }
                }
            }
        }

        #endregion Private Methods
    }
}
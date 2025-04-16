using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
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
        /// <param name="buildNr"></param>
        /// <returns>
        /// the new version
        /// </returns>
        public static string RetrieveNewVersion(string currentVersion, string version, string buildNr)
        {
            if (!string.IsNullOrEmpty(version))
            {
                currentVersion = version;
            }
            if (!string.IsNullOrEmpty(buildNr))
            {
                currentVersion += "-" + buildNr;
            }

            return currentVersion;
        }

        /// <summary>
        /// Will create a new version based on the old and new inputs
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <param name="version"></param>
        /// <param name="buildNr"></param>
        /// <returns>
        /// the new version
        /// </returns>
        public static string RetrieveNewPresentationVersion(string currentVersion, string version, string buildNr)
        {
            GenericUtilities.GetCurrentPresentationVersion(currentVersion, out string originalVersion, out string originalBuildNumber);

            string newVersion = !string.IsNullOrEmpty(version) ? version : originalVersion;
            if (!string.IsNullOrEmpty(buildNr))
            {
                newVersion += "-" + buildNr;
            }

            return newVersion;
        }

        /// <summary>
        /// Get current version based on string, for
        /// the format 1.0.0-1234
        /// where 1.0.0 will be the version
        /// and the 1234 will be the build number
        /// </summary>
        /// <param name="source">Source information to be parsed</param>
        /// <param name="version">Version Number</param>
        /// <param name="buildNr">Build Number</param>
        public static void GetCurrentPresentationVersion(string source, out string version, out string buildNr)
        {
            version = string.Empty;
            buildNr = string.Empty;

            if (!string.IsNullOrWhiteSpace(source))
            {
                string[] sourceInfo = source.Split('-');
                version = sourceInfo[0];
                if (sourceInfo.Length > 1)
                {
                    buildNr = sourceInfo[1];
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
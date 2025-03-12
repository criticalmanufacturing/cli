using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.Serialization;

[assembly: InternalsVisibleTo("tests")]

namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    public abstract class PackageTypeHandler : IPackageTypeHandler
    {
        #region Private Properties

        /// <summary>
        /// Sets whether the identifier should be omitted when extracting package dependencies.
        /// Only true when CmfPackage 'DependenciesDirectory' has a value
        /// </summary>
        /// <default>
        /// false
        /// </default>
        private static bool omitIdentifier = false;

        #endregion Private Properties

        #region Protected Properties

        /// <summary>
        /// the underlying file system
        /// </summary>
        protected IFileSystem fileSystem;

        /// <summary>
        /// The CMF package
        /// </summary>
        protected internal CmfPackage CmfPackage;

        /// <summary>
        /// The files to pack
        /// </summary>
        protected List<FileToPack> FilesToPack = new();

        /// <summary>
        /// Gets or sets the build steps.
        /// </summary>
        /// <value>
        /// The build steps.
        /// </value>
        internal IBuildCommand[] BuildSteps;

        /// <summary>
        /// Gets or sets the RelatedPackagesHandlers
        /// </summary>
        /// <value>
        /// The RelatedPackagesHandlers
        /// </value>
        internal Dictionary<RelatedPackage, IPackageTypeHandler> RelatedPackagesHandlers = new();

        #endregion Protected Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the default content to ignore.
        /// </summary>
        /// <value>
        /// The default content to ignore.
        /// </value>
        public List<string> DefaultContentToIgnore { get; }

        /// <summary>
        /// Where should the dependencies go, relative to the cmfpackage.json file
        /// </summary>
        public IDirectoryInfo DependenciesFolder { get; protected set; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageTypeHandler" /> class.
        /// </summary>
        /// <exception cref="CliException"></exception>
        public PackageTypeHandler(CmfPackage cmfPackage) : this(cmfPackage, cmfPackage.FileSystem) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        /// <param name="fileSystem"></param>
        public PackageTypeHandler(CmfPackage cmfPackage, IFileSystem fileSystem)
        {
            CmfPackage = cmfPackage;
            this.fileSystem = fileSystem;
            DefaultContentToIgnore = new List<string>()
            {
                ".gitattributes",
                ".gitignore",
                ".gitkeep",
                ".cmfpackageignore",
                "cmfpackage.json",
                "manifest.xml"
            };

            BuildSteps = Array.Empty<IBuildCommand>();

            cmfPackage.DFPackageType ??= cmfPackage.PackageType;

            cmfPackage.DependenciesDirectory ??= cmfPackage.DependenciesDirectory;

            if (!string.IsNullOrWhiteSpace(cmfPackage.DependenciesDirectory))
            {
                omitIdentifier = true;
                DependenciesFolder = fileSystem.DirectoryInfo.New(cmfPackage.DependenciesDirectory);
            }
            else
            {
                DependenciesFolder = fileSystem.DirectoryInfo.New(this.fileSystem.Path.Join(cmfPackage.GetFileInfo().Directory.FullName, "Dependencies"));
            }

            RelatedPackagesHandlers = new();

            foreach (var relatedPackage in CmfPackage.RelatedPackages ?? new())
            {
                RelatedPackagesHandlers.Add(relatedPackage, PackageTypeFactory.GetPackageTypeHandler(relatedPackage.CmfPackage));
            }
        }

        #endregion Constructors

        #region Private Methods

        /// <summary>
        /// Generates the deployment framework manifest.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <exception cref="CliException"></exception>
        internal virtual void GenerateDeploymentFrameworkManifest(IDirectoryInfo packageOutputDir)
        {
            Log.Debug("Generating DeploymentFramework manifest");
            string path = $"{packageOutputDir.FullName}/{CliConstants.DeploymentFrameworkManifestFileName}";

            // Get Template
            string fileContent = ResourceUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CliConstants.DeploymentFrameworkManifestFileName}");

            StringReader dFManifestReader = new(fileContent);
            XDocument dFManifestTemplate = XDocument.Load(dFManifestReader);

            // NOTE: We don't use an automatic serializer because we want full control on how the file is parsed
            XElement rootNode = dFManifestTemplate.Element("deploymentPackage", true);
            if (rootNode == null)
            {
                throw new CliException(string.Format(CoreMessages.InvalidManifestFile));
            }

            // List of empty Elements that we want to remove from the final file
            List<XElement> elementsToRemove = new();

            foreach (XElement element in rootNode.Elements())
            {
                // Get the Property Value based on the Token name
                string token = element.Value.Trim();

                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                // if is xmlInjection means that we need to inject xml content
                if (token.IgnoreCaseEquals(CliConstants.TokenXmlInjection) && CmfPackage.XmlInjection.HasAny())
                {
                    foreach (string xmlInjectionFile in CmfPackage.XmlInjection)
                    {
                        IFileInfo xmlFile = this.fileSystem.FileInfo.New($"{CmfPackage.GetFileInfo().Directory}/{xmlInjectionFile}");
                        string xmlFileContent = xmlFile.ReadToString();

                        if (!xmlFile.Exists || string.IsNullOrEmpty(xmlFileContent))
                        {
                            throw new CliException(string.Format(CoreMessages.NotFound, xmlFile.FullName));
                        }

                        StringReader reader = new(xmlFileContent);
                        XDocument xml = XDocument.Load(reader);

                        rootNode.Add(xml.Elements());
                    }

                    elementsToRemove.Add(element);

                    continue;
                }

                object propertyValue = CmfPackage.GetPropertyValueFromTokenName(token);

                // If a Property with the same name of the token was not found
                // We need to remove that element from the final xml file
                if (propertyValue.IsNullOrEmpty())
                {
                    elementsToRemove.Add(element);
                }
                else
                {
                    // If the Property is not a List we just need to set the Element Value with the Property Value
                    if (!propertyValue.IsList())
                    {
                        element.Value = propertyValue.ToString();
                    }
                    else
                    {
                        IList listProperty = (propertyValue as IList);
                        string typeName = listProperty[0].GetType().GetTypeInfo().Name.ToCamelCase();

                        // Cleanup the token
                        element.Value = string.Empty;

                        // Each Property in the list will be handled as an xml element with attributes
                        foreach (object property in listProperty)
                        {
                            List<XAttribute> xAttributes = new();
                            foreach (PropertyInfo propertyinfo in property.GetType().GetProperties())
                            {
                                if (propertyinfo.CustomAttributes.Any(attr => attr.AttributeType == typeof(XmlIgnoreAttribute)))
                                {
                                    continue;
                                }

                                var obj = propertyinfo.GetValue(property);
                                if (obj != null)
                                {
                                    xAttributes.Add(new XAttribute(propertyinfo.Name.ToCamelCase(), propertyinfo.GetValue(property)));
                                }
                            }

                            element.Add(new XElement(typeName, xAttributes));
                        }
                    }
                }
            }

            // Remove empty Elements
            foreach (XElement elementToRemove in elementsToRemove)
            {
                elementToRemove.Remove();
            }

            fileSystem.File.WriteAllText(path, dFManifestTemplate.ToString());
        }

        /// <summary>
        /// Gets the content to ignore.
        /// </summary>
        /// <param name="contentToPack">The content to pack.</param>
        /// <param name="packDirectory">The pack directory.</param>
        /// <param name="defaultContentToIgnore">The default content to ignore.</param>
        /// <returns></returns>
        private List<string> GetContentToIgnore(ContentToPack contentToPack, IDirectoryInfo packDirectory, List<string> defaultContentToIgnore)
        {
            List<string> contentToIgnore = new();

            if (defaultContentToIgnore != null)
            {
                contentToIgnore.AddRange(defaultContentToIgnore);
            }

            if (contentToPack.IgnoreFiles.HasAny())
            {
                foreach (string ignoreFileName in contentToPack.IgnoreFiles)
                {
                    contentToIgnore.Add($"{ignoreFileName}");

                    IFileInfo ignoreFile = packDirectory.GetFiles(ignoreFileName).FirstOrDefault();
                    if (ignoreFile == null)
                    {
                        string filePath = this.fileSystem.Path.Join(packDirectory.FullName, ignoreFileName);
                        Log.Warning(string.Format(CoreMessages.NotFound, filePath));
                    }

                    foreach (string ignore in ignoreFile.ReadToStringList())
                    {
                        string _ignore = ignore;

                        if (!string.IsNullOrEmpty(_ignore) &&
                            !_ignore.StartsWith("!"))
                        {
                            if (_ignore.StartsWith("/"))
                            {
                                _ignore = _ignore.Remove(0);
                            }

                            contentToIgnore.Add(_ignore);
                        }
                    }
                }

                // Avoid duplicates
                contentToIgnore = contentToIgnore.Distinct().ToList();
            }

            return contentToIgnore;
        }

        /// <summary>
        /// Final Archive the package
        /// </summary>
        /// <param name="packageOutputDir">The pack directory.</param>
        /// <param name="outputDir">The Output directory.</param>
        /// <returns></returns>
        internal virtual void FinalArchive(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            foreach (FileToPack fileToPack in FilesToPack)
            {
                // If the destination directory doesn't exist, create it.
                if (!fileToPack.Target.Directory.Exists)
                {
                    fileToPack.Target.Directory.Create();
                }

                fileToPack.Source.CopyTo(fileToPack.Target.FullName, true);
            }

            string tempzipPath = $"{CmfPackage.GetFileInfo().Directory.FullName}/{CmfPackage.PackageName}.zip";
            if (this.fileSystem.File.Exists(tempzipPath))
            {
                this.fileSystem.File.Delete(tempzipPath);
            }

            FileSystemUtilities.ZipDirectory(fileSystem, tempzipPath, packageOutputDir);

            // move to final destination
            string destZipPath = $"{outputDir.FullName}/{CmfPackage.ZipPackageName}";
            this.fileSystem.File.Move(tempzipPath, destZipPath, true);
        }

        /// <summary>
        /// Get Content To pack
        /// </summary>
        /// <param name="packageOutputDir">The pack directory.</param>
        /// <returns></returns>
        internal virtual List<FileToPack> GetContentToPack(IDirectoryInfo packageOutputDir)
        {
            List<FileToPack> filesToPack = new();

            if (CmfPackage.ContentToPack.HasAny())
            {
                IDirectoryInfo packageDirectory = CmfPackage.GetFileInfo().Directory;

                // TODO: Bulk Copy
                foreach (ContentToPack contentToPack in CmfPackage.ContentToPack)
                {
                    string _source = contentToPack.Source;
                    string _target = contentToPack.Target;

                    if (contentToPack.Action != null && contentToPack.Action != PackAction.Pack)
                    {
                        continue;
                    }

                    // Replace tokens with the expected value from CmfPackage

                    #region Token Replacement

                    if (_source.Contains(CliConstants.TokenVersion))
                    {
                        object _value = CmfPackage.GetPropertyValueFromTokenName(CliConstants.TokenVersion);
                        _source = _source.Replace(CliConstants.TokenVersion, _value.ToString());
                    }

                    if (_target.Contains(CliConstants.TokenVersion))
                    {
                        object _value = CmfPackage.GetPropertyValueFromTokenName(CliConstants.TokenVersion);
                        _target = _target.Replace(CliConstants.TokenVersion, _value.ToString());
                    }

                    #endregion Token Replacement

                    IDirectoryInfo[] _directoriesToPack = null;

                    try
                    {
                        // TODO: To be reviewed, files/directory search should be done with globs
                        _directoriesToPack = packageDirectory.GetDirectories(_source);
                    }
                    // Because the method GetDirectories throws an exception if the folder is not found
                    // we need to catch the error and don't throw an exception
                    catch (DirectoryNotFoundException ex)
                    {
                        Log.Debug(ex.Message);
                    }

                    if (_directoriesToPack.HasAny())
                    {
                        #region Directory Packing

                        foreach (IDirectoryInfo packDirectory in _directoriesToPack)
                        {
                            // If a package.json exists the packDirectoryName needs to change to the name in the package.json
                            dynamic _packageJson = packDirectory.GetFile(CoreConstants.PackageJson);

                            string _packDirectoryName = _packageJson == null ? packDirectory.Name : _packageJson.name;

                            string destPackDir = $"{packageOutputDir.FullName}/{_target}/{_packDirectoryName}";
                            List<string> contentToIgnore = GetContentToIgnore(contentToPack, packDirectory, DefaultContentToIgnore);
                            filesToPack.AddRange(FileSystemUtilities.GetFilesToPack(contentToPack, packDirectory.FullName, destPackDir, this.fileSystem, contentToIgnore));
                        }

                        #endregion Directory Packing
                    }

                    IFileInfo[] _filesToPack = null;

                    try
                    {
                       _filesToPack = packageDirectory.GetFiles(_source).OrderBy(file => file.Name).ToArray();
                    }
                    // Because the method GetFiles throws an exception if the folder is not found
                    // we need to catch the error and don't throw an exception
                    catch (DirectoryNotFoundException ex)
                    {
                        Log.Debug(ex.Message);
                    }

                    if (_filesToPack.HasAny())
                    {
                        #region Files Packing

                        List<string> contentToIgnore = GetContentToIgnore(contentToPack, packageDirectory, DefaultContentToIgnore);

                        foreach (IFileInfo packFile in _filesToPack)
                        {
                            // Skip files to ignore
                            if (contentToIgnore.Contains(packFile.Name))
                            {
                                continue;
                            }

                            string destPackFile = this.fileSystem.Path.Join(packageOutputDir.FullName, _target, packFile.Name);
                            filesToPack.Add(new()
                            {
                                ContentToPack = contentToPack,
                                Source = packFile,
                                Target = this.fileSystem.FileInfo.New(destPackFile)
                            });
                        }

                        #endregion Files Packing
                    }
                }
            }

            return filesToPack;
        }

        #endregion Private Methods

        #region Protected Methods

        /// <summary>
        /// Copies the install dependencies.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        protected virtual void CopyInstallDependencies(IDirectoryInfo packageOutputDir)
        {
        }

        #endregion Protected Methods

        #region Public Methods

        /// <summary>
        /// Bumps the specified version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="bumpInformation">The bump information.</param>
        public virtual void Bump(string version, string buildNr, Dictionary<string, object> bumpInformation = null)
        {
            // TODO: create "transaction" to rollback if anything fails
            // NOTE: Check pack strategy. Collect all packages to bump before bump.

            var currentVersion = CmfPackage.Version.Split("-")[0];
            var currentBuildNr = CmfPackage.Version.Split("-").Length > 1 ? CmfPackage.Version.Split("-")[1] : null;
            if (!currentVersion.IgnoreCaseEquals(version))
            {
                // TODO :: Uncomment if the cmfpackage.json support build number
                // cmfPackage.SetVersion(GenericUtilities.RetrieveNewVersion(currentVersion, version, buildNr));

                CmfPackage.SetVersion(!string.IsNullOrWhiteSpace(version) ? version : CmfPackage.Version);

                Log.Information($"Will bump {CmfPackage.PackageId} from version {currentVersion} to version {CmfPackage.Version}");
            }
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        public virtual void Build(bool test)
        {
            #region Pre-Build Actions

            foreach(var relatedPackageHandler in RelatedPackagesHandlers.Where(rp => !rp.Key.IsSet && rp.Key.PreBuild))
            {
                relatedPackageHandler.Value.Build();
                relatedPackageHandler.Key.IsSet = true;
            }

            #endregion Pre-Build Actions

            foreach (var step in BuildSteps)
            {
                if (!step.Test || step.Test == test)
                {
                    Log.Information($"Executing '{step.DisplayName}'");
                    step.Exec();
                }
            }

            #region Post-Build Actions

            foreach (var relatedPackageHandler in RelatedPackagesHandlers.Where(rp => !rp.Key.IsSet && rp.Key.PostBuild))
            {
                relatedPackageHandler.Value.Build();
                relatedPackageHandler.Key.IsSet = true;
            }

            #endregion Post-Build Actions
        }

        /// <summary>
        /// Packs the specified package output dir.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="outputDir">The output dir.</param>
        public virtual void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            #region Pre-Pack Actions

            foreach (var relatedPackageHandler in RelatedPackagesHandlers.Where(rp => !rp.Key.IsSet && rp.Key.PrePack))
            {
                var relatedPackagPackageOutputDir = FileSystemUtilities.GetPackageOutputDir(relatedPackageHandler.Key.CmfPackage, packageOutputDir, fileSystem);
                relatedPackageHandler.Value.Pack(relatedPackagPackageOutputDir, outputDir);
                relatedPackageHandler.Key.IsSet = true;
            }

            #endregion Pre-Pack Actions

            var filesToPack = GetContentToPack(packageOutputDir);
            if (CmfPackage.ContentToPack.HasAny() && !filesToPack.HasAny())
            {
                throw new CliException(string.Format(CoreMessages.ContentToPackNotFound, CmfPackage.PackageId, CmfPackage.Version));
            }

            if (filesToPack != null)
            {
                FilesToPack.AddRange(filesToPack);

                FilesToPack.ForEach(fileToPack =>
                {
                    Log.Debug($"Packing '{fileToPack.Source.FullName} to {fileToPack.Target.FullName} by contentToPack rule (Action: {fileToPack.ContentToPack.Action.ToString()}, Source: {fileToPack.ContentToPack.Source}, Target: {fileToPack.ContentToPack.Target})");
                    IDirectoryInfo _targetFolder = this.fileSystem.DirectoryInfo.New(fileToPack.Target.Directory.FullName);
                    if (!_targetFolder.Exists)
                    {
                        _targetFolder.Create();
                    }
                });
            }

            // TODO: To be removed? Install dependencies
            CopyInstallDependencies(packageOutputDir);

            GenerateDeploymentFrameworkManifest(packageOutputDir);

            FinalArchive(packageOutputDir, outputDir);

            Log.Debug($"{outputDir.FullName}/{CmfPackage.ZipPackageName} created");
            Log.Information($"{CmfPackage.PackageName} packed");

            #region Post-Pack Actions

            foreach (var relatedPackageHandler in RelatedPackagesHandlers.Where(rp => !rp.Key.IsSet && rp.Key.PostPack))
            {
                var relatedPackagPackageOutputDir = FileSystemUtilities.GetPackageOutputDir(relatedPackageHandler.Key.CmfPackage, packageOutputDir, fileSystem);
                relatedPackageHandler.Value.Pack(relatedPackagPackageOutputDir, outputDir);
                relatedPackageHandler.Key.IsSet = true;
            }

            #endregion Post-Pack Actions
        }

        /// <summary>
        /// Restore the the current package's dependencies to the dependencies folder
        /// </summary>
        /// <param name="repoUris">The Uris for the package repos</param>
        /// <exception cref="CliException">thrown when a repo uri is not available or in an incorrect format</exception>
        public virtual void RestoreDependencies(Uri[] repoUris)
        {
            Log.Debug($"Using repos at {string.Join(", ", repoUris.Select(r => r.OriginalString))}");
            Log.Debug($"Targeting dependencies folder at {this.DependenciesFolder.FullName}");
            var rootIdentifier = $"{this.CmfPackage.PackageId}@{this.CmfPackage.Version}";
            Log.Status($"Loading {rootIdentifier} dependency tree...", ctx =>
            {
                this.CmfPackage.LoadDependencies(repoUris, ctx, true);
                ctx.Status($"Restoring {rootIdentifier} dependency tree...");
                if (this.CmfPackage.Dependencies == null)
                {
                    Log.Information($"No dependencies declared for package {rootIdentifier}");
                    return;
                }

                // flatten dependency tree. We need to obtain all dependencies
                var allDependencies = this.CmfPackage.Dependencies.Flatten(
                    dependency => dependency.IsMissing || dependency.IsIgnorable ?
                        new DependencyCollection() :
                        dependency.CmfPackage.Dependencies).ToArray();

                var missingDependencies = allDependencies.Where(d => d.IsMissing).ToArray();
                if (missingDependencies.Any())
                {
                    Log.Warning($"Dependencies missing:{Environment.NewLine}{string.Join(Environment.NewLine, missingDependencies.Select(d => $"{d.Id}@{d.Version}"))}");
                }

                var foundDependencies = allDependencies.Where(d => !d.IsMissing && d.CmfPackage.Location == PackageLocation.Repository).ToArray();
                if (!foundDependencies.Any())
                {
                    Log.Information("No present remote dependencies to restore. Exiting...");
                    return;
                }
                else
                {
                    Log.Verbose($"Found {foundDependencies.Length} actionable dependencies in the {rootIdentifier} dependency tree. Restoring...");
                }

                if (this.DependenciesFolder.Exists)
                {
                    Log.Debug($"Deleting directory {this.DependenciesFolder.FullName} and all its contents");
                    this.DependenciesFolder.Delete(true);
                }
                if (!this.DependenciesFolder.Exists)
                {
                    this.DependenciesFolder.Create();
                    Log.Debug($"Created Dependencies directory at {this.DependenciesFolder.FullName}");
                }
                foreach (var dependency in foundDependencies)
                {
                    var identifier = $"{dependency.Id}@{dependency.Version}";
                    Log.Debug($"Processing dependency {identifier}...");
                    Log.Debug($"Found package {identifier} at {dependency.CmfPackage.Uri.AbsoluteUri}");
                    if(dependency.CmfPackage.SharedFolder != null)
                    {
                        var file = dependency.CmfPackage.SharedFolder.GetFile(dependency.CmfPackage.ZipPackageName);
                        ExtractZip(file.Item2, identifier);
                    }
                    else if (dependency.CmfPackage.Uri.IsDirectory())
                    {
                        using (Stream zipToOpen = this.fileSystem.FileInfo.New(dependency.CmfPackage.Uri.LocalPath).OpenRead())
                        {
                            ExtractZip(zipToOpen, identifier);
                        }
                    }
                }
            });
        }

        private void ExtractZip(Stream zipToOpen, string identifier)
        {
            using (ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read))
            {
                // these tuples allow us to rewrite entry paths
                var entriesToExtract = new List<Tuple<ZipArchiveEntry, string>>();
                entriesToExtract.AddRange(zip.Entries.Select(entry => new Tuple<ZipArchiveEntry, string>(entry, entry.FullName)));

                foreach (var entry in entriesToExtract)
                {
                    var target = this.fileSystem.Path.Join(this.DependenciesFolder.FullName, omitIdentifier ? null : identifier, entry.Item2);
                    var targetDir = this.fileSystem.Path.GetDirectoryName(target);
                    if (target.EndsWith("/"))
                    {
                        // this a dotnet bug: if a folder contains a ., the library assumes it's a file and adds it as an entry
                        // however, afterwards all folder contents are separate entries, so we can just skip these
                        continue;
                    }

                    if (!fileSystem.File.Exists(target)) // TODO: support overwriting if requested
                    {
                        var overwrite = false;
                        Log.Debug($"Extracting {entry.Item1.FullName} to {target}");
                        if (!string.IsNullOrEmpty(targetDir))
                        {
                            fileSystem.Directory.CreateDirectory(targetDir);
                        }

                        entry.Item1.ExtractToFile(target, overwrite, fileSystem);
                    }
                    else
                    {
                        Log.Debug($"Skipping {target}, file exists");
                    }
                }
            }
        }

        #endregion Public Methods
    }
}
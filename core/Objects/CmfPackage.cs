using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Core.Objects;
using Cmf.CLI.Core.Interfaces;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="CmfPackage" />
    [JsonObject]
    public class CmfPackage : IEquatable<CmfPackage>
    {
        #region Private Properties

        /// <summary>
        /// The file information
        /// </summary>
        private IFileInfo FileInfo;

        /// <summary>
        /// Should we set the defaults values as described in the package handler?
        /// </summary>
        internal bool IsToSetDefaultValues;

        private IFileSystem fileSystem;

        #endregion Private Properties

        #region Internal Properties

        [JsonIgnore]
        public IFileSystem FileSystem => fileSystem;

        /// <summary>
        /// Gets the file name of the package.
        /// </summary>
        /// <value>
        /// The file name of the package.
        /// </value>
        [JsonIgnore]
        public string PackageName => $"{PackageId}.{Version}";

        /// <summary>
        /// Gets the name of the zip package.
        /// </summary>
        /// <value>
        /// The name of the zip package.
        /// </value>
        [JsonIgnore]
        public string ZipPackageName => $"{PackageName}.zip";

        #endregion Internal Properties

        #region Public Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty(Order = 0)]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the package identifier.
        /// </summary>
        /// <value>
        /// The package identifier.
        /// </value>
        [JsonProperty(Order = 1)]
        public string PackageId { get; private set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [JsonProperty(Order = 2)]
        public string Version { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty(Order = 3)]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the type of the package.
        /// </summary>
        /// <value>
        /// The type of the package.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(Order = 4)]
        public PackageType PackageType { get; private set; }

        /// <summary>
        /// Gets or sets the target directory where the package contents should be installed.
        /// This is used when the package is installed using Deployment Framework and ignored when it is installed using Environment Manager.
        /// </summary>
        /// <value>
        /// The target directory.
        /// </value>
        [JsonProperty(Order = 5)]
        public string TargetDirectory { get; private set; }

        /// <summary>
        /// Gets or sets the target layer, which means the container in which the packages contents should be installed.
        /// This is used when the package is installed using Environment Manager and ignored when it is installed using Deployment Framework.
        /// </summary>
        /// <value>
        /// The target layer.
        /// </value>
        [JsonProperty(Order = 6)]
        public string TargetLayer { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is installable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is installable; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(Order = 7)]
        public bool? IsInstallable { get; private set; }

        /// <summary>
        /// Gets or sets the is unique install.
        /// </summary>
        /// <value>
        /// The is unique install.
        /// </value>
        [JsonProperty(Order = 8)]
        public bool? IsUniqueInstall { get; private set; }

        /// <summary>
        /// Gets or sets the is root package.
        /// </summary>
        /// <value>
        /// The is root package.
        /// </value>
        [JsonProperty(Order = 9)]
        [JsonIgnore]
        public string Keywords { get; private set; }

        /// <summary>
        /// Should we set the default steps as described in the handler?
        /// </summary>
        /// <value>
        /// true to set the default steps; otherwise, false.
        /// </value>
        [JsonProperty(Order = 10)]
        [JsonIgnore]
        public bool? IsToSetDefaultSteps { get; private set; }

        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>
        /// The dependencies.
        /// </value>
        [JsonProperty(Order = 11)]
        public DependencyCollection Dependencies { get; private set; }

        /// <summary>
        /// Gets or sets the steps.
        /// </summary>
        /// <value>
        /// The steps.
        /// </value>
        [JsonProperty(Order = 12)]
        public List<Step> Steps { get; set; }

        /// <summary>
        /// Gets or sets the content to pack.
        /// </summary>
        /// <value>
        /// The content to pack.
        /// </value>
        [JsonProperty(Order = 13)]
        public List<ContentToPack> ContentToPack { get; private set; }

        /// <summary>
        /// Gets or sets the deployment framework UI file.
        /// </summary>
        /// <value>
        /// The deployment framework UI file.
        /// </value>
        [JsonProperty(Order = 14)]
        public List<string> XmlInjection { get; private set; }

        /// <summary>
        /// Gets or sets the Test Package Id.
        /// </summary>
        /// <value>
        /// The Test Package Id.
        /// </value>
        [JsonProperty(Order = 15)]
        public DependencyCollection TestPackages { get; set; }

        /// <summary>
        /// The location of the package
        /// </summary>
        [JsonProperty(Order = 16)]
        [JsonIgnore]
        public PackageLocation Location { get; private set; }

        /// <summary>
        /// Handler Version
        /// </summary>
        [JsonProperty(Order = 17)]
        public int HandlerVersion { get; private set; }

        /// <summary>
        /// Should the Deployment Framework wait for the Integration Entries created by the package
        /// This fails the package installation if any Integration Entry fails
        /// </summary>
        [JsonProperty(Order = 18)]
        public bool? WaitForIntegrationEntries { get; private set; }

        /// <summary>
        /// The Uri of the package
        /// </summary>
        [JsonProperty(Order = 19)]
        [JsonIgnore]
        public Uri Uri { get; private set; }

        /// <summary>
        /// The df package type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(Order = 20)]
        public PackageType? DFPackageType { get; set; }

        /// <summary>
        /// Gets or sets the build steps.
        /// </summary>
        /// <value>
        /// The build steps.
        /// </value>
        [JsonProperty(Order = 21)]
        public List<ProcessBuildStep> BuildSteps { get; set; }

        /// <summary>
        /// Gets or sets the Related packages, and sets what are the expected behavior.
        /// </summary>
        /// <value>
        /// Packages that should be built/packed before/after the context package
        /// </value>
        [JsonProperty(Order = 22)]
        public RelatedPackageCollection RelatedPackages { get; set; }

        /// <summary>
        /// Gets or sets the target directory where the dependencies contents should be extracted.
        /// This is used when the package dependencies are restored in the restore and build commands.
        /// </summary>
        /// <value>
        /// The dependencies target directory.
        /// </value>
        [JsonProperty(Order = 23)]
        public string DependenciesDirectory { get; set; }

        /// <summary>
        /// Loaded SharedFolder when loading from cifs
        /// </summary>
        [JsonIgnore]
        public ISharedFolder SharedFolder { get; private set; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CmfPackage"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="description">The description.</param>
        /// <param name="packageType">Type of the package.</param>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="targetLayer">The target layer.</param>
        /// <param name="isInstallable">The is installable.</param>
        /// <param name="isUniqueInstall">The is unique install.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="isToSetDefaultSteps">The is to set default steps.</param>
        /// <param name="dependencies">The dependencies.</param>
        /// <param name="steps">The steps.</param>
        /// <param name="contentToPack">The content to pack.</param>
        /// <param name="xmlInjection">The XML injection.</param>
        /// <param name="waitForIntegrationEntries">should wait for integration entries to complete</param>
        /// <param name="testPackages">The test Packages.</param>
        [JsonConstructor]
        public CmfPackage(string name, string packageId, string version, string description, PackageType packageType,
                          string targetDirectory, string targetLayer, bool? isInstallable, bool? isUniqueInstall, string keywords,
                          bool? isToSetDefaultSteps, DependencyCollection dependencies, List<Step> steps,
                          List<ContentToPack> contentToPack, List<string> xmlInjection, bool? waitForIntegrationEntries, DependencyCollection testPackages = null) : this()
        {
            Name = name;
            PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Description = description;
            PackageType = packageType;
            TargetDirectory = targetDirectory;
            TargetLayer = targetLayer;
            IsInstallable = isInstallable ?? true;
            IsUniqueInstall = isUniqueInstall ?? false;
            Keywords = keywords;
            IsToSetDefaultSteps = isToSetDefaultSteps ?? true;
            Dependencies = dependencies;
            Steps = steps;
            ContentToPack = contentToPack;
            XmlInjection = xmlInjection;
            WaitForIntegrationEntries = waitForIntegrationEntries;
            TestPackages = testPackages;
        }

        /// <summary>
        /// initialize an empty CmfPackage
        /// </summary>
        public CmfPackage() : this(fileSystem: new FileSystem())
        {
        }

        /// <summary>
        /// Initialize an empty CmfPackage with a specific file system
        /// </summary>
        /// <param name="fileSystem"></param>
        public CmfPackage(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public CmfPackage(IFileInfo fileInfo) : this(ExecutionContext.Instance.FileSystem)
        {
            FileInfo = fileInfo;
        }

        /// <summary>
        /// Initialize CmfPackage with PackageId, Version and Uri
        /// </summary>
        public CmfPackage(string packageId, string version, Uri uri)
        {
            PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Uri = uri;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Validates the package.
        /// </summary>
        public void ValidatePackage()
        {
            // If is installable and is not a root Package ContentToPack are mandatory
            if (!PackageType.Equals(PackageType.Root) &&
                (IsInstallable ?? false))
            {
                if (!ContentToPack.HasAny())
                {
                    throw new CliException(string.Format(CoreMessages.MissingMandatoryPropertyInFile, nameof(ContentToPack), $"{FileInfo.FullName}"));
                }
            }

            if (PackageType.Equals(PackageType.Data) &&
                !(IsUniqueInstall ?? false))
            {
                throw new CliException(string.Format(CoreMessages.InvalidValue, this.GetType(), "IsUniqueInstall", true));
            }

            // criticalmanufacturing.deploymentmetadata and cmf.environment should be part of the dependencies in a package of Type Root
            if (PackageType.Equals(PackageType.Root) &&
                !Dependencies.Contains(Dependency.DefaultDependenciesToIgnore[0]) && !Dependencies.Contains(Dependency.DefaultDependenciesToIgnore[1]))
            {
                throw new CliException(string.Format(CoreMessages.MissingMandatoryDependency, $"{Dependency.DefaultDependenciesToIgnore[0]} and {Dependency.DefaultDependenciesToIgnore[1]}", string.Empty));
            }

            // When is fixed by the product team, this can be uncommented
            //// cmf.connectiot.packages should be part of the dependencies in a package of Type IoT
            //if (PackageType.Equals(PackageType.IoT) &&
            //    !Dependencies.HasAny(d => d.Id.IgnoreCaseEquals(Dependency.DefaultDependenciesToIgnore[2])))
            //{
            //    throw new CliException(string.Format(CliMessages.MissingMandatoryDependency, $"{ Dependency.DefaultDependenciesToIgnore[2] }", string.Empty));
            //}
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(CmfPackage other)
        {
            return other != null &&
                   PackageId.IgnoreCaseEquals(other.PackageId) &&
                   Version.IgnoreCaseEquals(other.Version) &&
                   Description.IgnoreCaseEquals(other.Description) &&
                   PackageType == other.PackageType &&
                   TargetDirectory.IgnoreCaseEquals(other.TargetDirectory) &&
                   TargetLayer.IgnoreCaseEquals(other.TargetLayer) &&
                   IsInstallable == other.IsInstallable &&
                   IsUniqueInstall == other.IsUniqueInstall &&
                   Keywords.IgnoreCaseEquals(other.Keywords) &&
                   XmlInjection.Equals(other.XmlInjection) &&
                   EqualityComparer<DependencyCollection>.Default.Equals(Dependencies, other.Dependencies) &&
                   EqualityComparer<List<Step>>.Default.Equals(Steps, other.Steps) &&
                   EqualityComparer<List<ContentToPack>>.Default.Equals(ContentToPack, other.ContentToPack) &&
                   EqualityComparer<IFileInfo>.Default.Equals(GetFileInfo(), other.GetFileInfo());
        }

        /// <summary>
        /// Gets or sets the file information.
        /// </summary>
        /// <returns>
        /// The file information.
        /// </returns>
        public IFileInfo GetFileInfo()
        {
            return FileInfo;
        }

        /// <summary>
        /// Gets or sets the file information.
        /// </summary>
        /// <param name="value">The file information.</param>
        public void SetFileInfo(IFileInfo value)
        {
            FileInfo = value;
        }

        /// <summary>
        /// Sets the defaults.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="targetLayer">The target layer container.</param>
        /// <param name="isInstallable">The is installable.</param>
        /// <param name="isUniqueInstall">The is unique install.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="waitForIntegrationEntries">should we wait for integration entries to complete</param>
        /// <param name="steps">The steps.</param>
        /// <exception cref="CliException"></exception>
        public void SetDefaultValues(
            string name = null,
            string targetDirectory = null,
            string targetLayer = null,
            bool? isInstallable = null,
            bool? isUniqueInstall = null,
            string keywords = null,
            bool? waitForIntegrationEntries = null,
            List<Step> steps = null)
        {
            if (IsToSetDefaultValues)
            {
                Name = string.IsNullOrEmpty(Name) ? string.IsNullOrEmpty(name) ? $"{PackageId.Replace(".", " ")}" : name : Name;

                TargetDirectory = string.IsNullOrEmpty(TargetDirectory) ? targetDirectory : TargetDirectory;

                TargetLayer = string.IsNullOrEmpty(TargetLayer) ? targetLayer : TargetLayer;

                IsInstallable ??= isInstallable;

                IsUniqueInstall ??= isUniqueInstall;

                Keywords = string.IsNullOrEmpty(Keywords) ? keywords : Keywords;

                WaitForIntegrationEntries ??= waitForIntegrationEntries;

                if ((IsToSetDefaultSteps ?? false) && steps.HasAny())
                {
                    List<Step> stepsToAdd = new();
                    foreach (Step defaultStep in steps)
                    {
                        if (!Steps.Has(defaultStep))
                        {
                            stepsToAdd.Add(defaultStep);
                        }
                    }

                    // To be sure that the Default Steps are in first place
                    if (Steps.HasAny())
                    {
                        stepsToAdd.AddRange(Steps);
                    }

                    Steps = stepsToAdd;
                }
            }
        }

        /// <summary>
        /// Sets the version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void SetVersion(string version)
        {
            Version = version;
        }

        /// <summary>
        /// Builds a dependency tree by attaching the CmfPackage objects to the parent's dependencies
        /// Can run recursively and fetch packages from a DF repository.
        /// Supports cycles
        /// </summary>
        /// <param name="repoUris">the address of the package repositories (currently only folders are supported)</param>
        /// <param name="recurse">should we run recursively</param>
        /// <returns>this CmfPackage for chaining, but the method itself is mutable</returns>
        public void LoadDependencies(IEnumerable<Uri> repoUris, StatusContext ctx, bool recurse = false)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity("CmfPackage LoadDependencies");
            activity?.SetTag("cmfPackage", $"{this.PackageId}.{this.Version}");
            List<CmfPackage> loadedPackages = new();
            loadedPackages.Add(this);
            ctx?.Status($"Working on {this.Name ?? (this.PackageId + "@" + this.Version)}");

            if (this.Dependencies.HasAny())
            {
                var allDirectories = repoUris?.All(r => r.IsDirectory());
                if (allDirectories == false)
                {
                    throw new CliException(CoreMessages.UrlsNotSupported);
                }

                IDirectoryInfo[] repoDirectories = repoUris?.Select(r => r.GetDirectory()).Where(d=> d.Exists==true).ToArray();
                if (ExecutionContext.Instance.RunningOnWindows && repoDirectories.Length == 0)
                {
                    throw new CliException($"None of the provided repositories exist: {string.Join(", ", repoUris.Select(d => d.OriginalString))}");
                }
                foreach (var dependency in this.Dependencies)
                {
                    ctx?.Status($"Working on dependency {dependency.Id}@{dependency.Version}");

                    #region Get Dependencies from Dependencies Directory

                    // 1) check if we have found this package before
                    var dependencyPackage = loadedPackages.FirstOrDefault(x => x.PackageId.IgnoreCaseEquals(dependency.Id) && x.Version.IgnoreCaseEquals(dependency.Version));

                    // 2) check if package is in repository
                    if (dependencyPackage == null)
                    {
                        dependencyPackage = LoadFromRepo(repoDirectories, dependency.Id, dependency.Version);
                    }

                    // 3) search in the source code repository (only if this is a local package)
                    if (dependencyPackage == null && this.Location == PackageLocation.Local)
                    {
                        dependencyPackage = FileInfo.Directory.LoadCmfPackagesFromSubDirectories(setDefaultValues: true).GetDependency(dependency);
                        if (dependencyPackage != null)
                        {
                            dependencyPackage.Uri = new Uri(dependencyPackage.FileInfo.FullName);
                        }
                    }

                    if (dependencyPackage != null)
                    {
                        loadedPackages.Add(dependencyPackage);
                        dependency.CmfPackage = dependencyPackage;
                        if (recurse)
                        {
                            dependencyPackage.LoadDependencies(repoUris, ctx, recurse);
                        }
                    }
                }

                #endregion Get Dependencies from Dependencies Directory
            }
        }

        /// <summary>
        /// Should Serialize Handler Version
        /// </summary>
        /// <returns>returns false if handler version is 0 otherwise true</returns>
        public bool ShouldSerializeHandlerVersion()
        {
            return HandlerVersion != 0;
        }

        /// <summary>
        /// Shoulds the serialize DF package type.
        /// </summary>
        /// <returns>returns false if DF Package Type is null and Package Type is different then Generic, otherwise true</returns>
        public bool ShouldSerializeDFPackageType()
        {
            return DFPackageType != null && PackageType == PackageType.Generic;
        }

        /// <summary>
        /// Shoulds the serialize Related Packages
        /// </summary>
        /// <returns>returns false if Related Packages is null or empty</returns>
        public bool ShouldSerializeRelatedPackages()
        {
            return RelatedPackages.HasAny();
        }

        /// <summary>
        /// Should the Dependencies Directory be serialized
        /// </summary>
        /// <returns>returns false if Dependencies Directory is null or empty</returns>
        public bool ShouldSerializeDependenciesDirectory()
        {
            return !string.IsNullOrWhiteSpace(DependenciesDirectory);
        }

        #region Static Methods

        /// <summary>
        /// Loads the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="setDefaultValues"></param>
        /// <param name="fileSystem">the underlying file system</param>
        /// <returns></returns>
        /// <exception cref="Cmf.CLI.Utilities.CliException">
        /// </exception>
        /// <exception cref="CliException"></exception>
        public static CmfPackage Load(IFileInfo file, bool setDefaultValues = false, IFileSystem fileSystem = null)
        {
            fileSystem ??= ExecutionContext.Instance.FileSystem;
            if (!file.Exists)
            {
                throw new CliException(string.Format(CoreMessages.NotFound, file.FullName));
            }

            string fileContent = file.ReadToString();
            CmfPackage cmfPackage = JsonConvert.DeserializeObject<CmfPackage>(fileContent);
            cmfPackage.IsToSetDefaultValues = setDefaultValues;
            cmfPackage.FileInfo = file;
            cmfPackage.Location = PackageLocation.Local;
            cmfPackage.fileSystem = fileSystem;

            cmfPackage.RelatedPackages?.Load(cmfPackage);

            return cmfPackage;
        }

        /// <summary>
        /// Load Method for an instantiated CmfPackage object
        /// </summary>
        /// <param name="setDefaultValues"></param>
        /// <exception cref="CliException"></exception>
        public void Load(bool setDefaultValues = false)
        {
            if (!FileInfo.Exists)
            {
                throw new CliException(string.Format(CoreMessages.NotFound, FileInfo.FullName));
            }

            string fileContent = FileInfo.ReadToString();
            JsonConvert.PopulateObject(fileContent, this);
            IsToSetDefaultValues = setDefaultValues;
            Location = PackageLocation.Local;

            RelatedPackages?.Load(this);

        }

        /// <summary>
        /// Similar to Load, but without deserialization.
        /// Only sets PackageId and Version
        /// </summary>
        /// <exception cref="CliException"></exception>
        public void Peek()
        {
            if (!FileInfo.Exists)
            {
                throw new CliException(string.Format(CoreMessages.NotFound, FileInfo.FullName));
            }

            Location = PackageLocation.Local;

            string fileContent = FileInfo.ReadToString();
            string packageIdPattern = "\"packageId\"\\s*:\\s*\"(.*?)\"";
            string versionPattern = "\"version\"\\s*:\\s*\"(.*?)\"";

            PackageId = Regex.Match(fileContent, packageIdPattern).Groups[1].Value;
            Version = Regex.Match(fileContent, versionPattern).Groups[1].Value;
        }

        /// <summary>
        /// Gets the URI from repos.
        /// </summary>
        /// <param name="repoDirectories">The repo directories.</param>
        /// <param name="packageId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static CmfPackage LoadFromRepo(IDirectoryInfo[] repoDirectories, string packageId, string version, bool fromManifest = true)
        {
            if(!ExecutionContext.Instance.RunningOnWindows && ExecutionContext.Instance.CIFSClients.HasAny())
            {
                return LoadFromCIFSShare(packageId, version);
            }

            if (version is null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            CmfPackage cmfPackage = null;

            string _dependencyFileName = $"{packageId}.{version}.zip";

            IFileInfo dependencyFile = repoDirectories?
                           .Select(r => r.GetFiles(_dependencyFileName).FirstOrDefault())
                           .Where(r => r != null)
                           .FirstOrDefault();

            if (dependencyFile != null)
            {
                if(fromManifest)
                {
                    using (Stream zipToOpen = dependencyFile.OpenRead())
                    {
                        using (ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read))
                        {
                                var manifest = zip.GetEntry(CoreConstants.DeploymentFrameworkManifestFileName);
                                if (manifest != null)
                                {
                                    using var stream = manifest.Open();
                                    using var reader = new StreamReader(stream);
                                    cmfPackage = FromManifest(reader.ReadToEnd(), setDefaultValues: true);
                                    if (cmfPackage != null)
                                    {
                                        cmfPackage.Uri = new(dependencyFile.FullName);
                                    }
                                }
                        }
                    }
                }
                else
                {
                    cmfPackage = new CmfPackage(packageId, version, new Uri(dependencyFile.FullName));
                }
            }
            return cmfPackage;
        }

        /// <summary>
        /// Load CmfPackage from a CIFS Share
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static CmfPackage LoadFromCIFSShare(string packageId, string version, bool fromManifest = true)
        {
            CmfPackage cmfPackage = null;
            string _dependencyFileName = $"{packageId}.{version}.zip";

            foreach(CIFSClient client in ExecutionContext.Instance.CIFSClients.Where(c=> c.IsConnected))
            {
                foreach (var share in client.SharedFolders.Where(sf => sf.Exists))
                {
                    var file = share.GetFile(_dependencyFileName);
                    if(file != null)
                    {                   
                        if(fromManifest)
                        {
                            using (ZipArchive zip = new(file.Item2, ZipArchiveMode.Read))
                            {
                                var manifest = zip.GetEntry(CoreConstants.DeploymentFrameworkManifestFileName);
                                if (manifest != null)
                                {
                                    using var stream = manifest.Open();
                                    using var reader = new StreamReader(stream);
                                    cmfPackage = FromManifest(reader.ReadToEnd(), setDefaultValues: true);
                                    if (cmfPackage != null)
                                    {   
                                        cmfPackage.Uri = file.Item1;
                                        cmfPackage.SharedFolder = share;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            cmfPackage = new CmfPackage(packageId, version, file.Item1);
                            cmfPackage.SharedFolder = share;
                            break;
                        }
                    }
                }
            }

            return cmfPackage;          
        }

        /// <summary>
        /// Create a CmfPackage object from a DF package manifest
        /// </summary>
        /// <param name="manifest">the manifest content</param>
        /// <param name="setDefaultValues">should set default values</param>
        /// <param name="fileSystem">the underlying file system</param>
        /// <returns>a CmfPackage</returns>
        public static CmfPackage FromManifest(string manifest, bool setDefaultValues = false, IFileSystem fileSystem = null)
        {
            fileSystem ??= new FileSystem();
            StringReader dFManifestReader = new(manifest);
            XDocument dFManifestTemplate = XDocument.Load(dFManifestReader);
            var tokens = new Dictionary<string, string>();

            XElement rootNode = dFManifestTemplate.Element("deploymentPackage", true);
            if (rootNode == null)
            {
                throw new CliException(string.Format(CoreMessages.InvalidManifestFile));
            }
            DependencyCollection deps = new();
            DependencyCollection testPackages = new();
            foreach (XElement element in rootNode.Elements())
            {
                // Get the Property Value based on the Token name
                string token = element.Value.Trim();

                if (element.Name.LocalName == "dependencies")
                {
                    var deplist = element.Elements().Select(depEl => new Dependency(depEl.Attribute("id").Value, depEl.Attribute("version").Value));
                    deps.AddRange(deplist);
                }

                if (element.Name.LocalName == "testPackages")
                {
                    var testPackagesList = element.Elements().Select(depEl => new Dependency(depEl.Attribute("id").Value, depEl.Attribute("version").Value));
                    testPackages.AddRange(testPackagesList);
                }

                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                tokens.Add(element.Name.LocalName.ToLowerInvariant(), token);
            }

            PackageType cliPackageType = PackageType.Generic;
            if (tokens.ContainsKey("clipackagetype"))
            {
                Enum.TryParse(tokens["clipackagetype"], out cliPackageType);
            }

            // TODO: we're extracting only the essentials here for `cmf ls` but we can get extra data from the manifests
            var cmfPackage = new CmfPackage(
                tokens.ContainsKey("name") ? tokens["name"] : null,
                tokens["packageid"],
                tokens["version"],
                tokens.ContainsKey("description") ? tokens["description"] : null,
                cliPackageType,
                "",
                "",
                false,
                false,
                tokens.ContainsKey("keywords") ? tokens["keywords"] : null,
                true,
                deps,
                null,
                null,
                null,
                waitForIntegrationEntries: false,
                testPackages
                );

            cmfPackage.Location = PackageLocation.Repository;
            cmfPackage.fileSystem = fileSystem;

            return cmfPackage;
        }

        #endregion Static Methods

        #region Utilities

        /// <summary>
        /// Determines whether [is root package].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is root package] [the specified CMF package]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRootPackage()
        {
            return Keywords != null && Keywords.Contains(CoreConstants.RootPackageDefaultKeyword);
        }

        /// <summary>
        /// Saves the CMF package.
        /// </summary>
        public void SaveCmfPackage()
        {
            DefaultContractResolver contractResolver = new()
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            };

            JsonSerializerSettings jsonSerializerSettings = new()
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractResolver,
                NullValueHandling = NullValueHandling.Ignore
            };

            string cmfPackageJson = JsonConvert.SerializeObject(this, jsonSerializerSettings);

            IFileInfo file = GetFileInfo();
            this.fileSystem.File.WriteAllText(file.FullName, cmfPackageJson);
        }

        /// <summary>
        /// Equalses the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as CmfPackage);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        #endregion Utilities

        #endregion Public Methods
    }
}
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using System.IO.Abstractions;

namespace Cmf.Common.Cli.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.IEquatable{Cmf.Common.Cli.Objects.CmfPackage}" />
    [JsonObject]
    public class CmfPackage : IEquatable<CmfPackage>
    {
        #region Private Properties

        /// <summary>
        /// The file information
        /// </summary>
        private IFileInfo FileInfo;

        /// <summary>
        /// The skip set default values
        /// </summary>
        private bool IsToSetDefaultValues;

        private IFileSystem fileSystem;

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets the name of the package.
        /// </summary>
        /// <value>
        /// The name of the package.
        /// </value>
        [JsonIgnore]
        internal string PackageName { get; private set; }

        /// <summary>
        /// Gets the name of the zip package.
        /// </summary>
        /// <value>
        /// The name of the zip package.
        /// </value>
        [JsonIgnore]
        internal string ZipPackageName { get; private set; }

        #endregion

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
        /// Gets or sets the target directory.
        /// </summary>
        /// <value>
        /// The target directory.
        /// </value>
        [JsonProperty(Order = 5)]
        public string TargetDirectory { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is installable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is installable; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(Order = 6)]
        public bool? IsInstallable { get; private set; }

        /// <summary>
        /// Gets or sets the is unique install.
        /// </summary>
        /// <value>
        /// The is unique install.
        /// </value>
        [JsonProperty(Order = 7)]
        public bool? IsUniqueInstall { get; private set; }

        /// <summary>
        /// Gets or sets the is root package.
        /// </summary>
        /// <value>
        /// The is root package.
        /// </value>
        [JsonProperty(Order = 8)]
        [JsonIgnore]
        public string Keywords { get; private set; }

        /// <summary>
        /// Gets or sets the set default steps.
        /// </summary>
        /// <value>
        /// The set default steps.
        /// </value>
        [JsonProperty(Order = 9)]
        [JsonIgnore]
        public bool? IsToSetDefaultSteps { get; private set; }

        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>
        /// The dependencies.
        /// </value>
        [JsonProperty(Order = 10)]
        public DependencyCollection Dependencies { get; private set; }

        /// <summary>
        /// Gets or sets the steps.
        /// </summary>
        /// <value>
        /// The steps.
        /// </value>
        [JsonProperty(Order = 11)]
        public List<Step> Steps { get; private set; }

        /// <summary>
        /// Gets or sets the content to pack.
        /// </summary>
        /// <value>
        /// The content to pack.
        /// </value>
        [JsonProperty(Order = 12)]
        public List<ContentToPack> ContentToPack { get; private set; }

        /// <summary>
        /// Gets or sets the deployment framework UI file.
        /// </summary>
        /// <value>
        /// The deployment framework UI file.
        /// </value>
        [JsonProperty(Order = 13)]
        public List<string> XmlInjection { get; private set; }

        /// <summary>
        /// Gets or sets the Test Package Id.
        /// </summary>
        /// <value>
        /// The Test Package Id.
        /// </value>
        [JsonProperty(Order = 14)]
        public DependencyCollection TestPackages { get; private set; }

        /// <summary>
        /// The location of the package
        /// </summary>
        [JsonProperty(Order = 15)]
        [JsonIgnore]
        public PackageLocation Location { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the package.
        /// </summary>
        private void ValidatePackage()
        {
            // If is installable and is not a root Package ContentToPack and XmlInjection are mandatory
            if (!PackageType.Equals(PackageType.Root) &&
                (IsInstallable ?? false))
            {
                if (!ContentToPack.HasAny())
                {
                    throw new CliException(string.Format(CliMessages.MissingMandatoryPropertyInFile, nameof(ContentToPack), $"{FileInfo.FullName}"));
                }

                if (!XmlInjection.HasAny())
                {
                    throw new CliException(string.Format(CliMessages.MissingMandatoryPropertyInFile, nameof(XmlInjection), $"{FileInfo.FullName}"));
                }
            }

            if (PackageType == PackageType.Data &&
                !(IsUniqueInstall ?? false))
            {
                throw new CliException(string.Format(CliMessages.InvalidValue, this.GetType(), "IsUniqueInstall", true));
            }
        }

        #endregion

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
        /// <param name="isInstallable">The is installable.</param>
        /// <param name="isUniqueInstall">The is unique install.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="isToSetDefaultSteps">The is to set default steps.</param>
        /// <param name="dependencies">The dependencies.</param>
        /// <param name="steps">The steps.</param>
        /// <param name="contentToPack">The content to pack.</param>
        /// <param name="xmlInjection">The XML injection.</param>
        /// <param name="testPackages">The test Packages.</param>
        [JsonConstructor]
        public CmfPackage(string name, string packageId, string version, string description, PackageType packageType,
                          string targetDirectory, bool? isInstallable, bool? isUniqueInstall, string keywords,
                          bool? isToSetDefaultSteps, DependencyCollection dependencies, List<Step> steps,
                          List<ContentToPack> contentToPack, List<string> xmlInjection, DependencyCollection testPackages = null) : this()
        {
            Name = name;
            PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Description = description;
            PackageType = packageType;
            TargetDirectory = targetDirectory;
            IsInstallable = isInstallable ?? true;
            IsUniqueInstall = isUniqueInstall ?? false;
            Keywords = keywords;
            IsToSetDefaultSteps = isToSetDefaultSteps ?? true;
            Dependencies = dependencies;
            Steps = steps;
            ContentToPack = contentToPack;
            XmlInjection = xmlInjection;
            TestPackages = testPackages;

            PackageName = $"{PackageId}.{Version}";
            ZipPackageName = $"{PackageName}.zip";
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

        #endregion

        #region Public Methods

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
        /// <param name="isInstallable">The is installable.</param>
        /// <param name="isUniqueInstall">The is unique install.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="steps">The steps.</param>
        /// <exception cref="CliException"></exception>
        public void SetDefaultValues(
            string name = null,
            string targetDirectory = null,
            bool? isInstallable = null,
            bool? isUniqueInstall = null,
            string keywords = null,
            List<Step> steps = null)
        {
            if (IsToSetDefaultValues)
            {
                Name = string.IsNullOrEmpty(Name) ? string.IsNullOrEmpty(name) ? $"{PackageId.Replace(".", " ")}" : name : Name;

                TargetDirectory = string.IsNullOrEmpty(TargetDirectory) ? targetDirectory : TargetDirectory;

                IsInstallable ??= isInstallable;

                IsUniqueInstall ??= isUniqueInstall;

                Keywords = string.IsNullOrEmpty(Keywords) ? keywords : Keywords;

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
        /// <param name="repo">the address of the repository (currently only folders are supported)</param>
        /// <param name="recurse">should we run recursively</param>
        /// <returns>this CmfPackage for chaining, but the method itself is mutable</returns>
        public CmfPackage LoadDependencies(string repo, bool recurse = false) {
            Uri repoUri = repo != null ? new Uri(repo) : null;
            var loadedPackages = new List<CmfPackage>();
            loadedPackages.Add(this);
            Log.Progress($"Working on {this.Name ?? (this.PackageId + "@" + this.Version)}");

            if (this.Dependencies.HasAny())
            {
                IDirectoryInfo repoDirectory = null;
                if (repoUri != null)
                {
                    if (repoUri.IsDirectory())
                    {
                        repoDirectory = this.fileSystem.DirectoryInfo.FromDirectoryName(repoUri.OriginalString);
                        // repoDirectory = new(repoUri.OriginalString);
                    }
                    else
                    {
                        throw new CliException(CliMessages.UrlsNotSupported);
                    }
                }

                foreach (var dependency in this.Dependencies)
                {
                    Log.Progress($"Working on dependency {dependency.Id}@{dependency.Version}");
                    string _dependencyFileName = $"{dependency.Id}.{dependency.Version}.zip";

                    #region Get Dependencies from Dependencies Directory
                    // 1) check if we have found this package before
                    var dependencyPackage = loadedPackages.FirstOrDefault(x => x.PackageId.IgnoreCaseEquals(dependency.Id) && x.Version.IgnoreCaseEquals(dependency.Version));

                    // 2) check if package is in repository
                    if (dependencyPackage == null && (repoDirectory?.Exists ?? false))
                    {
                        IFileInfo dependencyFile = repoDirectory.GetFiles(_dependencyFileName).FirstOrDefault();
                        if (dependencyFile != null)
                        {
                            var zip = ZipFile.Open(dependencyFile.FullName, ZipArchiveMode.Read);
                            var manifest = zip.GetEntry(CliConstants.DeploymentFrameworkManifestFileName);
                            if (manifest != null)
                            {
                                using (var stream = manifest.Open())
                                using (var reader = new StreamReader(stream))
                                {
                                    dependencyPackage = CmfPackage.FromManifest(reader.ReadToEnd(), setDefaultValues: true);
                                }

                            }
                        }
                    }

                    // 3) search in the source code repository
                    if (dependencyPackage == null)
                    {
                        dependencyPackage = this.GetFileInfo().Directory.LoadCmfPackagesFromSubDirectories(setDefaultValues: true).GetDependency(dependency);
                    }

                    if (dependencyPackage != null)
                    {
                        loadedPackages.Add(dependencyPackage);
                        dependency.CmfPackage = dependencyPackage;
                        if (recurse)
                        {
                            dependencyPackage.LoadDependencies(repo, recurse);
                        }
                    }
                }
                #endregion
            }
            return this;
        }

        #region Static Methods

        /// <summary>
        /// Loads the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="setDefaultValues"></param>
        /// <param name="fileSystem">the underlying file system</param>
        /// <returns></returns>
        /// <exception cref="Cmf.Common.Cli.Utilities.CliException">
        /// </exception>
        /// <exception cref="CliException"></exception>
        public static CmfPackage Load(IFileInfo file, bool setDefaultValues = false, IFileSystem fileSystem = null)
        {
            fileSystem ??= new FileSystem();
            if (!file.Exists)
            {
                throw new CliException(string.Format(CliMessages.NotFound, file.FullName));
            }

            string fileContent = file.ReadToString();
            CmfPackage cmfPackage = JsonConvert.DeserializeObject<CmfPackage>(fileContent);
            cmfPackage.IsToSetDefaultValues = setDefaultValues;
            cmfPackage.FileInfo = file;
            cmfPackage.Location = PackageLocation.Local;
            cmfPackage.ValidatePackage();
            cmfPackage.fileSystem = fileSystem;

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
                throw new CliException(string.Format(CliMessages.InvalidManifestFile));
            }
            DependencyCollection deps = new DependencyCollection();
            foreach (XElement element in rootNode.Elements())
            {
                // Get the Property Value based on the Token name
                string token = element.Value.Trim();

                if (element.Name.LocalName == "dependencies")
                {
                    var deplist = element.Elements().Select(depEl => new Dependency(depEl.Attribute("id").Value, depEl.Attribute("version").Value));
                    deps.AddRange(deplist);
                }

                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                tokens.Add(element.Name.LocalName.ToLowerInvariant(), token);
            }

            // TODO: we're extracting only the essentials here for `cmf ls` but we can get extra data from the manifests
            var cmfPackage = new CmfPackage(
                tokens.ContainsKey("name") ? tokens["name"] : null,
                tokens["packageid"],
                tokens["version"],
                tokens.ContainsKey("description") ? tokens["description"] : null,
                PackageType.Generic,
                "",
                false,
                false,
                tokens.ContainsKey("keywords") ? tokens["keywords"] : null,
                true,
                deps,
                null,
                null,
                null
                );

            cmfPackage.Location = PackageLocation.Repository;
            cmfPackage.fileSystem = fileSystem;

            return cmfPackage;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Determines whether [is root package].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is root package] [the specified CMF package]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRootPackage()
        {
            return Keywords != null && Keywords.Contains(CliConstants.RootPackageDefaultKeyword);
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
            File.WriteAllText(file.FullName, cmfPackageJson);
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

        #endregion

        #endregion
    }
}
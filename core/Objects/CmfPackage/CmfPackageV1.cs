using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cmf.CLI.Core.Objects;

[JsonObject]
public class CmfPackageV1 : IEquatable<CmfPackageV1>
{
    /// <summary>
    /// Gets the file name of the package.
    /// </summary>
    /// <value>
    /// The file name of the package.
    /// </value>
    [JsonIgnore]
    public string PackageDotRef => $"{PackageId}.{Version}";
    
    [JsonIgnore]
    public string PackageAtRef => $"{PackageId}@{Version}";
    
    [JsonIgnore]
    public IRepositoryClient? Client { get; set; }

    // [JsonIgnore]
    // public Stream Stream { get; set; }

    [JsonIgnore]
    public IFileInfo? SourceManifestFile { get; set; }
    
    /// <summary>
    /// Should we set the defaults values as described in the package handler?
    /// </summary>
    internal bool IsToSetDefaultValues;
    
    #region Serializable Properties
    /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty(Order = 0)]
        public string? Name { get; private set; }

        /// <summary>
        /// Gets or sets the package identifier.
        /// </summary>
        /// <value>
        /// The package identifier.
        /// </value>
        [JsonProperty(Order = 1)]
        public required string PackageId { get; init; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [JsonProperty(Order = 2)]
        public required string Version { get; init; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty(Order = 3)]
        public string? Description { get; private set; }

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
        public string? TargetDirectory { get; private set; }

        /// <summary>
        /// Gets or sets the target layer, which means the container in which the packages contents should be installed.
        /// This is used when the package is installed using Environment Manager and ignored when it is installed using Deployment Framework.
        /// </summary>
        /// <value>
        /// The target layer.
        /// </value>
        [JsonProperty(Order = 6)]
        public string? TargetLayer { get; private set; }

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
        public string? Keywords { get; private set; }

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
        public DependencyCollection? Dependencies { get; private set; }

        /// <summary>
        /// Gets or sets the steps.
        /// </summary>
        /// <value>
        /// The steps.
        /// </value>
        [JsonProperty(Order = 12)]
        public List<Step>? Steps { get; set; }

        /// <summary>
        /// Gets or sets the content to pack.
        /// </summary>
        /// <value>
        /// The content to pack.
        /// </value>
        [JsonProperty(Order = 13)]
        public List<ContentToPack>? ContentToPack { get; private set; }

        /// <summary>
        /// Gets or sets the deployment framework UI file.
        /// </summary>
        /// <value>
        /// The deployment framework UI file.
        /// </value>
        [JsonProperty(Order = 14)]
        public List<string>? XmlInjection { get; private set; }

        /// <summary>
        /// Gets or sets the Test Package Id.
        /// </summary>
        /// <value>
        /// The Test Package Id.
        /// </value>
        [JsonProperty(Order = 15)]
        public DependencyCollection? TestPackages { get; set; }
        
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
        public List<ProcessBuildStep>? BuildSteps { get; set; }

        /// <summary>
        /// Gets or sets the Related packages, and sets what are the expected behavior.
        /// </summary>
        /// <value>
        /// Packages that should be built/packed before/after the context package
        /// </value>
        [JsonProperty(Order = 22)]
        public RelatedPackageCollection? RelatedPackages { get; set; }

        /// <summary>
        /// Gets or sets the target directory where the dependencies contents should be extracted.
        /// This is used when the package dependencies are restored in the restore and build commands.
        /// </summary>
        /// <value>
        /// The dependencies target directory.
        /// </value>
        [JsonProperty(Order = 23)]
        public string? DependenciesDirectory { get; set; }
    #endregion

    #region constructors
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
    [SetsRequiredMembers]
    public CmfPackageV1(string? name, string? packageId, string? version, string? description, PackageType packageType,
                      string? targetDirectory, string? targetLayer, bool? isInstallable, bool? isUniqueInstall, string? keywords,
                      bool? isToSetDefaultSteps, DependencyCollection? dependencies, List<Step>? steps,
                      List<ContentToPack>? contentToPack, List<string>? xmlInjection, bool? waitForIntegrationEntries, DependencyCollection? testPackages = null)
            : this()
    {
        if (dependencies != null)
        {
            // Normalize version ranges to just the upper limit
            foreach (var dep in dependencies)
            {
                if (dep.Version != null && dep.Version.Contains('[') && dep.Version.Contains(']'))
                {
                    dep.Version = dep.Version.Replace("[", "").Replace("]", "").Split(',').Last().Trim();
                }
            }
        }

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
    public CmfPackageV1()
    {
    }
    #endregion
    
    public bool Equals(CmfPackageV1? other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((CmfPackageV1)obj);
    }

    public override int GetHashCode()
    {
        return this.PackageId.GetHashCode();
    }
}

public class CmfPackageV1Collection : List<CmfPackageV1>
{
    
}
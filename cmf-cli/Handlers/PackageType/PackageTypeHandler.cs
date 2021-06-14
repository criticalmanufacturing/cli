using Cmf.Common.Cli.Builders;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Interfaces;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Cmf.Common.Cli.Handlers
{
    /// <summary>
    ///
    /// </summary>
    public abstract class PackageTypeHandler : IPackageTypeHandler
    {
        #region Protected Properties

        /// <summary>
        /// The CMF package
        /// </summary>
        protected CmfPackage CmfPackage;

        /// <summary>
        /// Gets or sets the build steps.
        /// </summary>
        /// <value>
        /// The build steps.
        /// </value>
        protected IBuildCommand[] BuildSteps;

        /// <summary>
        /// The files to pack
        /// </summary>
        protected List<FileToPack> FilesToPack = new List<FileToPack>();

        /// <summary>
        /// The df package type
        /// </summary>
        protected PackageType DFPackageType;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the default content to ignore.
        /// </summary>
        /// <value>
        /// The default content to ignore.
        /// </value>
        public List<string> DefaultContentToIgnore { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageTypeHandler" /> class.
        /// </summary>
        /// <exception cref="CliException"></exception>
        public PackageTypeHandler(CmfPackage cmfPackage)
        {
            CmfPackage = cmfPackage;
            DefaultContentToIgnore = new List<string>()
            {
                ".gitattributes",
                ".gitignore",
                ".gitkeep",
                ".cmfpackageignore"
            };

            BuildSteps = Array.Empty<IBuildCommand>();

            DFPackageType = cmfPackage.PackageType;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Generates the deployment framework manifest.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <exception cref="Cmf.Common.Cli.Utilities.CliException"></exception>
        internal virtual void GenerateDeploymentFrameworkManifest(DirectoryInfo packageOutputDir)
        {
            Log.Information("Generating DeploymentFramework manifest");
            string path = $"{packageOutputDir.FullName}/{CliConstants.DeploymentFrameworkManifestFileName}";

            // Get Template
            string fileContent = GenericUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CliConstants.DeploymentFrameworkManifestFileName}");

            StringReader dFManifestReader = new(fileContent);
            XDocument dFManifestTemplate = XDocument.Load(dFManifestReader);

            // NOTE: We don't use an automatic serializer because we want full control on how the file is parsed
            XElement rootNode = dFManifestTemplate.Element("deploymentPackage", true);
            if (rootNode == null)
            {
                throw new CliException(string.Format(CliMessages.InvalidManifestFile));
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
                        FileInfo xmlFile = new($"{CmfPackage.GetFileInfo().Directory}/{xmlInjectionFile}");
                        string xmlFileContent = xmlFile.ReadToString();

                        if (!xmlFile.Exists || string.IsNullOrEmpty(xmlFileContent))
                        {
                            throw new CliException(string.Format(CliMessages.NotFound, xmlFile.FullName));
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
                        if (propertyValue is PackageType type)
                        {
                            propertyValue = DFPackageType;
                        }

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
                            // if is Dependency and package type is Test, then skip the dependency on Manifest
                            // This needs to be changed to get the Type from the dependency list.
                            // if (property is Dependency &&
                            //     ((Dependency)property).Type = PackageType.Test)
                            // {
                            //     continue;
                            // }

                            List<XAttribute> xAttributes = new();
                            foreach (PropertyInfo propertyinfo in property.GetType().GetProperties())
                            {
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

            dFManifestTemplate.Save(path);
        }

        /// <summary>
        /// Gets the content to ignore.
        /// </summary>
        /// <param name="contentToPack">The content to pack.</param>
        /// <param name="packDirectory">The pack directory.</param>
        /// <param name="defaultContentToIgnore">The default content to ignore.</param>
        /// <returns></returns>
        private static List<string> GetContentToIgnore(ContentToPack contentToPack, DirectoryInfo packDirectory, List<string> defaultContentToIgnore)
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

                    FileInfo ignoreFile = packDirectory.GetFiles(ignoreFileName).FirstOrDefault();
                    if (ignoreFile == null)
                    {
                        string filePath = Path.Join(packDirectory.FullName, ignoreFileName);
                        throw new CliException(string.Format(CliMessages.NotFound, filePath));
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
        internal virtual void FinalArchive(DirectoryInfo packageOutputDir, DirectoryInfo outputDir)
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
            if (File.Exists(tempzipPath))
            {
                File.Delete(tempzipPath);
            }
            ZipFile.CreateFromDirectory(packageOutputDir.FullName, tempzipPath);

            // move to final destination
            string destZipPath = $"{outputDir.FullName}/{CmfPackage.ZipPackageName}";
            File.Move(tempzipPath, destZipPath, true);
        }


        /// <summary>
        /// Get Content To pack
        /// </summary>
        /// <param name="packageOutputDir">The pack directory.</param>
        /// <returns></returns>
        internal virtual bool GetContentToPack(DirectoryInfo packageOutputDir)
        {
            bool foundContentToPack = false;

            #region Get Content to Pack

            if (CmfPackage.ContentToPack.HasAny())
            {
                DirectoryInfo packageDirectory = CmfPackage.GetFileInfo().Directory;

                // TODO: Bulk Copy
                foreach (ContentToPack contentToPack in CmfPackage.ContentToPack)
                {
                    string _source = contentToPack.Source;
                    string _target = contentToPack.Target;

                    if (contentToPack.Action != null && contentToPack.Action != Enums.PackAction.Pack)
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

                    #endregion

                    try
                    {
                        // TODO: To be reviewed, files/directory search should be done with globs

                        DirectoryInfo[] _directoriesToPack = packageDirectory.GetDirectories(_source);

                        if (_directoriesToPack.HasAny())
                        {
                            #region Directory Packing

                            foreach (DirectoryInfo packDirectory in _directoriesToPack)
                            {
                                // If a package.json exists the packDirectoryName needs to change to the name in the package.json
                                dynamic _packageJson = packDirectory.GetPackageJsonFile();

                                string _packDirectoryName = _packageJson == null ? packDirectory.Name : _packageJson.name;

                                string destPackDir = $"{packageOutputDir.FullName}/{_target}/{_packDirectoryName}";
                                List<string> contentToIgnore = GetContentToIgnore(contentToPack, packDirectory, DefaultContentToIgnore);
                                FilesToPack.AddRange(FileSystemUtilities.GetFilesToPack(contentToPack, packDirectory.FullName, destPackDir, contentToIgnore));

                                // If the FilesToPack is empty we assume that we don't have nothing to pack
                                if (FilesToPack.HasAny())
                                {
                                    foundContentToPack = true;
                                }
                            }

                            #endregion
                        }

                        FileInfo[] _filesToPack = packageDirectory.GetFiles(_source);

                        if (_filesToPack.HasAny())
                        {
                            #region Files Packing

                            List<string> contentToIgnore = GetContentToIgnore(contentToPack, packageDirectory, DefaultContentToIgnore);

                            foreach (FileInfo packFile in _filesToPack)
                            {
                                // Skip files to ignore
                                if (contentToIgnore.Contains(packFile.Name))
                                {
                                    continue;
                                }

                                DirectoryInfo _targetFolder = new($"{packageOutputDir.FullName}/{_target}");
                                if (!_targetFolder.Exists)
                                {
                                    _targetFolder.Create();
                                }
                                string destPackFile = $"{_targetFolder.FullName}/{packFile.Name}";
                                FilesToPack.Add(new()
                                {
                                    ContentToPack = contentToPack,
                                    Source = packFile,
                                    Target = new FileInfo(destPackFile)
                                });

                                foundContentToPack = true;
                            }

                            #endregion
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e.Message);
                    }
                }

                if (!foundContentToPack)
                {
                    Log.Warning(string.Format(CliMessages.ContentToPackNotFound, CmfPackage.PackageId, CmfPackage.Version));
                }
            }

            #endregion

            return foundContentToPack;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Copies the install dependencies.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        protected virtual void CopyInstallDependencies(DirectoryInfo packageOutputDir)
        {
        }

        #endregion

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

                // will save with new version
                CmfPackage.SaveCmfPackage();
            }
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        public virtual void Build()
        {
            foreach (var step in BuildSteps)
            {
                Log.Information($"Executing '{step.DisplayName}'");
                step.Exec();
            }
        }

        /// <summary>
        /// Packs the specified package output dir.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="outputDir">The output dir.</param>
        public virtual void Pack(DirectoryInfo packageOutputDir, DirectoryInfo outputDir)
        {
            GetContentToPack(packageOutputDir);

            // TODO: To be removed? Install dependencies
            CopyInstallDependencies(packageOutputDir);

            GenerateDeploymentFrameworkManifest(packageOutputDir);

            FinalArchive(packageOutputDir, outputDir);
            
            Log.Information($"{CmfPackage.PackageName} created");
        }

        #endregion

    }
}
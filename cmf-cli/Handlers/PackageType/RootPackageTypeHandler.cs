using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Utilities;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;


namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class RootPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public RootPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                name:
                    $"{cmfPackage.PackageId.Replace(".", " ")} (All)",
                keywords:
                    CliConstants.RootPackageDefaultKeyword
            );

            cmfPackage.DFPackageType = PackageType.Generic;
        }

        /// <summary>
        /// Packs the specified package output dir. If root package is an app, packs app files
        /// </summary>
        /// <param name="packageOutputDir"></param>
        /// <param name="outputDir"></param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            if (ExecutionContext.Instance.ProjectConfig?.RepositoryType == RepositoryType.App)
            {
                GenerateAppFiles(packageOutputDir, outputDir);
            }

            base.Pack(packageOutputDir, outputDir);
        }

        /// <summary>
        /// Generates the deployment framework app manifest and the app icon image.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <exception cref="CliException"></exception>
        internal virtual void GenerateAppFiles(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            IFileInfo cmfAppFile = fileSystem.FileInfo.New(CliConstants.CmfAppFileName);
            if (!cmfAppFile.Exists)
            {
                Log.Debug($"{CliConstants.CmfAppFileName} not found! No need to generate app manifest");
                return;
            }

            string appFileContent = cmfAppFile.ReadToString();

            var appData = JsonConvert.DeserializeObject<AppData>(appFileContent);

            Log.Debug("Generating App manifest");

            // Get Template
            string fileContent = ResourceUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CliConstants.AppManifestFileName}");

            StringReader dFManifestReader = new(fileContent);
            XDocument dFManifestTemplate = XDocument.Load(dFManifestReader);

            // NOTE: We don't use an automatic serializer because we want full control on how the file is parsed
            XElement rootNode = dFManifestTemplate.Element("App", true);
            if (rootNode == null)
            {
                throw new CliException(string.Format(CoreMessages.InvalidManifestFile));
            }

            // Set attributes for root node
            AppManifestSetAttributes(rootNode, appData);

            // Set attributes for root elements
            foreach (XElement element in rootNode.Elements())
            {
                AppManifestSetAttributes(element, appData);
            }

            fileSystem.Directory.CreateDirectory(packageOutputDir.FullName);

            // Save Icon file
            string iconPath = fileSystem.Path.Join(packageOutputDir.FullName, CliConstants.AppIcon);
            SaveAppIcon(packageOutputDir, appData, iconPath);

            // Save manifest file
            string manifestPath = fileSystem.Path.Join(packageOutputDir.FullName, CliConstants.DeploymentFrameworkManifestFileName);
            dFManifestTemplate.Save(manifestPath);

            // Create package zip file
            string appPackage = $"{appData.id}@{CmfPackage.Version}.zip";

            string tempzipPath = fileSystem.Path.Join(packageOutputDir.FullName, appPackage);
            if (fileSystem.File.Exists(tempzipPath))
            {
                fileSystem.File.Delete(tempzipPath);
            }

            FileSystemUtilities.ZipDirectory(fileSystem, tempzipPath, packageOutputDir);

            // move to final destination

            string destZipPath = fileSystem.Path.Join(outputDir.FullName, appPackage);
            fileSystem.File.Move(tempzipPath, destZipPath, true);

            // clean up folder files
            fileSystem.File.Delete(iconPath);
            fileSystem.File.Delete(manifestPath);
        }

        /// <summary>
        /// CMF App data
        /// </summary>
        private record AppData
        {
            public string id { get; set; }
            public string name { get; set; }
            public string author { get; set; }
            public string description { get; set; }
            public string licensedApplication { get; set; }
            public string icon { get; set; }
        }

        /// <summary>
        /// Set attributes for element
        /// </summary>
        /// <param name="element">XML element</param>
        /// <param name="appData">AppData object</param>
        private void AppManifestSetAttributes(XElement element, AppData appData)
        {
            for (int i = 0; i < element.Attributes().Count(); i++)
            {
                XAttribute attribute = element.Attributes().ElementAt(i);

                string token = attribute.Value;

                if (!string.IsNullOrEmpty(token) && !token.StartsWith('$'))
                    continue;

                object propertyValue = appData.GetPropertyValueFromTokenName(token);

                if (propertyValue == null)
                {
                    propertyValue = CmfPackage.GetPropertyValueFromTokenName(token);
                }

                if (propertyValue == null && token == "$(frameworkVersion)")
                {
                    propertyValue = $"^{ExecutionContext.Instance.ProjectConfig.MESVersion}";
                }

                if (propertyValue.IsNullOrEmpty())
                {
                    attribute.Remove();
                    i--;
                }
                else if (!propertyValue.IsList())
                {
                    attribute.Value = propertyValue.ToString();
                }
            }
        }

        /// <summary>
        /// Saves the app icon to the specified destination.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="appData">AppData object</param>
        /// <param name="iconPath">Path to save the icon</param>
        private void SaveAppIcon(IDirectoryInfo packageOutputDir, AppData appData, string iconPath)
        {
            if (string.IsNullOrWhiteSpace(appData.icon))
            {
                string projectRootPath = FileSystemUtilities.GetProjectRoot(fileSystem, throwException: true).FullName;
                string defaultIconPath = fileSystem.Path.Join(projectRootPath, CliConstants.AssetsFolder, CliConstants.DefaultAppIcon);
                appData.icon = defaultIconPath;
            }
            else if (!AppIconUtilities.IsIconValid(appData.icon))
            {
                throw new CliException(string.Format(CoreMessages.InvalidValue, CliConstants.CmfAppFileName));
            }

            string iconSource = appData.icon;

            byte[] iconBytes = fileSystem.File.ReadAllBytes(iconSource);

            fileSystem.File.WriteAllBytes(iconPath, iconBytes);
        }
    }
}
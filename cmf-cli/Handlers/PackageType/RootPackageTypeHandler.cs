using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Objects.CmfApp;
using Cmf.CLI.Core.Utilities;
using Cmf.CLI.Utilities;


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
        /// <param name="dryRun">if set to <c>true</c> list the package structure without creating files.</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false)
        {
            if (ExecutionContext.Instance.ProjectConfig?.RepositoryType == RepositoryType.App)
            {
                GenerateAppFiles(packageOutputDir, outputDir);
            }

            base.Pack(packageOutputDir, outputDir, dryRun);
        }

        /// <summary>
        /// Generates the deployment framework app manifest and the app icon image.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <exception cref="CliException"></exception>
        internal virtual void GenerateAppFiles(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            var appData = ExecutionContext.Instance.AppData ??
                throw new CliException("Could not retrieve repository AppData.");
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
            string iconTargetPath = fileSystem.Path.Join(packageOutputDir.FullName, CliConstants.AppIcon);
            SaveAppIcon(appData, iconTargetPath);

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
            fileSystem.File.Delete(iconTargetPath);
            fileSystem.File.Delete(manifestPath);
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
        /// <param name="iconTargetPath">Path to save the icon</param>
        private void SaveAppIcon(AppData appData, string iconTargetPath)
        {
            var projectRootPath = FileSystemUtilities
                .GetProjectRoot(fileSystem, throwException: true)
                .FullName;

            string fullIconPath;

            if (string.IsNullOrWhiteSpace(appData.icon))
            {
                fullIconPath = fileSystem.Path.Join(
                    projectRootPath,
                    CliConstants.AssetsFolder,
                    CliConstants.DefaultAppIcon
                );
            }
            else
            {
                fullIconPath = fileSystem.Path.Join(projectRootPath, appData.icon);

                if (!AppIconUtilities.IsIconValid(fullIconPath))
                {
                    throw new CliException(
                        string.Format(CoreMessages.InvalidValue, CoreConstants.CmfAppFileName)
                    );
                }
            }

            var iconBytes = fileSystem.File.ReadAllBytes(fullIconPath);
            fileSystem.File.WriteAllBytes(iconTargetPath, iconBytes);
        }

        
    }
}
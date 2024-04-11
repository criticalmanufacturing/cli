using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Objects.CmfApp;
using Cmf.CLI.Core.Utilities;
using Cmf.CLI.Utilities;
using System.IO;
using System.IO.Abstractions;
using static NuGet.Packaging.PackagingConstants;


namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class RootPackageTypeHandler : PackageTypeHandler
    {
        #region Private Properties

        /// <summary>
        /// The CMF app data object
        /// </summary>
        private CmfApp CmfApp;

        #endregion

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
            Log.Debug("Generating App manifest");

            IFileInfo cmfAppFile = fileSystem.FileInfo.New(CliConstants.CmfAppFileName);

            CmfApp = CmfApp.Load(cmfAppFile, fileSystem);

            if (string.IsNullOrWhiteSpace(CmfApp.Content.App.Image.File))
            {
                string defaultIconPath = fileSystem.Path.Join(FileSystemUtilities.GetProjectRoot(fileSystem, throwException: true).FullName,
                    CliConstants.AssetsFolder,
                    CliConstants.DefaultAppIcon);

                CmfApp.Content.App.Image.File = defaultIconPath;
            }
            else if (!AppIconUtilities.IsIconValid(CmfApp.Content.App.Image.File))
            {
                throw new CliException(string.Format(CoreMessages.InvalidValue, cmfAppFile.FullName));
            }

            fileSystem.Directory.CreateDirectory(packageOutputDir.FullName);

            string iconPath = fileSystem.Path.Join(packageOutputDir.FullName, CliConstants.AppIcon);
            CmfApp.SaveIcon(iconPath);
            CmfApp.Content.App.Image.File = CliConstants.AppIcon;

            string manifestPath = fileSystem.Path.Join(packageOutputDir.FullName, CliConstants.DeploymentFrameworkAppManifestFileName);
            CmfApp.Save(manifestPath);

            string appPackage = $"{CmfApp.Content.App.Name}@{CmfPackage.Version}.zip";
            
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

    }
}
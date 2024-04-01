using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Objects.CmfApp;
using Cmf.CLI.Core.Utilities;
using Cmf.CLI.Utilities;
using System.IO;
using System.IO.Abstractions;


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
            string path = packageOutputDir.FullName;

            IFileInfo cmfAppFile = fileSystem.FileInfo.New(CliConstants.CmfAppFileName);

            CmfApp = CmfApp.Load(cmfAppFile, fileSystem);

            if (string.IsNullOrWhiteSpace(CmfApp.Content.App.Image.File))
            {
                string defaultIconPath = Path.Combine(
                    FileSystemUtilities.GetProjectRoot(fileSystem, throwException: true).FullName,
                    CliConstants.AssetsFolder,
                    CliConstants.DefaultAppIcon);

                CmfApp.Content.App.Image.File = defaultIconPath;
            }
            else if (!AppIconUtilities.IsIconValid(CmfApp.Content.App.Image.File))
            {
                throw new CliException(string.Format(CoreMessages.InvalidValue, cmfAppFile.FullName));
            }

            fileSystem.Directory.CreateDirectory(path);

            string manifestPath = Path.Combine(path, CliConstants.AppManifestFileName);
            CmfApp.Save(manifestPath);

            string deploymentManifestPath = Path.Combine(path, CliConstants.AppDeploymentManifestFileName);
            CmfApp.Save(deploymentManifestPath);

            string iconDestinationPath = Path.Combine(path, CliConstants.AppIcon);

            CmfApp.SaveIcon(iconDestinationPath);

            string appPackage = $"{CmfApp.Content.App.Name}@{CmfPackage.Version}.zip";
            string tempzipPath = Path.Combine(packageOutputDir.FullName, appPackage);
            if (this.fileSystem.File.Exists(tempzipPath))
            {
                this.fileSystem.File.Delete(tempzipPath);
            }

            FileSystemUtilities.ZipDirectory(fileSystem, tempzipPath, packageOutputDir);

            // move to final destination
            string destZipPath = Path.Combine(outputDir.FullName, appPackage);
            this.fileSystem.File.Move(tempzipPath, destZipPath, true);

            // clean up folder files
            fileSystem.File.Delete(iconDestinationPath);
            fileSystem.File.Delete(manifestPath);
        }

    }
}
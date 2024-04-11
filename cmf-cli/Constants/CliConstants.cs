using Cmf.CLI.Core.Enums;
using System.Text.RegularExpressions;

namespace Cmf.CLI.Constants
{
    public static class CliConstants
    {
        public const string PackageName = "@criticalmanufacturing/cli";

        #region Folders

        /// <summary>
        /// The folder templates
        /// </summary>
        public const string FolderTemplates = "templateFiles";

        /// <summary>
        /// The folder install dependencies
        /// </summary>
        public const string FolderInstallDependencies = "installDependencies";

        /// <summary>
        /// Tests Folder
        /// </summary>
        public const string FolderTests = "/Tests";

        #endregion

        #region Files

        /// <summary>
        /// The deployment framework manifest template
        /// </summary>
        public const string DeploymentFrameworkManifestFileName = "manifest.xml";

        /// <summary>
        /// The app manifest template
        /// </summary>
        public const string DeploymentFrameworkAppManifestFileName = "app_manifest.xml";

        /// <summary>
        /// The CMF package file name
        /// </summary>
        public const string CmfPackageFileName = "cmfpackage.json";

        /// <summary>
        /// The CMF app file name
        /// </summary>
        public const string CmfAppFileName = "cmfapp.json";

        /// <summary>
        /// The assets folder
        /// </summary>
        public const string AssetsFolder = "assets";

        /// <summary>
        /// The default app icon file name
        /// </summary>
        public const string DefaultAppIcon = "default_app_icon.png";

        /// <summary>
        /// The final app icon file name
        /// </summary>
        public const string AppIcon = "app_icon.png";

        /// <summary>
        /// The CMF package default presentation configuration
        /// </summary>
        public const string CmfPackagePresentationConfig = "config.json";

        /// <summary>
        /// The CMF package default host configuration
        /// </summary>
        public const string CmfPackageHostConfig = "Cmf.Foundation.Services.HostService.dll.config";

        /// <summary>
        /// Dependencies File Name
        /// </summary>
        public const string FileDependencies = "dependencies.json";

        /// <summary>
        /// The CMF package default security portal configuration
        /// </summary>
        public const string CmfPackageSecurityPortalConfig = "config.json";

        #endregion

        #region Generic

        /// <summary>
        /// The root package default keyword
        /// </summary>
        public const string RootPackageDefaultKeyword = "cmf-root-package";

        /// <summary>
        /// Driver keyword for IoT Packages
        /// </summary>
        public const string Driver = "driver";

        /// <summary>
        /// npm.js repository url
        /// </summary>
        public const string NpmJsUrl = "https://registry.npmjs.com";

        /// <summary>
        /// The default organization for naming new packages
        /// </summary>
        public const string DefaultOrganization = "Cmf";

        /// <summary>
        /// the default product to name new packages
        /// </summary>
        public const string DefaultProduct = "Custom";
        /// <summary>
        /// Security Portal Path for deployment of strategies
        /// </summary>
        public const string DefaultStrategyPath = "$.tenants.config.$(tenant).strategies";

        /// <summary>
        /// Default repository type
        /// </summary>
        public const RepositoryType DefaultRepositoryType = RepositoryType.Customization;

        /// <summary>
        /// Default base layer for repository
        /// </summary>
        public const BaseLayer DefaultBaseLayer = BaseLayer.MES;

        /// <summary>
        /// Name of angular json file
        /// </summary>
        public const string AngularJson = "angular.json";
        #endregion

        #region Security Portal

        /// <summary>
        /// 
        /// </summary>
        public const string Strategy = "$(strategy)";

        /// <summary>
        /// 
        /// </summary>
        public const string StrategyPath = "$(strategyPath)";

        /// <summary>
        /// The token cache Id
        /// </summary>
        public const string MetadataUrl = "$(metadataUrl)";

        /// <summary>
        /// The token cache Id
        /// </summary>
        public const string RedirectUrl = "$(redirectUrl)";

        /// <summary>
        /// The token cache Id
        /// </summary>
        public const string ClientId = "$(clientId)";

        /// <summary>
        /// The token cache Id
        /// </summary>
        public const string Tenant = "$(tenant)";

        #endregion

        #region Tokens

        /// <summary>
        /// The token XML injection
        /// </summary>
        public const string TokenXmlInjection = "$(xmlInjection)";

        /// <summary>
        /// The token packages to remove
        /// </summary>
        public const string TokenPackagesToRemove = "$(packagesToRemove)";

        /// <summary>
        /// The token packages to add
        /// </summary>
        public const string TokenPackagesToAdd = "$(packagesToAdd)";

        /// <summary>
        /// The token version
        /// </summary>
        public const string TokenVersion = "$(version)";

        /// <summary>
        /// The token for JDT injection
        /// </summary>
        public const string TokenJDTInjection = "$(JDTInjection)";

        /// <summary>
        /// The token package identifier
        /// </summary>
        public const string TokenPackageId = "$(packageId)";

        /// <summary>
        /// The token cache Id
        /// </summary>
        public const string CacheId = "$(cacheId)";

        #endregion
    }
}
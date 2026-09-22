namespace Cmf.CLI.Core.Constants
{
    /// <summary>
    ///
    /// </summary>
    public static class CoreConstants
    {
        #region Files

        /// <summary>
        /// The deployment framework manifest template
        /// </summary>
        public const string DeploymentFrameworkManifestFileName = "manifest.xml";

        /// <summary>
        /// The CMF package file name
        /// </summary>
        public const string CmfPackageFileName = "cmfpackage.json";

        /// <summary>
        /// The package json
        /// </summary>
        public const string PackageJson = "package.json";

        /// <summary>
        /// The CMF app file name
        /// </summary>
        public const string CmfAppFileName = "cmfapp.json";

        /// <summary>
        /// The project config file name, located in the project root
        /// </summary>
        public const string ProjectConfigFileName = ".project-config.json";

        /// <summary>
        /// The repositories config file name, usually located in the project root
        /// </summary>
        public const string RepositoriesConfigFileName = "repositories.json";

        /// <summary>
        /// The package json default manifest Version
        /// </summary>
        public const int ManifestVersion = 1;

        #endregion

        #region Generic

        /// <summary>
        /// The root package default keyword
        /// </summary>
        public const string RootPackageDefaultKeyword = "cmf-root-package";

        /// <summary>
        /// npm.js repository url
        /// </summary>
        public const string NpmJsUrl = "https://registry.npmjs.com";

        /// <summary>
        /// URL for downloading Grafana plugin artifacts by exact version.
        /// </summary>
        public const string GrafanaPluginDownloadUrl = "https://grafana.com/api/plugins";

        /// <summary>
        /// Default folder where Grafana plugin preparation artifacts are stored during restore.
        /// </summary>
        public const string GrafanaPluginsStateFolder = ".cmf-grafana-plugins";

        /// <summary>
        /// Logical plugin payload folder, before the Grafana handler rebases it to the deployment path.
        /// </summary>
        public const string GrafanaPluginsPackageFolder = "plugins";

        /// <summary>
        /// Default plugin directory used by the CMF Grafana image.
        /// </summary>
        public const string DefaultGrafanaPluginsTargetPath = "/data/grafana/plugins";

        /// <summary>
        /// Name of the plugin preparation metadata file.
        /// </summary>
        public const string GrafanaPluginPreparationMetadataFile = "cmf-grafana-plugin.json";

        #endregion

        #region Tokens

        /// <summary>
        /// The token start element
        /// </summary>
        public const string TokenStartElement = "$(";

        /// <summary>
        /// The token end element
        /// </summary>
        public const string TokenEndElement = ")";
        
        #endregion
    }
}

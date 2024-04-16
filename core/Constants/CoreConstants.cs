namespace Cmf.CLI.Core.Constants
{
    /// <summary>
    /// </summary>
    public static class CoreConstants
    {
        #region Folders

        /// <summary>
        ///     The folder install dependencies
        /// </summary>
        public const string FolderInstallDependencies = "installDependencies";

        #endregion Folders

        #region Files

        /// <summary>
        ///     The deployment framework manifest template
        /// </summary>
        public const string DeploymentFrameworkManifestFileName = "manifest.xml";

        /// <summary>
        ///     The CMF package file name
        /// </summary>
        public const string CmfPackageFileName = "cmfpackage.json";

        /// <summary>
        ///     The package json
        /// </summary>
        public const string PackageJson = "package.json";

        /// <summary>
        ///     The project config file name, located in the project root
        /// </summary>
        public const string ProjectConfigFileName = ".project-config.json";

        /// <summary>
        ///     The repositories config file name, usually located in the project root
        /// </summary>
        public const string RepositoriesConfigFileName = "repositories.json";

        #endregion Files

        #region Generic

        /// <summary>
        ///     The root package default keyword
        /// </summary>
        public const string RootPackageDefaultKeyword = "cmf-root-package";

        /// <summary>
        ///     npm.js repository url
        /// </summary>
        public const string NpmJsUrl = "https://registry.npmjs.com";

        /// <summary>
        ///     Tfs server url
        /// </summary>
        public const string TfsServerUrl = "https://tfs.criticalmanufacturing.com/ImplementationProjects";

        /// <summary>
        ///     Local DB user name
        /// </summary>
        public const string LocalDbUser = "MESUser";

        /// <summary>
        ///     Local DB password
        /// </summary>
        public const string LocalDbPassword = "localEnvPassword";

        #endregion Generic

        #region Tokens

        /// <summary>
        ///     The token start element
        /// </summary>
        public const string TokenStartElement = "$(";

        /// <summary>
        ///     The token end element
        /// </summary>
        public const string TokenEndElement = ")";

        #endregion Tokens
    }
}
using System.IO;
using System.Reflection;

namespace Cmf.Common.Cli.Utilities
{
    /// <summary>
    ///
    /// </summary>
    public static class GenericUtilities
    {
        #region Public Methods

        /// <summary>
        /// Read Embedded Resource file content and return it.
        /// e.g. GetEmbeddedResourceContent("BuildScrips/cleanNodeModules.ps1")
        /// NOTE: Don't forget to set the BuildAction for your resource as EmbeddedResource. Resources must be in the [root]/resources folder
        /// </summary>
        /// <param name="resourceName">the path of the embedded resource inside the [root}/resources folder</param>
        /// <returns>
        /// the resource content
        /// </returns>
        public static string GetEmbeddedResourceContent(string resourceName)
        {
            var resourceId = $"Cmf.Common.Cli.resources.{resourceName.Replace("/", ".")}";

            var assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(resourceId);
            using StreamReader reader = new(stream);
            string result = reader.ReadToEnd();
            return result;
        }

        /// <summary>
        /// Will create a new version based on the old and new inputs
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <param name="version"></param>
        /// <param name="buildNr"></param>
        /// <returns>
        /// the new version
        /// </returns>
        public static string RetrieveNewVersion(string currentVersion, string version, string buildNr)
        {
            if (!string.IsNullOrEmpty(version))
            {
                currentVersion = version;
            }
            if (!string.IsNullOrEmpty(buildNr))
            {
                currentVersion += "-" + buildNr;
            }

            return currentVersion;
        }

        /// <summary>
        /// Will create a new version based on the old and new inputs
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <param name="version"></param>
        /// <param name="buildNr"></param>
        /// <returns>
        /// the new version
        /// </returns>
        public static string RetrieveNewPresentationVersion(string currentVersion, string version, string buildNr)
        {
            GenericUtilities.GetCurrentPresentationVersion(currentVersion, out string originalVersion, out string originalBuildNumber);

            string newVersion = !string.IsNullOrEmpty(version) ? version : originalVersion;
            if (!string.IsNullOrEmpty(buildNr))
            {
                newVersion += "-" + buildNr;
            }

            return newVersion;
        }

        /// <summary>
        /// Get current version based on string, for 
        /// the format 1.0.0-1234
        /// where 1.0.0 will be the version
        /// and the 1234 will be the build number
        /// </summary>
        /// <param name="source">Source information to be parsed</param>
        /// <param name="version">Version Number</param>
        /// <param name="buildNr">Build Number</param>
        public static void GetCurrentPresentationVersion(string source, out string version, out string buildNr)
        {
            version = string.Empty;
            buildNr = string.Empty;

            if (!string.IsNullOrWhiteSpace(source))
            {
                string[] sourceInfo = source.Split('-');
                version = sourceInfo[0];
                if (sourceInfo.Length > 1)
                {
                    buildNr = sourceInfo[1];
                }
            }
        }

        #endregion Public Methods
    }
}
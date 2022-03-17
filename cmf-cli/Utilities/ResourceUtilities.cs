using System.IO;
using System.Reflection;

namespace Cmf.CLI.Utilities
{
    public static class ResourceUtilities
    {
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
            var resourceId = $"Cmf.CLI.resources.{resourceName.Replace("/", ".")}";

            var assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(resourceId);
            using StreamReader reader = new(stream);
            string result = reader.ReadToEnd();
            return result;
        }
    }
}
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Cmf.Common.Cli.Constants;

namespace Cmf.Common.Cli.Objects
{
    /// <summary>
    /// The NPM Registry client interface
    /// </summary>
    public interface INPMClient
    {
        /// <summary>
        /// gets the latest version of the CLI from the NPM registry
        /// </summary>
        /// <param name="preRelease">get the pre-release latest version</param>
        /// <returns>a version identifier</returns>
        Task<string> GetLatestVersion(bool preRelease = false);
    }

    /// <summary>
    /// A live implementation of the NPM Registry client
    /// </summary>
    public class NPMClient : INPMClient
    {
        /// <summary>
        /// gets the latest version of the CLI from the NPM registry
        /// </summary>
        /// <param name="preRelease">get the pre-release latest version</param>
        /// <returns>a version identifier</returns>
        public async Task<string> GetLatestVersion(bool preRelease = false)
        {
            var client = this.GetClient();
            var res = await client.GetAsync($"{CoreConstants.NpmJsUrl.TrimEnd('/')}/{ExecutionContext.PackageId}");
            var body = await res.Content.ReadFromJsonAsync<JsonElement>();
            return (body).GetProperty("dist-tags").GetProperty(preRelease ? "next" : "latest").GetString();
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient();
            // remove the scope @ as it's not a valid user agent character
            client.DefaultRequestHeaders.Add("User-Agent",
                $"{ExecutionContext.PackageId.Replace("@", "")} v{ExecutionContext.CurrentVersion}");
            return client;
        }
    }
}
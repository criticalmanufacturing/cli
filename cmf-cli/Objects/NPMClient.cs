using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

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
        private string packageId = "@criticalmanufacturing/cli";
        private string registry = "https://registry.npmjs.org/";

        /// <summary>
        /// gets the latest version of the CLI from the NPM registry
        /// </summary>
        /// <param name="preRelease">get the pre-release latest version</param>
        /// <returns>a version identifier</returns>
        public async Task<string> GetLatestVersion(bool preRelease = false)
        {
            var client = this.GetClient();
            var res = await client.GetAsync($"{registry}{packageId}");
            var body = await res.Content.ReadFromJsonAsync<JsonElement>();
            return (body).GetProperty("dist-tags").GetProperty(preRelease ? "next" : "latest").GetString();
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                $"criticalmanufacturing/cli v{ExecutionContext.CurrentVersion}");
            return client;
        }
    }
}
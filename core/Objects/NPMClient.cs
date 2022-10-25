using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Cmf.CLI.Core.Constants;

namespace Cmf.CLI.Core.Objects
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

        /// <summary>
        /// Find plugins in the optionally provided registries
        /// Uses the NPMJS registry by default
        /// </summary>
        /// <param name="registries">The registries in which to search for plugins</param>
        /// <returns>A set of plugin packages from the registries</returns>
        IPackage[] FindPlugins(Uri[] registries);
    }

    public interface IPackage
    {
        public string Name { get; set; }
        public bool IsOfficial { get; set; }
        public string Registry { get; set; }
        public Uri Link { get; set; }
    }

    class Package : IPackage
    {
        public string Name { get; set; }
        public bool IsOfficial { get; set; }
        public string Registry { get; set; }
        public Uri Link { get; set; }
    }

    /// <summary>
    /// A live implementation of the NPM Registry client
    /// </summary>
    public class NPMClient : INPMClient
    {
        private record SearchResults
        {
            internal record PackageResult
            {
                internal record SearchPackage
                {
                    internal record PackageLinks
                    {
                        public string Npm { get; set; }
                    }
                    public string Name { get; set; }
                    public string Scope { get; set; }
                    public string Version { get; set; }
                    public PackageLinks Links { get; set; }
                }

                public SearchPackage Package { get; set; }
            }
            public List<PackageResult> Objects { get; set; }
        }

        
        /// <summary>
        /// gets the latest version of the CLI from the NPM registry
        /// </summary>
        /// <param name="preRelease">get the pre-release latest version</param>
        /// <returns>a version identifier</returns>
        public async Task<string> GetLatestVersion(bool preRelease = false)
        {
            var client = this.GetClient();
            try
            {
                var res = await client.GetAsync($"{CoreConstants.NpmJsUrl.TrimEnd('/')}/{ExecutionContext.PackageId}");
                var body = await res.Content.ReadFromJsonAsync<JsonElement>();
                return (body).GetProperty("dist-tags").GetProperty(preRelease ? "next" : "latest").GetString();
            }
            catch (Exception e)
            {
                Log.Debug(e.Message);
                Log.Warning("Could not retrieve latest version information. Try again later.");
            }

            return null;
        }

        public IPackage[] FindPlugins(Uri[] registries)
        {
            var client = this.GetClient();
            try
            {
                IEnumerable<IPackage>[] results = (registries ?? new[] { new Uri(CoreConstants.NpmJsUrl) }).Select(async registry =>
                {
                    var queryUri = $"{registry.AbsoluteUri.TrimEnd('/')}/-/v1/search?text=+keywords:cmf-cli-plugin";
                    Log.Debug($"Querying {queryUri} for packages with keyword 'cmf-cli-plugin'...");
                    
                        var res = await client.GetAsync(
                            queryUri);
                        Log.Debug($"Got response HTTP code {res.StatusCode}");
                        if (res.StatusCode != HttpStatusCode.OK)
                        {
                            Log.Error($"Search request to {registry.AbsoluteUri} failed: {res.StatusCode}");
                            return Array.Empty<IPackage>();
                        }
                        var body = await res.Content.ReadFromJsonAsync<SearchResults>();
                        return body.Objects.Select(obj => new Package()
                        {
                            Name = obj.Package.Name,
                            IsOfficial = string.Equals(obj.Package.Scope, "criticalmanufacturing"),
                            Registry = registry.AbsoluteUri,
                            Link = new Uri(obj.Package.Links.Npm)
                        } as IPackage);

                }).Select(t => t.Result).ToArray();
                
                return results.SelectMany(res => res).ToArray();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }

            return Array.Empty<IPackage>();
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
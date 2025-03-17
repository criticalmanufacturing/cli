using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    public interface INPMClientEx : INPMClient
    {
        Task<List<string>> SearchPackages(string query);
        
        Task<CmfPackageV1> FetchPackageVersion(string packageName, string version);

        Task<IFileInfo> DownloadPackage(string packageName, string version, IFileInfo output);

        Task PublishPackage(IFileInfo package);
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
    public class NPMClient : INPMClientEx
    {
        private readonly HttpClient client;
        private readonly string baseUrl;

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

        public NPMClient(string baseUrl = CoreConstants.NpmJsUrl, HttpClient client = null)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
            this.client = client ?? this.GetClient();
        }

        public NPMClient(object client) : this(client: client as HttpClient)
        {
            
        }

        
        /// <summary>
        /// gets the latest version of the CLI from the NPM registry
        /// </summary>
        /// <param name="preRelease">get the pre-release latest version</param>
        /// <returns>a version identifier</returns>
        public async Task<string> GetLatestVersion(bool preRelease = false)
        {
            var client = this.GetClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            try
            {
                var res = await client.GetAsync($"{this.baseUrl}/{ExecutionContext.PackageId}");
                var body = await res.Content.ReadFromJsonAsync<JsonElement>();
                return (body).GetProperty("dist-tags").GetProperty(preRelease ? "next" : "latest").GetString();
            }
            catch (Exception e)
            {
                Log.Debug(e.Message);
                Log.Warning($"Could not retrieve {ExecutionContext.PackageId} latest version information. Try again later.");
            }

            return null;
        }

        public IPackage[] FindPlugins(Uri[] registries)
        {
            var client = this.GetClient();
            try
            {
                IEnumerable<IPackage>[] results = (registries ?? new[] { new Uri(this.baseUrl) }).Select(async registry =>
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
        
        public async Task<List<string>> SearchPackages(string query)
        {
            var client = this.GetClient();
            var url = $"{this.baseUrl}/-/v1/search?text={query}";
            var res = await client.GetAsync(url);
            Log.Debug($"Got response HTTP code {res.StatusCode}");
            if (res.StatusCode != HttpStatusCode.OK)
            {
                Log.Error($"Search request to {CoreConstants.NpmJsUrl} failed: {res.StatusCode}");
                return [];
            }
            var searchResult = await res.Content.ReadFromJsonAsync<SearchResults>();

            return searchResult.Objects.Select(package => package.Package.Name).ToList();
        }
        
        public async Task<NpmPackageVersion> FetchPackageInfo(string packageName, string version)
        {
            var client = this.GetClient();
            var url = $"{this.baseUrl}/{packageName}";
            var res = await client.GetAsync(url);
            Log.Debug($"Got response HTTP code {res.StatusCode}");
            if (res.StatusCode != HttpStatusCode.OK)
            {
                Log.Error($"Could not get package {packageName}@{version} from {this.baseUrl}: {res.StatusCode}");
                return null;
            }
            var body = await res.Content.ReadAsStringAsync();
            
            var packageVersion = JsonConvert.DeserializeObject<NpmPackageInfo>(body);

            return !packageVersion.Versions.ContainsKey(version) ? null : packageVersion.Versions[version];
        }
        
        public async Task<CmfPackageV1> FetchPackageVersion(string packageName, string version)
        {
            var client = this.GetClient();
            var url = $"{this.baseUrl}/{packageName}";
            var res = await client.GetAsync(url);
            Log.Debug($"Got response HTTP code {res.StatusCode}");
            if (res.StatusCode != HttpStatusCode.OK)
            {
                Log.Debug($"Could not get package {packageName}@{version} from {this.baseUrl}: {res.StatusCode}");
                return null;
            }
            var body = await res.Content.ReadAsStringAsync();
            
            // Parse the JSON string into a JObject
            var bodyJson = JObject.Parse(body);

            // Extract the 'address' property as a JObject
            var versions = (JObject)bodyJson["versions"];

            if (!versions.ContainsKey(version))
            {
                return null;
            }
            
            // Convert the address JObject back into a string
            string versionManifest = versions[version].ToString();

            var ctrlr = new CmfPackageController(CmfPackageController.FromJson(versionManifest), null);

            return ctrlr.CmfPackage;
        }

        public async Task<IFileInfo> DownloadPackage(string packageName, string version, IFileInfo output)
        {
            var client = this.GetClient();
            
            var pkg = await this.FetchPackageInfo(packageName, version);
            if (pkg == null)
            {
                return null;
            }
            
            // Get the HTTP response as a stream
            // Open a FileStream for writing the downloaded file
            if (!output.Exists)
            {
                await using var stream = output.Create();
            }
            await using var fileStream = output.OpenWrite();
            Log.Warning($"Downloading package {pkg.Dist.Tarball}");
            // Get the HTTP response as a stream
            using var response = await client.GetAsync(pkg.Dist.Tarball, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode(); // Throw if not successful
            Log.Debug("Download got a successful code, saving to temp file");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            // Open the content stream from the HTTP response
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            Log.Debug("Got response");
            // Stream the content directly into the file
            await contentStream.CopyToAsync(fileStream);
            Log.Debug($"Saving to file {output.FullName} finished, took {stopwatch.ElapsedMilliseconds}ms");
            return output;
        }

        public async Task PublishPackage(IFileInfo package)
        {
            var toLowerCase = true; // TODO: complete implementation
           
            var ctrlr = new CmfPackageController(package, package.FileSystem);
            
            var manifest = ctrlr.ToJson(toLowerCase);

            var name = toLowerCase ? ctrlr.CmfPackage.PackageId.ToLowerInvariant() : ctrlr.CmfPackage.PackageId;
            var tgz = $"{name}-{ctrlr.CmfPackage.Version}.tgz";
            Log.Debug($"Trying to publish {name} to {this.baseUrl}");
            JObject root = null;
            
            try
            {
                Log.Debug("Load package content...");
                using var fileStream = package.OpenRead();
                using var memoryStream = new MemoryStream();
                
                fileStream.CopyTo(memoryStream);  // this can lead to large memory consumption
                byte[] fileBytes = memoryStream.ToArray();

                string dataBase64 = Convert.ToBase64String(fileBytes);

                using SHA1 sha1 = SHA1.Create();
                byte[] sha1HashBytes = sha1.ComputeHash(fileBytes);
                var sha1Hash = BitConverter.ToString(sha1HashBytes).Replace("-", "").ToLower();

                using SHA512 sha512 = SHA512.Create();
                byte[] sha512HashBytes = sha512.ComputeHash(fileBytes);
                var sha512_64 = Convert.ToBase64String(sha512HashBytes);
                // var sha512Hash = BitConverter.ToString(sha512HashBytes).Replace("-", "").ToLower();
                var sha512Hash = $"sha512-{sha512_64}";
                Log.Debug($"Package content with hash {sha512Hash} and checksum {sha1Hash}");
                
                root = JObject.Parse(
                    $$"""
                    { 
                        "_id": "{{name}}",
                        "name": "{{name}}",
                        "description": "{{ctrlr.CmfPackage.Description}}",
                        "dist-tags": { "latest": "{{ctrlr.CmfPackage.Version}}" },
                        "versions": {
                            "{{ctrlr.CmfPackage.Version}}" : {{manifest}}
                        },
                        "access": null,
                        "_attachments": {
                          "{{tgz}}": {
                            "content_type": "application/octet-stream",
                            "data": "{{dataBase64}}",
                            "length": {{package.Length}}
                          }
                        }
                    }
                    """);
                // patch version manifest
                root["versions"][ctrlr.CmfPackage.Version]["_id"] = $"{name}@{ctrlr.CmfPackage.Version}";
                root["versions"][ctrlr.CmfPackage.Version]["_cliVersion"] = ExecutionContext.CurrentVersion;
                root["versions"][ctrlr.CmfPackage.Version]["_integrity"] = sha512Hash;
                root["versions"][ctrlr.CmfPackage.Version]["dist"] = JObject.Parse($$"""
                      {
                        "integrity": "{{sha512Hash}}",
                        "shasum": "{{sha1Hash}}",
                        "tarball": "{{this.baseUrl.Replace("https://", "http://")}}/{{name}}/-/{{tgz}}"
                      }
                      """);
            }
            catch (Exception ex)
            {
                throw new CliException($"Could not publish package {ctrlr.CmfPackage.PackageAtRef}!", ex);
            }
            
            var payload = root.ToString(Formatting.None);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            // var content = JsonContent.Create(root);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            Log.Debug($"PUTing package to {this.baseUrl}...");
            try
            {
                var httpClient = this.GetClient();
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                var response = await httpClient.PutAsync($"{this.baseUrl}/{name}", content);
                if (!response.IsSuccessStatusCode)
                {
                    Log.Debug($"Failed to publish the package: {(int)response.StatusCode} {response.ReasonPhrase}");
                    Log.Warning(await response.Content.ReadAsStringAsync());
                    throw new CliException(
                        $"{(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                Log.Debug($"Failed to publish the package: {e.Message}{Environment.NewLine}{e.StackTrace}");
                throw new CliException(
                    $"Failed to publish package: {e.Message}");
            }
            
        }

        private HttpClient GetClient()
        {
            if (this.client != null)
            {
                return this.client;
            }
            var client = new HttpClient();

            // handle authentication
            char[] strip = ['/', '.', '-'];
            var uri = new Uri(this.baseUrl, UriKind.Absolute);
            var envvarPrefix = new string($"{uri.Host}{uri.PathAndQuery.TrimEnd('/')}".Select(ch => strip.Contains(ch) ? '_' : ch).ToArray());
            var type = Environment.GetEnvironmentVariable($"{envvarPrefix}__AUTH_TYPE");
            var username = Environment.GetEnvironmentVariable($"{envvarPrefix}__USERNAME");
            var token = Environment.GetEnvironmentVariable($"{envvarPrefix}__TOKEN");
            switch (type?.ToLowerInvariant())
            {
                case "bearer":
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    break;
                case "basic":
                    var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{token}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                    break;
            }
            
            if (this.baseUrl != CoreConstants.NpmJsUrl)
            {
                // remove the scope @ as it's not a valid user agent character
                client.DefaultRequestHeaders.Add("User-Agent",
                    $"{ExecutionContext.PackageId.Replace("@", "")} v{ExecutionContext.CurrentVersion}");
            }
            return client;
        }
    }

    public class NpmPackageInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> DistTags { get; set; }
        public Dictionary<string, NpmPackageVersion> Versions { get; set; }
    }
    
    public class NpmPackageVersion
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public NpmPackageDist Dist { get; set; }
    }

    public class NpmPackageDist
    {
        public string ShaSum { get; set; }
        public string Tarball { get; set; }
    }
}
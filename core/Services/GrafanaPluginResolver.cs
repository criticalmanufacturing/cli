using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cmf.CLI.Core.Services;

/// <summary>
/// Acquires and verifies plugin archives during explicit restore. Prepared reads and packing
/// use local state only, so a missing or stale archive cannot trigger an implicit download.
/// </summary>
public class GrafanaPluginResolver
{
    // Bound both the downloaded bytes held in memory and the work needed to inspect a ZIP.
    // Expansion and entry-count limits also apply to local archives, which are untrusted inputs.
    private const int MaxArchiveBytes = 256 * 1024 * 1024;
    private const long MaxExpandedBytes = 1024L * 1024 * 1024;
    // Redirects are followed explicitly so every destination gets the same URL safety checks.
    private static readonly HttpClient DefaultClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false
    }) { Timeout = TimeSpan.FromMinutes(2) };
    private readonly HttpClient httpClient;

    /// <summary>Uses the shared client unless a caller supplies one for testing.</summary>
    /// <remarks>Injected clients must disable automatic redirects; each redirect is validated here.</remarks>
    public GrafanaPluginResolver(HttpClient httpClient = null) => this.httpClient = httpClient ?? DefaultClient;

    private static string Digest(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string SourceIdentity(CmfPackage package, GrafanaPluginRequirement requirement)
    {
        if (requirement.Source == null) return "catalog";
        if (Uri.TryCreate(requirement.Source, UriKind.Absolute, out var uri) && uri.Scheme == "https")
            return requirement.Source;
        return package.FileSystem.Path.GetFullPath(package.FileSystem.Path.Combine(
            package.GetFileInfo().DirectoryName, requirement.Source));
    }

    // Bind preparation to every declared input without storing a potentially signed source URL.
    // The archive digest is separate: it detects changed cached bytes, not changed requirements.
    private static string Fingerprint(CmfPackage package, GrafanaPluginRequirement requirement) => Digest(
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
        {
            requirement.Id, requirement.Version, requirement.Platform,
            Sha256 = requirement.Sha256?.ToLowerInvariant(), Source = SourceIdentity(package, requirement)
        })));

    // Keep plugin state outside the ordinary dependencies directory, which CMF restore may clean.
    // The Grafana handler excludes this working directory from normal package content.
    private static string StateDirectory(CmfPackage package, GrafanaPluginRequirement requirement) =>
        package.FileSystem.Path.Combine(package.GetFileInfo().DirectoryName,
            CoreConstants.GrafanaPluginsStateFolder, requirement.Id);

    /// <summary>
    /// Reuses matching prepared archives or acquires and verifies replacements from the declared source.
    /// </summary>
    public async Task RestoreAsync(CmfPackage package, CancellationToken cancellationToken = default)
    {
        package.ValidateGrafanaPlugins();
        foreach (var requirement in package.GrafanaPlugins ?? new())
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Valid preparation is sufficient even if the original local archive is no longer present.
            // Only restore repairs invalid state; ReadPrepared itself stays read-only.
            try
            {
                ReadPrepared(package, requirement);
                continue;
            }
            catch (CliException) { /* Missing/stale state is repaired only during explicit restore. */ }

            byte[] bytes;
            string source = SourceIdentity(package, requirement);
            if (requirement.Source != null && !(Uri.TryCreate(source, UriKind.Absolute, out var uri) && uri.Scheme == "https"))
            {
                if (!package.FileSystem.File.Exists(source))
                    throw new CliException($"Local archive for {requirement.Id}@{requirement.Version} is missing. Check source relative to cmfpackage.json.");
                using var input = package.FileSystem.File.OpenRead(source);
                bytes = await ReadBoundedAsync(input, MaxArchiveBytes, cancellationToken);
            }
            else
            {
                string catalogChecksum = null;
                if (requirement.Source == null)
                {
                    // Select the deployment platform, never the build machine's OS/architecture.
                    // Catalog "any" artifacts are platform-independent; explicit sources bypass the catalog.
                    string versionUrl = $"{CoreConstants.GrafanaPluginDownloadUrl}/{requirement.Id}/versions/{Uri.EscapeDataString(requirement.Version)}";
                    var metadata = JObject.Parse(Encoding.UTF8.GetString(await DownloadAsync(new Uri(versionUrl), requirement, 4 * 1024 * 1024, cancellationToken)));
                    if ((string)metadata["version"] != requirement.Version)
                        throw new CliException("Catalog returned a different plugin version.");
                    var architectures = metadata["packages"] as JObject;
                    var artifact = architectures?[requirement.Platform ?? "any"] ?? architectures?["any"];
                    if (artifact == null)
                        throw new CliException($"No catalog artifact for {requirement.Id}@{requirement.Version}. Declare a supported deployment platform explicitly.");
                    catalogChecksum = (string)artifact["sha256"];
                    string downloadUrl = (string)artifact["downloadUrl"];
                    if (string.IsNullOrWhiteSpace(downloadUrl))
                        throw new CliException("Catalog artifact is missing its download URL.");
                    source = new Uri(new Uri("https://grafana.com"), downloadUrl).AbsoluteUri;
                }
                bytes = await DownloadAsync(new Uri(source), requirement, MaxArchiveBytes, cancellationToken);
                if (catalogChecksum != null && !string.Equals(Digest(bytes), catalogChecksum, StringComparison.OrdinalIgnoreCase))
                    throw new CliException($"Catalog checksum mismatch for {requirement.Id}@{requirement.Version}.");
            }

            Verify(bytes, requirement);
            var fs = package.FileSystem;
            string directory = StateDirectory(package, requirement);
            RejectReparsePoints(fs, directory);
            fs.Directory.CreateDirectory(directory);
            string archivePath = fs.Path.Combine(directory, "plugin.zip");
            string metadataPath = fs.Path.Combine(directory, CoreConstants.GrafanaPluginPreparationMetadataFile);
            RejectReparsePoints(fs, archivePath);
            RejectReparsePoints(fs, metadataPath);
            // Write metadata last; ReadPrepared checks its fingerprint and digest against the archive.
            // These are not atomic writes: incomplete or mismatched pairs are rejected on the next read.
            fs.File.WriteAllBytes(archivePath, bytes);
            fs.File.WriteAllText(metadataPath, JsonConvert.SerializeObject(new
            {
                Format = 1, requirement.Id, requirement.Version, requirement.Platform,
                Fingerprint = Fingerprint(package, requirement), ArchiveSha256 = Digest(bytes)
            }, Formatting.Indented));
        }
    }

    /// <summary>
    /// Reads matching local preparation without downloading or repairing anything, including during dry runs.
    /// </summary>
    /// <remarks>Preparation metadata is evidence of resolved inputs, not a replacement for the manifest.</remarks>
    public PreparedGrafanaPlugin ReadPrepared(CmfPackage package, GrafanaPluginRequirement requirement)
    {
        package.ValidateGrafanaPlugins();
        try
        {
            var fs = package.FileSystem;
            string directory = StateDirectory(package, requirement);
            string archivePath = fs.Path.Combine(directory, "plugin.zip");
            string metadataPath = fs.Path.Combine(directory, CoreConstants.GrafanaPluginPreparationMetadataFile);
            RejectReparsePoints(fs, archivePath);
            RejectReparsePoints(fs, metadataPath);
            if (!fs.File.Exists(archivePath) || !fs.File.Exists(metadataPath)) throw new InvalidDataException();
            if (fs.FileInfo.New(metadataPath).Length > 16384 || fs.FileInfo.New(archivePath).Length > MaxArchiveBytes)
                throw new InvalidDataException();
            var metadata = JObject.Parse(fs.File.ReadAllText(metadataPath));
            if ((int?)metadata["Format"] != 1 || (string)metadata["Fingerprint"] != Fingerprint(package, requirement))
                throw new InvalidDataException();
            byte[] bytes = fs.File.ReadAllBytes(archivePath);
            if ((string)metadata["ArchiveSha256"] != Digest(bytes)) throw new InvalidDataException();
            // Recheck archive rules even when the metadata matches; the cache is not a trust boundary.
            // Retain the checked bytes so packing uses this snapshot rather than reopening the cache file.
            return Verify(bytes, requirement);
        }
        catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is JsonException || ex is CliException || ex is ArgumentException || ex is FormatException || ex is InvalidCastException)
        {
            throw new CliException($"Plugin {requirement.Id}@{requirement.Version} is not prepared or its inputs are stale/corrupt. Run cmf restore on the Grafana package before packing.");
        }
    }

    private async Task<byte[]> DownloadAsync(Uri uri, GrafanaPluginRequirement requirement, int limit, CancellationToken cancellationToken)
    {
        // One deadline covers all redirect hops and body reads, not just receiving response headers.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(2));
        try
        {
            for (int redirects = 0; redirects <= 5; redirects++)
            {
                if (uri.Scheme != "https" || uri.UserInfo.Length != 0 || uri.Fragment.Length != 0)
                    throw new CliException("Plugin downloads and redirects must use HTTPS without URL credentials or fragments.");
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                if (requirement.Platform != null)
                {
                    var platform = requirement.Platform.Split('-');
                    request.Headers.Add("grafana-os", platform[0]);
                    request.Headers.Add("grafana-arch", platform[1]);
                }
                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
                {
                    if (response.Headers.Location == null) break;
                    uri = new Uri(uri, response.Headers.Location);
                    continue;
                }
                if (!response.IsSuccessStatusCode)
                    throw new CliException($"Plugin source returned HTTP {(int)response.StatusCode} for {requirement.Id}@{requirement.Version}. Check source access and version; no fallback was attempted.");
                if (response.Content.Headers.ContentLength > limit) throw new CliException("Plugin download exceeds the size limit.");
                using var body = await response.Content.ReadAsStreamAsync(timeout.Token);
                return await ReadBoundedAsync(body, limit, timeout.Token);
            }
            throw new CliException("Plugin source exceeded the redirect limit or returned an invalid redirect.");
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException || ex is IOException)
        {
            // HTTP exception messages can contain signed URLs. Do not expose them or their inner exception.
            cancellationToken.ThrowIfCancellationRequested();
            throw new CliException($"Could not download {requirement.Id}@{requirement.Version}: connection failed or timed out. Check the declared source and access.");
        }
    }

    private static async Task<byte[]> ReadBoundedAsync(Stream input, int limit, CancellationToken cancellationToken)
    {
        using var output = new MemoryStream();
        var buffer = new byte[81920];
        int count;
        // Content-Length can be absent or inaccurate; enforce the limit against bytes actually read.
        while ((count = await input.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
        {
            if (output.Length + count > limit) throw new CliException("Plugin archive exceeds the size limit.");
            output.Write(buffer, 0, count);
        }
        return output.ToArray();
    }

    internal static void RejectReparsePoints(IFileSystem fs, string path)
    {
        // Inspect existing ancestors too: an ordinary file can still sit beneath a linked directory.
        for (string current = fs.Path.GetFullPath(path); !string.IsNullOrEmpty(current); current = fs.Path.GetDirectoryName(current))
            if ((fs.File.Exists(current) || fs.Directory.Exists(current)) && (fs.File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new CliException("Plugin preparation cannot use symlinks or reparse points.");
    }

    /// <summary>
    /// Rejects ZIP paths that escape their root or have unsafe/ambiguous meanings on Windows or Unix.
    /// </summary>
    public static void ValidatePath(string path)
    {
        var parts = path.TrimEnd('/').Split('/');
        if (parts.Any(part => string.IsNullOrEmpty(part) || part is "." or ".." || part.EndsWith('.') || part.EndsWith(' ') ||
            part.Any(c => char.IsControl(c) || "\\:*?\"<>|".Contains(c)) ||
            Regex.IsMatch(part, @"\A(?:CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|\z)", RegexOptions.IgnoreCase)))
            throw new CliException("Unsafe path in plugin archive.");
    }

    /// <summary>
    /// Checks archive structure, requested identity/checksum, and declared backend executable presence.
    /// </summary>
    /// <remarks>This does not authenticate Grafana signatures or execute binaries; Grafana verifies signatures at runtime.</remarks>
    internal static PreparedGrafanaPlugin Verify(byte[] bytes, GrafanaPluginRequirement requirement)
    {
        try
        {
            if (bytes.Length > MaxArchiveBytes) throw new CliException("Plugin archive exceeds 256 MiB.");
            if (requirement.Sha256 != null && !string.Equals(requirement.Sha256, Digest(bytes), StringComparison.OrdinalIgnoreCase))
                throw new CliException($"Archive checksum mismatch for {requirement.Id}.");
            using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
            if (zip.Entries.Count > 10000) throw new CliException("Plugin archive exceeds 10000 entries.");
            // A case-sensitive build host must not accept paths that collide during Windows extraction.
            var paths = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            long total = 0;
            foreach (var entry in zip.Entries)
            {
                ValidatePath(entry.FullName);
                bool directory = entry.FullName.EndsWith('/');
                // The upper attribute word carries Unix file types; the lower word carries DOS flags.
                // Accept missing Unix metadata, but reject links and devices rather than materializing them.
                int type = (entry.ExternalAttributes >> 16) & 0xf000;
                if ((type != 0 && type != (directory ? 0x4000 : 0x8000)) || (entry.ExternalAttributes & 0x400) != 0)
                    throw new CliException("Plugin archive contains a symlink or unsupported special entry.");
                if (!paths.TryAdd(entry.FullName.TrimEnd('/'), directory)) throw new CliException("Conflicting paths in plugin archive.");
                if (entry.Length > MaxArchiveBytes || (total += entry.Length) > MaxExpandedBytes ||
                    (entry.Length > 1024 * 1024 && entry.Length > Math.Max(1, entry.CompressedLength) * 1000))
                    throw new CliException("Plugin archive exceeds expansion limits.");
                // Drain each entry under the declared size bound to detect truncated or oversized payloads.
                using var input = entry.Open();
                var buffer = new byte[81920];
                long read = 0;
                int count;
                while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    read += count;
                    if (read > entry.Length) throw new CliException("Invalid plugin archive entry size.");
                }
                if (read != entry.Length || (directory && read != 0)) throw new CliException("Truncated plugin archive entry.");
            }
            // ZIPs can omit directory entries, so duplicate-name checks alone miss file/child collisions.
            foreach (string path in paths.Keys)
            {
                int slash = path.IndexOf('/');
                while (slash >= 0)
                {
                    if (paths.TryGetValue(path[..slash], out bool directory) && !directory)
                        throw new CliException("Plugin archive file/directory collision.");
                    slash = path.IndexOf('/', slash + 1);
                }
            }
            var manifests = zip.Entries.Where(e => e.FullName == "plugin.json" || e.FullName.EndsWith("/plugin.json", StringComparison.Ordinal)).ToArray();
            // Use the shallowest manifest to identify the distribution prefix, not npm package.json.
            // Only enclosing directories may sit outside that prefix; unrelated payload roots are ambiguous.
            var rootManifest = manifests.OrderBy(e => e.FullName.Count(c => c == '/')).FirstOrDefault();
            if (rootManifest == null || rootManifest.Length > 1024 * 1024) throw new CliException("Plugin archive is missing valid plugin.json.");
            string root = rootManifest.FullName[..^"plugin.json".Length];
            if (zip.Entries.Any(e => !e.FullName.StartsWith(root, StringComparison.Ordinal) &&
                !(e.FullName.EndsWith('/') && root.StartsWith(e.FullName, StringComparison.Ordinal))))
                throw new CliException("Plugin archive must contain one distribution root; files outside it are ambiguous.");
            // Grafana loads dist instead of the enclosing directory when it exists, even if empty.
            // ZIP directory entries are optional, so child paths also establish that dist is present.
            // Keep root unchanged for packing: selecting the runtime manifest must not discard payload files.
            string runtimeRoot = zip.Entries.Any(e => e.FullName.StartsWith(root + "dist/", StringComparison.Ordinal))
                ? root + "dist/" : root;
            var runtimeManifest = zip.GetEntry(runtimeRoot + "plugin.json");
            if (runtimeManifest == null || runtimeManifest.Length > 1024 * 1024)
                throw new CliException("Plugin load directory is missing valid plugin.json.");
            using var manifestReader = new StreamReader(runtimeManifest.Open());
            var metadata = JObject.Parse(manifestReader.ReadToEnd());
            if ((string)metadata["id"] != requirement.Id || (string)metadata["info"]?["version"] != requirement.Version)
                throw new CliException($"plugin.json ID/version does not match {requirement.Id}@{requirement.Version}.");
            var executables = new HashSet<string>(StringComparer.Ordinal);
            // Inspect nested plugin backends in the runtime tree, not an unused outer wrapper's executable.
            // Record the target binaries so Windows-created upstream ZIPs can still produce executable files.
            foreach (var manifest in manifests.Where(e => e.FullName.StartsWith(runtimeRoot, StringComparison.Ordinal)))
            {
                if (manifest.Length > 1024 * 1024) throw new CliException("Plugin metadata exceeds the size limit.");
                using var reader = new StreamReader(manifest.Open());
                var plugin = JObject.Parse(reader.ReadToEnd());
                if ((bool?)plugin["backend"] != true) continue;
                string executable = (string)plugin["executable"];
                if (requirement.Platform == null) throw new CliException($"Backend plugin {requirement.Id} requires an explicit deployment platform.");
                if (string.IsNullOrEmpty(executable)) throw new CliException("Backend plugin is missing executable metadata.");
                ValidatePath(executable);
                if (executable.Contains('/')) throw new CliException("Backend executable must be a file name.");
                // Grafana appends the target OS/architecture and adds .exe only for Windows targets.
                string binary = manifest.FullName[..^"plugin.json".Length] + executable + "_" + requirement.Platform.Replace('-', '_') +
                    (requirement.Platform.StartsWith("windows-", StringComparison.Ordinal) ? ".exe" : "");
                var binaryEntry = zip.GetEntry(binary);
                if (binaryEntry == null || binaryEntry.Length == 0) throw new CliException($"Backend executable for {requirement.Platform} is missing from {requirement.Id}.");
                executables.Add(binary);
            }
            return new PreparedGrafanaPlugin(requirement.Id, bytes, root, executables);
        }
        catch (Exception ex) when (ex is InvalidDataException || ex is JsonException)
        {
            throw new CliException($"Invalid ZIP distribution for {requirement.Id}@{requirement.Version}.");
        }
    }
}

/// <summary>
/// Holds the checked upstream archive in memory and copies its payload directly into the CMF ZIP.
/// Avoiding filesystem staging preserves signed file contents and upstream Unix modes on Windows hosts.
/// </summary>
public sealed class PreparedGrafanaPlugin
{
    private readonly byte[] bytes;
    private readonly string root;
    private readonly HashSet<string> executables;
    public string Id { get; }
    internal PreparedGrafanaPlugin(string id, byte[] bytes, string root, HashSet<string> executables)
    {
        Id = id;
        this.bytes = bytes;
        this.root = root;
        this.executables = executables;
    }

    /// <summary>Package-relative payload paths for reporting and dry runs, without writing any files.</summary>
    public IEnumerable<string> Paths
    {
        get
        {
            using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
            // Materialize before disposing the archive; callers may enumerate after this getter returns.
            return zip.Entries.Where(e => e.FullName.StartsWith(root, StringComparison.Ordinal) && e.FullName.Length > root.Length)
                .Select(e => $"{CoreConstants.GrafanaPluginsPackageFolder}/{Id}/{e.FullName[root.Length..]}").ToArray();
        }
    }

    /// <summary>Copies unchanged payload bytes and records Unix file types/modes for the shared ZIP writer.</summary>
    /// <remarks>The caller reserves the plugins subtree; the shared writer applies Unix ZIP metadata after closing the archive.</remarks>
    public void WriteTo(ZipArchive output, IDictionary<string, int> unixModes)
        => WriteTo(output, unixModes, CoreConstants.GrafanaPluginsPackageFolder);

    /// <summary>Writes into a validated, package-relative plugin directory.</summary>
    public void WriteTo(ZipArchive output, IDictionary<string, int> unixModes, string pluginsDirectory)
    {
        GrafanaPluginResolver.ValidatePath(pluginsDirectory);
        using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        string prefix = $"{pluginsDirectory}/{Id}/";
        // Explicit 0755 directory entries carry permissions even when the upstream ZIP omitted parents.
        void DirectoryEntry(string path)
        {
            if (unixModes.ContainsKey(path)) return;
            output.CreateEntry(path);
            unixModes.Add(path, 0x41ed); // directory 0755
        }
        DirectoryEntry(pluginsDirectory + "/");
        DirectoryEntry(prefix);
        foreach (var entry in zip.Entries)
        {
            if (!entry.FullName.StartsWith(root, StringComparison.Ordinal) || entry.FullName.Length <= root.Length) continue;
            string path = prefix + entry.FullName[root.Length..];
            for (int slash = path.IndexOf('/'); slash >= 0; slash = path.IndexOf('/', slash + 1))
                DirectoryEntry(path[..(slash + 1)]);
            if (path.EndsWith('/')) continue;
            if (unixModes.ContainsKey(path)) throw new CliException("Plugin output path collision.");
            var target = output.CreateEntry(path);
            target.LastWriteTime = entry.LastWriteTime;
            // Strip privileged/world-writable modes. Retain upstream execute intent and
            // restore the declared backend's execute bits when the ZIP was made on Windows.
            bool executable = executables.Contains(entry.FullName) || ((entry.ExternalAttributes >> 16) & 0x49) != 0;
            unixModes.Add(path, executable ? 0x81ed : 0x81a4); // regular 0755 / 0644
            using var source = entry.Open();
            using var destination = target.Open();
            source.CopyTo(destination);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI.Commands;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Handlers;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace tests.Specs;

public class GrafanaPlugins
{
    private const string Id = "example-test-datasource";
    private const string Version = "1.2.3";

    private static byte[] Archive(bool backend = false, string extraPath = null, int extraMode = 0,
        string id = Id, string version = Version)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            void Add(string name, string value, int mode = 0)
            {
                var entry = zip.CreateEntry(name);
                entry.ExternalAttributes = mode << 16;
                using var writer = new StreamWriter(entry.Open());
                writer.Write(value);
            }
            Add("distribution/plugin.json", JsonConvert.SerializeObject(new
            {
                id, info = new { version }, backend, executable = backend ? "plugin" : null
            }));
            Add("distribution/module.js", "export const example = true;");
            Add("distribution/MANIFEST.txt", "synthetic signature bytes, not a signed production plugin");
            Add("distribution/assets/icon.svg", "<svg/>");
            Add("distribution/package.json", "{\"name\":\"must-not-rename-the-plugin\"}");
            if (backend) Add("distribution/plugin_linux_amd64", "synthetic backend", 0); // Windows ZIP, no Unix mode
            if (extraPath != null) Add(extraPath, "extra", extraMode);
        }
        return stream.ToArray();
    }

    private static byte[] ArchiveWithDist(string id = Id, string version = Version, bool backend = false)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            using var upstream = new ZipArchive(new MemoryStream(Archive(backend, id: id, version: version)), ZipArchiveMode.Read);
            foreach (var entry in upstream.Entries)
            {
                if (id == null && entry.FullName == "distribution/plugin.json") continue;
                using var input = entry.Open();
                using var output = zip.CreateEntry("distribution/dist/" + entry.FullName["distribution/".Length..]).Open();
                input.CopyTo(output);
            }
            using var writer = new StreamWriter(zip.CreateEntry("distribution/plugin.json").Open());
            // The outer identity matches the request, but its unused backend must not be required.
            writer.Write(JsonConvert.SerializeObject(new
            {
                id = Id, info = new { version = Version }, backend, executable = backend ? "unused_wrapper" : null
            }));
        }
        return stream.ToArray();
    }

    private static (MockFileSystem Fs, CmfPackage Package) Package(byte[] bytes = null, string source = "archives/plugin.zip", string platform = null)
    {
        var fs = new MockFileSystem();
        string root = fs.Path.GetFullPath("grafana-test");
        fs.AddFile(fs.Path.Combine(root, "cmfpackage.json"), new MockFileData(JsonConvert.SerializeObject(new
        {
            packageId = "Cmf.Test.Grafana", version = "1.0.0", packageType = "Grafana",
            grafanaPlugins = new[] { new { id = Id, version = Version, source, platform } }
        })));
        if (bytes != null) fs.AddFile(fs.Path.Combine(root, "archives", "plugin.zip"), new MockFileData(bytes));
        return (fs, CmfPackage.Load(fs.FileInfo.New(fs.Path.Combine(root, "cmfpackage.json")), true, fs));
    }

    private sealed class HttpStub : HttpMessageHandler
    {
        public readonly List<HttpRequestMessage> Requests = new();
        public Func<HttpRequestMessage, HttpResponseMessage> Respond { get; set; } = _ => throw new Exception("Unexpected network access");
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(Respond(request));
        }
        public GrafanaPluginResolver Resolver => new(new HttpClient(this));
    }

    [Theory]
    [InlineData("version", "latest")]
    [InlineData("version", "^1.0.0")]
    [InlineData("version", "1.x")]
    [InlineData("version", "banana")]
    [InlineData("version", "1.2")]
    [InlineData("version", null)]
    [InlineData("id", "../escape")]
    [InlineData("id", null)]
    [InlineData("sha256", "bad")]
    [InlineData("sha256", " ")]
    [InlineData("source", "http://example.test/plugin.zip")]
    [InlineData("source", "ftp://example.test/plugin.zip")]
    [InlineData("source", "https://user:password@example.test/plugin.zip")]
    [InlineData("source", "")]
    [InlineData("platform", "linux-x64")]
    public void GrafanaManifestRejectsInvalidRequirements(string field, string value)
    {
        var json = JObject.FromObject(new { packageId = "Test", version = "1.0.0", packageType = "Grafana" });
        var plugin = JObject.FromObject(new { id = Id, version = Version });
        plugin[field] = value == null ? JValue.CreateNull() : new JValue(value);
        json["grafanaPlugins"] = new JArray(plugin);
        var package = json.ToObject<CmfPackage>();
        Assert.Throws<CliException>(package.ValidateGrafanaPlugins);
    }

    [Fact]
    public void GrafanaManifestRoundTripsBothModelsAndOmitsUnusedDeclarations()
    {
        var (_, package) = Package();
        package.SaveCmfPackage();
        string json = package.FileSystem.File.ReadAllText(package.GetFileInfo().FullName);
        Assert.Equal(Id, (string)JObject.Parse(json)["grafanaPlugins"][0]["id"]);
        var alternate = JsonConvert.DeserializeObject<CmfPackageV1>(json);
        Assert.Equal(Id, alternate.GrafanaPlugins.Single().Id);
        Assert.Single(JsonConvert.DeserializeObject<CmfPackageV1>(JsonConvert.SerializeObject(alternate)).GrafanaPlugins);
        foreach (string empty in new[] { "", ",\"grafanaPlugins\":[]" })
        {
            string old = "{\"packageId\":\"Test\",\"version\":\"1.0.0\",\"packageType\":\"Grafana\"" + empty + "}";
            Assert.DoesNotContain("GrafanaPlugins", JsonConvert.SerializeObject(JsonConvert.DeserializeObject<CmfPackage>(old)));
            Assert.DoesNotContain("GrafanaPlugins", JsonConvert.SerializeObject(JsonConvert.DeserializeObject<CmfPackageV1>(old)));
        }
    }

    [Fact]
    public void GrafanaManifestRejectsDuplicatesAndNonGrafanaPackages()
    {
        var (_, package) = Package();
        package.GrafanaPlugins.Add(package.GrafanaPlugins[0]);
        Assert.Throws<CliException>(package.ValidateGrafanaPlugins);
        var wrongType = JsonConvert.DeserializeObject<CmfPackage>("{\"packageId\":\"Test\",\"version\":\"1.0.0\",\"packageType\":\"Generic\",\"grafanaPlugins\":[{\"id\":\"example-test-panel\",\"version\":\"1.0.0\"}]}");
        Assert.Throws<CliException>(wrongType.ValidateGrafanaPlugins);
        Assert.Throws<CliException>(() => Cmf.CLI.Factories.PackageTypeFactory.GetPackageTypeHandler(wrongType));
    }

    [Fact]
    public async Task GrafanaLocalRestoreUsesManifestDirectoryAndReusesVerifiedState()
    {
        var (fs, package) = Package(Archive());
        var stub = new HttpStub();
        await stub.Resolver.RestoreAsync(package);
        string source = fs.Path.Combine(package.GetFileInfo().DirectoryName, "archives", "plugin.zip");
        fs.File.Delete(source); // prepared bytes are sufficient even when original source is no longer online
        var before = fs.AllFiles.ToDictionary(p => p, p => fs.File.ReadAllBytes(p));
        await stub.Resolver.RestoreAsync(package);
        Assert.Empty(stub.Requests);
        foreach (var file in before) Assert.Equal(file.Value, fs.File.ReadAllBytes(file.Key));
        Assert.Contains($"plugins/{Id}/MANIFEST.txt", stub.Resolver.ReadPrepared(package, package.GrafanaPlugins[0]).Paths);
    }

    [Fact]
    public async Task GrafanaMissingLocalSourceDoesNotFallBackToNetwork()
    {
        var (_, package) = Package();
        var stub = new HttpStub();
        Assert.Contains("Local archive", (await Assert.ThrowsAsync<CliException>(() => stub.Resolver.RestoreAsync(package))).Message);
        Assert.Empty(stub.Requests);
    }

    [Theory]
    [InlineData("Version")]
    [InlineData("Platform")]
    [InlineData("Source")]
    [InlineData("Sha256")]
    public async Task GrafanaChangedRequirementInvalidatesPreparation(string property)
    {
        var (_, package) = Package(Archive());
        var resolver = new HttpStub().Resolver;
        await resolver.RestoreAsync(package);
        var requirement = package.GrafanaPlugins[0];
        string value = property switch { "Version" => "1.2.4", "Platform" => "linux-amd64", "Source" => "mirror.zip", _ => new string('a', 64) };
        typeof(GrafanaPluginRequirement).GetProperty(property).SetValue(requirement, value);
        Assert.Contains("cmf restore", Assert.Throws<CliException>(() => resolver.ReadPrepared(package, requirement)).Message);
    }

    [Fact]
    public async Task GrafanaCorruptedCacheFailsPackAndIsRepairedByRestore()
    {
        var (fs, package) = Package(Archive());
        var resolver = new HttpStub().Resolver;
        await resolver.RestoreAsync(package);
        string archive = fs.AllFiles.Single(p => p.Contains(".cmf-grafana-plugins") && p.EndsWith("plugin.zip"));
        fs.File.WriteAllBytes(archive, new byte[] { 1, 2, 3 });
        Assert.Throws<CliException>(() => resolver.ReadPrepared(package, package.GrafanaPlugins[0]));
        await resolver.RestoreAsync(package);
        Assert.NotEmpty(resolver.ReadPrepared(package, package.GrafanaPlugins[0]).Paths);
    }

    [Fact]
    public async Task GrafanaCatalogSelectsExplicitPlatformAndMirrorBypassesCatalog()
    {
        foreach (string source in new[] { null, "https://mirror.test/plugin.zip?secret=not-logged" })
        {
            byte[] bytes = Archive(true);
            var (_, package) = Package(source: source, platform: "linux-amd64");
            var stub = new HttpStub
            {
                Respond = request => request.RequestUri.AbsolutePath.EndsWith("/1.2.3")
                    ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"version\":\"1.2.3\",\"packages\":{\"linux-amd64\":{\"downloadUrl\":\"/api/plugins/example-test-datasource/versions/1.2.3/download?os=linux&arch=amd64\",\"sha256\":\"" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() + "\"}}}") }
                    : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }
            };
            await stub.Resolver.RestoreAsync(package);
            Assert.Equal(source == null ? 2 : 1, stub.Requests.Count);
            Assert.All(stub.Requests, request => Assert.Equal("amd64", request.Headers.GetValues("grafana-arch").Single()));
            Assert.Equal(source == null ? "grafana.com" : "mirror.test", stub.Requests[0].RequestUri.Host);
        }
    }

    [Fact]
    public async Task GrafanaCatalogRequiresPlatformForArchitectureSpecificArtifacts()
    {
        var (_, package) = Package(source: null);
        var stub = new HttpStub { Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"version\":\"1.2.3\",\"packages\":{\"linux-amd64\":{}}}") } };
        Assert.Contains("platform", (await Assert.ThrowsAsync<CliException>(() => stub.Resolver.RestoreAsync(package))).Message);
        Assert.Single(stub.Requests);
    }

    [Fact]
    public async Task GrafanaCatalogSupportsArchitectureIndependentPlugins()
    {
        var (_, package) = Package(source: null);
        var stub = new HttpStub { Respond = request => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = request.RequestUri.AbsolutePath.EndsWith("/download") ? new ByteArrayContent(Archive()) : new StringContent("{\"version\":\"1.2.3\",\"packages\":{\"any\":{\"downloadUrl\":\"/download\"}}}")
        } };
        await stub.Resolver.RestoreAsync(package);
        Assert.Equal(2, stub.Requests.Count);
        Assert.All(stub.Requests, request => Assert.False(request.Headers.Contains("grafana-os")));
    }

    [Theory]
    [InlineData("http://insecure.test/plugin.zip")]
    [InlineData("https://user:password@mirror.test/plugin.zip")]
    public async Task GrafanaRejectsUnsafeRedirects(string location)
    {
        var (_, package) = Package(source: "https://mirror.test/plugin.zip");
        var stub = new HttpStub { Respond = _ => new HttpResponseMessage(HttpStatusCode.Redirect) { Headers = { Location = new Uri(location) } } };
        await Assert.ThrowsAsync<CliException>(() => stub.Resolver.RestoreAsync(package));
        Assert.Single(stub.Requests);
    }

    [Fact]
    public async Task GrafanaMirrorFailureIsRedactedAndDoesNotFallBack()
    {
        var (_, package) = Package(source: "https://mirror.test/plugin.zip?secret=sensitive");
        var stub = new HttpStub { Respond = _ => throw new HttpRequestException("https://mirror.test/?secret=sensitive") };
        var ex = await Assert.ThrowsAsync<CliException>(() => stub.Resolver.RestoreAsync(package));
        Assert.DoesNotContain("sensitive", ex.ToString());
        Assert.Single(stub.Requests);
    }

    [Theory]
    [InlineData("../escape")]
    [InlineData("/absolute")]
    [InlineData("C:/absolute")]
    [InlineData("distribution/../escape")]
    [InlineData("distribution\\escape")]
    [InlineData("distribution/CON")]
    [InlineData("distribution/file:stream")]
    [InlineData("distribution/MODULE.js")]
    [InlineData("distribution/module.js/child")]
    [InlineData("another-root/file")]
    public void GrafanaRejectsUnsafeAndConflictingArchivePaths(string path)
    {
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(Archive(extraPath: path), new GrafanaPluginRequirement { Id = Id, Version = Version }));
    }

    [Theory]
    [InlineData(0xa1ff)] // symlink
    [InlineData(0x21a4)] // device
    [InlineData(0x11a4)] // FIFO
    public void GrafanaRejectsLinksAndSpecialEntries(int mode)
    {
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(Archive(extraPath: "distribution/link", extraMode: mode), new GrafanaPluginRequirement { Id = Id, Version = Version }));
    }

    [Fact]
    public void GrafanaRejectsWrongIdentityChecksumCorruptionAndBackendPlatform()
    {
        var requirement = new GrafanaPluginRequirement { Id = Id, Version = Version };
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(Archive(id: "different-test-panel"), requirement));
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(Archive(version: "9.9.9"), requirement));
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(new byte[] { 1, 2, 3 }, requirement));
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(Archive(true), requirement));
        requirement.Platform = "linux-arm64";
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(Archive(true), requirement));
        requirement.Platform = null;
        requirement.Sha256 = new string('a', 64);
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(Archive(), requirement));
    }

    [Theory]
    [InlineData("different-test-panel", Version)]
    [InlineData(Id, "9.9.9")]
    [InlineData(null, Version)]
    [InlineData(Id, null)]
    public async Task GrafanaRestoreRejectsMissingOrMismatchedDistManifest(string id, string version)
    {
        var (_, package) = Package(ArchiveWithDist(id, version));
        var stub = new HttpStub();
        await Assert.ThrowsAsync<CliException>(() => stub.Resolver.RestoreAsync(package));
        Assert.Empty(stub.Requests);
    }

    [Fact]
    public void GrafanaRejectsEmptyDistDirectoryInsteadOfUsingOuterManifest()
    {
        using var stream = new MemoryStream();
        stream.Write(Archive());
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Update, true))
            zip.CreateEntry("distribution/dist/");
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(stream.ToArray(),
            new GrafanaPluginRequirement { Id = Id, Version = Version }));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GrafanaDistPackPreservesCompletePayloadAndBackendModes(bool backend)
    {
        byte[] bytes = ArchiveWithDist(backend: backend);
        var (fs, package) = Package(bytes, platform: backend ? "linux-amd64" : null);
        var stub = new HttpStub();
        await stub.Resolver.RestoreAsync(package);
        new PackCommand(fs).Execute(package, fs.DirectoryInfo.New("output"), true, false);
        string output = fs.AllFiles.Single(p => p.EndsWith("Cmf.Test.Grafana.1.0.0.zip"));
        using var zip = new ZipArchive(fs.File.OpenRead(output), ZipArchiveMode.Read);
        using var upstream = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        foreach (var entry in upstream.Entries)
        {
            using var expected = new MemoryStream();
            using var actual = new MemoryStream();
            using var input = entry.Open();
            input.CopyTo(expected);
            using var packed = zip.GetEntry($"plugins/{Id}/" + entry.FullName["distribution/".Length..]).Open();
            packed.CopyTo(actual);
            Assert.Equal(expected.ToArray(), actual.ToArray());
        }
        if (backend)
            Assert.Equal(0x81ed, (zip.GetEntry($"plugins/{Id}/dist/plugin_linux_amd64").ExternalAttributes >> 16) & 0xffff);
        Assert.Empty(stub.Requests);
    }

    [Fact]
    public async Task GrafanaPreparedStateRevalidatesDistIdentity()
    {
        var (fs, package) = Package(ArchiveWithDist());
        var stub = new HttpStub();
        await stub.Resolver.RestoreAsync(package);
        string state = fs.Path.Combine(package.GetFileInfo().DirectoryName, ".cmf-grafana-plugins", Id);
        string metadataPath = fs.Path.Combine(state, "cmf-grafana-plugin.json");
        byte[] invalid = ArchiveWithDist(version: "9.9.9");
        fs.File.WriteAllBytes(fs.Path.Combine(state, "plugin.zip"), invalid);
        var metadata = JObject.Parse(fs.File.ReadAllText(metadataPath));
        // Model state accepted by the old resolver: its digest matches, but the runtime identity does not.
        metadata["ArchiveSha256"] = Convert.ToHexString(SHA256.HashData(invalid)).ToLowerInvariant();
        fs.File.WriteAllText(metadataPath, metadata.ToString());
        Assert.Throws<CliException>(() => stub.Resolver.ReadPrepared(package, package.GrafanaPlugins[0]));
        await stub.Resolver.RestoreAsync(package);
        Assert.NotEmpty(stub.Resolver.ReadPrepared(package, package.GrafanaPlugins[0]).Paths);
        Assert.Empty(stub.Requests);
    }

    [Fact]
    public async Task GrafanaPackIncludesCompletePayloadAndUnixModesWithoutNetwork()
    {
        var (fs, package) = Package(Archive(true, "distribution/tool", 0x8fff), platform: "linux-amd64");
        var stub = new HttpStub();
        await stub.Resolver.RestoreAsync(package);
        fs.AddFile(fs.Path.Combine(package.GetFileInfo().DirectoryName, ".cmf-grafana-plugins", "undeclared", "stale.txt"), "stale");
        string manifest = package.GetFileInfo().FullName;
        var json = JObject.Parse(fs.File.ReadAllText(manifest));
        json["contentToPack"] = new JArray(new JObject { ["source"] = "dashboard.json", ["target"] = "dashboards" });
        json["steps"] = new JArray(new JObject { ["type"] = "DeployFiles", ["contentPath"] = "custom/**" });
        fs.File.WriteAllText(manifest, json.ToString());
        fs.AddFile(fs.Path.Combine(package.GetFileInfo().DirectoryName, "dashboard.json"), "{}");
        package = CmfPackage.Load(fs.FileInfo.New(manifest), true, fs);
        new PackCommand(fs).Execute(package, fs.DirectoryInfo.New("output"), true, false);
        string output = fs.AllFiles.Single(p => p.EndsWith("Cmf.Test.Grafana.1.0.0.zip"));
        using var zip = new ZipArchive(fs.File.OpenRead(output), ZipArchiveMode.Read);
        Assert.NotNull(zip.GetEntry("dashboards/dashboard.json"));
        Assert.NotNull(zip.GetEntry($"plugins/{Id}/assets/icon.svg"));
        Assert.NotNull(zip.GetEntry($"plugins/{Id}/package.json"));
        using var upstream = new ZipArchive(new MemoryStream(Archive(true, "distribution/tool", 0x8fff)), ZipArchiveMode.Read);
        foreach (var entry in upstream.Entries)
        {
            using var expected = new MemoryStream();
            using var actual = new MemoryStream();
            entry.Open().CopyTo(expected);
            zip.GetEntry($"plugins/{Id}/" + entry.FullName["distribution/".Length..]).Open().CopyTo(actual);
            Assert.Equal(expected.ToArray(), actual.ToArray());
        }
        Assert.Equal(0x81ed, (zip.GetEntry($"plugins/{Id}/plugin_linux_amd64").ExternalAttributes >> 16) & 0xffff);
        Assert.Equal(0x81a4, (zip.GetEntry($"plugins/{Id}/module.js").ExternalAttributes >> 16) & 0xffff);
        Assert.Equal(0x81ed, (zip.GetEntry($"plugins/{Id}/tool").ExternalAttributes >> 16) & 0xffff);
        Assert.DoesNotContain(zip.Entries, e => e.FullName.Contains("stale") || e.FullName.Contains(".cmf-grafana"));
        using var reader = new StreamReader(zip.GetEntry("manifest.xml").Open());
        string deployment = reader.ReadToEnd();
        Assert.Contains("custom/**", deployment);
        Assert.DoesNotContain(Id, deployment);
        Assert.Empty(stub.Requests);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GrafanaDryRunNeverChangesFilesAndReportsMissingInputs(bool restore)
    {
        var (fs, package) = Package(Archive());
        var stub = new HttpStub();
        if (restore) await stub.Resolver.RestoreAsync(package);
        var before = fs.AllFiles.ToDictionary(p => p, p => fs.File.ReadAllBytes(p));
        var directories = fs.AllDirectories.OrderBy(p => p).ToArray();
        var handler = new GrafanaPackageTypeHandler(package, stub.Resolver);
        handler.Pack(fs.DirectoryInfo.New("staging"), fs.DirectoryInfo.New("output"), true);
        Assert.Equal(before.Keys.OrderBy(p => p), fs.AllFiles.OrderBy(p => p));
        Assert.Equal(directories, fs.AllDirectories.OrderBy(p => p));
        foreach (var file in before) Assert.Equal(file.Value, fs.File.ReadAllBytes(file.Key));
        Assert.Empty(stub.Requests);
        if (!restore) Assert.Throws<CliException>(() => handler.Pack(fs.DirectoryInfo.New("staging"), fs.DirectoryInfo.New("output")));
    }

    [Fact]
    public async Task GrafanaContentCannotCollideWithPluginPaths()
    {
        var (fs, package) = Package(Archive());
        var resolver = new HttpStub().Resolver;
        await resolver.RestoreAsync(package);
        var json = JObject.Parse(fs.File.ReadAllText(package.GetFileInfo().FullName));
        json["contentToPack"] = new JArray(new JObject { ["source"] = "file.js", ["target"] = "plugins" });
        fs.File.WriteAllText(package.GetFileInfo().FullName, json.ToString());
        fs.AddFile(fs.Path.Combine(package.GetFileInfo().DirectoryName, "file.js"), "collision");
        package = CmfPackage.Load(package.GetFileInfo(), fileSystem: fs);
        Assert.Throws<CliException>(() => new GrafanaPackageTypeHandler(package, resolver).Pack(fs.DirectoryInfo.New("stage"), fs.DirectoryInfo.New("output"), true));
    }

    [LinuxFact]
    public void GrafanaPreparationRejectsFilesystemSymlinks()
    {
        string root = Path.Combine(Path.GetTempPath(), "grafana-links-test-" + Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(root, "outside"));
        try
        {
            Directory.CreateSymbolicLink(Path.Combine(root, "link"), Path.Combine(root, "outside"));
            Assert.Throws<CliException>(() => GrafanaPluginResolver.RejectReparsePoints(new FileSystem(), Path.Combine(root, "link", "plugin.zip")));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task GrafanaRestoreHandlerPreservesOrdinaryCmfDependencies()
    {
        var (fs, package) = Package(Archive());
        string root = package.GetFileInfo().DirectoryName;
        var manifest = JObject.Parse(fs.File.ReadAllText(package.GetFileInfo().FullName));
        manifest["dependencies"] = new JArray(new JObject { ["id"] = "Cmf.Test.Business", ["version"] = "1.0.0" });
        fs.File.WriteAllText(package.GetFileInfo().FullName, manifest.ToString());
        fs.AddFile(fs.Path.Combine(root, "repo", "Cmf.Test.Business.1.0.0.zip"), new MockFileData(new tests.Objects.DFPackageBuilder()
            .CreateEntry("manifest.xml", "<deploymentPackage><packageId>Cmf.Test.Business</packageId><version>1.0.0</version></deploymentPackage>")
            .CreateEntry("content.txt", "ordinary dependency").ToByteArray()));
        Cmf.CLI.Core.Objects.ExecutionContext.Initialize(fs);
        package = CmfPackage.Load(package.GetFileInfo(), fileSystem: fs);
        var stub = new HttpStub();
        var handler = new GrafanaPackageTypeHandler(package, stub.Resolver);
        handler.RestoreDependencies(new[] { new Uri(fs.Path.Combine(root, "repo") + "/") });
        Assert.Contains(fs.AllFiles, p => p.EndsWith("content.txt"));
        Assert.Single(package.Dependencies);
        Assert.NotEmpty(stub.Resolver.ReadPrepared(package, package.GrafanaPlugins[0]).Paths);
        await stub.Resolver.RestoreAsync(package);
        Assert.Empty(stub.Requests);
    }

    [Fact]
    public void GrafanaBuildRestoresPluginsBeforeBuildSteps()
    {
        var (_, package) = Package(Archive());
        var handler = new GrafanaPackageTypeHandler(package);

        handler.Build(false);

        Assert.NotEmpty(new GrafanaPluginResolver().ReadPrepared(package, package.GrafanaPlugins[0]).Paths);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GrafanaRelatedPackageDryRunDoesNotCreateDirectories(bool prePack)
    {
        var (fs, package) = Package(Archive());
        var resolver = new HttpStub().Resolver;
        await resolver.RestoreAsync(package);
        var parent = new GrafanaPackageTypeHandler(package, resolver);
        var related = new RelatedPackage { CmfPackage = package, PrePack = prePack, PostPack = !prePack };
        parent.RelatedPackagesHandlers.Add(related, new GrafanaPackageTypeHandler(package, resolver));
        var before = fs.AllDirectories.OrderBy(p => p).ToArray();
        parent.Pack(fs.DirectoryInfo.New("staging"), fs.DirectoryInfo.New("output"), true);
        Assert.Equal(before, fs.AllDirectories.OrderBy(p => p));
    }

    [Fact]
    public async Task GrafanaRestoreHonorsCancellation()
    {
        var (_, package) = Package(source: "https://mirror.test/plugin.zip");
        var stub = new HttpStub();
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stub.Resolver.RestoreAsync(package, cancelled.Token));
        Assert.Empty(stub.Requests);
    }

    [Fact]
    public void GrafanaRejectsArchiveExpansionAndEntryCountLimits()
    {
        byte[] Create(bool manyEntries)
        {
            using var bytes = new MemoryStream();
            using (var zip = new ZipArchive(bytes, ZipArchiveMode.Create, true))
            {
                if (manyEntries)
                    for (int i = 0; i <= 10000; i++) zip.CreateEntry($"file{i}");
                else
                {
                    using var output = zip.CreateEntry("large").Open();
                    output.Write(new byte[2 * 1024 * 1024]);
                }
            }
            return bytes.ToArray();
        }
        var requirement = new GrafanaPluginRequirement { Id = Id, Version = Version };
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(Create(true), requirement));
        Assert.Throws<CliException>(() => GrafanaPluginResolver.Verify(Create(false), requirement));
    }

    [LinuxFact]
    public void GrafanaUnixPermissionsSurviveRealFilesystemExtraction()
    {
        if (!OperatingSystem.IsLinux()) throw new InvalidOperationException("Linux test scheduled on an unsupported host.");
        string root = Path.Combine(Path.GetTempPath(), "grafana-plugins-test-" + Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(root, "source"));
        try
        {
            var plugin = GrafanaPluginResolver.Verify(Archive(true), new GrafanaPluginRequirement { Id = Id, Version = Version, Platform = "linux-amd64" });
            string zipPath = Path.Combine(root, "package.zip");
            FileSystemUtilities.ZipDirectory(new FileSystem(), zipPath, new FileSystem().DirectoryInfo.New(Path.Combine(root, "source")), plugin.WriteTo);
            ZipFile.ExtractToDirectory(zipPath, Path.Combine(root, "extracted"));
            var binaryMode = File.GetUnixFileMode(Path.Combine(root, "extracted", "plugins", Id, "plugin_linux_amd64"));
            var assetMode = File.GetUnixFileMode(Path.Combine(root, "extracted", "plugins", Id, "module.js"));
            Assert.Equal((UnixFileMode)0x1ed, binaryMode);
            Assert.Equal((UnixFileMode)0x1a4, assetMode);
        }
        finally { Directory.Delete(root, true); }
    }

    private sealed class LinuxFactAttribute : FactAttribute
    {
        public LinuxFactAttribute() { if (!OperatingSystem.IsLinux()) Skip = "Requires Linux filesystem permission semantics."; }
    }
}

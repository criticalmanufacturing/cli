using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;
using System;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Grafana package type handler.
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class GrafanaPackageTypeHandler : PackageTypeHandler
    {
        // CmfEntrypoint's Grafana provisioning destination, relative to the generated package root (/).
        private const string ProvisioningDirectory = "etc/grafana/provisioning";
        private readonly IGrafanaPluginResolver pluginResolver;
        private readonly List<PreparedGrafanaPlugin> preparedPlugins = new();
        private readonly Dictionary<string, FileToPack> provisioningFiles = new(StringComparer.Ordinal);
        private readonly HashSet<string> transformSources = new(StringComparer.Ordinal);
        private bool HasPlugins => CmfPackage.GrafanaPlugins?.Count > 0;
        private string PluginsDirectory => CmfPackage.EffectiveGrafanaPluginsTargetPath[1..];
        /// <summary>
        /// Initializes a new instance of the <see cref="GrafanaPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="pluginResolver">The resolver, or the CLI registration/default resolver when omitted.</param>
        public GrafanaPackageTypeHandler(CmfPackage cmfPackage, IGrafanaPluginResolver pluginResolver = null) : base(cmfPackage)
        {
            cmfPackage.ValidateGrafanaPlugins();
            this.pluginResolver = pluginResolver ?? ExecutionContext.ServiceProvider?.GetService<IGrafanaPluginResolver>() ?? new GrafanaPluginResolver();
            DefaultContentToIgnore.Add(CoreConstants.GrafanaPluginsStateFolder);
            cmfPackage.SetDefaultValues
            (
                targetLayer: "grafana",
                isInstallable: true,
                isUniqueInstall: true,
                steps: new List<Step>()
                {
                    new(StepType.DeployFiles)
                    {
                        ContentPath = "**/**"
                    },
                }
            );

            IEnumerable<IBuildCommand> buildSteps = cmfPackage.BuildSteps?.Select(pbs => new SingleStepCommand() { BuildStep = pbs } as IBuildCommand);

            if (buildSteps != null && buildSteps.Any()) {
                BuildSteps = buildSteps.ToArray();
            }
        }

        public override void Build(bool test)
        {
            RestoreDependencies(ExecutionContext.Instance?.RepositoriesConfig?.Repositories?.ToArray() ?? Array.Empty<Uri>());
            base.Build(test);
        }

        public override void RestoreDependencies(Uri[] repoUris)
        {
            CmfPackage.ValidateGrafanaPlugins();
            base.RestoreDependencies(repoUris);
            pluginResolver.RestoreAsync(CmfPackage).GetAwaiter().GetResult();
        }

        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false)
        {
            CmfPackage.ValidateGrafanaPlugins();
            if (HasPlugins && PathsOverlap(PluginsDirectory, "manifest.xml"))
                throw new CliException("grafanaPluginsTargetPath collides with the package manifest.");
            if (CmfPackage.GrafanaPlugins?.Any(plugin => plugin.Platform != null && !plugin.Platform.StartsWith("linux-", StringComparison.Ordinal)) == true)
                throw new CliException("Single-ZIP Grafana deployment requires Linux plugins for the CMF Linux entrypoint.");
            FilesToPack.Clear();
            preparedPlugins.Clear();
            foreach (var requirement in CmfPackage.GrafanaPlugins ?? new())
            {
                try
                {
                    var plugin = pluginResolver.ReadPrepared(CmfPackage, requirement);
                    preparedPlugins.Add(plugin);
                    if (dryRun)
                        foreach (string path in plugin.Paths)
                            Log.Information($"  Prepared plugin -> {PluginsDirectory}/{path[(CoreConstants.GrafanaPluginsPackageFolder.Length + 1)..]}");
                }
                catch (CliException ex) when (dryRun)
                {
                    Log.Warning(ex.Message);
                }
            }
            try { base.Pack(packageOutputDir, outputDir, dryRun); }
            finally
            {
                preparedPlugins.Clear();
                provisioningFiles.Clear();
                transformSources.Clear();
                FilesToPack.Clear();
            }
        }

        internal override List<FileToPack> GetContentToPack(IDirectoryInfo packageOutputDir)
        {
            var files = base.GetContentToPack(packageOutputDir);
            // Also exclude explicit selections below the cache, not only recursive globs.
            files.RemoveAll(file => file.Source.FullName.Replace('\\', '/').Split('/')
                .Contains(CoreConstants.GrafanaPluginsStateFolder, StringComparer.OrdinalIgnoreCase));
            provisioningFiles.Clear();
            if (HasPlugins)
            {
                var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var file in files)
                {
                    string path = fileSystem.Path.GetRelativePath(packageOutputDir.FullName, file.Target.FullName).Replace('\\', '/');
                    GrafanaPluginResolver.ValidatePath(path);
                    if (path.Split('/')[0].Equals(CoreConstants.GrafanaPluginsPackageFolder, StringComparison.OrdinalIgnoreCase) || !paths.Add(path))
                        throw new CliException("contentToPack collides with a declared plugin or another output. The plugins/ directory is reserved when grafanaPlugins is used.");
                    string destination = $"{ProvisioningDirectory}/{path}";
                    if (CmfPackage.GrafanaPlugins.Any(plugin => PathsOverlap(destination, $"{PluginsDirectory}/{plugin.Id}")))
                        throw new CliException("Grafana provisioning content collides with a declared plugin destination.");
                    provisioningFiles.Add(path, file);
                    file.Target = fileSystem.FileInfo.New(fileSystem.Path.Join(packageOutputDir.FullName, destination));
                }
                foreach (string path in paths)
                    for (int slash = path.IndexOf('/'); slash >= 0; slash = path.IndexOf('/', slash + 1))
                        if (paths.Contains(path[..slash]))
                            throw new CliException("Grafana provisioning content has a file/directory collision.");

                // Validate and resolve steps even during dry run, before creating output files.
                ValidateDeploymentSteps();
                foreach (string path in transformSources)
                {
                    var source = provisioningFiles[path];
                    files.Add(new FileToPack(source.Source, fileSystem.FileInfo.New(fileSystem.Path.Join(packageOutputDir.FullName, path)), source.ContentToPack));
                }
            }
            return files;
        }

        private void ValidateDeploymentSteps()
        {
            // Only the step attributes used by Grafana routing are needed for read-only preflight.
            var root = new XElement("deploymentPackage");
            if (CmfPackage.Steps?.Count > 0)
                root.Add(new XElement("steps", CmfPackage.Steps.Select(step => new XElement("step",
                    step.Type == null ? null : new XAttribute("type", step.Type),
                    step.ContentPath == null ? null : new XAttribute("contentPath", step.ContentPath),
                    step.File == null ? null : new XAttribute("file", step.File),
                    step.RelativePath == null ? null : new XAttribute("relativePath", step.RelativePath)))));
            foreach (string injection in CmfPackage.XmlInjection ?? new())
            {
                string path = fileSystem.Path.Combine(CmfPackage.GetFileInfo().DirectoryName, injection);
                if (!fileSystem.File.Exists(path))
                    throw new CliException(string.Format(CoreMessages.NotFound, path));
                string xml = fileSystem.File.ReadAllText(path);
                if (string.IsNullOrEmpty(xml))
                    throw new CliException(string.Format(CoreMessages.NotFound, path));
                root.Add(XDocument.Parse(xml).Elements());
            }
            ApplyDeploymentLayout(root);
        }

        /// <inheritdoc />
        internal override void GenerateDeploymentFrameworkManifest(IDirectoryInfo packageOutputDir)
        {
            base.GenerateDeploymentFrameworkManifest(packageOutputDir);
            if (!HasPlugins) return;
            string path = fileSystem.Path.Combine(packageOutputDir.FullName, CoreConstants.DeploymentFrameworkManifestFileName);
            var document = XDocument.Parse(fileSystem.File.ReadAllText(path));
            ApplyDeploymentLayout(document.Root);
            fileSystem.File.WriteAllText(path, document.ToString());
        }

        private void ApplyDeploymentLayout(XElement root)
        {
            if (root.Elements("targetLayerDirectory").Any() || root.Elements("steps").Count() > 1)
                throw new CliException("grafanaPlugins cannot be combined with injected targetLayerDirectory or additional steps sections.");
            root.Add(new XElement("targetLayerDirectory", "/"));
            var steps = root.Element("steps");
            if (steps == null)
            {
                steps = new XElement("steps");
                root.Add(steps);
            }
            transformSources.Clear();
            var rebased = steps.Elements("step").SelectMany(RebaseStep).ToList();
            steps.RemoveNodes();
            foreach (var step in rebased)
            {
                // The legacy default-step comparison can prepend an identical DeployFiles step.
                // Only coalesce adjacent identical copies; later copies may intentionally follow a transform.
                if ((string)step.Attribute("type") == "DeployFiles" && XNode.DeepEquals(steps.Elements().LastOrDefault(), step)) continue;
                steps.Add(step);
            }
            foreach (var plugin in CmfPackage.GrafanaPlugins)
                steps.Add(new XElement("step", new XAttribute("type", "DeployFiles"),
                    new XAttribute("contentPath", $"{PluginsDirectory}/{plugin.Id}/**")));
        }

        private IEnumerable<XElement> RebaseStep(XElement step)
        {
            // Never mutate the source cmfpackage.json or its in-memory steps.
            var copy = new XElement(step);
            switch ((string)copy.Attribute("type"))
            {
                case "DeployFiles":
                    copy.SetAttributeValue("contentPath", $"{ProvisioningDirectory}/{RelativePath((string)copy.Attribute("contentPath"), true)}");
                    copy.Attribute("targetDirectory")?.Remove();
                    copy.Attribute("relativePath")?.Remove();
                    break;
                case "TaggedFile":
                    string pattern = RelativePath((string)copy.Attribute("contentPath"), true);
                    string expression = "\\A" + Regex.Escape(pattern).Replace("\\*\\*/", "(?:.*/)?")
                        .Replace("\\*\\*", ".*").Replace("\\*", "[^/]*").Replace("\\?", "[^/]") + "\\z";
                    var matcher = new Regex(expression, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
                    var matches = provisioningFiles.Keys.Where(path => matcher.IsMatch(path)).OrderBy(path => path, StringComparer.Ordinal).ToArray();
                    if (matches.Length == 0)
                        throw new CliException($"Grafana TaggedFile pattern '{pattern}' does not match any packaged provisioning files.");
                    foreach (string path in matches)
                    {
                        RelativePath(path, false);
                        var tagged = new XElement(copy);
                        tagged.SetAttributeValue("contentPath", $"{ProvisioningDirectory}/{path}");
                        yield return tagged;
                    }
                    yield break;
                case "TransformFile":
                    string file = RelativePath((string)copy.Attribute("file"), false);
                    string relative = (string)copy.Attribute("relativePath");
                    relative = string.IsNullOrEmpty(relative) ? "" : "/" + RelativePath(relative, false);
                    if (!provisioningFiles.ContainsKey(file) || PathsOverlap(file, "manifest.xml") ||
                        PathsOverlap(file, ProvisioningDirectory) ||
                        CmfPackage.GrafanaPlugins.Any(plugin => PathsOverlap(file, $"{PluginsDirectory}/{plugin.Id}") ||
                            PathsOverlap(ProvisioningDirectory + relative + "/" + file, $"{PluginsDirectory}/{plugin.Id}")))
                        throw new CliException("Grafana TransformFile must select a packaged provisioning file outside reserved ZIP metadata/destination paths.");
                    // The runtime uses `file` both to select the ZIP source and name the target.
                    // Keep a source-only entry at that path and rebase the target using relativePath.
                    transformSources.Add(file);
                    copy.SetAttributeValue("file", file);
                    copy.SetAttributeValue("relativePath", ProvisioningDirectory + relative);
                    break;
                default:
                    throw new CliException("Grafana packages with grafanaPlugins support DeployFiles, TaggedFile and TransformFile steps only.");
            }
            yield return copy;
        }

        private static bool PathsOverlap(string left, string right) =>
            left.Equals(right, StringComparison.OrdinalIgnoreCase) ||
            left.StartsWith(right + "/", StringComparison.OrdinalIgnoreCase) ||
            right.StartsWith(left + "/", StringComparison.OrdinalIgnoreCase);

        private static string RelativePath(string path, bool allowGlob)
        {
            path = path?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(new[] { '[', ']', '{', '}', '(', ')', '!', '#', '%' }) >= 0 ||
                (!allowGlob && path.IndexOfAny(new[] { '*', '?' }) >= 0))
                throw new CliException("Grafana step paths must be package-relative paths; only *, ? and ** glob patterns are supported.");
            GrafanaPluginResolver.ValidatePath(allowGlob ? path.Replace('*', 'x').Replace('?', 'x') : path);
            return path;
        }

        /// <inheritdoc />
        internal override void FinalArchive(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        {
            if (!HasPlugins)
            {
                base.FinalArchive(packageOutputDir, outputDir);
                return;
            }
            foreach (var file in FilesToPack)
            {
                if (!file.Target.Directory.Exists) file.Target.Directory.Create();
                file.Source.CopyTo(file.Target.FullName, true);
            }

            using var bytes = new MemoryStream();
            var unixModes = new Dictionary<string, int>(StringComparer.Ordinal);
            using (var archive = new ZipArchive(bytes, ZipArchiveMode.Create, true))
            {
                foreach (var file in packageOutputDir.GetFiles("*", SearchOption.AllDirectories))
                {
                    string path = fileSystem.Path.GetRelativePath(packageOutputDir.FullName, file.FullName).Replace('\\', '/');
                    using var source = file.OpenRead();
                    using var destination = archive.CreateEntry(path).Open();
                    source.CopyTo(destination);
                }
                foreach (var plugin in preparedPlugins) plugin.WriteTo(archive, unixModes, PluginsDirectory);
            }
            SetUnixZipMetadata(bytes, unixModes);

            string temporary = fileSystem.Path.Combine(CmfPackage.GetFileInfo().DirectoryName, CmfPackage.PackageName + ".zip");
            using (var output = fileSystem.FileStream.New(temporary, FileMode.Create, FileAccess.Write))
            {
                bytes.Position = 0;
                bytes.CopyTo(output);
            }
            fileSystem.File.Move(temporary, fileSystem.Path.Combine(outputDir.FullName, CmfPackage.ZipPackageName), true);
        }

        // Windows-built plugin entries also need a Unix creator field for Linux execute permissions.
        private static void SetUnixZipMetadata(Stream stream, IDictionary<string, int> modes)
        {
            using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true);
            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
            stream.Position = stream.Length - 22;
            if (reader.ReadUInt32() != 0x06054b50)
                throw new InvalidDataException("Expected a ZIP without a comment.");
            stream.Position += 6;
            int count = reader.ReadUInt16();
            stream.Position += 4;
            long offset = reader.ReadUInt32();
            if (count == ushort.MaxValue || offset == uint.MaxValue)
                throw new InvalidDataException("Grafana payload packaging exceeds the supported ZIP size.");
            for (int i = 0; i < count; i++)
            {
                stream.Position = offset;
                if (reader.ReadUInt32() != 0x02014b50)
                    throw new InvalidDataException("Invalid ZIP central directory.");
                stream.Position = offset + 28;
                int nameLength = reader.ReadUInt16();
                int extraLength = reader.ReadUInt16();
                int commentLength = reader.ReadUInt16();
                stream.Position = offset + 46;
                string name = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(nameLength));
                if (modes.TryGetValue(name, out int mode))
                {
                    stream.Position = offset + 5;
                    writer.Write((byte)3);
                    stream.Position = offset + 38;
                    writer.Write((mode << 16) | (name.EndsWith("/") ? 0x10 : 0));
                }
                offset += 46 + nameLength + extraLength + commentLength;
            }
        }
    }
}

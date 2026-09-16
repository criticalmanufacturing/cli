using System.Collections.Generic;
using System.IO.Abstractions;
using System;
using System.IO.Compression;
using System.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
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
        private readonly GrafanaPluginResolver pluginResolver;
        private readonly List<PreparedGrafanaPlugin> preparedPlugins = new();
        /// <summary>
        /// Initializes a new instance of the <see cref="GrafanaPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public GrafanaPackageTypeHandler(CmfPackage cmfPackage, GrafanaPluginResolver pluginResolver = null) : base(cmfPackage)
        {
            this.pluginResolver = pluginResolver ?? ExecutionContext.ServiceProvider?.GetService<GrafanaPluginResolver>() ?? new GrafanaPluginResolver();
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
            preparedPlugins.Clear();
            foreach (var requirement in CmfPackage.GrafanaPlugins ?? new())
            {
                try
                {
                    var plugin = pluginResolver.ReadPrepared(CmfPackage, requirement);
                    preparedPlugins.Add(plugin);
                    if (dryRun)
                        foreach (string path in plugin.Paths) Log.Information($"  Prepared plugin -> {path}");
                }
                catch (CliException ex) when (dryRun)
                {
                    Log.Warning(ex.Message);
                }
            }
            try { base.Pack(packageOutputDir, outputDir, dryRun); }
            finally { preparedPlugins.Clear(); }
        }

        internal override List<FileToPack> GetContentToPack(IDirectoryInfo packageOutputDir)
        {
            var files = base.GetContentToPack(packageOutputDir);
            // Also exclude explicit selections below the cache, not only recursive globs.
            files.RemoveAll(file => file.Source.FullName.Replace('\\', '/').Split('/')
                .Contains(CoreConstants.GrafanaPluginsStateFolder, StringComparer.OrdinalIgnoreCase));
            if (CmfPackage.GrafanaPlugins?.Count > 0)
            {
                var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var file in files)
                {
                    string path = fileSystem.Path.GetRelativePath(packageOutputDir.FullName, file.Target.FullName).Replace('\\', '/');
                    GrafanaPluginResolver.ValidatePath(path);
                    if (path.Split('/')[0].Equals(CoreConstants.GrafanaPluginsPackageFolder, StringComparison.OrdinalIgnoreCase) || !paths.Add(path))
                        throw new CliException("contentToPack collides with a declared plugin or another output. The plugins/ directory is reserved when grafanaPlugins is used.");
                }
            }
            return files;
        }

        internal override void AddArchiveEntries(ZipArchive archive, IDictionary<string, int> unixModes)
        {
            foreach (var plugin in preparedPlugins) plugin.WriteTo(archive, unixModes);
        }
    }
}

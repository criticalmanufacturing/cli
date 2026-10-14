using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Services;

namespace Cmf.CLI.Core.Interfaces;

/// <summary>Prepares and reads verified plugin distributions for Grafana packages.</summary>
public interface IGrafanaPluginResolver
{
    /// <summary>Acquires missing or stale distributions and records verified local preparation.</summary>
    /// <param name="package">The Grafana package declaring the plugin requirements.</param>
    /// <param name="cancellationToken">Cancels acquisition and preparation.</param>
    /// <returns>A task representing preparation of the declared plugins.</returns>
    Task RestoreAsync(CmfPackage package, CancellationToken cancellationToken = default);

    /// <summary>Reads verified local preparation without downloading or changing files.</summary>
    /// <param name="package">The Grafana package owning the preparation.</param>
    /// <param name="requirement">The exact plugin requirement to verify.</param>
    /// <returns>The verified distribution ready for packaging.</returns>
    PreparedGrafanaPlugin ReadPrepared(CmfPackage package, GrafanaPluginRequirement requirement);
}

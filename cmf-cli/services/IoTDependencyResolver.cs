using System;
using System.Collections.Generic;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Versioning;

namespace Cmf.CLI.Services;

/// <summary>
/// resolves the third-party library versions supplied by the Connect IoT runtime for a MES version
/// </summary>
public interface IIoTDependencyResolver
{
    /// <summary>
    /// Reads the library versions declared by a Connect IoT runtime package, falling back to the known versions
    /// </summary>
    /// <param name="runtimePackage">The runtime package that supplies the libraries, e.g. the controller engine or the driver</param>
    /// <param name="productVersion">The MES version</param>
    /// <param name="registryUrl">The npm registry to read the runtime package from</param>
    /// <returns>IoTDeps representing the Connect IoT runtime libraries</returns>
    IoTDeps Resolve(string runtimePackage, NuGetVersion productVersion, string registryUrl);
}

/// <summary>
/// reads the Connect IoT runtime package manifest for the MES version dist-tag
/// </summary>
public class IoTDependencyResolver : IIoTDependencyResolver
{
    public const string EnginePackage = "@criticalmanufacturing/connect-iot-controller-engine";
    public const string DriverPackage = "@criticalmanufacturing/connect-iot-driver";

    // the registry can take well over the default npm client timeout to return package metadata
    private static readonly TimeSpan RegistryTimeout = TimeSpan.FromSeconds(30);

    private readonly IDependencyVersionService dependencyVersionService;
    private readonly Func<string, INPMPackageInfoResolver> clientFactory;

    public IoTDependencyResolver(IDependencyVersionService dependencyVersionService, Func<string, INPMPackageInfoResolver> clientFactory = null)
    {
        this.dependencyVersionService = dependencyVersionService;
        this.clientFactory = clientFactory ?? (registryUrl => new NPMClient(registryUrl, timeout: RegistryTimeout));
    }

    /// <summary>
    /// Resolves the library versions for the current project's MES version and npm registry
    /// </summary>
    /// <param name="runtimePackage">The runtime package that supplies the libraries</param>
    /// <returns>IoTDeps representing the Connect IoT runtime libraries</returns>
    public static IoTDeps ResolveForProject(string runtimePackage)
    {
        var projectConfig = ExecutionContext.Instance.ProjectConfig;
        var resolver = ExecutionContext.ServiceProvider.GetService<IIoTDependencyResolver>();

        return resolver?.Resolve(runtimePackage, projectConfig.MESVersion, projectConfig.NPMRegistry.ToString())
            ?? ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().IoT(projectConfig.MESVersion);
    }

    public IoTDeps Resolve(string runtimePackage, NuGetVersion productVersion, string registryUrl)
    {
        var fallback = this.dependencyVersionService.IoT(productVersion);
        var distTag = GenericUtilities.GetNpmDistTag(productVersion);

        NpmPackageVersion manifest = null;
        try
        {
            manifest = this.clientFactory(registryUrl).FetchPackageInfo(runtimePackage, distTag).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Log.Debug($"Could not read {runtimePackage}@{distTag} from {registryUrl}: {e.Message}");
        }

        if (manifest == null)
        {
            Log.Debug($"Using the known Connect IoT library versions for MES {productVersion}");
            return fallback;
        }

        string Declared(string library, string knownVersion) =>
            manifest.Dependencies?.GetValueOrDefault(library) ?? manifest.PeerDependencies?.GetValueOrDefault(library) ?? knownVersion;

        var deps = new IoTDeps()
        {
            Moment = Declared("moment", fallback.Moment),
            Inversify = Declared("inversify", fallback.Inversify),
            ReflectMetadata = Declared("reflect-metadata", fallback.ReflectMetadata),
            ZoneJs = Declared("zone.js", fallback.ZoneJs)
        };
        Log.Debug($"Connect IoT library versions from {runtimePackage}@{manifest.Version}: {deps}");

        return deps;
    }
}

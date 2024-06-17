using System;

namespace Cmf.CLI.Services;

/// <summary>
/// this service returns expected dependency versions as a function of the CM framework/MES version 
/// </summary>
public interface IDependencyVersionService
{
    string DotNetSdk(Version version);
    string AngularCLI(Version version);
}

/// <summary>
/// this service returns expected dependency versions as a function of the CM framework/MES version 
/// </summary>
public class DependencyVersionService : IDependencyVersionService
{
    public const string NET3SDK = "3.1.102";
    public const string NET6SDK = "6.0.201"; // avoid >2xx as it requires HTTPS for nuget pulls
    public const string NET8SDK = "8.0.301";
    public const string NG15 = "15";
    public const string NG17 = "17";
    public string DotNetSdk(Version version) => version.Major > 10 ? NET8SDK : version.Major > 8 ? NET6SDK : NET3SDK;
    public string AngularCLI(Version version) => version.Major > 10 ? NG17 : NG15;
}
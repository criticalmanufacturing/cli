using System;

namespace Cmf.CLI.Services;

public record AngularDeps
{
    public string CLI { get; init; }
    public string Zone { get; init; }
    public string Typescript { get; init; }
    public string ESLint { get; init; }
    public string TSESLint { get; init; }
}

/// <summary>
/// this service returns expected dependency versions as a function of the CM framework/MES version 
/// </summary>
public interface IDependencyVersionService
{
    string DotNetSdk(Version version);
    string Node(Version version);
    string AngularCLI(Version version);
    AngularDeps Angular(Version version);
}

/// <summary>
/// this service returns expected dependency versions as a function of the CM framework/MES version 
/// </summary>
public class DependencyVersionService : IDependencyVersionService
{
    public const string NET3SDK = "3.1.102";
    public const string NET6SDK = "6.0.201"; // avoid >2xx as it requires HTTPS for nuget pulls
    public const string NET8SDK = "8.0.301";
    public const string NODE20 = "20";
    public const string NODE18 = "18";
    public const string NODE12 = "12.20.2";
    public const string NG15 = "15.2.1";
    public const string NG17 = "17.2.1";
    public const string NG15_ZONE = "0.12.0";
    public const string NG17_ZONE = "0.14.3";
    public const string NG15_TSESLINT = "5.44.0";
    public const string NG17_TSESLINT = "6.10.0";
    public const string NG15_ESLINT = "8.28.0";
    public const string NG17_ESLINT = "8.53.0";
    public const string NG15_TS = "4.8.4";
    public const string NG17_TS = "5.3.3";

    public string DotNetSdk(Version version) => version.Major > 10 ? NET8SDK : version.Major > 8 ? NET6SDK : NET3SDK;
    public string Node(Version version) => version.Major > 10 ? NODE20 : version.Major > 9 ? NODE18 : NODE12;

    public AngularDeps Angular(Version version) => version.Major > 10
        ? new AngularDeps()
        {
            CLI = NG17,
            Zone = NG17_ZONE,
            Typescript = NG17_TS,
            ESLint = NG17_ESLINT,
            TSESLint = NG17_TSESLINT
        }
        : new AngularDeps()
        {
            CLI = NG15,
            Zone = NG15_ZONE,
            Typescript = NG15_TS,
            ESLint = NG15_ESLINT,
            TSESLint = NG15_TSESLINT
        };
    public string AngularCLI(Version version) => Version.Parse(this.Angular(version).CLI).Major.ToString();
}
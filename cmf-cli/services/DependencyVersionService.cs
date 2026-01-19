using System;

namespace Cmf.CLI.Services;

public record AngularDeps
{
    public Version CLI { get; init; }
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
    /// <summary>
    /// Returns the expected .NET SDK version for the given product version
    /// </summary>
    /// <param name="productVersion">The product version to use</param>
    /// <returns>String representing the .NET SDK version</returns>
    string DotNetSdk(Version productVersion);
    
    /// <summary>
    /// Returns the expected Node.js version for the given product version
    /// </summary>
    /// <param name="productVersion">The product version to use</param>
    /// <returns>String representing the Node.js version</returns>
    string Node(Version productVersion);

    /// <summary>
    /// Returns the expected Angular CLI major version for the given product version
    /// </summary>
    /// <param name="productVersion">The product version to use</param>
    /// <returns>String representing the Angular CLI major version</returns>
    string AngularCLI(Version productVersion);

    /// <summary>
    /// Returns the expected Angular dependencies for the given product version
    /// </summary>
    /// <param name="productVersion">The product version to use</param>
    /// <returns>AngularDeps representing the Angular dependencies</returns>
    AngularDeps Angular(Version productVersion);
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
    public const string NG21 = "21.1.0";
    public const string NG15_ZONE = "0.12.0";
    public const string NG17_ZONE = "0.14.3";
    public const string NG21_ZONE = "0.16.0";
    public const string NG15_TSESLINT = "5.44.0";
    public const string NG17_TSESLINT = "6.10.0";
    public const string NG21_TSESLINT = "8.52.0";
    public const string NG15_ESLINT = "8.28.0";
    public const string NG17_ESLINT = "8.53.0";
    public const string NG21_ESLINT = "9.39.2";
    public const string NG15_TS = "4.8.4";
    public const string NG17_TS = "5.3.3";
    public const string NG21_TS = "5.9.3";

    public string DotNetSdk(Version productVersion) => productVersion.Major > 10 ? NET8SDK : productVersion.Major > 8 ? NET6SDK : NET3SDK;
    public string Node(Version productVersion) => productVersion.Major > 10 ? NODE20 : productVersion.Major > 9 ? NODE18 : NODE12;

    public AngularDeps Angular(Version productVersion) =>
        productVersion.Major switch
        {
            <= 10 => new AngularDeps()
            {
                CLI = Version.Parse(NG15),
                Zone = NG15_ZONE,
                Typescript = NG15_TS,
                ESLint = NG15_ESLINT,
                TSESLint = NG15_TSESLINT
            },
            11 => new AngularDeps()
            {
                CLI = Version.Parse(NG17),
                Zone = NG17_ZONE,
                Typescript = NG17_TS,
                ESLint = NG17_ESLINT,
                TSESLint = NG17_TSESLINT
            },
            12 => new AngularDeps()
            {
                CLI = Version.Parse(NG21),
                Zone = NG21_ZONE,
                Typescript = NG21_TS,
                ESLint = NG21_ESLINT,
                TSESLint = NG21_TSESLINT
            },
            _ => throw new NotSupportedException($"No Angular dependencies defined for MES version {productVersion}")
        };

    public string AngularCLI(Version productVersion) => this.Angular(productVersion).CLI.Major.ToString();
}
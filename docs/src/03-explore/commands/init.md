# init

Initialize a MES customization or App project without requiring JSON files:

```powershell
cmf init test --tenant test --baseVersion 12.0.0-beta.2 --ciRepo https://dev.criticalmanufacturing.io --releaseRepos https://dev.criticalmanufacturing.io
```

## Current initialization behavior

* Provide the project name and `--baseVersion` (alias `--MESVersion`), plus `--tenant` unless supplied by `--config`. Provide `--ciRepo` and `--releaseRepos`, or the legacy `--deploymentDir` alternative.
* `--config` and `--infrastructure` (alias `--infra`) are optional. The package version defaults to `1.0.0`; normal initialization does not need `--version`.
* `--nugetVersion` and `--testScenariosNugetVersion` default to the MES version. `--ngxSchematicsVersion` defaults to `release-<digits>` for stable releases (for example, `release-1101` for MES `11.0.1`). For prereleases, `init` resolves the matching npm dist-tag (for example, `beta-1200`) against the configured registry and saves the exact version it references. An explicit override skips the lookup.
* Each registry resolves independently: command-line option, infrastructure file, then default. `--npmRegistry` defaults to `https://criticalmanufacturing.io/repository/npm/`; `--nugetRegistry` defaults to `https://criticalmanufacturing.io/repository/nuget/index.json`. Existing credential options remain supported.
* A config file remains useful for full environment setup. MES v10/v11 HTML scaffolding uses its domain and environment settings; test scaffolding uses its hostname and ports. Supplied files are copied unchanged into `EnvironmentConfigs`, including for MES v12.
* New configs omit unused database server/replica backup paths, AlwaysOn, ReportServer, Installation, Temporary, and Gateway fields; model API properties remain for legacy compatibility.
* `--DevTasksVersion`, `--HTMLStarterVersion`, and `--yoGeneratorVersion` are hidden legacy options but remain accepted. MES v9 and earlier require CM CLI **5.8.0 or earlier**.

Use `cmf init --help` for the installed version's options.

<!-- BEGIN USAGE -->

Usage
-----

```
cmf init <projectName> [<rootPackageName> [<workingDir>]] [options]
```

### Arguments

Name | Description
---- | -----------
`<projectName>` | Project name
`<rootPackageName>` | [default: Cmf.Custom.Package]
`<workingDir>` | Working Directory [default: .]

### Options

Name | Description
---- | -----------
`--version <version>` | Package Version [default: 1.0.0]
`-c, --config <config>` | Optional configuration file exported from Setup. Provides tenant and legacy MES 10/11 environment settings; copied to EnvironmentConfigs.
`--appConfig <appConfig>` | App Configuration file
`-t, --repositoryType <App or Customization>` | The type of repository we should initialize. Are we customizing MES or creating a new Application? [default: Customization]
`--baseVersion, --MESVersion <baseVersion> (REQUIRED)` | Target CM framework/MES version
`--ngxSchematicsVersion <ngxSchematicsVersion>` | @criticalmanufacturing/ngx-schematics version.
`--nugetVersion <nugetVersion>` | NuGet versions to target. Defaults to --MESVersion when not specified.
`--testScenariosNugetVersion <testScenariosNugetVersion>` | Test Scenarios NuGet version. Defaults to --MESVersion when not specified.
`--deploymentDir <deploymentDir>` | Deployments directory. Deprecated, supports only file paths/network shares. When using NPM feeds, use --ciRepo and --releaseRepos instead.
`--ciRepo <ciRepo>` | The repository (network share or NPM feed) where CI packages are published to. Must be passed only and only if --deploymentDir is not.
`--releaseRepos <releaseRepos>` | The list of repositories (network shares and/or NPM feeds) where approved packages to be delivered are published to. Must be passed only and only if --deploymentDir is not.
`--tenant <tenant>` | MES tenant name. Required unless supplied by --config; overrides the config file tenant.
`--infra, --infrastructure <infrastructure>` | Optional infrastructure JSON file. CLI registry options override this file; omitted registries use Critical Manufacturing defaults.
`--nugetRegistry <nugetRegistry>` | NuGet registry that contains the MES packages. Defaults to https://criticalmanufacturing.io/repository/nuget/index.json unless supplied by --infrastructure.
`--npmRegistry <npmRegistry>` | NPM registry that contains the MES packages. Defaults to https://criticalmanufacturing.io/repository/npm/ unless supplied by --infrastructure.
`--ISOLocation <ISOLocation>` | MES ISO file
`--nugetRegistryUsername <nugetRegistryUsername>` | NuGet registry username
`--nugetRegistryPassword <nugetRegistryPassword>` | NuGet registry password
`--appId <appId>` | Application identifier. Use only if repository type is App.
`--appName <appName>` | Application name. Use only if repository type is App.
`--appAuthor <appAuthor>` | Application author. Use only if repository type is App.
`--appDescription <appDescription>` | Application description. Use only if repository type is App.
`--appLicensedApplication <appLicensedApplication>` | License for new application. Use only if repository type is App.
`--appIcon <appIcon>` | Application icon. Use only if repository type is App.
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

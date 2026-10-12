# Project Types

This guide provides an overview of the various customization projects supported.

## General Information

The @criticalmanufacturing/cli allows you to create three types of projects:

1. A custom project for MES v10 onwards;
1. A custom project for MES v9 or prior versions;
1. A MES App project.

MES v9 and earlier projects require CM CLI **5.8.0 or earlier**; current CLI versions target MES v10 onwards.

The type of project is defined during the execution of the `cmf init`
command, depending on the MES version selected and the value of
`--repositoryType` parameter.

### Using an Infrastructure Settings File

`--infrastructure` (alias `--infra`) is optional. Registry URLs resolve independently in this order: `--npmRegistry` / `--nugetRegistry`, infrastructure file, default. The defaults are `https://criticalmanufacturing.io/repository/npm/` and `https://criticalmanufacturing.io/repository/nuget/index.json`. Existing registry credential options remain supported. Refer to the [infrastructure config file](../../../03-explore/config-files/infrastructure.json/index.md) specification for more information.

`--config` is also optional: `--tenant` allows initialization without JSON files. A configuration file remains useful for full environment setup: MES v10/v11 HTML scaffolding uses its domain and environment settings; test scaffolding uses its hostname and ports. Supplied files are copied unchanged into `EnvironmentConfigs`, including for MES v12.

New configuration files omit unused database server/replica backup paths, AlwaysOn, ReportServer, Installation, Temporary, and Gateway fields. The corresponding model API properties remain available for legacy compatibility.

## MES v10 onwards

For CM MES v10 and onwards customization projects, the initialization command has the following required parameters:

* Project name and MES version (`--baseVersion`, alias `--MESVersion`);
* `--tenant`, unless supplied by `--config`;
* `--ciRepo` and `--releaseRepos`, or the legacy `--deploymentDir` alternative.

The package version defaults to `1.0.0`; `--version` is only needed to override it. `--nugetVersion` and `--testScenariosNugetVersion` default to the MES version. Supply `--ISOLocation` only when required by Windows-hosted MES components.

```PowerShell
cmf init test --tenant test --baseVersion 12.0.0-beta.2 --ciRepo https://dev.criticalmanufacturing.io --releaseRepos https://dev.criticalmanufacturing.io
```

### Determining `ngx-schematics` Version

By default, `init` selects the MES dist-tag (`release-<digits>`) for stable releases. For prereleases, it resolves the matching tag (for example, `beta-1200` for `12.0.0-beta.2`) in the configured npm registry and stores the exact ngx-schematics version returned by that tag. Use `--ngxSchematicsVersion` to override this selection and skip the lookup. To inspect a stable release's dist-tag:

1. Construct the MES `dist-tag`:

    * ***Format:*** `release-{{MES_VERSION}}` 
    * ***`{{MES_VERSION}}`:*** Concatenation of MES major, minor, and patch versions without separators.

    For prerelease MES versions, use the first prerelease label instead of `release`:
    `12.0.0-beta.2` maps to `beta-1200`. During `cmf init`, when no explicit
    `ngxSchematicsVersion` is supplied, the CLI queries the configured npm registry for
    `@criticalmanufacturing/ngx-schematics` and saves the exact version referenced by that
    tag in `.project-config.json`. Stable versions continue to save the
    `release-{{MES_VERSION}}` tag without this lookup. Subsequent HTML, Help, and IoT
    commands use `NGXSchematicsVersion` from the project configuration without resolving it again.

2. Use the `npm view` command (Replace `${dist_tag}` by the proper value):

    ```bash
    npm view @criticalmanufacturing/ngx-schematics@${dist_tag} version
    ```

Per example, for MES version `10.2.5`:

* Use the MES dist-tag: `release-1025`
* And the command:

   ```bash
   npm view @criticalmanufacturing/ngx-schematics@release-1025 version 
   ```

!!!note "Examples of Compatibility Matrix"

    | MES version | MES dist-tag  | CM ngx-schematics version |
    |:------------|:--------------|:--------------------------|
    | 10.2.5      | release-1025  | 1.3.6                     |
    | 11.0.1      | release-1101  | 11.0.1                    |

## MES v9 or below

For CM MES v9 or prior MES versions customization projects,
use CM CLI **5.8.0 or earlier**. The following legacy examples and compatibility matrix apply to those CLI versions:

* Customization Project Name;
* Customization Project Version;
* DEV Infrastructure Settings;
* DEV Environment Settings;
* CM MES, NuGets, test libraries version (usually the same);
* CM MES ISO location;
* [CM HTML Generator](https://www.npmjs.com/package/@criticalmanufacturing/generator-html) library version;
* [CM Dev Tasks](https://www.npmjs.com/package/@criticalmanufacturing/dev-tasks) library version;
* [Yeoman](https://yeoman.io/) library version;
* Deployment Directory(the base folder for storing project installation packages).

``` powershell
cmf init {{project_name}} 
    --version {{my_project_version}} 
    --infra   {{dev_infra_file_path}} `
    --config  {{dev_env_file_path}} `
    --MESVersion {{mes_version}} `
    --nugetVersion {{mes_version}} `
    --testScenariosNugetVersion {{mes_version}} `
    --ISOLocation {{mes_iso_path}} `
    --DevTasksVersion {{cm_dev_tasks_lib_version}} `
    --HTMLStarterVersion {{cm_html_starter_lib_version}} `
    --yoGeneratorVersion {{yeoman_library_version}} `
    --deploymentDir {{deployment_directory_path}} `
```

e.g.:

``` powershell
cmf init ExampleProject `
    --version 1.0.0 `
    --infra ..\config\infra.json `
    --config ..\config\ExampleEnvironment.json `
    --MESVersion 9.0.11 `
    --nugetVersion 9.0.11 `
    --testScenariosNugetVersion 9.0.11 `
    --ISOLocation \\setups\CriticalManufacturing.iso `
    --HTMLStarterVersion 8.0.0 `
    --DevTasksVersion 9.0.4 `
    --yoGeneratorVersion 3.1.0 `
    --deploymentDir \\files\Deployments
```

### Compatibility Matrix

| MES Version | HTML Starter | Dev Tasks  | Yeoman  |
|:------------|:-------------|:-----------|:--------|
| 5.x.x       | 5.1.9        | 5.1.9      | 1.0.1   |
| 6.0.x       | 6.0.0        | 6.0.0      | 1.0.1   |
| 6.1.x       | 6.1.0        | 6.1.0      | 1.0.1   |
| 6.3.x       | 6.3.0        | 6.3.0      | 1.0.1   |
| 6.4.x       | 6.3.0        | 6.4.0      | 1.0.1   |
| 7.0.x       | 6.3.0        | 7.0.1      | 3.1.0   |
| 7.1.x       | 7.1.1        | 7.1.1      | 3.1.0   |
| 7.2.x       | 7.2.3        | 7.1.1      | 3.1.0   |
| 7.x.x       | 7.2.3        | 7.1.1      | 3.1.0   |
| 8.0.x       | 8.0.7        | 8.0.2      | 3.1.0   |
| 8.x.x       | 8.1.1        | 8.1.3      | 3.1.0   |
| 9.x.x       | 8.1.1        | 8.1.3      | 3.1.0   |

!!! note

    Use `npm info` to determine the recommended dependencies
    version. e.g.:
    
    ``` powershell
    # Check for MES release tags (`release-{{MES_VERSION}}`)
    npm info @criticalmanufacturing/generator-html
    npm info @criticalmanufacturing/dev-tasks
    
    # Yeoman dependency is stated on the dependency list
    # of the generator-html package, e.g.:
    npm info @criticalmanufacturing/generator-html@8.1.1
    ```

## MES App

A MES App project must have as its target a MES v10 or higher version.
As so all requirements are defined for an [MES v10 onwards customization project](#mes-v10-onwards).

To create an App, you must specify the following additional parameters on the `cmf init` command:

  * Application Name;
  * Application ID;
  * Application Author;
  * Application Description;
  * Application Licensed Name;
  * Repository Type argument must be set to App.

``` powershell
cmf init {{project_name}} `
    --tenant {{tenant}} `
    --baseVersion {{mes_version}} `
    --ciRepo {{ci_repository_url}} `
    --releaseRepos {{release_repository_url}} `
    --appName {{app_name}} `
    --appId {{app_id}} `
    --appAuthor {{app_author}} `
    --appDescription {{app_description}} `
    --appLicensedApplication {{app_licensed_application_name}} `
    --repositoryType "App"
```

e.g.:

``` powershell
cmf init ExampleProject `
    --tenant test `
    --baseVersion 11.0.0 `
    --ciRepo https://dev.criticalmanufacturing.io `
    --releaseRepos https://dev.criticalmanufacturing.io `
    --appName "My App" `
    --appId "MyApp" `
    --appAuthor "Critical Manufacturing" `
    --appDescription "My First App" `
    --appLicensedApplication "My App" `
    --repositoryType "App"
```

# Project Types

This guide provides an overview of the various customization projects supported.

## General Information

The @criticalmanufacturing/cli allows you to create three types of projects:

1. A custom project for MES v10 onwards;
1. A custom project for MES v9 or prior versions;
1. A MES App project.

The type of project is defined during the execution of the `cmf init`
command, depending on the MES version selected and the value of
`--repositoryType` parameter.

### Using an Infrastructure Settings File

The examples below assume you're using an infrastructure settings file to store development environment details. Refer to the [infrastructure config file](../../../03-explore/config-files/infrastructure.json/index.md) specification for more information.

## MES v10 onwards

For CM MES v10 and onwards customization projects, the
initialization command has the following required parameters:

* Customization Project Name;
* Customization Project Version;
* DEV Infrastructure Settings;
* DEV Environment Settings;
* CM MES, NuGets, test libraries version (usually the same version);
* CM MES ISO location;
* [CM Angular schematics](https://github.com/criticalmanufacturing/ngx-schematics) libraries version;
* Deployment Directory (the base folder for storing project installation packages).

``` powershell
cmf init {{project_name}} 
    --version {{my_project_version}} 
    --infra   {{dev_infra_file_path}} `
    --config  {{dev_env_file_path}} `
    --MESVersion {{mes_version}} `
    --nugetVersion {{mes_version}} `
    --testScenariosNugetVersion {{mes_version}} `
    --ISOLocation {{mes_iso_path}} `
    --ngxSchematicsVersion {{ngx_version}} `
    --deploymentDir {{deployment_directory_path}}
```

e.g.:

``` powerShell
cmf init ExampleProject `
    --version 1.0.0 `
    --infra ..\config\infra.json `
    --config ..\config\ExampleEnvironment.json `
    --MESVersion 11.0.0 `
    --nugetVersion 11.0.0 `
    --testScenariosNugetVersion 11.0.0 `
    --ISOLocation \\directory\CriticalManufacturing.iso `
    --ngxSchematicsVersion 11.0.0 `
    --deploymentDir \\files\Deployments
```

### Compatibility Matrix

| MES version | ngx-schematics version |
|:------------|:-----------------------|
| 10.1.0      | 1.1.2                  |
| 10.1.1      | 1.1.4                  |
| 10.1.2      | 1.1.5                  |
| 10.1.3      | 1.2.1                  |
| 10.1.4      | 1.3.3                  |
| 10.2.0      | 1.3.2                  |
| 10.2.1      | 1.3.3                  |
| 10.2.2      | 1.3.3                  |
| 10.2.3      | 1.3.4                  |
| 10.2.4      | 1.3.4                  |
| 10.2.5      | 1.3.6                  |
| 10.2.6      | 1.3.6                  |
| 11.0.0      | 11.0.0                 |
| 11.0.1      | 11.0.1                 |

!!! note

    Use the `npm info` command to determine the recommended
    `ngx-schematics` version for your project target MES version.

    ``` powershell
    # Check for MES release tags (`release-{{MES_VERSION}}`)
    npm info @criticalmanufacturing/ngx-schematics
    ```

## MES v9 or below

For CM MES v9 or prior MES versions customization projects,
the initialization command has the following required parameters:

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
    --yoGeneratorVersion 8.1.1 `
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
  * Application MES Target Framework;
  * Application Licensed Name;
  * Repository Type argument must be set to App.

``` powershell
cmf init {{project_name}} `
    --version {{my_project_version}} ` 
    --infra   {{dev_infra_file_path}} `
    --config  {{dev_env_file_path}} `
    --MESVersion {{mes_version}} `
    --nugetVersion {{mes_version}} `
    --testScenariosNugetVersion {{mes_version}} `
    --ISOLocation {{mes_iso_path}} `
    --ngxSchematicsVersion {{ngx_version}} `
    --deploymentDir {{deployment_directory_path}} `
    --appName {{app_name}} `
    --appId {{app_id}} `
    --appAuthor {{app_author}} `
    --appDescription {{app_description}} `
    --appTargetFramework {{app_mes_target_framework}} `
    --appLicensedApplication {{app_licensed_application_name}} `
    --repositoryType "App"
```

e.g.:

``` powershell
cmf init ExampleProject `
    --version 1.0.0 `
    --infra ..\config\infra.json `
    --config ..\config\ExampleEnvironment.json `
    --MESVersion 11.0.0 `
    --ngxSchematicsVersion 11.0.0 `
    --nugetVersion 11.0.0 `
    --testScenariosNugetVersion 11.0.0 `
    --deploymentDir \\directory\Deployments `
    --appName "My App" `
    --appId "MyApp" `
    --appAuthor "Critical Manufacturing" `
    --appDescription "My First App" `
    --appTargetFramework 11.0.0 `
    --appLicensedApplication "My App" `
    --repositoryType "App"
```

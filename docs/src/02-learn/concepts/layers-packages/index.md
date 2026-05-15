# Layers packages

The CM MES architecture is composed of various system layers highly customizable.

This page provides an overview of the various layers packages available for
customizing MES projects, that can be created using the CM CLI. Each layer package serves a specific purpose and offers a structured approach for managing your customizations.

## `cmf init`

Before creating layers packages, ensure you have initialized your project using the
`cmf init` command. This command defines the project's unique name based on the
specified TENANT name specified on the environment settings file. This name will
be used throughout your layers packages for consistent identification.

## MES Customization

Learn more about CM MES extensibility points in the Critical Manufacturing Developer Portal: [https://developer.criticalmanufacturing.com](https://developer.criticalmanufacturing.com).

## Layers Packages Explained

Each layer package focuses on a specific aspect of your customization.

### 1. Business

This package provides a [.NET](https://en.wikipedia.org/wiki/.NET) solution
for backend development, including:

* Exposing new REST API services;
* Extending orchestration and business logic used by API services or DEE actions;

#### Scaffolding

To generate a Business package use the command:

``` powershell
cmf new business
```

#### Package Structure

The created package is composed of three .NET projects:

* __Common__ - Stores constants, utilities, and other reusable classes.
* __Orchestration__ - Houses custom business contracts, logic, and IOC configuration.
* __Services__ - Defines custom REST API services to be consumed by the UI or
  other MES Rest API client.

``` log
📦Project
 ┣ ...
 ┣ 📂Cmf.Custom.Business
 ┃ ┣ 📂Cmf.Custom.Common                 # Common Project 
 ┃ ┃ ┣ 📜Cmf.Custom.{{TENANT}}.Common.csproj
 ┃ ┃ ┣ 📜{{TENANT}}Constants.cs
 ┃ ┃ ┗ 📜{{TENANT}}Utilities.cs
 ┃ ┣ 📂Cmf.Custom.Orchestration          # Orchestration Project
 ┃ ┃ ┣ 📂Abstractions
 ┃ ┃ ┃ ┗ 📜I{{TENANT}}Orchestration.cs 
 ┃ ┃ ┣ 📂InputObjects
 ┃ ┃ ┣ 📂OutputObjects
 ┃ ┃ ┣ 📜Cmf.Custom.{{TENANT}}.Orchestration.csproj
 ┃ ┃ ┣ 📜{{TENANT}}Orchestration.cs
 ┃ ┃ ┗ 📜OrchestrationStartupModule.cs
 ┃ ┣ 📂Cmf.Custom.Services               # REST Services Project
 ┃ ┃ ┣ 📜Cmf.Custom.{{TENANT}}.Services.csproj 
 ┃ ┃ ┗ 📜{{TENANT}}Controller.cs            
 ┃ ┣ 📜Business.sln
 ┃ ┗ 📜cmfpackage.json
 ┣ ...
```

### 2. Master Data

This package allows you to load or update:

* Exported Objects (e.g., UI pages, queries);
* Process Rules (DEE actions executed during package installation);
* Master Data (system and/or business entity data).

#### Scaffolding

To generate the Master Data package use the command:

``` powershell
cmf new data --businessPackage .\Cmf.Custom.Business\
```

!!! note
    The association of "Data" package with "Business" package, results in a change in the "Business" .NET solution file, to include the "Actions" .NET project. This improves the developer's experience, as he will be able to edit all .NET code in the same integrated environment.

#### Package Structure

The Master Data package structure includes folders for:

* __DEEs__: Stores logic for Process Rules (executed during installation) and DEE Actions (executed by DEE extension points on the MES backend logic).
* __ExportedObjects__: Contains objects (e.g., UI pages, queries) to be created or updated.
* __MasterData__: Holds system and/or business entity data for updates.

```log
📦Project
 ┣ ...
 ┣ 📂Cmf.Custom.Data
 ┃ ┣ 📂DEEs             # Folder to store DEE Actions / Process Rules
 ┃ ┃ ┣ 📂ProcessRules   
 ┃ ┃ ┃ ┣ 📂1.0.0
 ┃ ┃ ┃ ┃ ┣ 📂After                
 ┃ ┃ ┃ ┃ ┃ ┗ 📜...      # Process rules for v1.0.0 release to be executed after
 ┃ ┃ ┃ ┃ ┗ 📂Before
 ┃ ┃ ┃ ┃ ┃ ┗ 📜...      # Process rules to run on v1.0.0 installation before host restart
 ┃ ┃ ┃ ┣ 📂EntityTypes
 ┃ ┃ ┃ ┃ ┗ 📜...        # Process rules to create/change MES entities/tables
 ┃ ┃ ┃ ┗ 📜readme.md
 ┃ ┃ ┣ 📜.cmfpackageignore
 ┃ ┃ ┣ 📜Cmf.Custom.{{TENANT}}.Actions.csproj
 ┃ ┃ ┗ 📜DeeDevBase.cs
 ┃ ┣ 📂ExportedObjects
 ┃ ┃ ┗ 📂1.0.0
 ┃ ┃ ┃ ┗ 📜...          # Exported objects to be created/updated on v1.0.0 installation
 ┃ ┣ 📂MasterData
 ┃ ┃ ┣ 📂1.0.0
 ┃ ┃ ┃ ┗ 📜...          # Exported objects to be created/updated on v1.0.0 installation
 ┃ ┃ ┗ 📜readme.md
 ┃ ┗ 📜cmfpackage.json
 ┣ ...
```

!!! note

    Inspect the package `cmfpackage.json` file contents to understand:
    
    * The expected location for files used on a Master Data package;
    * How to extend the package build behavior.

### 3. UI

This layer consists of two sub-packages for extending the MES UI:

* __HTML__ - Allows customization of existing MES UI pages, controls, routes and
  creation of new controls, pages, or wizards.
* __Help__ - Enables adding documentation entries to the MES Help system to
  explain your customizations.

#### Scaffolding

The commands for generating UI packages depends on your MES version:

=== "MES v10 or above"

    ``` powershell    
    # HTML
    cmf new html

    # HELP
    cmf new help
    ```

=== "MES v9 or below"

    To scaffold HTML and Help, their Deployment Framework package needs to be specified. These files can be found in the MES ISO/disk. Assuming that ISO is mounted on `H:` drive and MES target version is 9.1.8, use the commands:

    ``` powershell
    #HTML
    cmf new html --htmlPackage H:\packages\Cmf.Presentation.HTML.9.1.8.zip

    #HELP
    cmf new help --documentationPackage H:\packages\Cmf.Documentation.9.1.8.zip
    ```

#### Package Structure

The resulting file structure depends on the MES version. For details on its usage check CM Training Presentation Training Courses or the Developer Portal.

!!! hint "NPM Authentication"
    To authenticate with the NPM registry, you'll need to add your authentication information to your `.npmrc` file. This file is located at `%USERPROFILE%/.npmrc` on Windows and `~/.npmrc` on Linux.  The [Critical Manufacturing Developer Portal](https://developer.criticalmanufacturing.com/) provides instructions on how to configure and use our public NuGet, NPM, and Docker repositories.

### 4. IoT

This package facilitates developing custom IoT tasks and converters. It also includes a structure for managing IoT Workflows and MasterData under source control.

#### Scaffolding

The command for generating IoT packages depends on your MES version. For full details on how to generate an IoT package library check the [IoT Scaffolding Guide](../../../03-explore/guides/iot-scaffolding.md).

=== "MES v11 onwards"

    To generate IoT package with support for Automation Task Library (ATL), use:

    ```powershell
    cmf new iot
    ```

    In alternative, although deprecated, you can sill generate a Tasks Package Library, using the "MES v10" command.

=== "MES v10"

    ```powershell
    cmf new iot --htmlPackageLocation Cmf.Custom.Baseline.HTML --isAngularPackage
    ```

=== "up to MES v9"

    ``` powershell
    cmf new iot
    ```

#### Package Structure

The generated package includes sub-packages for:

* __IoT.Data__: Stores IoT-specific MasterData.
* __IoT.Packages__: Stores custom tasks and converters implementation.

``` log
📦Project
 ┣ ...
 ┣ 📂Cmf.Custom.IoT
 ┃ ┣ 📂Cmf.Custom.IoT.Data
 ┃ ┃ ┣ 📂AutomationWorkFlows
 ┃ ┃ ┃ ┗ 📜...                  # Exported Workflows files
 ┃ ┃ ┣ 📂MasterData
 ┃ ┃ ┃ ┗ 📂1.0.0
 ┃ ┃ ┃ ┃ ┗ 📜...                # IoT MasterData files
 ┃ ┃ ┗ 📜cmfpackage.json
 ┃ ┣ 📂Cmf.Custom.IoT.Packages
 ┃ ┃ ┣ 📂src
 ┃ ┃ ┃ ┗ 📜...                  # Location for Custom Tasks and Converters code
 ┃ ┃ ┣ 📜cmfpackage.json
 ┃ ┗ 📜cmfpackage.json
 ┣ 📂...
```

### 5. Database

This package allows creating scripts for execution on MES databases during installation.

#### Scaffolding

To create a Database package use the command:

``` powershell
cmf new database
```

#### Package Structure

The created package includes sub-packages for scripts to be executed:

* __Pre__: Before MES Host restart.
* __Post__: After MES Host restart.

Each sub-package further contains folders for specific MES databases (ONLINE, ODS,
DWH) to store relevant SQL scripts.

``` log
📦Project
 ┣ ...
 ┣ 📂Cmf.Custom.Database
 ┃ ┣ 📂Post
 ┃ ┃ ┣ 📂DWH
 ┃ ┃ ┣ 📂ODS
 ┃ ┃ ┣ 📂ONLINE
 ┃ ┃ ┗ 📜cmfpackage.json
 ┃ ┣ 📂Pre
 ┃ ┃ ┣ 📂DWH
 ┃ ┃ ┣ 📂ODS
 ┃ ┃ ┣ 📂ONLINE
 ┃ ┃ ┗ 📜cmfpackage.json
 ┣ ...
```

### 6. Tests

The Tests package provides a framework for creating regression tests to ensure the quality and reliability of your MES customization project. It utilizes the .NET MsTests framework to define and execute test cases.

While the Tests package follows the same structure and creation process as other layer packages, it is not installable as part of the MES system. It is primarily used for internal development and testing purposes.

The package includes dedicated folders for different test types (e.g., Biz, GUI, IoT) and provides necessary configuration files for test execution.

#### Scaffolding

To create a Tests package use the command:

``` powershell
cmf new test
```

#### Package Structure

The created package structure incorporates folders for various test categories:

* __Tests.Biz__: Contains project files for Business logic (BE) tests.
* __Tests.Biz.Common__: Houses project files for commonly used functionalities across tests.
* __Tests.GUI__: Includes project files for Graphical User Interface (GUI) tests.
* __Tests.GUI.PageObjects__: Provides project files for defining objects to interact with GUI functionalities during testing.
* __Tests.IoT__: Encompasses project files for Internet of Things (IoT) specific tests.
* __Tests.Performance__: Accommodates project files for Performance Tests, designed to measure and validate system performance characteristics and benchmarks.
* __MasterData__: This folder serves as a repository to store test master data. This data is used to set up the MES environment before actual test execution.

``` log
 📦Project
 ┣ ...
 ┣ 📂Cmf.Custom.Tests
 ┃ ┣ 📂Cmf.Custom.Tests.Biz
 ┃ ┃ ┣ 📜Cmf.Custom.Tests.Biz.csproj        #== BE Tests Project
 ┃ ┣ 📂Cmf.Custom.Tests.Biz.Common
 ┃ ┃ ┗ 📜Cmf.Custom.Tests.Biz.Common.csproj #== Common Functions Project
 ┃ ┣ 📂Cmf.Custom.Tests.GUI
 ┃ ┃ ┣ 📜Cmf.Custom.Tests.GUI.csproj        #== GUI Tests Project
 ┃ ┣ 📂Cmf.Custom.Tests.GUI.PageObjects
 ┃ ┃ ┗ 📜Cmf.Custom.Tests.GUI.PageObjects.csproj
 ┃ ┣ 📂Cmf.Custom.Tests.IoT
 ┃ ┃ ┣ 📂Framework
 ┃ ┃ ┣ 📂Tests
 ┃ ┃ ┣ 📜Cmf.Custom.Tests.IoT.csproj        #== IoT Tests Project
 ┃ ┣ 📂Cmf.Custom.Tests.Performance
 ┃ ┃ ┗ 📜package.json
 ┃ ┣ 📂MasterData
 ┃ ┃ ┣ 📂Files                 #== Master Data for Test Environment
 ┃ ┃ ┗ 📜cmfpackage.json
 ┃ ┣ 📜app.config
 ┃ ┣ 📜cmfpackage.json
 ┃ ┣ 📜integration.runsettings
 ┃ ┣ 📜local.runsettings
 ┃ ┗ 📜Tests.sln
 ┣ ...
```

### 7. Reporting

This package layer allows the installation of additional [SQL ServerReporting Services (SSRS)](https://learn.microsoft.com/en-us/sql/reporting-services) reports to the MES.

#### Scaffolding

To create a Reporting package use the command:

``` powershell
cmf new reporting
```

#### Package Structure

The created package structure incorporates folders to store RDL files that will be installed on the MES SSRS folder.

``` log
 📦Project
 ┣ ...
 ┣ 📂Cmf.Custom.Reporting
 ┃ ┣ 📂1.0.0
 ┃ ┃ ┗ 📂Custom           # SRSS folder to store the reporting services
 ┃ ┗ 📜cmfpackage.json
 ┣ ...
```

### 8. Grafana

This package layer allows you to create/update [`grafana`](https://grafana.com/) dashboards in the MES infrastructure (only applicable to environments running in containers with MES v10 or above).

#### Scaffolding

To create a Grafana package use the command:

``` powershell
cmf new grafana
```

#### Package Structure

The created package structure incorporates folders to store _grafana_ dashboards and data sources.

``` log
 📦Project
 ┣ ...
 ┣ 📂Cmf.Custom.Grafana
 ┃ ┣ 📂1.0.0
 ┃ ┃ ┣ 📂dashboards
 ┃ ┃ ┃ ┣ 📂{{TENANT}}
 ┃ ┃ ┃ ┗ 📜dashboards.yaml
 ┃ ┃ ┗ 📂datasources
 ┃ ┃ ┃ ┗ 📜datasources.yaml
 ┃ ┣ 📜cmfpackage.json
 ┃ ┗ 📜README.md
 ┣ ...
```

### 9. SecurityPortal

This package layer allows the configuration of authentication strategies on the MES Security Portal component.

!!! warning "Prefer Usage of DevOps Center"
    Whenever possible security strategies should be defined through CM DevOps Center. Use this package, only if, your strategy cannot be defined there or the target environment is not running on containers.

#### Scaffolding

To create a SecurityPortal package use the command:

``` powershell
cmf new securityportal
```

#### Package Structure

This is a simple package. Use the `config.json` file to define the additional security settings/strategies for the CM MES Security Portal component.

``` log
 📦Project
 ┣ ...
 ┣ 📂Cmf.Custom.SecurityPortal
 ┃ ┣ 📜cmfpackage.json
 ┃ ┗ 📜config.json
 ┣ ...
```

## Deprecated layer packages

### Exported Objects

Even though the CLI does not provide scaffolding for an `ExportedObjects` package, as it generally is better to include these in Master Data packages, it is possible to create such a package and the tool will pack it as with any other package type.

As an example, an ExportedObjects package manifest looks like this:

```json
{
  "packageId": "Cmf.Custom.Baseline.ExportedObjects",
  "version": "1.0.0",
  "description": "Baseline Exported Objects Package",
  "packageType": "ExportedObjects",
  "isInstallable": true,
  "isUniqueInstall": false,
  "contentToPack": [
    {
      "source": "$(version)/*",
      "target": "ExportedObjects"
    }
  ]
}
```

You would create a `cmfpackage.json` file inside the objects folder with this content. This will pack any XML file in a folder named after the current package version, so in this case `1.0.0` and place it in the package file in an `ExportedObjects` folder.

Afterwards, do not forget to add this new package as a dependency of your root/feature package, to make sure it gets installed when required.

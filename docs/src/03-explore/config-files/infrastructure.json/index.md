# Infrastructure config file

The Infrastructure configuration file defines a set of your  Company infrastructure mandatory for the development of a CM MES Customization project.

## Overview

Currently, the infrastructure mandatory to do development of an MES Customization project is:

1. NPM Repository - storing the NPM packages for your target MES version;
2. NuGet repository - storing the NuGet packages for your target MES version.

!!! note

    If you work at Critical Manufacturing, you may find our internal infrastructure configuration file on:
    * Our `Projects` AzureDevops;
    * Under Project: **COMMON*
    * Inside GIT Repository: **Tools**
    * At the following path: `/Infrastructure/CMF-internal.json`.

    This file includes other settings not mentioned in here, but that are required by CM internal pipelines.

## Example

    ```json
    {
        "NPMRegistry": "http://host.example/repository/npm",
        "NuGetRegistry": "https://host.example/repository/nuget-hosted",
        "NuGetRegistryUsername": "user",
        "NuGetRegistryPassword": "password"
    }
    ```

## Usage

The infrastructure file must be passed to the @criticalmanufacturing/cli `init` command as an argument. e.g.:

    ```PowerShell
    cmf init "MyProject" --infra "my_infrastructure.json" --config "my_envirionment_environment.json"
    ```

!!! warning

    Store this file on a safe location, as you may require it in the future to re-scaffold your project.

# Infrastructure config file

The optional infrastructure configuration file defines registry URLs and credentials for development of a CM MES project.

## Overview

Development uses these registries:

1. NPM Repository - storing the NPM packages for your target MES version;
2. NuGet repository - storing the NuGet packages for your target MES version.

`cmf init` defaults to `https://criticalmanufacturing.io/repository/npm/` and `https://criticalmanufacturing.io/repository/nuget/index.json`. No infrastructure file is needed when using these feeds. For each registry, command-line options take precedence over the file; missing file values fall back to the defaults. Credential options also override credentials from the file.

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

To override the defaults using a file, pass `--infrastructure` (alias `--infra`):

```PowerShell
cmf init MyProject --tenant test --baseVersion 12.0.0-beta.2 --ciRepo https://dev.criticalmanufacturing.io --releaseRepos https://dev.criticalmanufacturing.io --infra my_infrastructure.json
```

!!! warning

    Store files containing credentials securely and do not commit secrets to source control.

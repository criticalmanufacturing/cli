# Infrastructure config file

## Structure
Information regarding repositories and Azure projects usually don't change very often. For this set of information, `init` accepts a file with a few keys that specify:

1. The NPM repository URL
1. The NuGet repository URL
1. The Azure DevOps collection URL
1. The Azure DevOps project name
1. The type of Azure build agents used, Cloud or Hosted
    1. Cloud means Microsoft hosted agents with can run a multitude of VMs
    1. Hosted means self-hosted agents. Using self-hosted, for now, requires Windows agents with a set of pre-requisites installed.

## Usage
The infrastructure file must be passed to the `init` command as an argument, e.g.:
```
cmf init --infrastructure <file> --config...
```

## Example
```json
{
    "ISOLocation": "path/to/MES/isos",
    "NPMRegistry": "http://local.example/repository/npm",
    "NuGetRegistry": "https://local.example/repository/nuget-hosted",
    "NuGetRegistryUsername": "user",
    "NuGetRegistryPassword": "password",
    "AzureDevopsCollectionURL": "https://azure.example.com/Projects",
    "AgentPool": "Projects",
    "AgentType": "Cloud"
}
```
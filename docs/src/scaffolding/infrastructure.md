# Infrastructure config file
> If you work at Critical Manufacturing, there is a CMF-internal.json infra file which specifies our internal infrastructure. It's in the Deployment Services AzureDevOps, **COMMON** project, **Tools** repository, at `/infrastructure`.

## Structure
Information regarding repositories usually don't change very often. For this set of information, `init` accepts a file with a few keys that specify:

1. The NPM repository URL
1. The NuGet repository URL

## Usage
The infrastructure file must be passed to the `init` command as an argument, e.g.:
```
cmf init --infrastructure <file> --config...
```

## Example
```json
{
    "NPMRegistry": "http://local.example/repository/npm",
    "NuGetRegistry": "https://local.example/repository/nuget-hosted",
    "NuGetRegistryUsername": "user",
    "NuGetRegistryPassword": "password"
}
```
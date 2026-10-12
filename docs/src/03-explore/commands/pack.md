# pack

### Description

cmf pack is a package creator for the CM MES developments. It puts files and folders in place so that CM Deployment Framework is able to install them.

It is extremely configurable to support a variety of use cases. Most commonly, we use it to pack the developments of CM MES customizations.

Run `cmf pack -h` to get a list of available arguments and options.

### Important

cmf pack comes with preconfigured [Steps](https://github.com/criticalmanufacturing/cli/blob/development/docs/cmf/Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step) per [PackageType](https://github.com/criticalmanufacturing/cli/blob/development/docs/cmf/Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackageType) to run during the installation. This pre defined steps are assuming a restrict structure during the installation, this can be disabled using the flag `isToSetDefaultSteps:false` in your `cmfpackage.json`.

### How it works

When the cmf pack is executed it will search in the working directory, for a `cmfpackage.json` file, that then is serialized to the [CmfPackage](https://github.com/criticalmanufacturing/cli/blob/development/docs/cmf/Cmf_Common_Cli_Objects.md#cmfpackage-class) this will guarantee that the `cmfpackage.json` has all the valid and needed fields. Then it will get which is the [PackageType](https://github.com/criticalmanufacturing/cli/blob/development/docs/cmf/Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackageType), and based on that will generate the package.

### Root package dependencies

For cmfpackagev2 root packages (including feature and IoT roots), `cmf pack` populates virtual environment dependencies in the in-memory `CmfPackage.Dependencies` collection using `MESVersion` from `.project-config.json`. These dependencies are available throughout packing and are serialized by the normal deployment manifest generator:

- `Cmf.Environment` is added for all MES versions.
- `CriticalManufacturing.DeploymentMetadata` is added only for MES major versions **10 or earlier**, and never for App repositories.

Both generated dependencies have `mandatory: false`. Existing dependencies are matched case-insensitively to avoid duplicates; their versions and flags are preserved when applicable. Deployment metadata is excluded from the in-memory root dependencies for MES 11+ and App repositories, even if present in the source package.

These dependencies do not need to be declared in root templates or source `cmfpackage.json` files. Packing (including a dry run) updates the in-memory package without rewriting the source file. Without a configured MES version, dependencies are left unchanged.

<!-- BEGIN USAGE -->

Usage
-----

```
cmf pack [options] [<workingDir>]
```

### Arguments

Name | Description
---- | -----------
`<workingDir>` | Working Directory [default: .]

### Options

Name | Description
---- | -----------
`-o, --outputDir <outputDir>` | Output directory for created package [default: Package]
`-f, --force` | Overwrite all packages even if they already exists
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

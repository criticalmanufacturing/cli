# Pack Command

Create a Cmf Deployment Framework zip file from a package folder

### Synopsis

```bash
cmf pack
```

### Version

1.0.0

### Arguments and Options

| Aliases         | Default Value | Description                                               |
| --------------- | ------------- | --------------------------------------------------------- |
| workingDir      | .             | Working Directory                                         |
| -o, --outputDir | Package       | Output directory for created package                      |
| -r, --repo      |               | Repository where dependencies are located (url or folder) |
| -f, --force     | false         | Overwrite all packages even if they already exists        |

### Description

cmf pack is a package creator for the CM MES developments. It puts files and folders in place so that CM Deployment Framework is able to install them.

It is extremely configurable to support a variety of use cases. Most commonly, we use it to pack the developments of CM MES customizations.

Run `cmf pack -h` to get a list of available arguments and options.

### Important

cmf pack comes with preconfigured [Steps](./../cmf/Step.md) per [PackageType](./../cmf/PackageType.md) to run during the installation. This pre defined steps are assuming a restrict structure during the installation, this can be disabled using the flag `isToSetDefaultSteps:false` in your `cmfpackage.json`.

### How it works

When the cmf pack is executed it will search in the working directory, for a `cmfpackage.json` file, that then is serialized to the [CmfPackage](./../cmf/CmfPackage.md) this will guarantee that the `cmfpackage.json` has all the valid and needed fields. Then it will get which is the [PackageType](./../cmf/PackageType.md), and based on that will generate the package.

If a package lists a dependency, cmf pack will search for that dependencies locally (in the same folder that the tool is running) and if a repo (**-r**) is specified the tool will also search by the dependencies on that repo.

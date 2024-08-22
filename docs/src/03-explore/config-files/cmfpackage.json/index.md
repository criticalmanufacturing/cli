# cmfpackage.json
This document is all you need to know about what's required in your **cmfpackage.json** file.

### packageId

The *most* important things in your cmfpackage.json are the packageId and version fields as they will be required. The packageId and version together form an identifier that is assumed to be completely unique. Changes to the package should come along with changes to the version.

The packageId is automatically generated when the package is created via the [`cmf new`](../../commands/new.md) and by default contains the packageType (eg: *Cmf.Feature.Business*).

### version

The *most* important things in your cmfpackage.json are the packageId and version fields as they will be required. The packageId and version together form an identifier that is assumed to be completely unique. Changes to the package should come along with changes to the version.

Version must be parseable by [node-semver](https://github.com/npm/node-semver).

### description

Put a description in it.  It's a string.

### packageType

The packageType is defined via an Enum, check all the valid values [here](packagetype.md).

### isInstallable

Boolean value *(default true)*. A value of true indicates that the package is prepared to be installed. This value is usually only false for packages of type Tests.

### isUniqueInstall

Boolean value *(default false)*. A value of true indicates that the package is only installed once per environment, a second run of this package doesn't re-install it. This value is usually only true for packages of type Data, IoTData and Database.

### isToSetDefaultSteps

Boolean value *(default true)*. A value of true indicates that a set of predefined steps (each packageType has a set of steps) will be used.
This value is usually only false when some debug is needed or for some reason the default steps are not working as expected.

### steps

The `steps` field is an array of steps that will be executed during the package deployment. This is by default empty, because each package type has a set of predefined steps.
This field is very useful when you want to add steps to be executed after the predefined or when combined with the property `isToSetDefaultSteps` as *false* you can override all the predefined steps.

Example:
```json
{
    "packageId": "Cmf.Custom.Data",
    "version": "1.1.0",
    "description": "Cmf Custom Data Package",
    "packageType": "Data",
    "isInstallable": true,
    "isUniqueInstall": true,
    "steps": [
    {
        "type":"Generic",
        "onExecute":"scriptToRun.ps1"
    }]
}
```

### contentToPack

The `contentToPack` field is an array of file patterns (source and target, check below) that describes the files and folders to be included when your package is packed.
File patterns follow a similar syntax to .gitignore, but reversed: including a file, directory, or glob pattern (\*, \*\*/\*, and such) will make it so that file is included in the zip when it's packed.

Some special files and directories are also included or excluded regardless of whether they exist in the files array (see [here](defaultcontenttoignore.md)).

You can also provide a `.cmfpackageignore` file, which will keep files from being included.

Properties:

> #### source
> File pattern where the files/directories to pack are located (relative to the `cmfpackage.json`).
>
> #### target
> File pattern where the files/directories should be placed in the zip.
>
> #### contentType
> Usually used in packages of Type Data. Defined via an Enum, check all the valid values [here](contenttype.md).
>
> #### ignoreFiles
> File pattern that should point to `.cmfpackageignore` files.
>
> #### action
> Action that will occur during the packing. Defined via an Enum, check all the valid values [here](packaction.md).

The properties **source** and **target** have support for token replacement of any property of the cmfpackage.
Example:
```json
{
  "packageId": "Cmf.Custom.Database.Post",
  "version": "1.1.0",
  "description": "Cmf Custom Database Post Scripts Package",
  "packageType": "Database",
  "isInstallable": true,
  "isUniqueInstall": false,
  "contentToPack": [
    {
      "source": "Online/$(version)/*",
      "target": "Online/$(version)"
    }
  ]
}
```
This means that the contentToPack will look in to the folder `/Online/1.1.0`.


### dependencies
Dependencies are specified in an array of a simple object that maps a packageId to a version.
It can point to a local dependency (in the same repo) or to a remote dependency (in a remote repo).
> Remote dependencies depend on remote repos(currently we only support folders), these repos are defined in the file [`repositories.json`](../repositories.json/index.md)

Example:
```json
{
    "packageId": "Cmf.Custom.Data",
    "version": "1.1.0",
    "description": "Cmf Custom Data Package",
    "packageType": "Data",
    "isInstallable": true,
    "isUniqueInstall": true,
    "dependencies": [
    {
      "id": "Cmf.Custom.Business",
      "version": "1.0.0"
    }]
}
```


### relatedPackages
In some cases, you want to guarantee that a set packages are built or packed together.
To do this you just need to add this property, point to the relativePath of the package and define when it should be build or packed.
Example:
```json
{
    "packageId": "Cmf.Custom.Data",
    "version": "1.1.0",
    "description": "Cmf Custom Data Package",
    "packageType": "Data",
    "isInstallable": true,
    "isUniqueInstall": true,
    "relatedPackages": [
    {
        "path": "../Cmf.Custom.Business",
        "preBuild": true,
        "postBuild": false,
        "prePack": false,
        "postPack": false
    }]
}
```

### testPackages
Just like the `dependencies` property, the `testPackages` are specified in an array of a simple object that maps a packageId to a version.
This is useful to link packages of type Tests, to any other packages. This allow the [`cmf assemble`](../../commands/assemble.md) command to assemble it together with the relative package.

## **Generic Type Packages**
This type doesn't have any predefined BuildStep, Step or ContentToPack, so it will completely rely on what is defined to know how it should be built, packed and deployed. Check the above properties that are only available for this PackageType.

### buildSteps
Array of terminal commands *(similar to [package.json scripts](https://docs.npmjs.com/cli/v10/configuring-npm/package-json#scripts))* that will be used to build the package during the `cmf build` command execution.

Example:
```json
"buildSteps": [
{
    "args": ["build -c Release"],
    "command": "dotnet",
    "workingDirectory": "."
}
```

### dFPackageType
The dfPackageType is defined via an Enum, check all the valid values [here](packagetype.md).

### targetLayer
String value that should match a container layer from CM Framework. Valid values should be checked in the official documentation [here](https://help.criticalmanufacturing.com/).

### Example
```json
{
    "packageId": "Cmf.Custom.Generic.Package",
    "version": "1.0.0",
    "description": "Generic Package",
    "packageType": "Generic",
    "dfPackageType": "Business",
    "targetLayer": "host",
    "isInstallable": true,
    "isUniqueInstall": false,
    "buildSteps": [
    {
        "args": [
        "build -c Release"
        ],
        "command": "dotnet",
        "workingDirectory": "."
    }
    ],
    "contentToPack": [
    {
        "source": "Release/netcoreapp3.1/*.*",
        "target": ""
    }],
    "steps": [
    {
        "type": "DeployFiles",
        "contentPath": "**/**"
    }]
}
```
# Bump IoT Customization Command

Will change / bump the version of the IoT Customization that you have on your project

### Synopsis

```bash
cmf bumpIoTCustomization -v 1.0.0 -pckNames controller-engine-custom-file-tasks -isTag
```

### Version

1.0.0

### Description

It will receive a package path (if no path is specified it will assume where command is executed). If the package path is of type IoT, it will execute the bump of the IoT custom packages beneath it. if the package type is not of type IoT, it will search for the cmfPackages type IoT beneath it in the folder tree.

The bumpIoTCustomPackages, will retrieve all the IoT custom packages beneath ./src/* and will, change the version (**-version** argument) of the package.json and the /src/metadata.ts. It also allows for the user to specify which packages to bump (i.e **-pckNames** controller-engine-custom-file-tasks).

There are two modes to bump the version, either replace the version for a new version, or tag with with the new version, with the flag **-isTag.** For example if the package has version 1.0.0 and I specify a new version 1.1.0, I can replace it by not passing the -isTag or I can pass the flag and it will be changed to 1.0.0-1.1.0. 

### Important

It assumes a structure of IoT/src/customPackage.



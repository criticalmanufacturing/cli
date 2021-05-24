# Bump Command

Will change / bump the version of the CmfPackages throughout your project, also per CmfPackageType it may have particular bumps.

### Synopsis

```bash
cmf bump -v 1.0.0 -r 1.0.0 -a
```

### Version

1.0.0

### Description

**General**

If the flag -a (default false) is set as true, it will go through the tree of cmfPackages and will update them all their dependencies to the version specified. Per CmfPackage it will also execute the specific bump for the package type. Below we will describe what the bump does per package type.

**IoT**

When reaching a CmfPackage with the type IoT, it will trigger this bump. This bump will retrieve all the custom tasks from the *".dev-tasks.json"* file and will perform the version bump. The particular packages to bump can be specified by filling the *packagesBuildBump* field inside the, if no particular package is specified it will assume all. it will then bump the package.json and the src/metadata.ts foreach package, by adding **"-version"**. Example if the package has 1.0.0 and the version invoked in the bump is 1.1.0 it will be tagged 1.0.0-1.1.0.

**IoTData**

When reaching a CmfPackage with the type IoTData, it will trigger this bump. It will search for packages of type IoT on the same level or below. It will check the custom packages to bump from the *.dev-tasks.json* file. If a root (**-r**) is specified, it will filter by packages beneath the root directory.

It will then BumpWorkflowFiles, as customization and will add a tag with the version.

It will also BumpIoTMasterData, as customization and will add a tag with the version. This will only change if you have custom drivers.



### Important

*No particular point.*


# Angular 15 IoT packages

Version v10 has introduced a dependency between Connect IoT customization and the GUI. This means that the GUI in order to have the Connect IoT customization, will require in compile time access to the same. If you have Connect IoT customization you must have an html package.

## IoT to HTML relation

When generating the Connect IoT customization, you will now have to pass a new parameter htmlPackageLocation:

```powershell

cmf new iot --htmlPackageLocation Cmf.Custom.Baseline.HTML

```

 This will introduce an entry in the [`cmfpackage.json`](../../../03-explore/config-files/cmfpackage.json/index.md) of the iot package with the related package. When building or packing the iot package it will now build and and pack the HTML package after building the IoT package.

## HTML to IoT relation

The reverse relation from the HTML package to the iot package has to be added manually. In order to do this, in your HTML package [`cmfpackage.json`](../../../03-explore/config-files/cmfpackage.json/index.md) add an "relatedPackages" entry, e.g.:

```json

"relatedPackages": [
{
    "path": "../Cmf.Custom.Baseline.IoT/Cmf.Custom.Baseline.IoT.Packages",
    "preBuild": true,
    "postBuild": false,
    "prePack": false,
    "postPack": false
}]

```

Also, in the `package.json` of the html package, a dependency for the custom iot package should be created. For example, if you created a new library called connect-iot-controller-engine-custom-tasks:

```powershell

npm install ../Cmf.Custom.Baseline.IoT/Cmf.Custom.Baseline.IoT.Packages

```

This will add an entry in the `package.json` file like:

```json

"@criticalmanufacturing/connect-iot-controller-engine-custom-tasks": "file:../Cmf.Custom.Baseline.IoT/Cmf.Custom.Baseline.IoT

```

Finally, add it to the gui `app.module.ts`:

```ts

import { Metadata as CustomTasks } from '@criticalmanufacturing/connect-iot-controller-engine-custom-tasks/metadata';
    MesUIModule.forRoot({
      tasks: [
        CustomTasks
      ]
    }),

```

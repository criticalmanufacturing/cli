# Angular 15 IoT packages

Version v10 has introduced a dependency between Connect IoT customization and the GUI. This means that the GUI in order to have the Connect IoT customization, will require in compile time access to the same. If you have Connect IoT customization you must have an html package.

When generating the Connect IoT customization `cmf new iot --htmlPackageLocation Cmf.Custom.Baseline.HTML` you will now have to pass a new parameter htmlPackageLocation. This will introduce an entry in the [`cmfpackage.json`](../reference/api/cmfpackage.json/index.md) of the iot package with the related package. When building or packing the iot package it will now build and and pack the HTML package after building the iot package.

The relation between the HTML package will have to be added manually. In order to do this, in your HTML package [`cmfpackage.json`](../reference/api/cmfpackage.json/index.md) add an entry for Related Package in the [`cmfpackage.json`](../reference/api/cmfpackage.json/index.md)
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
In the `package.json` of the html package add a new entry for the custom iot package. For example, if you created a new library called connect-iot-controller-engine-custom-tasks:

```sh
npm install ../Cmf.Custom.Baseline.IoT/Cmf.Custom.Baseline.IoT.Packages
```
This will add this entry in the `package.json`:
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
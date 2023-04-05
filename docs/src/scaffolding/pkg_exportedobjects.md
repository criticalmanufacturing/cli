# Exported Objects package

Even though the CLI does not provide scaffolding for an `ExportedObjects` package, as it generally is better to include these in Master Data packages, it is possible to create such a package and the tool will pack it as with any other package type.

As an example, an ExportedObjects package manifest looks like this:

```json
{
  "packageId": "Cmf.Custom.Baseline.ExportedObjects",
  "version": "1.0.0",
  "description": "Baseline Exported Objects Package",
  "packageType": "ExportedObjects",
  "isInstallable": true,
  "isUniqueInstall": false,
  "contentToPack": [
    {
      "source": "$(version)/*",
      "target": "ExportedObjects"
    }
  ]
}
```

You would create a `cmfpackage.json` file inside the objects folder with this content. This will pack any XML file in a folder named after the current package version, so in this case `1.0.0` and place it in the package file in an `ExportedObjects` folder.

Afterwards, do not forget to add this new package as a dependency of your root/feature package, to make sure it gets installed when required.
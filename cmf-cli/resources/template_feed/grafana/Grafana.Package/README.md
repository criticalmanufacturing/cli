# Grafana custom Package Datasources

If you want to know more about data sources in general [Official Documentation here!](https://grafana.com/docs/grafana/latest/datasources/)

This information is for local development only!

## Bundling plugins for offline deployment

Optionally add `grafanaPlugins` to `cmfpackage.json`. Declare each upstream plugin's
`id` and exact `version`, plus a deployment `platform` for backend plugins. An optional
`source` can point to a local ZIP (relative to this manifest) or HTTPS mirror; `sha256`
can pin the archive digest. Set the optional `grafanaPluginsTargetPath` to the absolute
Grafana plugin directory used by the target image; it defaults to `/data/grafana/plugins`.
No plugins are added to new packages by default.

```sh
cmf restore .
cmf pack . --dry-run
cmf pack . --outputDir ./Package --force
```

Ignore `.cmf-grafana-plugins/` in source control. Restore prepares verified archives
there; pack includes complete distributions under `plugins/<id>/` without downloading.
Keep supplied ZIPs outside the versioned content directories. Ordinary CMF dependencies
continue to use `dependencies`.

See the [Grafana plugin manifest reference](https://criticalmanufacturing.github.io/cli/03-explore/config-files/cmfpackage.json/grafana-plugins/)
for source selection, permissions, cache invalidation and deployment prerequisites.
Confirm the target image's plugin search path and Environment Manager's directory and
permission mapping before deploying: `targetLayer: grafana` alone does not establish them.

In order to create custom data sources or dashboards you should put them in the respective folders.

If you are building an app and want to use the gRPC Data Manager data source you can import the following configuration into your local grafana instance (the variables between <> should be replaced):

``` json
{
  "id": 1,
  "uid": "<deployed-datasource-uid>",
  "orgId": 1,
  "name": "CMF gRPC Datasource",
  "type": "criticalmanufacturing-grpc-datasource",
  "typeName": "CMF gRPC Datasource",
  "typeLogoUrl": "public/plugins/criticalmanufacturing-grpc-datasource/img/logo.svg",
  "access": "proxy",
  "url": "",
  "user": "",
  "database": "",
  "basicAuth": false,
  "isDefault": false,
  "jsonData": {
    "endpoint": "<data-manager-url>:<port>"
  },
  "readOnly": false
}
```

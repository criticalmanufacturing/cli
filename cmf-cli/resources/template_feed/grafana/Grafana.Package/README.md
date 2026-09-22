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
there; pack includes complete distributions under `data/grafana/plugins/<id>/` by
default, without downloading. A custom `grafanaPluginsTargetPath` changes that exact
subtree. One `cmfpackage.json` produces one Grafana ZIP: existing content is placed
under `etc/grafana/provisioning/` and the generated manifest uses package-level
`targetLayerDirectory: /` with scoped deployment steps. No EnvManager changes or
companion package are required, and unrelated image plugins are preserved.
Keep supplied ZIPs outside the versioned content directories. Ordinary CMF dependencies
continue to use `dependencies`.

See the [Grafana plugin manifest reference](https://criticalmanufacturing.github.io/cli/03-explore/config-files/cmfpackage.json/grafana-plugins/)
for source selection, permissions, cache invalidation and deployment prerequisites.
Confirm the target image's plugin search path matches the selected destination and
verify offline plugin loading and executable permissions before deploying. Provisioning
remains at `/etc/grafana/provisioning`. Custom steps support `DeployFiles`, `TaggedFile`
and `TransformFile`; tagging patterns are resolved to specific packaged files.

Clear `GF_INSTALL_PLUGINS` in the deployment configuration so startup does not
download plugins already supplied by the package. For runtimes with Grafana's
background preinstaller (including Grafana 12.0.4), also set
`GF_PLUGINS_PREINSTALL_DISABLED=true`. Keep image-provided plugins and normal
signature verification enabled. The manifest reference documents the minimal
runtime settings for update checks and public-key retrieval. These settings do not
block internet access; enforce and verify public-egress restrictions separately.

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

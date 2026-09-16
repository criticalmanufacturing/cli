# Grafana plugins for offline deployment

Declare upstream plugin distributions separately from CMF `dependencies`. The
manifest owns the exact plugin versions; restoring does not update those versions.
Packages without `grafanaPlugins` keep their existing behavior.

```json
{
  "packageId": "Cmf.Custom.Grafana",
  "version": "1.0.0",
  "packageType": "Grafana",
  "grafanaPlugins": [
    {
      "id": "example-custom-datasource",
      "version": "1.2.3",
      "source": "../PluginArchives/example-custom-datasource-1.2.3.zip",
      "platform": "linux-amd64"
    }
  ]
}
```

This is an illustrative plugin ID/version, not an application dependency or an
existing public plugin recommendation. Existing `contentToPack`, custom deployment
steps and related-package packing continue to work. A plugins-only package is allowed.

Declared plugins are installed under `/data/grafana/plugins` by default, which is
the plugin directory used by the CMF Grafana image. Override it with
`grafanaPluginsTargetPath` when deploying to another image:

```json
{
  "packageType": "Grafana",
  "grafanaPluginsTargetPath": "/var/lib/grafana/plugins",
  "grafanaPlugins": []
}
```

| Field | Meaning |
| --- | --- |
| `id` | Required lowercase, hyphen-separated upstream plugin ID; duplicates are rejected. |
| `version` | Required exact `major.minor.patch`, optionally with SemVer prerelease/build metadata. No `latest`, ranges or wildcards. |
| `source` | Optional HTTPS ZIP URL or local ZIP path. Relative paths resolve against `cmfpackage.json`. HTTP, URL credentials and URL fragments are rejected. |
| `sha256` | Optional 64-digit hexadecimal digest of the original ZIP. Always checked when supplied. |
| `platform` | Deployment target: `linux-amd64`, `linux-arm64`, `linux-arm`, `darwin-amd64`, `darwin-arm64`, or `windows-amd64`. Required for backend plugins and architecture-specific catalog artifacts. |
| `grafanaPluginsTargetPath` | Optional absolute plugin directory. Defaults to `/data/grafana/plugins`. |

The build machine never determines the deployment platform. Backend distributions
must include the executable named by `plugin.json` for the selected target, including
backend components in nested plugin manifests. No downloaded executable is run.

## Build, restore, inspect, pack

```sh
cmf build ./Grafana --test
cmf restore ./Grafana
cmf pack ./Grafana --dry-run
cmf pack ./Grafana --outputDir ./Package --force
```

Build and explicit restore preserve ordinary CMF dependency restoration, then prepare
the declared plugins. Build performs this preparation before the package build steps;
restore performs it as an explicit preparation command. If `source` is omitted, the
resolver reads the exact version's `packages` metadata at
`https://grafana.com/api/plugins/<id>/versions/<version>`, selects the declared
platform (or `any` when available), and follows the advertised `downloadUrl`.
Catalog SHA-256 values are also verified when supplied by the catalog.

An HTTPS mirror URL or local archive bypasses the public catalog entirely for that
plugin. Failures never fall back to Grafana.com. This guarantee concerns Grafana
plugin acquisition; ordinary CMF dependencies still use the configured repositories.
Keep local input archives outside directories selected by `contentToPack`.

Build or restore stores `plugin.zip` and `cmf-grafana-plugin.json` beneath the package's
`.cmf-grafana-plugins/<id>/` folder, separately from `Dependencies`. Add
`.cmf-grafana-plugins/` to your project's `.gitignore`. Packing excludes preparation
state, including when a content rule explicitly selects its files.

Preparation records the ID, version, platform, archive SHA-256, and a fingerprint
of the requirement including source identity and expected checksum. Source URLs
are hashed in metadata so signed query parameters are not stored there. Repeated
build or restore reuses verified inputs. Changing any requirement invalidates them;
corrupt state is reacquired during the next build or restore. Changing bytes behind
an unchanged source does not invalidate a verified cache: change `sha256` or remove
that plugin's preparation folder to request reacquisition. The original source can
be unavailable after preparation.

Pack reads and verifies prepared inputs and never downloads plugins. Missing,
corrupt or stale preparation fails with an instruction to run build or restore. Dry run
lists prepared payload paths and warns about missing prerequisites; it does not
download, extract, create archives, or change manifests or preparation state.
These guarantees concern plugin acquisition. Existing CLI startup version checks
and telemetry are not changed by this feature.

## Payload, signatures and permissions

```text
manifest.xml
<existing selected Grafana files>
plugins/
  <verified-plugin-id>/
    plugin.json
    module.js
    MANIFEST.txt
    <all other distributed files and directories>
```

The plugin's distribution root is normalized to its verified ID, independently of
any npm `package.json` name. All distributed file bytes, including signature files,
are preserved. Nested plugin manifests are retained. The CLI checks identity and
integrity, not the cryptographic authenticity of Grafana signatures; Grafana's normal
signature verification remains enabled. The new property never becomes a CMF dependency.

When declarations are present, `plugins/` is reserved for those declared payloads;
ordinary content cannot overwrite it. Undeclared cached plugins are never included.
ZIP paths with traversal, absolute paths, duplicate/case-conflicting names,
file/directory collisions, symlinks, special files or unsafe Windows names are
rejected before packing. Archives are limited to 256 MiB compressed, 256 MiB per
entry, 1 GiB total expanded, 10,000 entries and a maximum 1000:1 expansion ratio
for entries over 1 MiB. Downloads have a two-minute timeout and at most five HTTPS
redirects. Preparation paths cannot pass through existing filesystem links.

Plugin bytes are copied from the retained ZIP straight into the CMF ZIP, with no
filesystem extraction during preparation or packing. Plugin directories are `0755`;
regular assets are `0644`; upstream executable files and the declared target backend
are `0755`. Privileged and writable-by-everyone bits are removed. The ZIP contains
Unix mode attributes and a Unix creator field, including when built on Windows.
The optional ZIP metadata hook leaves other package types' modes unchanged.

## Deployment contract still requires platform verification

The repository confirms Grafana defaults to `targetLayer: grafana` and a `DeployFiles`
step with `contentPath: **/**`. Its template places dashboards under
`/etc/grafana/provisioning/dashboards`. Neither establishes the runtime plugin path.

Before claiming offline deployment works on a target environment, obtain:

- The target image's `GF_PATHS_PLUGINS` or `[paths] plugins` configuration and volume mounts.
- The Environment Manager mapping of package-relative `plugins/<id>` to that path.
- Evidence that Environment Manager extraction and synchronization preserve ZIP Unix execute permissions.
- A linked integration run with the required image already available and public internet blocked, covering plugin loading, backend execution, panels and restart.

Also review that image's plugin preinstallation and signature-key fetching behavior.
This feature does not package Grafana itself, change startup options, disable signature
verification, or add a runtime downloader. User-defined deployment steps remain the
user's responsibility. Linux ZIP extraction tests establish archive behavior, not
Environment Manager's deployment behavior.

## References

- [Grafana air-gapped plugin installation](https://grafana.com/docs/grafana/latest/administration/plugin-management/plugin-install/)
- [Grafana plugin executable metadata](https://grafana.com/developers/plugin-tools/reference/plugin-json)
- [Grafana repository API client](https://github.com/grafana/grafana/blob/main/pkg/plugins/repo/client.go)

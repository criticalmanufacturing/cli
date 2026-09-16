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
existing public plugin recommendation. One Grafana `cmfpackage.json` produces one
Grafana ZIP containing both provisioning files and plugins; no companion package,
extra manifest or manually configured dependency is needed. A plugins-only package
is allowed. Explicitly configured related packages retain their existing behavior.

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
| `grafanaPluginsTargetPath` | Optional absolute Linux plugin directory. Defaults to `/data/grafana/plugins`. Root `/`, trailing slashes, traversal, empty components, Windows paths and glob characters are rejected. |

The single-ZIP deployment layout targets the Linux CMF entrypoint, including when
packing on Windows. The build machine never determines the plugin platform. Backend distributions
must include the executable named by `plugin.json` for the selected target, including
backend components in nested plugin manifests. No downloaded executable is run.
Acquisition can validate all listed platforms, but this deployment layout rejects
non-Linux platform declarations at pack time.

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
etc/grafana/provisioning/
  <existing selected Grafana files>
data/grafana/plugins/
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

With a custom target, the plugin subtree instead uses that exact path without its
leading `/`; the final directory component is preserved, even if it is not `plugins`.
Source `contentToPack` paths stay relative to the package; the CLI adds the provisioning
prefix only in the output. The source manifest is not rewritten with deployment paths.

When declarations are present, source content targeting `plugins/` remains reserved;
ordinary provisioning content also cannot overlap a declared plugin's destination.
Undeclared cached plugins are never included.
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

## Single-ZIP deployment

The generated `manifest.xml` sets `targetLayer: grafana` (by default) and
`targetLayerDirectory: /`. The existing CMF entrypoint honors this **package-level**
destination, unlike per-step `DeployFiles.targetDirectory`. No EnvManager change,
startup script, image rebuild or second package is needed.

The CLI prefixes provisioning deployment patterns with `etc/grafana/provisioning/`
and generates a separate, narrowly scoped `DeployFiles` step for each declared plugin.
There is no blanket extraction at `/`: `manifest.xml` remains ZIP metadata, and all
ZIP entry paths are relative, without traversal. Provisioning stays at
`/etc/grafana/provisioning`; this layout does not discover an arbitrary image's
provisioning directory at runtime.

Custom `DeployFiles`, `TaggedFile` and `TransformFile` steps retain their order.
Only adjacent identical `DeployFiles` steps are coalesced. Relative step paths may
use `*`, `?` and `**` patterns where applicable; advanced minimatch expressions,
unsafe paths and unsupported step types fail packing with an error. `TaggedFile`
patterns must match packaged provisioning files and expand to explicit paths, so
the runtime never recursively scans `/` for tagging. Tagged/transformed file names
must not contain glob or URI fragment/escape characters (`#` and `%`).

`TransformFile.file` must identify a packaged provisioning file. Its `relativePath`
is rebased beneath provisioning. Because the runtime uses `file` as both its ZIP
source and destination name, the ZIP also retains a source-only copy at the original
relative path; scoped deployment steps do not extract that copy. Conflicting
metadata/destination paths and injected `targetLayerDirectory` or additional `steps`
sections are rejected.

Installation writes declared plugin files, without deleting the plugin root or
unrelated image plugins such as `criticalmanufacturing-grpc-datasource`. A declared
plugin with the same ID as an image plugin overwrites matching files. Native extraction
does not remove obsolete files from earlier versions; verify same-ID upgrades and
Grafana signature checks on the target image. Removing a declaration is not an uninstall.

`grafanaPluginsTargetPath` does not change Grafana's configuration. Its effective
plugin search path must match the selected destination. For the CMF image default,
no additional `GF_PATHS_PLUGINS` override is needed when it already uses
`/data/grafana/plugins`.

## Runtime settings for offline deployments

Bundling plugins removes the need to download those distributions at deployment
time; it does not change Grafana's runtime settings. Keep the application's existing
authentication, data-source and CMF entrypoint settings. Configure the following
on the Grafana container's Deployment, not only on a running pod:

```dotenv
GF_INSTALL_PLUGINS=
GF_PLUGINS_PREINSTALL_DISABLED=true
GF_ANALYTICS_REPORTING_ENABLED=false
GF_ANALYTICS_CHECK_FOR_UPDATES=false
GF_ANALYTICS_CHECK_FOR_PLUGIN_UPDATES=false
GF_PLUGINS_PUBLIC_KEY_RETRIEVAL_DISABLED=true
```

`GF_INSTALL_PLUGINS` must be an empty value, not the string `false`. Remove the old
plugin/version list from the deployment manifest as well, so later deployments
cannot restore the startup downloader. An explicitly empty container variable
also overrides a list inherited from the image.

`GF_PLUGINS_PREINSTALL_DISABLED` controls Grafana's separate background installer
on versions supporting preinstallation, including the observed Grafana 12.0.4
runtime. An empty `GF_INSTALL_PLUGINS` alone does not stop that installer from
fetching suggested apps. Check the actual `Starting Grafana` version in the logs;
the CMF image tag or the default plugin path does not identify the Grafana version.
Disabling preinstallation does not disable discovery of plugins already present
in the image or extracted from the package.

The analytics settings stop usage reporting and update checks. Disabling public-key
retrieval stops key downloads, not plugin signature verification; plugins must still
validate against a key available to that Grafana runtime. Do not add unsigned-plugin
exceptions for the packaged distributions. Existing image-specific exceptions for
CMF plugins are separate from this feature.

Additional `GF_PATHS_PLUGINS` and `GF_PATHS_PROVISIONING` overrides are unnecessary
when the image already uses the selected plugin destination and
`/etc/grafana/provisioning`. Plugin-admin, news-feed, Gravatar and external-snapshot
settings are optional for testing package installation; leaving those features
enabled can still cause requests when they are used.

These settings are not a network firewall. Enforce public-egress restrictions
separately while allowing required internal services. Failed requests to Grafana.com
do not prove that all internet access is blocked. The CLI neither applies these
environment variables nor manages network policies.

## Verify the target environment

Before claiming offline deployment works on a target environment, obtain:

- The target image's `GF_PATHS_PLUGINS` or `[paths] plugins` configuration and volume mounts.
- Evidence that the generated package-level destination is honored and both provisioning and plugins reach their intended paths.
- Evidence that Environment Manager extraction and synchronization preserve ZIP Unix execute permissions.
- A linked integration run with the required image already available and public internet blocked, covering plugin loading, backend execution, panels and restart.

Also review that image's plugin preinstallation and signature-key fetching behavior.
This feature does not package Grafana itself, change startup options, disable signature
verification, or add a runtime downloader. User-defined deployment steps remain the
user's responsibility. Linux ZIP extraction tests establish archive behavior, not
Environment Manager's deployment behavior.

Installation logs supplied on 2026-09-21 provided target-runtime evidence for the
single-ZIP layout: the unchanged entrypoint extracted provisioning and all five
declared plugins to their intended paths, tagging and dashboard provisioning
completed, and Grafana 12.0.4 registered the packaged plugins alongside the image's
CMF gRPC and OData plugins. ClickHouse and GraphQL backend queries returned
`status=ok`. This demonstrates coexistence and working backend executables in that
run, not full visual or restart validation. The same logs showed four background
preinstallation requests timing out; a repeat run after disabling preinstallation,
with independently verified public-egress restrictions, is still needed before
claiming complete offline validation.

## References

- [Grafana air-gapped plugin installation](https://grafana.com/docs/grafana/latest/administration/plugin-management/plugin-install/)
- [Grafana plugin executable metadata](https://grafana.com/developers/plugin-tools/reference/plugin-json)
- [Grafana repository API client](https://github.com/grafana/grafana/blob/main/pkg/plugins/repo/client.go)
- [Grafana configuration and preinstallation settings](https://grafana.com/docs/grafana/latest/setup-grafana/configure-grafana/#preinstall_disabled)

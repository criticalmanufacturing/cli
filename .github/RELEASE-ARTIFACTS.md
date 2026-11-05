# Release binary publication

The release workflow publishes the Windows and Linux ZIPs to GitHub and the registry,
then downloads every ZIP anonymously from **all three** installer sources:
GitHub Releases, `criticalmanufacturing.io`, and
`repository.criticalmanufacturing.com.cn`. Each download must match the local
archive's size and SHA-256. HTTP success alone is insufficient. Verification
retries reads ten times, thirty seconds apart, to allow mirror replication.
Any missing or mismatched archive fails the release job **before npm publication**.

## Registry setup

Configure a GitHub Actions secret named `CRITICALMANUFACTURING_IO_TOKEN` in this repository,
containing a registry token with read/deploy access to workspace `repository`,
repository `tools`. Enter the value directly in GitHub's secret settings.

Uploads use the registry's multipart API:

`POST https://admin.criticalmanufacturing.io/api/1/workspaces/repository/repositories/tools/packages/single`

Use Bearer authentication, `packageVersion=latest`, and one package per ZIP.
The package name and multipart filename must both equal the exact, versioned
mirror filename in `.github/release-artifacts.json`. For example:

* package/file: `cmf-cli.win-x64-6.0.0.zip`
* storage version: `latest`
* installer URL: `https://criticalmanufacturing.io/repository/tools/cmf-cli.win-x64-6.0.0.zip`
* corresponding registry path: `/api/universal/repository/tools/cmf-cli.win-x64-6.0.0.zip/latest/cmf-cli.win-x64-6.0.0.zip`

This preserves the existing proxy and npm installer URL contract. Each release
has a distinct package; `latest` is only its fixed storage version. Existing
packages are skipped only when their stored bytes match the original archive.
Different bytes or unexpected storage versions stop publication. The API's
per-file `isSucceeded` result is checked even on HTTP 200. Uploads are never
automatically retried after an ambiguous response.

The China mirror must replicate these exact filenames and bytes through its
existing distribution process. These workflows verify its public downloads;
they do not invent a China upload endpoint or reuse the production admin token
on another host. If replication is missing, the release fails and names every
failed URL. After restoring mirror distribution, select **Re-run failed jobs**
in Actions to repeat `publish-and-verify-binaries`. That job downloads the original
GitHub release ZIPs before publishing to the registry and verifying all three sources;
it does not rebuild ZIPs or upload release assets. The successful `build-binaries`
job is kept, and the separate npm/NuGet jobs start only after verification passes.
Use this option rather than **Re-run all jobs** when registry publication or mirror
verification failed.

Release publication validates that the tag equals the package version. Tags use
the numeric version without a `v` prefix, matching the existing npm installers.
Run `node --test .github/scripts/release-artifacts.test.mjs` for offline and local
HTTP regression tests; no publishing credentials or third-party packages needed.

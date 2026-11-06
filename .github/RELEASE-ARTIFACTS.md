# Release binary publication

The release workflow uploads the Windows and Linux ZIPs to GitHub and
`criticalmanufacturing.io`, then verifies anonymous downloads from GitHub,
`criticalmanufacturing.io`, and `repository.criticalmanufacturing.com.cn`.
Each download must match the original ZIP's size and SHA-256 before npm or
NuGet publication proceeds. Verification retries reads ten times, thirty
seconds apart, to allow mirror replication.

## Upload setup

Use the existing repository credentials:

- `CRITICALMANUFACTURING_IO_USER`: Actions secret (or repository variable).
- `CRITICALMANUFACTURING_IO_TOKEN`: Actions secret used as the Basic authentication password.
- `CRITICALMANUFACTURING_IO_IP`: Actions secret containing the upload server IP.

The workflow uses `curl --user` with these credentials and the registry's multipart
upload API at `https://admin.criticalmanufacturing.io/api/1/workspaces/repository/repositories/tools/packages/single`.
The Portal token is the Basic authentication password; it is not a registry Bearer token.
Each ZIP is uploaded as its versioned filename, for example
`cmf-cli.win-x64-6.0.2-0.zip`, with storage version `latest`.
`curl --resolve` sends uploads directly to the configured IP while retaining the
HTTPS hostname and certificate validation. Public verification uses normal DNS.
HTTP errors or an unsuccessful per-file upload result fail the job. Uploads are
not automatically retried.

The China mirror uses its existing distribution process. If uploading or mirror
verification fails, use **Re-run failed jobs** after correcting the problem.
This downloads and republishes the original GitHub ZIPs, then checks all three
sources. It does not rebuild binaries or replace GitHub release assets.

Release publication validates that the tag equals the package version, without
a `v` prefix. Run `node --test .github/scripts/release-artifacts.test.mjs` for
verification tests; no publishing credentials are needed.

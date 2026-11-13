# End-to-end CLI validation

Use the opt-in developer helper in
[`scripts/e2e`](https://github.com/criticalmanufacturing/cli/tree/development/scripts/e2e)
to validate a CLI candidate through a complete MES customization workflow.
It creates a fresh project, starts its devcontainer, scaffolds six package types,
installs the local MES stack, builds packages, logs into the MES homepage with a
headless browser, and packs the packages.

Use a separate configuration file for each MES version, with its own `licenseId`.
Failed runs can continue with `--resume` using the same configuration and candidate
version; use a fresh workspace for a changed candidate. Commands and working
directories are logged, and credential synchronization runs before scaffolding.
Stack installation overlaps package work, and a successful run tests stack
teardown with volume removal. Failed runs retain the stack for inspection.
Use `--ci` or `CI=true` for unattended validation: provide a portal PAT or bearer
token and a license through environment variables, and use a fresh workspace.
CI rejects resume and fails a blank page after initial password confirmation;
local runs can retry that page once and record the recovery. Browser progress
screenshots and failure diagnostics are retained with the report.
See the helper README for setup, configuration,
reports, and cleanup. This flow requires portal and private registry credentials,
Docker access, and an appropriate MES license. The existing scaffolding validation
workflow remains the smaller CI check.

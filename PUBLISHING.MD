
# cmf-cli Release & Publishing Guide

Binary publication uses the existing `CRITICALMANUFACTURING_IO_USER`,
`CRITICALMANUFACTURING_IO_TOKEN` and `CRITICALMANUFACTURING_IO_IP` settings, and verifies every
ZIP on GitHub, criticalmanufacturing.io and the China mirror before publishing npm.
See [release artifact publication](.github/RELEASE-ARTIFACTS.md) for setup and
mirror requirements.

## Trusted publishing

npm and NuGet packages are published with
[trusted publishing](https://docs.npmjs.com/trusted-publishers/) using GitHub
Actions OIDC, so no long-lived npm or NuGet tokens are stored in this repository.
The `publish-npm` and `publish-core-nuget` jobs request short-lived credentials
with `permissions: id-token: write`; there is no `NPM_TOKEN` or `NUGET_API_KEY`
secret.

### npm

Configure a trusted publisher for `@criticalmanufacturing/cli` on npmjs.com
(Package settings → Trusted publishing → GitHub Actions):

- Organization or user: `criticalmanufacturing`
- Repository: `cli`
- Workflow filename: `npm-publish.yml`
- Environment name: leave empty
- Allowed actions: enable **npm publish** (direct publish, not only staged publishing)

Create the configuration shortly before the next release: a new npm trusted
publisher must complete its first successful publish within two days.

The publish job uses Node 24 because trusted publishing requires npm CLI 11.5.1
or later. After the first successful tokenless release, restrict token access on
the package (Settings → Publishing access → **Require two-factor authentication
and disallow tokens**), revoke the old `NPM_TOKEN` on npmjs.com, and delete the
GitHub secret.

### NuGet

Configure a [trusted publishing policy](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
on nuget.org (Account menu → Trusted publishing) for the owner that owns
`CriticalManufacturing.CLI.Core`:

- Repository owner: `criticalmanufacturing`
- Repository: `cli`
- Workflow file: `npm-publish.yml`
- Environment: leave empty

Create a repository variable named `NUGET_USER` with the nuget.org profile or
organization name that owns the policy (for example `CriticalManufacturing`).
The policy applies to every package owned by that account, not only
`CriticalManufacturing.CLI.Core`. The job exchanges the GitHub OIDC token for a
one-hour NuGet API key immediately before `dotnet nuget push`. After the first
successful tokenless release, revoke the old `NUGET_API_KEY` on nuget.org and
delete the GitHub secret.

Both trusted publisher configurations must match the workflow filename exactly
(`npm-publish.yml`) and leave the environment empty. If the workflow file is
renamed or an environment is added, update both registries first.

## Stable Releases

1. Bump the version:
   - Run one of:
     - `npm run bump:feature` (for new features)
     - `npm run bump:breaking` (for breaking changes)
     - `npm run bump:patch` (for bug fixes)
2. Open a PR to the `development` branch.
3. After merging to `development`, open a PR from `development` to `main` titled:
   `Release Major.Minor.Patch`
   - **Important:** Merge (do not rebase) this PR.
4. After merging to `main`, follow the process to publish to NPM as `@latest`.
5. Merge back changes from `main` to `development` via another PR.

## Unstable Releases

1. Bump the pre-release version:
   - Run one of:
     - `npm run bump:pre:feature`
     - `npm run bump:pre:breaking`
     - `npm run bump:pre:patch`
2. Open a PR to the `development` branch.
3. After merging, follow the process to publish to NPM as `@next`.

## Publishing to NPM from GitHub

To publish a new version to NPM using GitHub Releases:

1. Go to [GitHub Releases](https://github.com/criticalmanufacturing/cli/releases) and click **Draft a new release**.

### For Unstable Releases (`@next`)
- Branch: `development`
- Tag: Use the bumped version (e.g., `5.5.0-1`)
- Release title: Use the version (e.g., `5.5.0-1`)
- Click **Generate Release Notes**
- Check **Set as a pre-release**
- Click **Publish release**
- This triggers the [npm-publish GitHub Action](https://github.com/criticalmanufacturing/cli/actions/workflows/npm-publish.yml) to publish to the `@next` tag.

### For Stable Releases (`@latest`)
- Branch: `main`
- Tag: Use the bumped version (e.g., `5.5.0`)
- Release title: Use the version (e.g., `5.5.0`)
- Click **Generate Release Notes**
- Check **Set as the latest release**
- Click **Publish release**
- This triggers the [npm-publish GitHub Action](https://github.com/criticalmanufacturing/cli/actions/workflows/npm-publish.yml) to publish to the `@latest` tag.

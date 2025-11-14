
# cmf-cli Release & Publishing Guide

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

# Migration

A big effort is made to ensure that a new CLI release does not break previous projects' scaffolding and maintains compatibility with previous MES releases.

Nevertheless, please consider the conventions and instructions in this guide when upgrading the CLI for your project.

## Conventions

### Minor versions upgrade

Minor CLI version upgrades should not affect your project, only provide enhancements or fixes that should be retro-compatible.

If you found an issue, open a Support Request on the CM Customer Portal or send us a pull request with a fix proposal.

### Major versions upgrade

The CLI major version is incremented every time there are breaking changes on the CLI. In such case, if you decide to upgrade to a new release, please execute the following:

* Review the release log to get hints on what was changed;
* Install the new CLI;
* Re-scaffold your project;
* Merge your customization changes into the new project structure.

Please, review some version-specific migration details:

* [Migration to V3 or above](migration-v3.md)

### MES v12 target directories

MES v12 and newer no longer ignore the package-level `targetDirectory` value. The CLI therefore does not inject this legacy value when packing a project targeting MES v12 or newer, and it omits `targetDirectory` when converting `manifest.xml` or `package.json` for those versions.

For MES versions before v12, the existing `targetDirectory` behavior is preserved. When upgrading a project to MES v12 or newer, use the package's supported target layer and review custom deployment steps instead of relying on the package-level target directory.

Package conversion requires an MES version dependency (`Cmf.Environment` or `CriticalManufacturing.DeploymentMetadata`) when `targetDirectory` is present. If the dependency is missing, malformed, or has a conflicting major version, the CLI reports the package, source file, detected metadata, and reason rather than guessing whether the value is safe to use.

### Release Tags

The @criticalmanufactuing/cli has release tags that you may use to dynamically retrieve the latest @releases:

* __latest__: tag applied to the latest stable package that has been released;
* __next__: tag applied to the latest release, usually used by the CLI early adopters to use and test the latest features/fixes.

## Upgrade Step-by-Step

To upgrade the @criticalmanufacturing/cli:

* Open a command line with administration privileges;
* Install any new [prerequisites software](../../01-install/index.md), if needed;
* Use the following command to upgrade to the latest stable version:

    ``` powershell
    npm install -g --force @criticalmanufacturing/cli@latest
    ```

* Or use the following command to upgrade to the next release (for early adopters):

    ``` powershell
    npm install -g --force @criticalmanufacturing/cli@next
    ```

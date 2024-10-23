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

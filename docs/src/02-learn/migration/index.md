# Migration

A big effort is made to assure that a new CLI release does not break previous projects scaffolding and maintain compatibility with previous MES releases.

Nevertheless, please consider the instructions in this guide when upgrading the CLI for your project.

## Minor versions upgrade

Minor versions upgrade should not affect your project, only provides enhancements or fixes that should be retro-compatible.

If you found an issue, open a Support Request on CM Customer Portal or send us a pull request with a fix proposal.

## Major versions upgrade

The CLI major version is incremented every time there are breaking changes on the CLI. On such case, if you decide to upgrade to a new release, please execute the following:

* Review the release log to get hints on what was changed;
* Re-scaffold your project;
* Merge your customization changes into the new project structure.

Please, review some version specific migration details:

* [Migration to V3 or above](migration-v3.md)

## Release Tags

The @criticalmanufactuing/cli has release tags that you may use to dynamically retrieve the latest @releases:

* __latest__: tag applied to the latest stable package that has been released;
* __vnext__: tag applied to the latest release, usually used by the CLI early adopters to use and test the latest features/fixes.

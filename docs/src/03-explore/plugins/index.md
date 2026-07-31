# Plugins

The Critical Manufacturing cli is designed with a plugin system for extensibility. In the future, it will be possible to search for plugins straight from cli.

In the meanwhile, some plugins are already in development. Here follows a non-exhaustive plugin list:

- [Portal SDK](https://www.npmjs.com/package/@criticalmanufacturing/portal) - command line tools to interact with the Critical Manufacturing Customer Portal.

!!! warning "NPM `allow-scripts` requirement"

    Plugins are distributed as NPM packages, so the same
    [`allow-scripts` requirement](../../01-install/index.md#2-install-cli)
    that applies to installing the CLI also applies to them. Before
    installing a plugin, add it to the `allow-scripts` allowlist, e.g.:

    ```PowerShell
    npm config set allow-scripts=@criticalmanufacturing/portal --location=user
    ```

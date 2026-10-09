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

## Portal SDK commands

The Portal SDK is installed separately from cmf-cli. Install a version that includes
the verb–noun commands introduced in [portal-sdk PR #146](https://github.com/criticalmanufacturing/portal-sdk/pull/146).
As of October 9, 2026, these commands are available in `2.0.0-rc.1` (`@next`);
`@latest` is still `1.20.1` and uses the older commands.

```PowerShell
npm install -g @criticalmanufacturing/portal@2.0.0-rc.1
cmf portal --version
cmf portal --help
cmf portal deploy env --help
```

cmf-cli discovers the `cmf-portal` executable on `PATH` and forwards everything
after `cmf portal` to it. The SDK parses its own commands and options, so no
cmf-cli upgrade or SDK library dependency is required for this command convention.

| Previous command after `cmf portal` | New command after `cmf portal` |
| --- | --- |
| `deploy --name <name>` | `deploy env <name>` |
| `deployagent --name <name>` | `deploy agent <name>` |
| `install-app --name <name>` | `deploy app <name>` |
| `undeploy --name <name>` | `undeploy env <name>` |
| `uninstall-app --name <name>` | `undeploy app <name>` |
| `createinfrastructure --name <name>` | `create infrastructure <name>` |
| `checkagentconnection --name <agent-name>` | `healthcheck agent <agent-name>` |
| `publish --path <path>` | `publish deploymentpackage <path>` |
| `publish-package --path <path>` | `publish installationpackage <path>` |
| `download-artifacts --name <name>` | `download artifacts <name>` |

Names and publish paths are positional arguments in the new commands. Keep any
other options your operation requires, placing them after the noun. Put the name
before multi-value options such as `--replace-tokens` so it is not consumed as an
option value. `env`, `app` and `infrastructure` also accept the aliases
`environment`, `application` and `infra`, respectively.

For example:

```PowerShell
cmf portal deploy env "MES Demo" --site "My Site" --package=@criticalmanufacturing/mes:11.0.0 --target dockerswarm --license "License 1,License 2" --replace-tokens MyToken=value
cmf portal deploy app "My App" --customer-environment "MES Demo" --app-version 1.0.0 --license "My License"
cmf portal healthcheck agent --customer-environment "MES Demo"
cmf portal publish deploymentpackage "./deployment manifests" --datagroup "My Group"
cmf portal publish installationpackage "./packages/My Package.zip"
cmf portal download artifacts "MES Demo" --output "./deployment artifacts"
```

Use `--package=@scope/name:version` for scoped npm package names. A separate value
starting with `@` is interpreted as a response file by the SDK's updated parser.

The SDK retains the previous commands with deprecation warnings. `cmf portal login`
is unchanged, including the automatic Portal login used by `cmf login` and
`cmf login sync`. `cmf portal login` authenticates with the SDK; `cmf login portal`
configures cmf-cli repository credentials.

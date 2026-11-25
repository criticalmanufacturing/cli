# upgrade base

Upgrade the MES baseline of a package directory and its project configuration:

```sh
cmf upgrade base . 12.0.0-beta.1
```

`BaseVersion` requires NuGet MES versions with major version 10 or higher; prereleases are supported. Use `--manifest` to select the manifest used for the upgrade.

Arguments and target project configurations are validated before files change, including targets outside the current project. Errors identify the argument or configuration field and expected format.

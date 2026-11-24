# upgrade base

Upgrade the MES baseline of a package directory and its project configuration:

```sh
cmf upgrade base . 12.0.0-beta.1 --iotVersion 12.0.0-beta.1
```

`BaseVersion` and a supplied `--iotVersion` require NuGet MES versions with major version 10 or higher; prereleases are supported. Omitting `--iotVersion` preserves the existing IoT workflow and masterdata behavior.

Arguments and target project configurations are validated before files change, including targets outside the current project. Errors identify the argument or configuration field and expected format.

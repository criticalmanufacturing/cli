# new test

The project configuration must provide `MESVersion` and `TestScenariosNugetVersion` as NuGet versions. The CLI requires MES 10 or later; the minimum does not apply to test-scenario or package versions.

`--version` defaults to `1.0.0` and accepts NuGet release, prerelease, metadata, and four-part versions. Invalid or missing required versions report the option or configuration field before scaffolding or dependency resolution; files remain unchanged.

<!-- BEGIN USAGE -->

Usage
-----

```
cmf new test [options]
```

### Options

Name | Description
---- | -----------
`--version <version>` | Package Version [default: 1.0.0]
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

# new business

<!-- BEGIN USAGE -->

Usage
-----

```
cmf new business [options] [<workingDir>]
```

### Arguments

Name | Description
---- | -----------
`<workingDir>` | Working Directory [default: ]

### Options

Name | Description
---- | -----------
`--version <version>` | Package Version [default: 1.0.0]
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

## Application Version Assembly

For App repositories, `cmf new business` includes the `ApplicationVersion/Version.csproj` project and its solution references by default. Customization repositories do not include this project.

To omit the version assembly from an App repository, pass `--addApplicationVersionAssembly false`. Use `--addApplicationVersionAssembly` (or `-av`) to explicitly include it; this option is only supported for App repositories.

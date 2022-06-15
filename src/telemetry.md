## Telemetry implementation
Basic telemetry currently only tracks the CLI startup and logs:

- CLI name and version
- latest version available in NPM
- if the CLI version is stable or testing
- if the CLI is outdated

**no identifiable information is collected in basic telemetry**

However, any user can optionally enable extended telemetry, which might help with troubleshooting. **Extended telemetry includes identifiable information** and as such should be used with care. This includes the basic telemetry, plus:

- for the version (startup) log, it also includes
    - current working directory
    - hostname
    - IP
    - username

It also tracks and logs the package tree if, for any command, the tree must be computed. This includes all of the above information plus the package name, for each package in the tree.

Enabling telemetry can be done via environment variables:

- `cmf_cli_enable_telemetry` - enable basic telemetry. If this is off (the default), no telemetry will be collected, even if extended telemetry is on. To enable, set to `true` or `1`, do not set or set to `false` or `0` to disable.
- `cmf_cli_enable_extended_telemetry` - enable extended telemetry. Note the above warnings regarding the impact of keeping this on. To enable, set to `true` or `1`, do not set or set to `false` or `0` to disable.
- `cmf_cli_telemetry_enable_console_exporter` - also print the telemetry information to the console. This is for auditing or troubleshooting as it makes the console output extremely verbose
- `cmf_cli_telemetry_host` - specify an alternate telemetry endpoint (if you're hosting your own)

## Pipelines
**NOTE: The CI-Package pipeline is shipped with telemetry ON**. The provided pipelines are built for Critical Manufacturing use and provided as an example for the user to build their own. As at CM we keep telemetry on for troubleshooting, the generated pipelines have telemetry on. To disable, search for the `cmf_cli_enable_telemetry` environment variable in the pipeline YAML and disable it.
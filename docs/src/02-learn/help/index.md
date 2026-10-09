# Help

## Contextual Help

The @criticalmanufacturing/cli provides help in the context of each command. To access it, use the `-h` or `--help` switch.

Follows a set of examples, to understand its usage:

* To see all commands available on the CLI, use:

    ``` powershell
    cmf -h
    ```

    or

    ``` powershell
    cmf --help
    ```

* To see a specific command help, write the command and then add the help switch:

    ``` powershell
    # e.g for the project "init" command use:
    cmf init -h

    # e.g to list available package types use:
    cmf new --help

    # e.g. to get the details for "new business" command use:
    cmf new business --help
    ```

## Debug-Level Messages

Expected command errors (`CliException`), such as missing tenant information, normally show only the error message; use debug logging to see their stack traces. Unexpected exceptions always show full details, including stack traces and inner exceptions, regardless of the log level. Failures still return nonzero exit codes; existing telemetry handling remains in place.

Controlled errors also include missing non-interactive login arguments, invalid credential configuration or portal tokens, credential-file access failures, duplicate package dependencies, missing workspace files, unsupported MES dependency versions, and SMB file operation failures. These retain their existing exit codes. Programming errors and cancellation are not converted into controlled credential errors.

To enable debug-level logs on the console when running `cmf` commands, use the  `-l Debug` option before any other  parameters.

Per example, to active debug messages for the `cmf build` command, use:

```powershell
cmf -l Debug build
```

## Version Check

The CLI checks for a newer version at startup. This can be skipped via an environment variable:

`cmf_cli_skip_version_check` - skip the version check at startup. Useful in CI/CD pipelines to avoid network calls. Set to `true` to skip, do not set or set to `false` to keep the default behavior.

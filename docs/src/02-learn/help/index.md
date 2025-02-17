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

To enable debug-level logs on the console when running `cmf` commands, use the  `-l Debug` option before any other  parameters.

Per example, to active debug messages for the `cmf build` command, use:

```powershell
cmf -l Debug build
```

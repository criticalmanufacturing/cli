# Help

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

# login sync

<!-- BEGIN USAGE -->

Usage
-----

```
cmf login [<repositoryType> [<repository>]] sync [options]
```

Sync credentials from the `.cmf-auth.json` file into each specific tool (npm, nuget, docker, etc...) configuration files. Useful mostly in situations where the login command was performed on a different machine or the creation of the auth file was performed manually.

### Arguments

Name | Description
---- | -----------
`<CIFS|Docker|NPM|NuGet|Portal>` | Type of repository for login (values: portal, docker, npm, nuget, cifs)
`<repository>` | URL of repository for login

### Options

Name | Description
---- | -----------
`-?, -h, --help` | Show help and usage information

<!-- END USAGE -->

## Overview

This command reads the authentication from the [`.cmf-auth.json`](../login#cmf-auth-file), and syncs it into NPM, Docker, NuGet.

By default, the command also checks if the official CM Portal token is missing, is expired or is expiring soon (with a 5 days threshold), and if so, it will perform the login (equivalent to running `cmf login --store-only`). 

!!! note Disable Automatic Token Renewal
    Although not normally recommended, this step can be disabled through an environment variable:
    ```shell
    cmf_cli_disable_portal_token_renew=1
    ```

The command also syncs the credentials based on their repository types. Syncing is the operation of logging into the repository using the official tools. Depending on the repository type, it has two methods to perform the sync, either writing directly to a **file** used by the tool, or running a **command** from the tool and letting it handle the rest.

Repository Type | Method      | Details
--------------- | ----------- | --------
**Portal**      | File        | `{ApplicationData}/cmfportal/cmfportaltoken`
**NPM**         | File        | `{Home}/.npmrc`
**NuGet**       | File        | **Windows** `{ApplicationData}/NuGet/NuGet.config`<br />**Linux** `{HOME}/.nuget/NuGet/NuGet.config`
**Docker**      | Command     | `docker login <repoUrl> -u <username> -p <password>`
**CIFS**        | N/A         | _CIFS authentication is only used internally by the cmf cli itself and thus needs no synchronization_

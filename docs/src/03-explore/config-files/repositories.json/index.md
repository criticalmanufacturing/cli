# repositories.json

This document is all you need to know about what's required in your **repositories.json** file.

## Settings

### CIRepository

Path that points to a folder that contain packages that are treat as Continuous Integration packages.

`cmf publish --ci` publishes to this repository, and `cmf assemble` uses it when `--cirepo` is not passed. `cmf assemble` copies only the dependencies found in this repository to its output.

When the project is created with `cmf init --deploymentDir`, this entry points to the `development` branch folder, `<deploymentDir>/CIPackages/development`. To use another branch, edit this entry.

### Repositories

Array of paths that point to folders that contain package that are treat as official (i.e. upstream dependencies or already releases packages).

When publishing packages to an NPM feed, CLI v6 uses streaming uploads by default to reduce memory usage. Set the environment variable `cmf_cli_feature__use_streaming_publish` to `false` or `0` to disable streaming publishing.

## Example

Follows an example of `repositories.json` file.

```json
{
    "CIRepository": "\\\\fileshare\\my-continuous-integration\\packages\\repository",
    "Repositories": [
        "\\\\fileshare\\my-official\\packages\\repository",
        "\\\\fileshare\\my-released\\packages\\repository"
    ]
}
```

Example generated on Linux by `cmf init --deploymentDir /home/user/deployments`:

```json
{
    "CIRepository": "/home/user/deployments/CIPackages/development",
    "Repositories": [
        "/home/user/deployments/Delivered"
    ]
}
```

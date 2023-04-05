# restore

### Description
`cmf restore` allows fetching development dependencies from Deployment Framework (DF) packages, as an alternative to the stricter NuGet and NPM packages.

### How it works
Running this command, any dependencies defined in `cmfpackage.json` will be obtained from the configured repositories (either specified via command option or registed in the `repositories.json` file) and are then unpacked to the `Dependencies` folder inside the package. Then each solution can add references/link packages from the Dependencies folder.

<!-- BEGIN USAGE -->

Usage
-----

```
cmf restore [options] <packagePath>
```

### Arguments

Name | Description
---- | -----------
`<packagePath>` | Package path

### Options

Name | Description
---- | -----------
`-r, --repo, --repos <repos>` | Repositories where dependencies are located (folder)
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

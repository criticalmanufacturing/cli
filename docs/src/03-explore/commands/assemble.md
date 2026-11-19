# assemble

### Description

cmf assemble is a command that will read all the dependencies of a given package of type Root, and assemble it together.

If a [`repositories.json`](../config-files/repositories.json/index.md) file is available in the working directory, the repos will be also read from that file.

The command copies the root package and the dependencies found in the CI repository to the output directory. Dependencies found in other repositories are not copied; their locations are written to `dependencies.json` in the output directory.

The CI repository is `--cirepo` or, when omitted, the `CIRepository` entry of `repositories.json`. One of them is required.

Missing dependencies are handled as follows:

* A missing mandatory dependency stops the command with an error that lists the packages not found.
* A missing optional dependency (`"mandatory": false` in [`cmfpackage.json`](../config-files/cmfpackage.json/index.md#dependencies)) is skipped with a warning.
* Repository folders that do not exist are ignored.

Run `cmf assemble -h` to get a list of available arguments and options.

<!-- BEGIN USAGE -->

Usage
-----

```
cmf assemble [options] [<workingDir>]
```

### Arguments

Name | Description
---- | -----------
`<workingDir>` | Working Directory [default: .]

### Options

Name | Description
---- | -----------
`-o, --outputDir <outputDir>` | Output directory for assembled package [default: Assemble]
`--cirepo <cirepo>` | Repository where Continuous Integration packages are located (url or folder)
`-r, --repo, --repos <repos>` | Repository or repositories where published dependencies are located (url or folder)
`--includeTestPackages` | Include test packages on assemble
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

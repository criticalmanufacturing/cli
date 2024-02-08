# assemble

### Description

cmf assemble is a command that will read all the dependencies of a given package of type Root, and assemble it together.

If a [`repositories.json`](../api/repositories.json/index.md) file is available in the working directory, the repos will be also read from that file.

The command will copy all defined dependencies from the repos, and paste it on the defined outputDir. If the package is not found, and error is thrown.

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

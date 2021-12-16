<!-- BEGIN USAGE -->

Usage
-----

```
cmf bump [<packagePath>] iot customization [options] [<packagePath>]
```

### Arguments

Name | Description
---- | -----------
`<packagePath>` | Package Path [default: .]

### Options

Name | Description
---- | -----------
`-v, --version <version>` | Will bump all versions to the version specified
`-b, --buildNrVersion <buildNrVersion>` | Will add this version next to the version (v-b)
`-pckNames, --packageNames <packageNames>` | Packages to be bumped
`-isToTag` | Instead of replacing the version will add -$version [default: False]
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

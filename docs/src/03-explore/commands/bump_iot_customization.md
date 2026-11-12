# bump iot customization

The final version, including any suffix, must be an exact npm SemVer such as `1.2.3` or `1.2.3-beta.1+build.4`. Short (`1.2`) and four-part (`1.2.3.4`) versions are rejected before files change. Empty suffixes are ignored; package versions below 10 remain supported.

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
`-p, --preReleaseVersion <preReleaseVersion>` | Will add this version as pre-release version (v-p)
`-pckNames, --packageNames <packageNames>` | Packages to be bumped
`-isToTag` | Instead of replacing the version will add -$version [default: False]
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

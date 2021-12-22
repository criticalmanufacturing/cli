<!-- BEGIN USAGE -->

Usage
-----

```
cmf bump [<packagePath>] iot configuration [options] [<path>]
```

### Arguments

Name | Description
---- | -----------
`<path>` | Working Directory [default: .]

### Options

Name | Description
---- | -----------
`-v, --version <version>` | Will bump all versions to the version specified
`-b, --buildNrVersion <buildNrVersion>` | Will add this version next to the version (v-b)
`-md, --masterData` | Will bump IoT MasterData version (only applies to .json) [default: False]
`-iot` | Will bump IoT Automation Workflows [default: True]
`-pckNames, --packageNames <packageNames>` | Packages to be bumped
`-r, --root <root>` | Specify root to specify version where we want to apply the bump
`-g, --group <group>` | Group of workflows to change, typically they are grouped by Automation Manager
`-wkflName, --workflowName <workflowName>` | Specific workflow to be bumped
`-isToTag` | Instead of replacing the version will add -$version [default: False]
`-mdCustomization` | Instead of replacing the version will add -$version [default: False]
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

# new iot

<!-- BEGIN USAGE -->

Usage
-----

```
cmf new iot [options] [<workingDir>] [command]
```

### Arguments

| Name           | Description                   |
| -------------- | ----------------------------- |
| `<workingDir>` | Working Directory [default: ] |

### Options

| Name                                                     | Description                                                                                                                                      |
| -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| `--version <version>`                                    | Package Version [default: 1.0.0]                                                                                                                 |
| `--isAngularPackage <isAngularPackage> (OPTIONAL)`       | If true the Customization package is an angular package. [default: false]                                                                        |
| `--htmlPackageLocation <htmlPackageLocation> (OPTIONAL)` | Location of the HTML Package (This is mandatory only if the package is an angular package and creates a link between the iot package and the UI) |
| `-?, -h, --help`                                         | Show help and usage information                                                                                                                  |

### Commands

| Name                          | Description  |
| ----------------------------- | ------------ |
| `configuration <path>`        | [default: .] |
| `customization <packagePath>` | [default: .] |


<!-- END USAGE -->

# new

Run package-scaffolding commands inside an initialized CMF project. Outside a project, they report an actionable error directing you to `cmf init` before creating files or starting interactive prompts. IoT generators additionally require the appropriate IoT package context.

<!-- BEGIN USAGE -->

Usage
-----

```
cmf new [options] [command]
```

### Options

Name | Description
---- | -----------
`--reset` | Reset template engine. Use this if after an upgrade the templates are not working correctly.
`-?, -h, --help` | Show help and usage information

### Commands

Name | Description
---- | -----------
`business <workingDir>` | [default: ]
`database <workingDir>` | [default: ]
`data <workingDir>` | [default: ]
`feature <packageName> <workingDir>` | [default: ]
`help <workingDir>` | [default: ]
`html <workingDir>` | [default: ]
`iot <workingDir>` | [default: ]
`securityPortal <workingDir>` | [default: ]
`test` |


<!-- END USAGE -->

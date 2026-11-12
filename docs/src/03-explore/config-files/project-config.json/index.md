# .project-config.json

The CLI validates nonblank version fields when loading the project configuration. Errors identify the file, field, rejected value, and expected format before processing changes files. Fields may be omitted, null, or blank unless required by a command.

| Field | Format | Required |
| --- | --- | --- |
| `MESVersion` | NuGet | By commands that use it, including `cmf new test` |
| `NugetVersion` | NuGet | No |
| `TestScenariosNugetVersion` | NuGet | By `cmf new test` |
| `DevTasksVersion`, `HTMLStarterVersion`, `YoGeneratorVersion` | Semantic version | No; used in MES 10/11 configurations |
| `NGXSchematicsVersion` | npm version or dist-tag | Existing defaults apply |

Both version formats support prereleases and build metadata. NuGet also accepts four-part versions such as `1.2.3.4`; semantic versions use `major.minor.patch`. Schematics dist-tags such as `release-1120` remain supported.

The CLI requires MES 10 or later at command entry; loading configuration alone does not enforce that minimum or require `MESVersion`. Other versions may be below 10. MES 12+ selects the V2 model; earlier or omitted MES versions select V1.

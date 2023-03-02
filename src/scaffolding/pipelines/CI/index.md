## Continuous Integration (CI) process

### CI-Changes
This pipeline calculates all changed packages and triggers a run of CI-Package. The steps are as follows:
1. Determine which packages are affected by comparing the HEAD commit of each `cmfpackage.json` file in the repository between the main branch and the last built HEAD (value persisted in an AzureDevOps VariableGroup called BuiltHEADs).
1. Trigger CI-Package for all changed packages.

### CI-Package
This pipeline builds the packages specified by the parameter `packages` (a YAML object), which is automatically filled in by CI-Changes. The steps are as follows:
1. Invoke for each package:
   1. Run `cmf build --test`, making sure the changed packages build and their unit tests pass.
   1. Run `cmf pack`, to generate a package that can be installed via DevOps Center or Critical Manufacturing Setup. This package is then placed in the CI repository (as defined in the variable **CIPackages** in `GlobalVariables.yml`)
   1. Set current git HEAD as the last built HEAD in an AzureDevOps VariableGroup called BuiltHEADs.

The pipeline process has no bearing on the package name: each package name and version are specified in their manifest (`cmfpackage.json`) file. As such, when developing new functionality on a released package, don't forget to [bump](../commands/bump.md) its version first, regarding the type of the changes to bump the patch, minor or major numbers. The PR process will not allow merging a package with an ID/Version pair which has already been approved for release.

### CI-Publish
At least once a day, this pipeline runs to assemble all the packages in an AzureDevops artifact that should be included in a release run for E2E testing.

It does this by calculating our current package tree, starting from root, and checking which of these packages are already present in the Deployed repository. If they exist there, they were already released and should not be considered release candidates. All remaining packages are release candidates and should be included in a release run. This calculation is performed entirely by the `cmf`  CLI tool, using the `cmf assemble` command.

The release candidates are packed and published as a build artifact, as our Package artifact. As well, the environment configurations are packed into our Configurations artifact, and the relevant test packages are packed as our Tests artifact.
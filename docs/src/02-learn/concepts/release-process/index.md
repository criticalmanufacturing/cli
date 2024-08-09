# Release Process

By default, @criticalmanufacturing/cli scaffolding does not provide any built-in CI/CD pipelines, giving you the flexibility to choose any tool/platform that suits your needs.

However, on following sub-sections we share as a reference our internal process, that you can use to build your own pipeline.

### Pull Requests (PRs) Pipeline

For each changed package, we run the command `cmf build --test`, which compiles the package and runs unit tests if available, comparing with the target branch.

!!! note

    A package is considered as "changed" when any file is
    modified inside a folder with a ***cmfpackage.json***
    file.

An alternative is to run `cmf build --test` for all packages.

### Continuous Integration (CI) Piepeline

After merging code into the main branch, we perform the following steps:

1. Run `cmf build --test` to ensure the successful building of all packages and execution of unit tests.
2. Run `cmf pack -o {{packages_ci_out_dir_path}}` to generate a package that can be installed via DevOpsCenter or Critical Manufacturing Setup.

### Continuous Deployment (CD) Pipeline

Install the package on the target MES environment (according to below instructions) and run the regression tests.

=== "Containers Environment"

    1. Follow the instructions in the [DevOps Center documentation](https://portal.criticalmanufacturing.com/Help/devops-center/guide/).
    2. Copy the generated packages to the folder defined in volume **Boot Packages**.
    3. In the Configuration > General Data step, set the Package to Install as `RootPackageId@PackageVersion`.

=== "Traditional Environment (Windows VMs)"

    1. Follow the Traditional Installation instructions in the [MES documentation](https://help.criticalmanufacturing.com/9.1/InstallationGuide/Installation).
    2. In the Package Sources step, add the path where your packages are located ({{packages_ci_out_dir_path}} specified on the pack command).

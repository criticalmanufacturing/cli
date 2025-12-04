# Release Process

By default, @criticalmanufacturing/cli scaffolding does not provide any built-in CI/CD pipelines, giving you the flexibility to choose any tool/platform that suits your needs.

However, on following sub-sections we share some commands that should be used to build your own pipeline.

### Pull Request validation

Changed packages can be validated by running the command `cmf build --test`, within the package folder.

This package validation can, and should, be done only on changed packages.

!!! note

    A package can be considered "changed" when any file is
    modified inside a folder with a ***cmfpackage.json***
    file.

### Continuous Integration

After the changes are merged into the main branch, you can perform the following steps to have a package to be installed:

1. `cmf build --test` that builds and validates the packages
2. `cmf pack -o {{packages_ci_out_dir_path}}` to generate a package that can be installed via DevOpsCenter or Critical Manufacturing Setup.

### Continuous Deployment

Install the package on the target MES environment (according to below instructions) and run the regression tests.

=== "Containers Environment"

    1. Follow the instructions in the [DevOps Center documentation](https://portal.criticalmanufacturing.com/Help/devops-center/guide/devops_center_main_page_about/).
    2. Copy the generated packages to the folder defined in **Installation Data Path**.
    3. In the Configuration > General Data step, set the Package to Install as `RootPackageId@PackageVersion`.

=== "Traditional Environment (Windows VMs)"

    1. Follow the Traditional Installation instructions in the [MES documentation](https://help.criticalmanufacturing.com/9.1/InstallationGuide/Installation).
    2. In the Package Sources step, add the path where your packages are located ({{packages_ci_out_dir_path}} specified on the pack command).

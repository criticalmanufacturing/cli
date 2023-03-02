# Pull Request (PR) process

## PR-Changes
This pipeline calculates all changed packages and triggers a run of PR-Package. The steps are as follows:
1. Check if all PR-linked work items have test cases.
1. Determine which packages are affected by comparing the HEAD commit of each `cmfpackage.json` file in the repository between the PR branch and the target branch.
1. Trigger PR-Package for all changed packages.

## PR-Package
This pipeline builds the packages specified by the parameter `packages` (a YAML object), which is automatically filled in by PR-Changes. The steps are as follows:
1. Check if any of the triggered packages have already been released (by released we mean: package already exist in the folder defined in the variable **ApprovedPackages** in `GlobalVariables.yml`).
2. Invoke for each package `cmf build --test`, making sure the changed packages build and their unit tests pass.
If any of the previous steps fail, the PR-Changes pipeline (which triggered PR-Package) will fail, and the Pull Request will be blocked. The developer then has to correct the code, push it to the PR branch, and the process starting with PR-Changes will run again automatically to validate the code.

By following this process, we can ensure that code changes are properly tested before they are merged into the main branch.
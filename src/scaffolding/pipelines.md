# Pipelines description
This page gives an overview of our pipeline design and working mode. For specific details, you can consult the generated pipelines, by running the `cmf init` command as detailed [here](./index.md).     
    
> Please remember that these pipelines are tailored to CM's internal structure and are not expected to run unmodified in your infrastructure. They are provided as examples and should not replace your own pipelines adapted to you specific process.

## Pull Request (PR) process
The Pull Request process runs each time a Pull Request is opened or code is pushed to the request branch. It is comprised of two pipelines.

### PR-Changes
The PR-Changes pipeline calculates which packages are affected by which git commits. It does this by comparing the HEAD commit of each `cmfpackage.json` file in the repository between the PR branch and the target branch. For each changed package, a PR-Package pipeline run is triggered.

### PR-Package
This pipeline builds the package by invoking `cmf build --test`, making sure the changed packages build and their unit tests pass. If this build fails, the Pull Request is blocked. The developer then has to correct the code, push it to the PR branch, and the process starting with PR-Changes will run again automatically to validate the code.

## Continuous Integration (CI) process
Our CI process consists of 2 pipelines responsible for generating packages anytime our main branch changes. 

### CI-Changes
The CI-Changes pipeline calculates which packages are affected by which git commits. It does this by comparing the last built HEAD for that specific package. For each altered package, a run of CI-Package is triggered.

### CI-Package
This pipeline builds the package specified by its parameters, which are filled in automatically by CI-Changes.

The build process happens exclusively using the `cmf` CLI tool. The pipeline invokes `cmf build --test`, which builds the package and runs its automatic test suite. If successful on both steps, it then invokes `cmf pack` to generate a package that can be installed via DevOps Center or Critical Manufacturing Setup. This package is then placed in the CI repository (as defined in the variable CIPackages in `GlobalVariables.yml`) and the current git HEAD is registered as the last built HEAD for this package.

The pipeline process has no bearing on the package name: each package name and version are specified in their manifest (`cmfpackage.json`) file. As such, when developing new functionality on a released package, don't forget to [bump](../commands/bump.md) its version first, regarding the type of the changes to bump the patch, minor or major numbers. The PR process will not allow merging a package with an ID/Version pair which has already been approved for release.

### CI-Publish
At least once a day, this pipeline runs to calculate which packages should be included in a release run for E2E testing.

It does this by calculating our current package tree, starting from root, and checking which of these packages are already present in the Deployed repository. If they exist there, they were already released and should not be considered release candidates. All remaining packages are release candidates and should be included in a release run. This calculation is performed entirely by the `cmf`  CLI tool, using the `cmf assemble` command.

The release candidates are packed and published as a build artifact, as our Package artifact. As well, the environment configurations are packed into our Configurations artifact, and the relevant test packages are packed as our Tests artifact.

## Continuous Delivery (CD) process
Our CD process doesn't actually deliver to client environments, but to our production-like Integration environments. It can be executed by one of two pipelines, depending if we're targeting a Windows machine or a Container installation. However, both pipelines implement different approaches to the same process:

1. Restore the environment to a previous known good state - this can be either the same MES and customization version currently running in Production at the client if we're building cumulative packages (not recommended) or the last sprint's delivered versions for client testing when building iterative packages.
1. Install Release Candidate packages, and their MES dependency if needed - perform the actual package installations.
1. Create an environment restore point - this restore point will be used the next known good state if these release candidates are approved for release.
1. Load test master data - load additional data that helps with testing.
1. Create the Daily backup - backup only the ONLINE database, for development use. This backup can be loaded locally in our development machines to support our local environments. It contains the test master data, which is useful for development tests.
1. Run E2E tests - run the tests that require the full environment to be installed. We run the Backend, Frontend and IoT tests separately, though this is a technicality.
1. Approval gate - this is a manual step. If the team is happy with this run and with the quality of the Release Candidate packages, this run can be approved and the release candidate packages can be released to the client. 
1. Set Known Good State - this only runs if the run is approved. It will set the Restore Point created in step 3 as the known good state for the next run.

The pipelines are as follows:

### CI-Release (for Windows machine installations)
This pipeline needs to backup the entire MES installation. As such, both the database and application layers are backed-up. The tool most used in this process is the CmfDeploy client, which is part of the Deployment Framework that ships in the Critical manufacturing disks/ISOs. The details of each step are:

1. restore - this uses the Cmf.BackupRestore.Tools (installed via CmfDeploy) to restore both the database and the application layer to the backed-up state. 
	1. Running CmfDeploy uses Powershell remoting to mount the ISO file in the target machine and run the installation from there, as this is a requirement when changing the application layer. This is true for all steps.
	> Beware that the application layer part includes only the business tier, the HTML and Help sites. Which means these restores do not work if the MES version changes: if we upgraded MES and then restored to a backup in a previous version, we'd get a mixed environment that will most likely break.
1. Install the Release Candidate root package using CmfDeploy. All already installed packages will be skipped, and only new packages will be installed.
1. Use Cmf.backupRestore.Tools to create a temporary restore point including all databases and application the 3 layer solutions (check the NOTE at step 1).
1. Load the test master data files (using CmfDeploy). The test master data files are also DF packages, though not usually shipped to the client. They are installed the same way as the actual system upgrades.
1. Create the Daily backup - use CmfDeploy with Cmf.BackupRestore.Tools to backup only the ONLINE database
1. Run E2E tests - this is the only part that runs on the build server: it uses the VsTest azure task to run all E2E tests, from each layer in turn, Business, HTML, IoT.
1. Approval Gate
1. Set Known Good State - rename and move the backups so they are picked up by the next run

### CD-Containers
Unlike the Windows machine pipeline, this pipeline always reinstalls MES by using brand new containers. This makes the process more reliable and the backups quicker. The most used tool in this process is `portal-sdk`, a `cmf` CLI plugin that can be installed from `npm`.

1. restore - for containers, we don't actually restore anything, we simply drop all databases to reinstall MES
1. Installation - create a new version of our Integration stack by using the portal-sdk plugin. Set as target package the release candidate root package. This package depends on a specific MES versio which will get installed first. This installation also uses the Known Good State backups to maintain the database contents as they were on the last approved release run.
1. Use CmfDeploy to create a temporary database backup, by using the CmfDeploy `backup` command.
1. Load the test MD by creating a new version of our environment using as target the Test MD package. If we have multiple test MD packages, we do this once for each one.
1. Create the Daily backup by the exact same process as step 3, but only backup the ONLINE database.
1. Run E2E tests - this is similar to the Windows machine scenario
1. Approval Gate
1. set know good state, by moving the backups created in step 3 (ONLINE, ODS and DWH) to a known location which the next run will use as a known good state. These backups can't be renamed, so we just keep them separate until approval.
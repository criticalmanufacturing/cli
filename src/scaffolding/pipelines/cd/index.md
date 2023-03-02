# Continuous Delivery (CD) process

1. Restore the environment to a previous known good state - this can be either the same MES and customization version currently running in Production at the client if we're building cumulative packages (not recommended) or the last sprint's delivered versions for client testing when building iterative packages.
1. Install Release Candidate packages, and their MES dependency if needed - perform the actual package installations.
1. Create an environment restore point - this restore point will be used the next known good state if these release candidates are approved for release.
1. Load test master data - load additional data that helps with testing.
1. Create the Daily backup - backup only the ONLINE database, for development use. This backup can be loaded locally in our development machines to support our local environments. It contains the test master data, which is useful for development tests.
1. Run E2E tests - run the tests that require the full environment to be installed. We run the Backend, Frontend and IoT tests separately, though this is a technicality.
1. Approval gate - this is a manual step. If the team is happy with this run and with the quality of the Release Candidate packages, this run can be approved and the release candidate packages can be released to the client.
1. Set Known Good State - this only runs if the run is approved. It will set the Restore Point created in step 3 as the known good state for the next run.

The pipelines are as follows:

## CD-VM (for Windows machine installations)
**Pre-Requirements**
AzureDevOps Agent in the target machine (installed running )


This pipeline requires a backup of the MES databases. As such, before run this pipeline
a database backup should exist
The tool most used in this process is the CmfDeploy client, which is part of the Deployment Framework that ships in the Critical manufacturing disks/ISOs. The details of each step are:

1. Artifacts - this step download all the required artifacts that will be used during the pipeline:
	- Configurations
	- Packages
	- Test Packages
	- Dependencies

2. Prepare Environment
   - Drop databases
   - Stop MES services on target VM

3. Installation
   - Install the Release Candidate root package using CmfDeploy. All already installed packages will be skipped, and only new packages will be installed.

4. restore - this uses the Cmf.BackupRestore.Tools (installed via CmfDeploy) to restore both the database and the application layer to the backed-up state.
	1. Running CmfDeploy uses Powershell remoting to mount the ISO file in the target machine and run the installation from there, as this is a requirement when changing the application layer. This is true for all steps.
	> Beware that the application layer part includes only the business tier, the HTML and Help sites. Which means these restores do not work if the MES version changes: if we upgraded MES and then restored to a backup in a previous version, we'd get a mixed environment that will most likely break.
5. Install the Release Candidate root package using CmfDeploy. All already installed packages will be skipped, and only new packages will be installed.
6. Use Cmf.backupRestore.Tools to create a temporary restore point including all databases and application the 3 layer solutions (check the NOTE at step 1).
7. Load the test master data files (using CmfDeploy). The test master data files are also DF packages, though not usually shipped to the client. They are installed the same way as the actual system upgrades.
8. Create the Daily backup - use CmfDeploy with Cmf.BackupRestore.Tools to backup only the ONLINE database
9.  Run E2E tests - this is the only part that runs on the build server: it uses the VsTest azure task to run all E2E tests, from each layer in turn, Business, HTML, IoT.
10. Approval Gate
11. Set Known Good State - rename and move the backups so they are picked up by the next run

## CD-Containers
This pipeline always reinstalls MES by using brand new containers. This makes the process more reliable and the backups quicker. The most used tool in this process is `portal-sdk`, a `cmf` CLI plugin that can be installed from `npm`.

1. restore - for containers, we don't actually restore anything, we simply drop all databases to reinstall MES
2. Installation - create a new version of our Integration stack by using the portal-sdk plugin. Set as target package the release candidate root package. This package depends on a specific MES versio which will get installed first. This installation also uses the Known Good State backups to maintain the database contents as they were on the last approved release run.
3. Use CmfDeploy to create a temporary database backup, by using the CmfDeploy `backup` command.
4. Load the test MD by creating a new version of our environment using as target the Test MD package. If we have multiple test MD packages, we do this once for each one.
5. Create the Daily backup by the exact same process as step 3, but only backup the ONLINE database.
6. Run E2E tests - this is similar to the Windows machine scenario
7. Approval Gate
8. set know good state, by moving the backups created in step 3 (ONLINE, ODS and DWH) to a known location which the next run will use as a known good state. These backups can't be renamed, so we just keep them separate until approval.
# Initialize a Project

This tutorial walks you through initializing a new CM MES project using the `cmf init` command.

## 1. Create the project folder

Create a folder to store your new customization project files.

!!! warning

    On Windows, some applications and libraries do not support file paths longer than 256 characters. 
    CM MES customization projects have long file paths. To avoid problems on Windows OS, you should:

    * Use short project file names;
    * Initialize your projects on a folder as nearest as possible to the filesystem drive root.

## 2. Open PowerShell command line

Open a PowerShell terminal and navigate to your new project folder.

## 3. Check Node.js and NPM Version

Run the following commands to check your current versions:

```pwsh
# Check Node.js version
node -v

#Check NPM version
npm -v
```

Validate that their versions match the compatibility list stated in the [installation guide](../../../../01-install/index.md).
If needed, use `nvm` command to fix it.

## 4. Initialize Project

You can initialize a MES Customization or App project workspace using the `cmf init` command. The following examples illustrate its usage.

!!! note "Software and MES ISO Dependencies"

    * **Dependencies Version**: To determine the software dependencies version to use on the `init` command, check the instructions on the [Project Types Concept](../../../concepts/project-types/index.md) page.
    * **MES ISO location is optional**: You only need to provide this parameter if your MES or one of its optional components **runs** on a Windows environment. More details on MES components are available on the [MES System Architecture][help-MES-architecture] page.

=== "MES v10 or above"

    ```powershell
    cmf init test --tenant test --baseVersion 12.0.0-beta.2 --ciRepo https://dev.criticalmanufacturing.io --releaseRepos https://dev.criticalmanufacturing.io
    ```

    No JSON files are required. Provide the project name and `--baseVersion` (also known as `--MESVersion`), plus `--tenant` unless supplied by `--config`. Also provide `--ciRepo` and `--releaseRepos`, or the legacy `--deploymentDir` alternative.

    The package version defaults to `1.0.0`. NuGet and test scenario versions default to the MES version; ngx-schematics uses `release-<digits>` for stable releases and the exact MES version for prereleases. Override these versions only when needed.

    `--config` and `--infrastructure` (alias `--infra`) are optional. Registry URLs resolve independently from command-line options, then the infrastructure file, then the defaults: `https://criticalmanufacturing.io/repository/npm/` and `https://criticalmanufacturing.io/repository/nuget/index.json`. Use `--npmRegistry` or `--nugetRegistry` to override them; existing credential options remain supported.

    For a full environment setup, add `--config ..\config\env.json`. MES v10/v11 HTML scaffolding still uses its domain and environment settings; test scaffolding uses its hostname and ports. A supplied file is copied unchanged into `EnvironmentConfigs`, including for MES v12.

=== "MES v9 or below"

    These legacy projects require CM CLI **5.8.0 or earlier**.

    ```powershell
    cmf init ExampleProject `
        --version 1.0.0 `
        --infra ..\config\infra.json `
        --config ..\config\ExampleEnvironment.json `
        --MESVersion 9.0.11 `
        --nugetVersion 9.0.11 `
        --testScenariosNugetVersion 9.0.11 `
        --deploymentDir \\vm-project\Deployments `
        --ISOLocation \\setups\CriticalManufacturing.iso `
        --DevTasksVersion 8.1.3 `
        --HTMLStarterVersion 8.1.1 `
        --yoGeneratorVersion 3.1.0
    ```

=== "MES App"

    ```powershell
    cmf init ExampleProject `
        --tenant test `
        --baseVersion 11.0.0 `
        --ciRepo https://dev.criticalmanufacturing.io `
        --releaseRepos https://dev.criticalmanufacturing.io `
        --appName "My App" `
        --appId "MyApp" `
        --appAuthor "Critical Manufacturing" `
        --appDescription "My First App" `
        --appLicensedApplication "My App" `
        --repositoryType "App"
    ```

## 5. Review the created project structure

The `cmf init` command should have terminated with success and created a basic project structure similar to:

```log
📦ExampleProject
┣ 📂.config               # Dotnet tools configuration
┃ ┗ 📜dotnet-tools.json
┣ 📂EnvironmentConfigs    # Environments configuration repository
┃ ┗ 📜ExampleEnvironment.json
┣ 📂Libs                  # External libs dependencies (binaries)
┃ ┗ 📂...
┣ 📜.gitignore            # Spec files to ignore
┣ 📜.project-config.json  # Project configuration used during scaffolding
┣ 📜cmfpackage.json       # Project root package
┣ 📜global.json           # Dotnet global.json
┣ 📜NuGet.Config          # NuGet repository configuration
┗ 📜repositories.json     # The build/release repositories configuration
```

!!! note

    The initial project structure may vary, depending on the CM CLI
    version and the project type selected (`--repositoryType` argument).
    The environment JSON shown above is only copied when `--config` is supplied.

## 6. Validate `repositories.json`

Verify the `repositories.json` file in the project root folder conforms to the [specification](../../../../03-explore/config-files/repositories.json/index.md).

## 7. Add LBOs SDK

Store your environment's LBOs in the `Libs\LBOs` directory of your project.

## 8. Store project on source control

Use a source control system (like Git) to manage your project versions.

Store the result of `cmf init` in the source control.

!!! note

    The CM CLI assumes that you are using `git`. If that is not
    the case, adapt `.gitignore` files to your source control system.

[help-MES-architecture]: https://help.criticalmanufacturing.com/installationguide/systemarchitecture/

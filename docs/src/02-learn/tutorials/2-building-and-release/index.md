# Build and Release

This tutorial guides you through building and releasing your MES customization project using the CM CLI.

## Building Packages

### Option A. Building Individual Packages

1. **Navigate to the Package Directory:** Locate the directory containing the `cmfpackage.json` file for the specific package you want to build.
2. **Execute the Build Command:** Run the following command in the package directory:

   ```powershell
   cmf build --test
   ```

   * The `--test` flag (optional) executes unit tests associated with the package during the build process.

### Option B. Building All Packages Recursively

1. **Navigate to Project Root:** Open your terminal and navigate to the root directory of your MES project.
2. **Execute the Script:** Run the following PowerShell script to build all packages within the project recursively:

    ```powershell
    # Get all directories containing a cmfpackage.json file
    $directories = Get-ChildItem -Recurse -Directory | Where-Object { Test-Path "$($_.FullName)\cmfpackage.json" }

    # Iterate through each directory and execute the cmf build --test command
    foreach ($directory in $directories) {
        cd $directory.FullName
        Write-Host "Building package in directory: $($directory.FullName)"
        cmf build --test
        cd ..
    }
    ```

   * This script finds all subdirectories containing a `cmfpackage.json` file and executes the `cmf build --test` command in each directory, effectively building all packages within your project.

## Packaging

Once you've built your packages, use the `cmf pack` command
to transform them into a CM Deployment Framework (DF) package
suitable for deployment:

1. **Navigate to Project Root:** Ensure you're in the root directory of your MES project.
2. **Package the Root Package:** Run the following command to build and package the root project:

   ```powershell
   cmf build --test
   cmf pack -o {{ci_repo_path}}
   ```

   * Replace `{{ci_repo_path}}` with the desired output path for the deployment package. If omitted, the package will be created in the "./Package" directory.

3. **Package All Built Subdirectories** Utilize the same script from the building process (modified slightly) to package all built subdirectories:

   ```powershell
   # Get all directories containing a cmfpackage.json file
   $directories = Get-ChildItem -Recurse -Directory | Where-Object { Test-Path "$($_.FullName)\cmfpackage.json" }

   # Iterate through each directory and execute the cmf build --test command
   foreach ($directory in $directories) {
       cd $directory.FullName
       Write-Host "Building and packing the package in directory: $($directory.FullName)"
       cmf build --test  # Exclude line if all packages have already been built
       cmf pack -o {{ci_repo_path}}
   }
   ```

   * This script assumes all packages have already been built using the previous steps. It iterates through each subdirectory, executes `cmf pack` to create deployment packages, and places them in the specified output path.

## Build Release Package

To gather a Deployment Framework package and all its dependencies and place them under same directory to create a release package, you may use the `cmf assemble` command:

``` powershell
# Go to project root
cd {{project_root_path}}

# Run the assemble command
cmf assemble
```

* The assemble command assume that the current layer package
  and all its dependencies were already build, pack and are stored
  either on `CIRepo` or `Repositories` defined on project
  `{{project_root_path}}/repositories.json` settings file.

* By default the packages found will be placed copied to the
  "./Assemble" directory.

!!! note

    Use `cmf ls` on repository root to check if all
    dependencies are available.

## Install

To install the release package, follow the process presented on the [Release Process Concept](../../concepts/release-process/index.md#continuous-deployment) page.

## Additional Considerations

### Managing Package Versions After Release

After releasing a package, any modifications necessitate updating the package's version number. Additionally, dependent packages must have their versions adjusted accordingly to maintain consistency and prevent conflicts.

**e.g.:**

Consider a project with three packages:

* **Project Root:** The main project package.
* **HTML:** Contains the user interface components.
* **HELP:** Provides project documentation, updated on every release (at least the release notes シ).

Initially, all packages are released as version 1.0.0. To introduce new UI changes, follow these steps:

**Version Bumping Procedure**

1. **Increase Project Root Version:**
    * Navigate to the project's root directory.
    * Execute `cmf bump --version 1.0.1` to increment the version to 1.0.1.

2. **Update HTML Package:**
    * Change directory to the HTML package (as it's being modified).
    * Run `cmf bump --version 1.0.1` to align its version with the project root.

3. **Adjust HELP Package (Optional):**
    * Change the directory to the Help package (as it's being modified).
    * Run `cmf bump --version 1.0.1` to align its version with the project root.

5. **Update Dependencies:**
   * Modify the `cmfpackage.json` file in the project root to reflect the new versions of dependent packages (HTML and HELP in this case).

6. Use `cmf ls` to verify that dependencies are correctly configured.

!!! note

    These guidelines ensure that your project's packages remain synchronized and manageable.

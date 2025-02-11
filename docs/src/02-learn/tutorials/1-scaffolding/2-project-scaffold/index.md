# Project Scaffold

After the project initialization, you need to start building the project structure using the `cmf new` command.

## 1. Understanding Project Scaffolds

The two main project scaffold types offered by CM CLI are:

* **Traditional Scaffold:** This is a well-suited option for projects with a simple, monolithic structure. It includes all necessary layers in a single package set.
* **Feature-Driven Scaffold:** This approach is ideal for more complex projects with multiple features and potential dependencies between them. Each feature's code and resources are organized into separate packages within the project repository.

!!! note

    Refer to the [Project Scaffold Concepts](../../../concepts/project-scaffold/index.md) page for a detailed comparison and best practices for choosing the right scaffold type.

## 2. Selecting Your Scaffold Type

* Review the information provided on the **Project Scaffold Concepts** page.
* Based on your project's complexity and needs, determine the most suitable scaffold type (Traditional or Feature-Driven).

## 3. Creating Project Layers Packages

Once you've chosen your scaffold type, proceed with creating the required project layers packages using the `cmf new` command.

!!! important

    Only add package layers that are truly necessary for your project. You can always add additional layers later on.

A full description of each package is available at [Layers Packages Concept](../../../concepts/layers-packages/index.md) page.

### 3.1. Traditional Scaffold - Step-by-Step

This section guides you through creating a traditional project scaffold. Adapt the commands based on your specific project requirements and adaptions documented on the [Layers Packages Concept](../../../concepts/layers-packages/index.md) page:

=== "MES v11 or above"

    ``` powershell
    #Go to your project root
    cd {{project_root_path}}
    
    #Create Business Package Layer
    cmf new business
    
    # Create Data Package Layer
    cmf new data --businessPackage .\Cmf.Custom.Business\
    
    # Create HTML Package Layer
    cmf new html
    
    # Create Help Package Layer
    cmf new help
    
    # Create  Test Package Layer
    cmf new test
    
    # Create Database Package Layer (Optional)
    cmf new database
    
    # Create Microsoft Reporting Services Package (Optional)
    cmf new reporting 
    
    # Create a grafana Package Layer (optional)
    cmf new grafana
    
    # Create IoT Package Layer (Optional)
    cmf new iot
    
    # Create IoT Package Layer for Angular (Optional)
    cmf new iot --isAngular true --htmlPackageLocation .\Cmf.Custom.Html
    ```
    Make sure to follow all the steps [here](../../../concepts/layers-packages/angular-17-iot-packages.md). Otherwise, you can have unexpected errors in the package. For IoT feel free to check [here](../../../../03-explore/guides/IoT-on-MES-v11/index.md).

=== "MES v10 or above"

    ``` powershell
    #Go to your project root
    cd {{project_root_path}}
    
    #Create Business Package Layer
    cmf new business
    
    # Create Data Package Layer
    cmf new data --businessPackage .\Cmf.Custom.Business\
    
    # Create HTML Package Layer
    cmf new html
    
    # Create Help Package Layer
    cmf new help
    
    # Create  Test Package Layer
    cmf new test
    
    # Create Database Package Layer (Optional)
    cmf new database
    
    # Create Microsoft Reporting Services Package (Optional)
    cmf new reporting 
    
    # Create a grafana Package Layer (optional)
    cmf new grafana
    
    # Create IoT Package Layer (Optional)
    cmf new iot --htmlPackageLocation .\Cmf.Custom.Html 
    ```
    Make sure to follow all the steps [here](../../../concepts/layers-packages/angular-15-iot-packages.md). Otherwise, you can have unexpected errors in the package. For IoT feel free to check [here](../../../../03-explore/guides/IoT-on-MES-v10/index.md)

=== "MES v9 or below"

    ``` powershell
    #Go to your project root
    cd {{project_root_path}}
    
    #Create Business Package Layer
    cmf new business
    
    # Create Data Package Layer
    cmf new data --businessPackage .\Cmf.Custom.Business\
    
    # Create HTML Package Layer
    cmf new html `
         --version 1.0.0 `
         --htmlPackage H:\packages\Cmf.Presentation.HTML.9.1.8.zip
    
    # Create Help Package Layer
    cmf new help `
        --version 1.0.0 `
        --documentationPackage H:\packages\Cmf.Documentation.9.1.8.zip
    
    # Create  Test Package Layer
    cmf new test
    
    # Create Database Package Layer (Optional)
    cmf new database
    
    # Create Microsoft Reporting Services Package (Optional)
    cmf new reporting 
    
    # Create a grafana Package Layer (optional)
    cmf new grafana
    
    # Create IoT Package Layer (Optional)
    cmf new iot
    ```

### 3.2 Feature-Driven Scaffold - Example

This section illustrates creating a feature-driven project scaffold for a project with multiple features and dependencies:

**Project Features:**

* Baseline (common features)
* FeatureX1 (optional feature)
* FeatureX2 (optional feature)
* SiteA (site-specific features)
* SiteB (site-specific features)

**Feature Dependencies:**

* SiteA depends on Baseline and FeatureX1
* SiteB depends on Baseline, FeatureX2, and FeatureY (from another repository)

**Scaffolding Steps:**

``` powershell
# Navigate to your project's root directory
cd {{project_root_path}}

# Create Test Package Layer
cmf new test

# Create individual feature packages
cmf new feature Cmf.Custom.{{TENANT}}.Baseline
cmf new feature Cmf.Custom.{{TENANT}}.FeatureX1
cmf new feature Cmf.Custom.{{TENANT}}.FeatureX2
cmf new feature Cmf.Custom.{{TENANT}}.SiteA
cmf new feature Cmf.Custom.{{TENANT}}.SiteB

# Add Package Layers to Each Feature Directory (Examples)
# (Navigate to each feature directory and create necessary layers packages)

cd {{project_root_path}}/Features/Cmf.Custom.{{TENANT}}.Baseline
cmf new business
cmf new html
...

cd {{project_root_path}}/Features/Cmf.Custom.{{TENANT}}.FeatureX1
cmf new business
cmf new html
cmf new reporting
...

# (Follow similar structure for other features)
...

# Edit cmfpackage.json and add the package dependencies
# for SiteA and SiteB feature packages
code {{project_root_path}}/Features/Cmf.Custom.{{TENANT}}.SiteA/cmfpackage.json
...
code {{project_root_path}}/Features/Cmf.Custom.{{TENANT}}.SiteB/cmfpackage.json
...

# Edit repositories.json to include the path to the directory
# with FeatureY release packages
code {{project_root_path}}/repository.json
```

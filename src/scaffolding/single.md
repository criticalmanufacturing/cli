# Traditional scaffolding

A "traditional project" does not contain [feature packages](./features.md), is developed entirely by one team in one repository and is delivered directly to one customer.

These projects are usually composed of Business, UI, Help and Master Data customization, with optionally Exported Objects and IoT.

The objective is to obtain a structure equivalent to what `solgen` provided.

Please consult each commands help page for details of what each switch does.

## Initialize the repository

These types of projects usually fully own their git repository and as such need to be initialized to obtain the base repo structure, as well as the build pipelines if we are targeting Azure DevOps.

This is done with the `cmf init` command:

=== "Classic"
    ```pwsh
    cmf init Example `
        --infra ..\COMMON\infrastructure\CMF-internal.json `
        -c Example.json `
        --repositoryUrl https://tfs.criticalmanufacturing.com/Projects/Test/_git/Test `
        --MESVersion 8.2.1 `
        --DevTasksVersion 8.2.0 `
        --HTMLStarterVersion 8.0.0 `
        --yoGeneratorVersion 8.1.1 `
        --nugetVersion 8.2.1 `
        --testScenariosNugetVersion 8.2.1 `
        --deploymentDir \\vm-project.criticalmanufacturing.com\Deployments `
        --ISOLocation \\setups\CriticalManufacturing.iso `
        --version 1.0.0
    ```
=== "Containers"
    ```pwsh
    cmf init Example `
        --infra ..\COMMON\infrastructure\CMF-internal.json `
        -c Example.json `
        --repositoryUrl https://tfs.criticalmanufacturing.com/Projects/Test/_git/Test `
        --MESVersion 8.2.1 `
        --DevTasksVersion 8.2.0 `
        --HTMLStarterVersion 8.0.0 `
        --yoGeneratorVersion 8.1.1 `
        --nugetVersion 8.2.1 `
        --testScenariosNugetVersion 8.2.1 `
        --deploymentDir \\vm-project.criticalmanufacturing.com\Deployments `
        --ISOLocation \\setups\CriticalManufacturing.iso `
        --version 1.0.0 `
        --releaseCustomerEnvironment EnvironmentName `
        --releaseSite EnvironmentSite `
        --releaseDeploymentPackage \@criticalmanufacturing\mes:8.3.1 `
        --releaseLicense EnvironmentLicense `
        --releaseDeploymentTarget EnvironmentTarget
    ```
    `EnvironmentTarget` can take any value recognized by the Portal SDK, which can be found [here](https://github.com/criticalmanufacturing/portal-sdk/blob/main/src/Common/DeploymentTarget.cs).

Note: The `` ` `` character escapes multiline commands in `powershell`. For bash, the `\` character does the same thing.

The infrastructure file specifies the repositories to be used to get the project dependencies.
If you are scaffolding a Deployment Services project, there is a CMF-internal.json infra file which specifies our internal infrastructure. It's in the **COMMON** project, **Tools** repository, at `/infrastructure`.
If scaffolding a customer or partner project, you will need to create this infrastructure file first. Check [Infrastructure](./infrastructure.md) for more details.

As in previous scenarios, the versions for the various input options must be previously determined. Unlike with `solgen`, `cmf init` will not assume default/current values for these options.

This will also create a root package, which may or may not be shipped to the customer. Unlike with `solgen`, this root package has no dependencies, initially. Each time a layer package is created, it will be registered in the higher level package found. For a traditional repository, this will be the root package.

If you are using version cmf-cli version 1 or 2, follow the instructions defined in the [Post-scaffolding package tailoring](./post-scaffolding-package-tailoring.md). You will not be able to generate the layer packages before doing this.
In version 3, this is already done by the CLI.

## Layer packages

Each application layer is deployed in a different package. This allows the team to deliver only what was actively modified during a sprint, and keep the previous versions of the unchanged layers in an installed system.


### Business
The business package is straightforward and is generated with the `cmf new business` command:

```
cmf new business --version 1.0.0
```

This creates a [.NET](https://en.wikipedia.org/wiki/.NET) solution for backend development. Please note that unlike with `solgen`, the Actions project is not included in the business solution.

### Master Data
The Master Data package includes also the Exported Objects. Unlike in a `solgen` solution, Exported Objects are loaded via Master Data and not using a specific ExportedObjects sub-package.

As the Master Data package also includes the Process Rules, it can optionally register the Actions package in a specific Business solution. For the traditional scenario, the command would be:

```
cmf new data --version 1.0.0 --businessPackage .\Cmf.Custom.Business\
```

### UI
The UI and Help packages are also scaffolded differently from a `solgen` project. To fully scaffold these solutions, the corresponding Deployment Framework package needs to be specified. These can be found in the MES ISO/disk. Make sure you use the correct version: a mismatch may cause all kinds of problems when running.

The corresponding commands are:

#### HTML
```
cmf new html --version 1.0.0 --htmlPackage H:\packages\Cmf.Presentation.HTML.8.2.1.zip
```

If you require NPM registry authentication, the current procedure is to include the auth information in the apps\customization.web\.npmrc file as is standard.

#### Help
```
cmf new help --version 1.0.0 --documentationPackage H:\packages\Cmf.Documentation.8.2.1.zip
```

### IoT
The IoT package contains both IoTData and IoTPackages as sub-packages. They are always created together.
```
cmf new iot --version 1.0.0
```

### Database
The database package contains both Pre, Post and Reporting sub-packages.
```
cmf new database --version 1.0.0
```

### Tests
The tests package is generated and built like any other layer package, but it is not installable. It is also not usually delivered to customers, unless requested.
```
cmf new test --version 1.0.0
```

## Demo

This demo show the usual initial setup for a new project. For the first sprints, which focus heavily on modelling, the Data package is of the most importance. Obviously a Tests package is also needed. As an extra, the Business package is also initialized. This allows the Process Rules in the Data package to have a .NET solution where they can be compiled for checking.

Note that the GIF is quite large and can take a bit to load.

![Traditional Scaffolding Demo](./traditional.gif "Traditional Scaffolding")

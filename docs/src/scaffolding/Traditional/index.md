# Traditional scaffolding

A "traditional project" does not contain [feature packages](./../Feature Package), is developed entirely by one team in one repository and is delivered directly to one customer.

These projects are usually composed of Business, UI, Help and Master Data customization, with optionally Exported Objects and IoT.


Please consult each commands help page for details of what each switch does.

## Initialize the repository

These types of projects usually fully own their git repository and as such need to be initialized to obtain the base repo structure.

This is done with the `cmf init` command:

=== "MES v10"
    ```pwsh
    cmf init ExampleProject `
        --infra ..\infrastructure\infra.json `
        --config ..\config\ExampleEnvironment.json `
        --MESVersion 10.0.0 `
        --ngxSchematicsVersion 10.0.0 `
        --nugetVersion 10.0.0 `
        --testScenariosNugetVersion 10.0.0 `
        --deploymentDir \\directory\Deployments `
        --ISOLocation \\directory\CriticalManufacturing.iso `
        --version 1.0.0
    ```
=== "up to MES v9"
    ```pwsh
    cmf init ExampleProject `
        --infra ..\infrastructure\infra.json `
        --config ..\config\ExampleEnvironment.json `
        --repositoryUrl https://repository.local/Projects/Test/_git/Test `
        --MESVersion 9.0.11 `
        --DevTasksVersion 9.0.4 `
        --HTMLStarterVersion 8.0.0 `
        --yoGeneratorVersion 8.1.1 `
        --nugetVersion 9.0.11 `
        --testScenariosNugetVersion 9.0.11 `
        --deploymentDir \\vm-project.criticalmanufacturing.com\Deployments `
        --ISOLocation \\setups\CriticalManufacturing.iso `
        --version 1.0.0 `
    ```

Note: The `` ` `` character escapes multiline commands in `powershell`. For bash, the `\` character does the same thing.

The infrastructure file specifies the repositories to be used to get the project dependencies.
You will need to create this infrastructure file first. Check [Infrastructure](./../infrastructure.md) for more details.

As in previous scenarios, the versions for the various input options must be previously determined. `cmf init` will not assume default/current values for these options.

This will also create a root package, which may or may not be shipped to the customer. This root package has no dependencies, initially. Each time a layer package is created, it will be registered in the higher level package found. For a traditional repository, this will be the root package.

If you are currently using version cmf-cli version 2x, follow the instructions defined in the [Post-scaffolding package tailoring](./../Post-Scaffolding%20Tailoring). You will not be able to generate the layer packages before doing this. In version 3, this is already done by the CLI.

## Layer packages

Each application layer is deployed in a different package. This allows the team to deliver only what was actively modified during a sprint, and keep the previous versions of the unchanged layers in an installed system.


### Business
The business package is straightforward and is generated with the `cmf new business` command:

```
cmf new business --version 1.0.0
```

This creates a [.NET](https://en.wikipedia.org/wiki/.NET) solution for backend development. Actions project is not included in the business solution.

### Master Data
The Master Data package includes also the Exported Objects. Exported Objects are loaded via Master Data and not using a specific ExportedObjects sub-package.

As the Master Data package also includes the Process Rules, it can optionally register the Actions package in a specific Business solution. For the traditional scenario, the command would be:

```
cmf new data --version 1.0.0 --businessPackage .\Cmf.Custom.Business\
```

### UI
=== "MES v10"
    The html and help packages are straightforward and are generated with the `cmf new html` and `cmf new help` commands:

    The corresponding commands are:

    #### HTML
    ```
    cmf new html --version 1.0.0
    ```

    If you require NPM registry authentication, the current procedure is to include the auth information in the apps\customization.web\.npmrc file as is standard.

    #### Help
    ```
    cmf new help --version 1.0.0
    ```
=== "up to MES v9"
    To fully scaffold UI and Help solutions, the corresponding Deployment Framework package needs to be specified. These can be found in the MES ISO/disk. Make sure you use the correct version: a mismatch may cause all kinds of problems when running.

    The corresponding commands are:

    #### HTML
    ```
    cmf new html --version 1.0.0 --htmlPackage H:\packages\Cmf.Presentation.HTML.10.0.0.zip
    ```

    If you require NPM registry authentication, the current procedure is to include the auth information in the apps\customization.web\.npmrc file as is standard.

    #### Help
    ```
    cmf new help --version 1.0.0 --documentationPackage H:\packages\Cmf.Documentation.10.0.0.zip
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

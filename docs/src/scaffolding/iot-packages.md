# IoT packages

Version v10 has introduced a new way of generating Connect IoT Customization. The customization no longer depends on angular and frontend customization. With the introduction of the Automation Task Libraries the frontend elements of tasks and converters are now defined in a json template.

When generating the Connect IoT customization `cmf new iot` it will generate three packages. It will generate a `Data` package, this package is an installable package. It will also create a `Data` package and an `IoT` package.

This tree, enables to enclose the whole loop of a customization on one atomic set of packages. Connect IoT implementations, typically hold a set of masterdata, with important entities and Automation Workflows. This will be what constitutes your `Data` package. The `Data` package is split into two folders, one folder AutomationWorkFlows to hold the exported json files that constitute the automation workflows and another called MasterData to hold the masterdata files. The path defined in the masterdata inside the Masterdata folder for worklows can pass relative paths with origin the folder AutomationWorkFlows. The `IoT` package will hold all the custom components, like Tasks, Converters and Drivers.

After running the new command, it will generate a customization workspace. In this workspace, the user can create new TasksLibraries (packages that hold Connect ioT runtime components) and inside the TaskLibrary, create new tasks, converters or drivers.

The user may have use cases, where he does not need a `Data` package or an `IoT` package, for this cases, the user can delete the packages and remove them from the `cmfpackage.json` of the root package.

In a clean run where the goal would be to create a new custom task, we would do the following steps:

![New IoT Scaffolding Demo](../images/iot/cmf_new_iot.gif "New IoT Scaffolding")

In order to generate a TaskLibrary package with a task:

![Generate Task Library Package Demo](../images/iot/generate_tasklibrary.gif "Generate Task Library Package")

# Guide: IoT on MES v10

The MES Version v10 simplifies customization by introducing Automation Task Libraries. These libraries allow frontend elements of tasks and converters to be defined using JSON templates, eliminating the need for Angular and frontend customization.

As so, from now on, when generating the Connect IoT customization `cmf new iot` it will generate three packages:

* `IoT` - The IoT root package;
* `IoT.Data` - To store master data for IoT-related MES entities (e.g. controllers, workflows, drivers, etc);
* `IoT.Package` - To hold all the custom IoT components, like Tasks, Converters and Drivers.

**e.g.**:

``` log
📦Cmf.Custom.IoT
┣ 📂Cmf.Custom.IoT.Data
┃ ┣ 📂AutomationWorkFlows
┃ ┃ ┣ 📂FileHandler
┃ ┃ ┃ ┗ 📜Setup.json
┃ ┣ 📂MasterData
┃ ┃ ┗ 📂1.0.0
┃ ┃ ┃ ┗ 📜FileHandlerMasterData.json
┃ ┗ 📜cmfpackage.json
┣ 📂Cmf.Custom.IoT.Packages
┃ ┣ 📂src
┃ ┃ ┗ 📜...
┃ ┣ 📜cmfpackage.json
┗ 📜cmfpackage.json
```

The `IoT.Data` package is composed of two folders:

*  AutomationWorkFlows - To hold the exported JSON files that constitute the IoT automation workflows to be used in the MES.
*  MasterData - To hold the actual master data files with IoT entities.

!!! important

    When referring to IoT Workflows on the IoT master data files, their path should be relative to the AutomationWorkFlows folder.
    
    **e.g.**: Compare the below master data paths with the previous IoT file structure example.
    
    ``` json
    {
        "AutomationControllerWorkflow": {
            "1": {
            "AutomationController": "InterfaceController",
            "Name": "Setup",
            "DisplayName": "Setup",
            "IsFile": "Yes",
            "Workflow": "FileHandler/Setup.json",
            "Order": "1"
            }
        }
    }
    ```

The `IoT.Packages` should be used as the workspace to create new TasksLibraries (packages that hold Connect ioT runtime components). Inside the TaskLibrary create new tasks, converters or drivers.

!!! hint

    If you don't require the IoT package or any of its sub-packages, simply delete it and remove its references from the root cmfpackage.json file.

In a clean run where the goal would be to create a new custom task, we would do the following steps:

![New IoT Scaffolding Demo](cmf_new_iot.gif "New IoT Scaffolding")

To generate a TaskLibrary package with a task, we would execute the following steps:

![Generate Task Library Package Demo](generate_tasklibrary.gif "Generate Task Library Package")

# Bump IoT Configuration Command

Will change / bump the version of the IoT Configuration (Masterdata + AutomationWorkflowFiles) that you have on your project

### Synopsis

```bash
cmf bumpIoTConfiguration -v 1.0.0 -md -iot -pcknames controller-engine-custom-file-tasks -r 1.0.0 -g Manager01 -wkflName Setup.json -isToTag -mdCustomization
```

### Version

1.0.0

### Description

It will receive a package path (if no path is specified it will assume where command is executed). It will search for all the directories with a name *AutomationWorkflowFiles*. If the **-root** argument is configured, it will only use the directories beneath the root.

If the argument **-iot**  (default true) is set, it will update the AutomationWorkflows, the **-g** can specify which parent directory you want to specify to be used. The **-wkflName** argument, can be used to specify that you only want to bump a specific workflow and the **-pcknames**, particular packages inside the workflow. It will then change the version (if the -istToTag is selected it will not change the version but will add *-version*) of all the (defined) tasks and converters inside the workflow.

 If the argument **-md** (default false) is set, It will search for all the .json in the directory above the *AutomationWorkflowFiles*, these will be assumed to be masterdata files. It will then change the version of the *<DM>AutomationProtocol* , *<DM>AutomationManager*  and *<DM>AutomationController*. You can also specify the particular packages that you want to update **-pcknames** (only valid for *<DM>AutomationProtocol* ), also if the flag **-mdCustomization** is set it will mean that only the sheet  *<DM>AutomationProtocol* will be changed as is the only one that can have customization.

### Important

It assumes a structure of IoT/src/customPackage.


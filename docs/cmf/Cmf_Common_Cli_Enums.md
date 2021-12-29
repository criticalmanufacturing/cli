#### [cmf](index.md 'index')
## Cmf.Common.Cli.Enums Namespace
### Enums
<a name='Cmf_Common_Cli_Enums_ContentType'></a>
## ContentType Enum
```csharp
public enum ContentType

```
#### Fields
<a name='Cmf_Common_Cli_Enums_ContentType_AutomationWorkFlows'></a>
`AutomationWorkFlows` 9  
The automation work flows  
  
<a name='Cmf_Common_Cli_Enums_ContentType_ChecklistImages'></a>
`ChecklistImages` 5  
The checklist images  
  
<a name='Cmf_Common_Cli_Enums_ContentType_DEE'></a>
`DEE` 6  
The dee  
  
<a name='Cmf_Common_Cli_Enums_ContentType_Documents'></a>
`Documents` 7  
The documents  
  
<a name='Cmf_Common_Cli_Enums_ContentType_EntityTypes'></a>
`EntityTypes` 4  
The entity types  
  
<a name='Cmf_Common_Cli_Enums_ContentType_ExportedObjects'></a>
`ExportedObjects` 10  
The exported objects  
  
<a name='Cmf_Common_Cli_Enums_ContentType_Generic'></a>
`Generic` 0  
The generic  
  
<a name='Cmf_Common_Cli_Enums_ContentType_Maps'></a>
`Maps` 8  
The maps  
  
<a name='Cmf_Common_Cli_Enums_ContentType_MasterData'></a>
`MasterData` 1  
The master data  
  
<a name='Cmf_Common_Cli_Enums_ContentType_ProcessRulesPost'></a>
`ProcessRulesPost` 3  
The process rules post  
  
<a name='Cmf_Common_Cli_Enums_ContentType_ProcessRulesPre'></a>
`ProcessRulesPre` 2  
The process rules pre  
  
  
<a name='Cmf_Common_Cli_Enums_MessageType'></a>
## MessageType Enum
```csharp
public enum MessageType

```
#### Fields
<a name='Cmf_Common_Cli_Enums_MessageType_ImportObject'></a>
`ImportObject` 0  
The import object  
  
  
<a name='Cmf_Common_Cli_Enums_PackAction'></a>
## PackAction Enum
The action to apply to the content specified to be packed  
```csharp
public enum PackAction

```
#### Fields
<a name='Cmf_Common_Cli_Enums_PackAction_Pack'></a>
`Pack` 0  
pack the source content into the package  
  
<a name='Cmf_Common_Cli_Enums_PackAction_Transform'></a>
`Transform` 1  
Use the source content to apply a transformation to another file  
This currently doesn't use the Target property, as it is handler dependent  
  
<a name='Cmf_Common_Cli_Enums_PackAction_Untar'></a>
`Untar` 2  
Use the source content to apply untar the file to a target destination  
This currently handler dependent (IoT Package)  
  
  
<a name='Cmf_Common_Cli_Enums_PackageLocation'></a>
## PackageLocation Enum
Possible source for a CmfPackage  
```csharp
public enum PackageLocation

```
#### Fields
<a name='Cmf_Common_Cli_Enums_PackageLocation_Local'></a>
`Local` 0  
Local filesystem  
  
<a name='Cmf_Common_Cli_Enums_PackageLocation_Repository'></a>
`Repository` 1  
a specified repository  
  
  
<a name='Cmf_Common_Cli_Enums_PackageType'></a>
## PackageType Enum
```csharp
public enum PackageType

```
#### Fields
<a name='Cmf_Common_Cli_Enums_PackageType_Business'></a>
`Business` 1  
The business  
  
<a name='Cmf_Common_Cli_Enums_PackageType_Data'></a>
`Data` 3  
The global data  
  
<a name='Cmf_Common_Cli_Enums_PackageType_Database'></a>
`Database` 10  
The database  
  
<a name='Cmf_Common_Cli_Enums_PackageType_ExportedObjects'></a>
`ExportedObjects` 9  
The exported objects  
  
<a name='Cmf_Common_Cli_Enums_PackageType_Generic'></a>
`Generic` 0  
The metadata  
  
<a name='Cmf_Common_Cli_Enums_PackageType_Help'></a>
`Help` 6  
The help  
  
<a name='Cmf_Common_Cli_Enums_PackageType_HTML'></a>
`HTML` 5  
The HTML  
  
<a name='Cmf_Common_Cli_Enums_PackageType_IoT'></a>
`IoT` 7  
The iot  
  
<a name='Cmf_Common_Cli_Enums_PackageType_IoTData'></a>
`IoTData` 12  
The IoT Data  
  
<a name='Cmf_Common_Cli_Enums_PackageType_None'></a>
`None` 11  
None  
  
<a name='Cmf_Common_Cli_Enums_PackageType_Presentation'></a>
`Presentation` 4  
The presentation  
  
<a name='Cmf_Common_Cli_Enums_PackageType_Reporting'></a>
`Reporting` 8  
The reporting  
  
<a name='Cmf_Common_Cli_Enums_PackageType_Root'></a>
`Root` 2  
The root  
  
<a name='Cmf_Common_Cli_Enums_PackageType_Tests'></a>
`Tests` 13  
The test  
  
  
<a name='Cmf_Common_Cli_Enums_StepType'></a>
## StepType Enum
```csharp
public enum StepType

```
#### Fields
<a name='Cmf_Common_Cli_Enums_StepType_CreateIntegrationEntries'></a>
`CreateIntegrationEntries` 8  
The create integration entries  
  
<a name='Cmf_Common_Cli_Enums_StepType_DeployFiles'></a>
`DeployFiles` 1  
The deploy files  
  
<a name='Cmf_Common_Cli_Enums_StepType_DeployReports'></a>
`DeployReports` 4  
The run SQL  
  
<a name='Cmf_Common_Cli_Enums_StepType_EnqueueSql'></a>
`EnqueueSql` 9  
The enqueue SQL  
  
<a name='Cmf_Common_Cli_Enums_StepType_ExportedObjects'></a>
`ExportedObjects` 7  
The exported objects  
  
<a name='Cmf_Common_Cli_Enums_StepType_Generic'></a>
`Generic` 0  
The generic  
  
<a name='Cmf_Common_Cli_Enums_StepType_MasterData'></a>
`MasterData` 6  
The master data  
  
<a name='Cmf_Common_Cli_Enums_StepType_ProcessRules'></a>
`ProcessRules` 5  
The process rules  
  
<a name='Cmf_Common_Cli_Enums_StepType_RunSql'></a>
`RunSql` 3  
The run SQL  
  
<a name='Cmf_Common_Cli_Enums_StepType_TransformFile'></a>
`TransformFile` 2  
The transform file  
  
  

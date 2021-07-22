#### [cmf](index.md 'index')
## Cmf.Common.Cli.Commands Namespace
### Classes
<a name='Cmf_Common_Cli_Commands_BaseCommand'></a>
## BaseCommand Class
```csharp
public abstract class BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; BaseCommand  

Derived  
&#8627; [BuildCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BuildCommand 'Cmf.Common.Cli.Commands.BuildCommand')  
&#8627; [BumpCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BumpCommand 'Cmf.Common.Cli.Commands.BumpCommand')  
&#8627; [BumpIoTCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BumpIoTCommand 'Cmf.Common.Cli.Commands.BumpIoTCommand')  
&#8627; [BumpIoTCustomizationCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand 'Cmf.Common.Cli.Commands.BumpIoTCustomizationCommand')  
&#8627; [HelpCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_HelpCommand 'Cmf.Common.Cli.Commands.HelpCommand')  
&#8627; [ListDependenciesCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_ListDependenciesCommand 'Cmf.Common.Cli.Commands.ListDependenciesCommand')  
&#8627; [LocalCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_LocalCommand 'Cmf.Common.Cli.Commands.LocalCommand')  
&#8627; [New3_Command](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_New3_Command 'Cmf.Common.Cli.Commands.New3_Command')  
&#8627; [PackCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PackCommand 'Cmf.Common.Cli.Commands.PackCommand')  
&#8627; [PluginCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PluginCommand 'Cmf.Common.Cli.Commands.PluginCommand')  
&#8627; [PowershellCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PowershellCommand 'Cmf.Common.Cli.Commands.PowershellCommand')  
&#8627; [PublishCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PublishCommand 'Cmf.Common.Cli.Commands.PublishCommand')  
&#8627; [TemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_TemplateCommand 'Cmf.Common.Cli.Commands.TemplateCommand')  
### Constructors
<a name='Cmf_Common_Cli_Commands_BaseCommand_BaseCommand()'></a>
## BaseCommand.BaseCommand() Constructor
constructor for System.IO filesystem  
```csharp
public BaseCommand();
```
  
<a name='Cmf_Common_Cli_Commands_BaseCommand_BaseCommand(System_IO_Abstractions_IFileSystem)'></a>
## BaseCommand.BaseCommand(IFileSystem) Constructor
constructor  
```csharp
public BaseCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BaseCommand_BaseCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Fields
<a name='Cmf_Common_Cli_Commands_BaseCommand_fileSystem'></a>
## BaseCommand.fileSystem Field
The underlying filesystem  
```csharp
protected IFileSystem fileSystem;
```
#### Field Value
[System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')
  
### Methods
<a name='Cmf_Common_Cli_Commands_BaseCommand_AddChildCommands(System_CommandLine_Command)'></a>
## BaseCommand.AddChildCommands(Command) Method
Register all available commands, identified using the CmfCommand attribute.  
```csharp
public static void AddChildCommands(System.CommandLine.Command command);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BaseCommand_AddChildCommands(System_CommandLine_Command)_command'></a>
`command` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
Command to which commands will be added
  
  
<a name='Cmf_Common_Cli_Commands_BaseCommand_AddPluginCommands(System_CommandLine_Command)'></a>
## BaseCommand.AddPluginCommands(Command) Method
Adds the plugin commands.  
```csharp
public static void AddPluginCommands(System.CommandLine.Command command);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BaseCommand_AddPluginCommands(System_CommandLine_Command)_command'></a>
`command` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
The command.
  
  
<a name='Cmf_Common_Cli_Commands_BaseCommand_Configure(System_CommandLine_Command)'></a>
## BaseCommand.Configure(Command) Method
Configure command  
```csharp
public abstract void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BaseCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_BaseCommand_FindChildCommands(System_Type_System_Collections_Generic_List_System_Type_)'></a>
## BaseCommand.FindChildCommands(Type, List&lt;Type&gt;) Method
Finds the child commands.  
```csharp
private static System.CommandLine.Command FindChildCommands(System.Type cmd, System.Collections.Generic.List<System.Type> commandTypes);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BaseCommand_FindChildCommands(System_Type_System_Collections_Generic_List_System_Type_)_cmd'></a>
`cmd` [System.Type](https://docs.microsoft.com/en-us/dotnet/api/System.Type 'System.Type')  
The command.
  
<a name='Cmf_Common_Cli_Commands_BaseCommand_FindChildCommands(System_Type_System_Collections_Generic_List_System_Type_)_commandTypes'></a>
`commandTypes` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.Type](https://docs.microsoft.com/en-us/dotnet/api/System.Type 'System.Type')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
The command types.
  
#### Returns
[System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
<a name='Cmf_Common_Cli_Commands_BaseCommand_Parse_T_(System_CommandLine_Parsing_ArgumentResult_string)'></a>
## BaseCommand.Parse&lt;T&gt;(ArgumentResult, string) Method
parse argument/option  
```csharp
protected T Parse<T>(System.CommandLine.Parsing.ArgumentResult argResult, string @default=null);
```
#### Type parameters
<a name='Cmf_Common_Cli_Commands_BaseCommand_Parse_T_(System_CommandLine_Parsing_ArgumentResult_string)_T'></a>
`T`  
the (target) type of the argument/parameter
  
#### Parameters
<a name='Cmf_Common_Cli_Commands_BaseCommand_Parse_T_(System_CommandLine_Parsing_ArgumentResult_string)_argResult'></a>
`argResult` [System.CommandLine.Parsing.ArgumentResult](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Parsing.ArgumentResult 'System.CommandLine.Parsing.ArgumentResult')  
the arguments to parse
  
<a name='Cmf_Common_Cli_Commands_BaseCommand_Parse_T_(System_CommandLine_Parsing_ArgumentResult_string)_default'></a>
`default` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
the default value if no value is passed for the argument
  
#### Returns
[T](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand_Parse_T_(System_CommandLine_Parsing_ArgumentResult_string)_T 'Cmf.Common.Cli.Commands.BaseCommand.Parse&lt;T&gt;(System.CommandLine.Parsing.ArgumentResult, string).T')  
#### Exceptions
[System.ArgumentOutOfRangeException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentOutOfRangeException 'System.ArgumentOutOfRangeException')  
  
  
<a name='Cmf_Common_Cli_Commands_BuildCommand'></a>
## BuildCommand Class
```csharp
public class BuildCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; BuildCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_BuildCommand_BuildCommand()'></a>
## BuildCommand.BuildCommand() Constructor
Build command Constructor  
```csharp
public BuildCommand();
```
  
<a name='Cmf_Common_Cli_Commands_BuildCommand_BuildCommand(System_IO_Abstractions_IFileSystem)'></a>
## BuildCommand.BuildCommand(IFileSystem) Constructor
Build Command Constructor specify fileSystem  
Must have this for tests  
```csharp
public BuildCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BuildCommand_BuildCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_BuildCommand_Configure(System_CommandLine_Command)'></a>
## BuildCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BuildCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_BuildCommand_Execute(System_IO_Abstractions_IDirectoryInfo)'></a>
## BuildCommand.Execute(IDirectoryInfo) Method
Executes the specified package path.  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo packagePath);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BuildCommand_Execute(System_IO_Abstractions_IDirectoryInfo)_packagePath'></a>
`packagePath` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package path.
  
  
#### See Also
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_BumpCommand'></a>
## BumpCommand Class
```csharp
public class BumpCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; BumpCommand  

Derived  
&#8627; [BumpIoTConfigurationCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand 'Cmf.Common.Cli.Commands.BumpIoTConfigurationCommand')  
### Methods
<a name='Cmf_Common_Cli_Commands_BumpCommand_Configure(System_CommandLine_Command)'></a>
## BumpCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BumpCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)'></a>
## BumpCommand.Execute(CmfPackage, string, string, string, bool) Method
Executes the specified CMF package.  
```csharp
public void Execute(Cmf.Common.Cli.Objects.CmfPackage cmfPackage, string version, string buildNr, string root, bool all);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version for build Nr.
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_root'></a>
`root` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The root.
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_all'></a>
`all` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [all].
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(System_IO_DirectoryInfo_string_string_string_bool)'></a>
## BumpCommand.Execute(DirectoryInfo, string, string, string, bool) Method
Executes the specified package path.  
```csharp
public void Execute(System.IO.DirectoryInfo packagePath, string version, string buildNr, string root, bool all);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(System_IO_DirectoryInfo_string_string_string_bool)_packagePath'></a>
`packagePath` [System.IO.DirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.DirectoryInfo 'System.IO.DirectoryInfo')  
The package path.
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(System_IO_DirectoryInfo_string_string_string_bool)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(System_IO_DirectoryInfo_string_string_string_bool)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version for build Nr.
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(System_IO_DirectoryInfo_string_string_string_bool)_root'></a>
`root` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The root.
  
<a name='Cmf_Common_Cli_Commands_BumpCommand_Execute(System_IO_DirectoryInfo_string_string_string_bool)_all'></a>
`all` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [all].
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
#### See Also
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCommand'></a>
## BumpIoTCommand Class
iot command group  
```csharp
public class BumpIoTCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; BumpIoTCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_BumpIoTCommand_Configure(System_CommandLine_Command)'></a>
## BumpIoTCommand.Configure(Command) Method
Configure command (no-op, command is a group only)  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BumpIoTCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand'></a>
## BumpIoTConfigurationCommand Class
```csharp
public class BumpIoTConfigurationCommand : Cmf.Common.Cli.Commands.BumpCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [BumpCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BumpCommand 'Cmf.Common.Cli.Commands.BumpCommand') &#129106; BumpIoTConfigurationCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Configure(System_CommandLine_Command)'></a>
## BumpIoTConfigurationCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)'></a>
## BumpIoTConfigurationCommand.Execute(IDirectoryInfo, string, string, bool, bool, string, string, string, string, bool, bool) Method
Executes the specified package directory.  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo path, string version, string buildNr, bool isToBumpMasterdata, bool isToBumpIoT, string packageNames, string root, string group, string workflowName, bool isToTag, bool onlyMdCustomization);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_path'></a>
`path` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package directory.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_isToBumpMasterdata'></a>
`isToBumpMasterdata` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [is to bump masterdata].
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_isToBumpIoT'></a>
`isToBumpIoT` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [is to bump io t].
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_packageNames'></a>
`packageNames` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The package names.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_root'></a>
`root` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The root.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_group'></a>
`group` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The group.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_workflowName'></a>
`workflowName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Name of the workflow.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_isToTag'></a>
`isToTag` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [is to tag].
  
<a name='Cmf_Common_Cli_Commands_BumpIoTConfigurationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_bool_bool_string_string_string_string_bool_bool)_onlyMdCustomization'></a>
`onlyMdCustomization` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [only md customization].
  
  
#### See Also
- [BumpCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BumpCommand 'Cmf.Common.Cli.Commands.BumpCommand')
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand'></a>
## BumpIoTCustomizationCommand Class
```csharp
public class BumpIoTCustomizationCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; BumpIoTCustomizationCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Configure(System_CommandLine_Command)'></a>
## BumpIoTCustomizationCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)'></a>
## BumpIoTCustomizationCommand.Execute(CmfPackage, string, string, string, bool) Method
Executes the BumpIoTCustomPackages for specified CMF package.  
```csharp
public void Execute(Cmf.Common.Cli.Objects.CmfPackage cmfPackage, string version, string buildNr, string packageNames, bool isToTag);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_packageNames'></a>
`packageNames` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The package names.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_string_string_string_bool)_isToTag'></a>
`isToTag` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [is to tag].
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_string_bool)'></a>
## BumpIoTCustomizationCommand.Execute(IDirectoryInfo, string, string, string, bool) Method
Executes the specified package path.  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo packagePath, string version, string buildNr, string packageNames, bool isToTag);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_string_bool)_packagePath'></a>
`packagePath` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package path.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_string_bool)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_string_bool)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_string_bool)_packageNames'></a>
`packageNames` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The package names.
  
<a name='Cmf_Common_Cli_Commands_BumpIoTCustomizationCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string_string_bool)_isToTag'></a>
`isToTag` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [is to tag].
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
#### See Also
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_GenerateBasedOnTemplatesCommand'></a>
## GenerateBasedOnTemplatesCommand Class
```csharp
public class GenerateBasedOnTemplatesCommand : Cmf.Common.Cli.Commands.PowershellCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [PowershellCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PowershellCommand 'Cmf.Common.Cli.Commands.PowershellCommand') &#129106; GenerateBasedOnTemplatesCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_GenerateBasedOnTemplatesCommand_Configure(System_CommandLine_Command)'></a>
## GenerateBasedOnTemplatesCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_GenerateBasedOnTemplatesCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_GenerateBasedOnTemplatesCommand_Execute()'></a>
## GenerateBasedOnTemplatesCommand.Execute() Method
Executes this instance.  
```csharp
public void Execute();
```
  
<a name='Cmf_Common_Cli_Commands_GenerateBasedOnTemplatesCommand_GetPowershellScript()'></a>
## GenerateBasedOnTemplatesCommand.GetPowershellScript() Method
Gets the powershell script.  
```csharp
protected override string GetPowershellScript();
```
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
#### See Also
- [PowershellCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PowershellCommand 'Cmf.Common.Cli.Commands.PowershellCommand')
  
<a name='Cmf_Common_Cli_Commands_GenerateLBOsCommand'></a>
## GenerateLBOsCommand Class
```csharp
public class GenerateLBOsCommand : Cmf.Common.Cli.Commands.PowershellCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [PowershellCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PowershellCommand 'Cmf.Common.Cli.Commands.PowershellCommand') &#129106; GenerateLBOsCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_GenerateLBOsCommand_Configure(System_CommandLine_Command)'></a>
## GenerateLBOsCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_GenerateLBOsCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_GenerateLBOsCommand_Execute()'></a>
## GenerateLBOsCommand.Execute() Method
Executes this instance.  
```csharp
public void Execute();
```
  
<a name='Cmf_Common_Cli_Commands_GenerateLBOsCommand_GetPowershellScript()'></a>
## GenerateLBOsCommand.GetPowershellScript() Method
Gets the powershell script.  
```csharp
protected override string GetPowershellScript();
```
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
#### See Also
- [PowershellCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PowershellCommand 'Cmf.Common.Cli.Commands.PowershellCommand')
  
<a name='Cmf_Common_Cli_Commands_GenerateMenuItemsCommand'></a>
## GenerateMenuItemsCommand Class
```csharp
public class GenerateMenuItemsCommand : Cmf.Common.Cli.Commands.PowershellCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [PowershellCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PowershellCommand 'Cmf.Common.Cli.Commands.PowershellCommand') &#129106; GenerateMenuItemsCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_GenerateMenuItemsCommand_Configure(System_CommandLine_Command)'></a>
## GenerateMenuItemsCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_GenerateMenuItemsCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_GenerateMenuItemsCommand_Execute()'></a>
## GenerateMenuItemsCommand.Execute() Method
Executes this instance.  
```csharp
public void Execute();
```
  
<a name='Cmf_Common_Cli_Commands_GenerateMenuItemsCommand_GetPowershellScript()'></a>
## GenerateMenuItemsCommand.GetPowershellScript() Method
Gets the powershell script.  
```csharp
protected override string GetPowershellScript();
```
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
#### See Also
- [PowershellCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PowershellCommand 'Cmf.Common.Cli.Commands.PowershellCommand')
  
<a name='Cmf_Common_Cli_Commands_GetLocalEnvironmentCommand'></a>
## GetLocalEnvironmentCommand Class
```csharp
public class GetLocalEnvironmentCommand : Cmf.Common.Cli.Commands.PowershellCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [PowershellCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PowershellCommand 'Cmf.Common.Cli.Commands.PowershellCommand') &#129106; GetLocalEnvironmentCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_GetLocalEnvironmentCommand_Configure(System_CommandLine_Command)'></a>
## GetLocalEnvironmentCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_GetLocalEnvironmentCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_GetLocalEnvironmentCommand_Execute(System_IO_DirectoryInfo)'></a>
## GetLocalEnvironmentCommand.Execute(DirectoryInfo) Method
Executes the specified target.  
```csharp
public void Execute(System.IO.DirectoryInfo target);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_GetLocalEnvironmentCommand_Execute(System_IO_DirectoryInfo)_target'></a>
`target` [System.IO.DirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.DirectoryInfo 'System.IO.DirectoryInfo')  
The target.
  
  
<a name='Cmf_Common_Cli_Commands_GetLocalEnvironmentCommand_GetPowershellScript()'></a>
## GetLocalEnvironmentCommand.GetPowershellScript() Method
Gets the powershell script.  
```csharp
protected override string GetPowershellScript();
```
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
#### See Also
- [PowershellCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PowershellCommand 'Cmf.Common.Cli.Commands.PowershellCommand')
  
<a name='Cmf_Common_Cli_Commands_HelpCommand'></a>
## HelpCommand Class
```csharp
public class HelpCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; HelpCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_HelpCommand_Configure(System_CommandLine_Command)'></a>
## HelpCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_HelpCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
#### See Also
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_InitCommand'></a>
## InitCommand Class
Init command  
```csharp
public class InitCommand : Cmf.Common.Cli.Commands.TemplateCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [TemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_TemplateCommand 'Cmf.Common.Cli.Commands.TemplateCommand') &#129106; InitCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_InitCommand_InitCommand()'></a>
## InitCommand.InitCommand() Constructor
constructor  
```csharp
public InitCommand();
```
  
<a name='Cmf_Common_Cli_Commands_InitCommand_InitCommand(System_IO_Abstractions_IFileSystem)'></a>
## InitCommand.InitCommand(IFileSystem) Constructor
constructor  
```csharp
public InitCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_InitCommand_InitCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_InitCommand_Configure(System_CommandLine_Command)'></a>
## InitCommand.Configure(Command) Method
configure command signature  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_InitCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_InitCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo_System_Uri)'></a>
## InitCommand.Execute(IDirectoryInfo, string, IFileInfo, Uri) Method
Execute the command  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo workingDir, string rootPackageName, System.IO.Abstractions.IFileInfo config, System.Uri nugetRegistry);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_InitCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo_System_Uri)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
  
<a name='Cmf_Common_Cli_Commands_InitCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo_System_Uri)_rootPackageName'></a>
`rootPackageName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Commands_InitCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo_System_Uri)_config'></a>
`config` [System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
  
<a name='Cmf_Common_Cli_Commands_InitCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo_System_Uri)_nugetRegistry'></a>
`nugetRegistry` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
  
  
  
<a name='Cmf_Common_Cli_Commands_ListDependenciesCommand'></a>
## ListDependenciesCommand Class
List dependencies command  
```csharp
public class ListDependenciesCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; ListDependenciesCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_ListDependenciesCommand_ListDependenciesCommand()'></a>
## ListDependenciesCommand.ListDependenciesCommand() Constructor
constructor  
```csharp
public ListDependenciesCommand();
```
  
<a name='Cmf_Common_Cli_Commands_ListDependenciesCommand_ListDependenciesCommand(System_IO_Abstractions_IFileSystem)'></a>
## ListDependenciesCommand.ListDependenciesCommand(IFileSystem) Constructor
constructor  
```csharp
public ListDependenciesCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_ListDependenciesCommand_ListDependenciesCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_ListDependenciesCommand_Configure(System_CommandLine_Command)'></a>
## ListDependenciesCommand.Configure(Command) Method
configure command signature  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_ListDependenciesCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_ListDependenciesCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string)'></a>
## ListDependenciesCommand.Execute(IDirectoryInfo, string) Method
Execute the command  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo workingDir, string repo);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_ListDependenciesCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
  
<a name='Cmf_Common_Cli_Commands_ListDependenciesCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string)_repo'></a>
`repo` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
  
  
<a name='Cmf_Common_Cli_Commands_LocalCommand'></a>
## LocalCommand Class
```csharp
public class LocalCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; LocalCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_LocalCommand_Configure(System_CommandLine_Command)'></a>
## LocalCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_LocalCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
#### See Also
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_New3_Command'></a>
## New3_Command Class
Command to test templates  
```csharp
public class New3_Command : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; New3_Command  
### Constructors
<a name='Cmf_Common_Cli_Commands_New3_Command_New3_Command()'></a>
## New3_Command.New3_Command() Constructor
constructor  
```csharp
public New3_Command();
```
  
<a name='Cmf_Common_Cli_Commands_New3_Command_New3_Command(System_IO_Abstractions_IFileSystem)'></a>
## New3_Command.New3_Command(IFileSystem) Constructor
constructor  
```csharp
public New3_Command(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New3_Command_New3_Command(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_New3_Command_Configure(System_CommandLine_Command)'></a>
## New3_Command.Configure(Command) Method
configure command signature  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New3_Command_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_New3_Command_Execute(System_Collections_Generic_IReadOnlyCollection_string_)'></a>
## New3_Command.Execute(IReadOnlyCollection&lt;string&gt;) Method
Execute the command  
```csharp
public static void Execute(System.Collections.Generic.IReadOnlyCollection<string> args);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New3_Command_Execute(System_Collections_Generic_IReadOnlyCollection_string_)_args'></a>
`args` [System.Collections.Generic.IReadOnlyCollection&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IReadOnlyCollection-1 'System.Collections.Generic.IReadOnlyCollection`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IReadOnlyCollection-1 'System.Collections.Generic.IReadOnlyCollection`1')  
  
  
  
<a name='Cmf_Common_Cli_Commands_PackCommand'></a>
## PackCommand Class
```csharp
public class PackCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; PackCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_PackCommand_Configure(System_CommandLine_Command)'></a>
## PackCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PackCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_Cmf_Common_Cli_Objects_CmfPackageCollection_bool_bool)'></a>
## PackCommand.Execute(CmfPackage, IDirectoryInfo, Uri, CmfPackageCollection, bool, bool) Method
Executes the specified CMF package.  
```csharp
public void Execute(Cmf.Common.Cli.Objects.CmfPackage cmfPackage, System.IO.Abstractions.IDirectoryInfo outputDir, System.Uri repoUri, Cmf.Common.Cli.Objects.CmfPackageCollection loadedPackages, bool force, bool skipDependencies);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_Cmf_Common_Cli_Objects_CmfPackageCollection_bool_bool)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_Cmf_Common_Cli_Objects_CmfPackageCollection_bool_bool)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The output dir.
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_Cmf_Common_Cli_Objects_CmfPackageCollection_bool_bool)_repoUri'></a>
`repoUri` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
The repo URI.
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_Cmf_Common_Cli_Objects_CmfPackageCollection_bool_bool)_loadedPackages'></a>
`loadedPackages` [CmfPackageCollection](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackageCollection 'Cmf.Common.Cli.Objects.CmfPackageCollection')  
The loaded packages.
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_Cmf_Common_Cli_Objects_CmfPackageCollection_bool_bool)_force'></a>
`force` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [force].
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_Cmf_Common_Cli_Objects_CmfPackageCollection_bool_bool)_skipDependencies'></a>
`skipDependencies` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
#### Exceptions
[CmfPackageCollection](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackageCollection 'Cmf.Common.Cli.Objects.CmfPackageCollection')  
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool_bool)'></a>
## PackCommand.Execute(IDirectoryInfo, IDirectoryInfo, string, bool, bool) Method
Executes the specified working dir.  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo workingDir, System.IO.Abstractions.IDirectoryInfo outputDir, string repo, bool force, bool skipDependencies);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool_bool)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The working dir.
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool_bool)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The output dir.
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool_bool)_repo'></a>
`repo` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The repo.
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool_bool)_force'></a>
`force` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [force].
  
<a name='Cmf_Common_Cli_Commands_PackCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool_bool)_skipDependencies'></a>
`skipDependencies` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
  
#### See Also
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_PluginCommand'></a>
## PluginCommand Class
```csharp
public class PluginCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; PluginCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_PluginCommand_PluginCommand(string_string)'></a>
## PluginCommand.PluginCommand(string, string) Constructor
Initializes a new instance of the [PluginCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_PluginCommand 'Cmf.Common.Cli.Commands.PluginCommand') class.  
```csharp
public PluginCommand(string commandName, string commandPath);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PluginCommand_PluginCommand(string_string)_commandName'></a>
`commandName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Name of the command.
  
<a name='Cmf_Common_Cli_Commands_PluginCommand_PluginCommand(string_string)_commandPath'></a>
`commandPath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The command path.
  
  
### Fields
<a name='Cmf_Common_Cli_Commands_PluginCommand_commandName'></a>
## PluginCommand.commandName Field
The command name  
```csharp
private readonly string commandName;
```
#### Field Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
  
<a name='Cmf_Common_Cli_Commands_PluginCommand_commandPath'></a>
## PluginCommand.commandPath Field
The command path  
```csharp
private readonly string commandPath;
```
#### Field Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
  
### Methods
<a name='Cmf_Common_Cli_Commands_PluginCommand_Configure(System_CommandLine_Command)'></a>
## PluginCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PluginCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_PluginCommand_Execute(System_Collections_Generic_IReadOnlyCollection_string_)'></a>
## PluginCommand.Execute(IReadOnlyCollection&lt;string&gt;) Method
Executes the plugin with the supplied parameters  
```csharp
public void Execute(System.Collections.Generic.IReadOnlyCollection<string> args);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PluginCommand_Execute(System_Collections_Generic_IReadOnlyCollection_string_)_args'></a>
`args` [System.Collections.Generic.IReadOnlyCollection&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IReadOnlyCollection-1 'System.Collections.Generic.IReadOnlyCollection`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IReadOnlyCollection-1 'System.Collections.Generic.IReadOnlyCollection`1')  
The arguments.
  
  
#### See Also
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_PowershellCommand'></a>
## PowershellCommand Class
```csharp
public abstract class PowershellCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; PowershellCommand  

Derived  
&#8627; [GenerateBasedOnTemplatesCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_GenerateBasedOnTemplatesCommand 'Cmf.Common.Cli.Commands.GenerateBasedOnTemplatesCommand')  
&#8627; [GenerateLBOsCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_GenerateLBOsCommand 'Cmf.Common.Cli.Commands.GenerateLBOsCommand')  
&#8627; [GenerateMenuItemsCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_GenerateMenuItemsCommand 'Cmf.Common.Cli.Commands.GenerateMenuItemsCommand')  
&#8627; [GetLocalEnvironmentCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_GetLocalEnvironmentCommand 'Cmf.Common.Cli.Commands.GetLocalEnvironmentCommand')  
### Methods
<a name='Cmf_Common_Cli_Commands_PowershellCommand_ExecutePwshScriptAsync(System_Collections_IDictionary_string_string)'></a>
## PowershellCommand.ExecutePwshScriptAsync(IDictionary, string, string) Method
Executes the PWSH script asynchronously.  
```csharp
protected System.Threading.Tasks.Task<PSDataCollection<PSObject>> ExecutePwshScriptAsync(System.Collections.IDictionary parameters=null, string script=null, string hostname=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PowershellCommand_ExecutePwshScriptAsync(System_Collections_IDictionary_string_string)_parameters'></a>
`parameters` [System.Collections.IDictionary](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.IDictionary 'System.Collections.IDictionary')  
The parameters.
  
<a name='Cmf_Common_Cli_Commands_PowershellCommand_ExecutePwshScriptAsync(System_Collections_IDictionary_string_string)_script'></a>
`script` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The script.
  
<a name='Cmf_Common_Cli_Commands_PowershellCommand_ExecutePwshScriptAsync(System_Collections_IDictionary_string_string)_hostname'></a>
`hostname` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The hostname.
  
#### Returns
[System.Threading.Tasks.Task&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task-1 'System.Threading.Tasks.Task`1')[System.Management.Automation.PSDataCollection](https://docs.microsoft.com/en-us/dotnet/api/System.Management.Automation.PSDataCollection 'System.Management.Automation.PSDataCollection')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task-1 'System.Threading.Tasks.Task`1')  
  
<a name='Cmf_Common_Cli_Commands_PowershellCommand_ExecutePwshScriptSync(System_Collections_IDictionary_string_string)'></a>
## PowershellCommand.ExecutePwshScriptSync(IDictionary, string, string) Method
Executes the PWSH script synchronously.  
```csharp
protected PSDataCollection<PSObject> ExecutePwshScriptSync(System.Collections.IDictionary parameters=null, string script=null, string hostname=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PowershellCommand_ExecutePwshScriptSync(System_Collections_IDictionary_string_string)_parameters'></a>
`parameters` [System.Collections.IDictionary](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.IDictionary 'System.Collections.IDictionary')  
The parameters.
  
<a name='Cmf_Common_Cli_Commands_PowershellCommand_ExecutePwshScriptSync(System_Collections_IDictionary_string_string)_script'></a>
`script` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The script.
  
<a name='Cmf_Common_Cli_Commands_PowershellCommand_ExecutePwshScriptSync(System_Collections_IDictionary_string_string)_hostname'></a>
`hostname` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The hostname.
  
#### Returns
[System.Management.Automation.PSDataCollection](https://docs.microsoft.com/en-us/dotnet/api/System.Management.Automation.PSDataCollection 'System.Management.Automation.PSDataCollection')  
  
<a name='Cmf_Common_Cli_Commands_PowershellCommand_GetPowershellScript()'></a>
## PowershellCommand.GetPowershellScript() Method
Gets the powershell script.  
```csharp
protected abstract string GetPowershellScript();
```
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Commands_PowershellCommand_GetRunspace(string)'></a>
## PowershellCommand.GetRunspace(string) Method
Gets the a remote pwsh runspace.  
```csharp
protected static Runspace GetRunspace(string hostname);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PowershellCommand_GetRunspace(string)_hostname'></a>
`hostname` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The hostname.
  
#### Returns
[System.Management.Automation.Runspaces.Runspace](https://docs.microsoft.com/en-us/dotnet/api/System.Management.Automation.Runspaces.Runspace 'System.Management.Automation.Runspaces.Runspace')  
  
#### See Also
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_PublishCommand'></a>
## PublishCommand Class
```csharp
public class PublishCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; PublishCommand  
### Methods
<a name='Cmf_Common_Cli_Commands_PublishCommand_Configure(System_CommandLine_Command)'></a>
## PublishCommand.Configure(Command) Method
Configure command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PublishCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_bool)'></a>
## PublishCommand.Execute(CmfPackage, IDirectoryInfo, Uri, bool) Method
Executes the specified CMF package.  
```csharp
public void Execute(Cmf.Common.Cli.Objects.CmfPackage cmfPackage, System.IO.Abstractions.IDirectoryInfo outputDir, System.Uri repoUri, bool publishTests);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_bool)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_bool)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The output dir.
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_bool)_repoUri'></a>
`repoUri` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
The repo URI.
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_Uri_bool)_publishTests'></a>
`publishTests` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
True to publish test packages
  
#### Exceptions
[CmfPackageCollection](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackageCollection 'Cmf.Common.Cli.Objects.CmfPackageCollection')  
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool)'></a>
## PublishCommand.Execute(IDirectoryInfo, IDirectoryInfo, string, bool) Method
Executes the specified working dir.  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo workingDir, System.IO.Abstractions.IDirectoryInfo outputDir, string repo, bool publishTests);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The working dir.
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The output dir.
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool)_repo'></a>
`repo` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The repo.
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_Execute(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_string_bool)_publishTests'></a>
`publishTests` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
True to publish test packages
  
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishDependenciesFromPackage(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string_System_Collections_Generic_List_string__bool)'></a>
## PublishCommand.PublishDependenciesFromPackage(IDirectoryInfo, Uri, string, string, List&lt;string&gt;, bool) Method
Publish Dependencies from one package. recursive operation  
```csharp
private void PublishDependenciesFromPackage(System.IO.Abstractions.IDirectoryInfo outputDir, System.Uri repoUri, string packageId, string packageVersion, System.Collections.Generic.List<string> loadedPackages, bool publishTests);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishDependenciesFromPackage(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string_System_Collections_Generic_List_string__bool)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
Destination for the dependencies package and also used for the current package
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishDependenciesFromPackage(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string_System_Collections_Generic_List_string__bool)_repoUri'></a>
`repoUri` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
Source Location where the package dependencies should be downloaded
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishDependenciesFromPackage(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string_System_Collections_Generic_List_string__bool)_packageId'></a>
`packageId` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Source Package ID
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishDependenciesFromPackage(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string_System_Collections_Generic_List_string__bool)_packageVersion'></a>
`packageVersion` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Source Package Version
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishDependenciesFromPackage(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string_System_Collections_Generic_List_string__bool)_loadedPackages'></a>
`loadedPackages` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
List of packages already processed.
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishDependenciesFromPackage(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string_System_Collections_Generic_List_string__bool)_publishTests'></a>
`publishTests` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
True to publish test packages
  
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishPackageToOutput(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string)'></a>
## PublishCommand.PublishPackageToOutput(IDirectoryInfo, Uri, string, string) Method
Publish a package to the output directory  
```csharp
private void PublishPackageToOutput(System.IO.Abstractions.IDirectoryInfo outputDir, System.Uri repoUri, string packageId, string packageVersion);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishPackageToOutput(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
Destiny for the package
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishPackageToOutput(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string)_repoUri'></a>
`repoUri` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
Source Location where the package should be downloaded
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishPackageToOutput(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string)_packageId'></a>
`packageId` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Package Id to publish
  
<a name='Cmf_Common_Cli_Commands_PublishCommand_PublishPackageToOutput(System_IO_Abstractions_IDirectoryInfo_System_Uri_string_string)_packageVersion'></a>
`packageVersion` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Package version to publish
  
  
#### See Also
- [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand')
  
<a name='Cmf_Common_Cli_Commands_TemplateCommand'></a>
## TemplateCommand Class
```csharp
public abstract class TemplateCommand : Cmf.Common.Cli.Commands.BaseCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; TemplateCommand  

Derived  
&#8627; [InitCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_InitCommand 'Cmf.Common.Cli.Commands.InitCommand')  
### Constructors
<a name='Cmf_Common_Cli_Commands_TemplateCommand_TemplateCommand(string_System_IO_Abstractions_IFileSystem)'></a>
## TemplateCommand.TemplateCommand(string, IFileSystem) Constructor
constructor  
```csharp
protected TemplateCommand(string commandName, System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_TemplateCommand_TemplateCommand(string_System_IO_Abstractions_IFileSystem)_commandName'></a>
`commandName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Commands_TemplateCommand_TemplateCommand(string_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
<a name='Cmf_Common_Cli_Commands_TemplateCommand_TemplateCommand(string)'></a>
## TemplateCommand.TemplateCommand(string) Constructor
constructor  
```csharp
protected TemplateCommand(string commandName);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_TemplateCommand_TemplateCommand(string)_commandName'></a>
`commandName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_TemplateCommand_RunCommand(System_Collections_Generic_IReadOnlyCollection_string_)'></a>
## TemplateCommand.RunCommand(IReadOnlyCollection&lt;string&gt;) Method
Execute the command  
```csharp
public void RunCommand(System.Collections.Generic.IReadOnlyCollection<string> args);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_TemplateCommand_RunCommand(System_Collections_Generic_IReadOnlyCollection_string_)_args'></a>
`args` [System.Collections.Generic.IReadOnlyCollection&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IReadOnlyCollection-1 'System.Collections.Generic.IReadOnlyCollection`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IReadOnlyCollection-1 'System.Collections.Generic.IReadOnlyCollection`1')  
  
  
  

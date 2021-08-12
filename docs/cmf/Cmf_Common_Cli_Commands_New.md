#### [cmf](index.md 'index')
## Cmf.Common.Cli.Commands.New Namespace
### Classes
<a name='Cmf_Common_Cli_Commands_New_BusinessCommand'></a>
## BusinessCommand Class
Generates the Business layer structure  
```csharp
public class BusinessCommand : Cmf.Common.Cli.Commands.LayerTemplateCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [TemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_TemplateCommand 'Cmf.Common.Cli.Commands.TemplateCommand') &#129106; [LayerTemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_LayerTemplateCommand 'Cmf.Common.Cli.Commands.LayerTemplateCommand') &#129106; BusinessCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_New_BusinessCommand_BusinessCommand()'></a>
## BusinessCommand.BusinessCommand() Constructor
constructor  
```csharp
public BusinessCommand();
```
  
<a name='Cmf_Common_Cli_Commands_New_BusinessCommand_BusinessCommand(System_IO_Abstractions_IFileSystem)'></a>
## BusinessCommand.BusinessCommand(IFileSystem) Constructor
constructor  
```csharp
public BusinessCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_BusinessCommand_BusinessCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_New_BusinessCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)'></a>
## BusinessCommand.GenerateArgs(IDirectoryInfo, IDirectoryInfo, List&lt;string&gt;, JsonDocument) Method
generates additional arguments for the templating engine  
```csharp
protected override System.Collections.Generic.List<string> GenerateArgs(System.IO.Abstractions.IDirectoryInfo projectRoot, System.IO.Abstractions.IDirectoryInfo workingDir, System.Collections.Generic.List<string> args, System.Text.Json.JsonDocument projectConfig);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_BusinessCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectRoot'></a>
`projectRoot` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the project root
  
<a name='Cmf_Common_Cli_Commands_New_BusinessCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the current feature root (project root if no features exist)
  
<a name='Cmf_Common_Cli_Commands_New_BusinessCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_args'></a>
`args` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the base arguments: output, package name, version, id segment and tenant
  
<a name='Cmf_Common_Cli_Commands_New_BusinessCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectConfig'></a>
`projectConfig` [System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/System.Text.Json.JsonDocument 'System.Text.Json.JsonDocument')  
a JsonDocument with the .project-config.json content
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the complete list of arguments
  
  
<a name='Cmf_Common_Cli_Commands_New_DataCommand'></a>
## DataCommand Class
Generates Data package structure  
```csharp
public class DataCommand : Cmf.Common.Cli.Commands.LayerTemplateCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [TemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_TemplateCommand 'Cmf.Common.Cli.Commands.TemplateCommand') &#129106; [LayerTemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_LayerTemplateCommand 'Cmf.Common.Cli.Commands.LayerTemplateCommand') &#129106; DataCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_New_DataCommand_DataCommand()'></a>
## DataCommand.DataCommand() Constructor
constructor for System.IO filesystem  
```csharp
public DataCommand();
```
  
<a name='Cmf_Common_Cli_Commands_New_DataCommand_DataCommand(System_IO_Abstractions_IFileSystem)'></a>
## DataCommand.DataCommand(IFileSystem) Constructor
constructor  
```csharp
public DataCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_DataCommand_DataCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_New_DataCommand_Configure(System_CommandLine_Command)'></a>
## DataCommand.Configure(Command) Method
configure the command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_DataCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
base command
  
  
<a name='Cmf_Common_Cli_Commands_New_DataCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IDirectoryInfo)'></a>
## DataCommand.Execute(IDirectoryInfo, string, IDirectoryInfo) Method
Execute the command  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo workingDir, string version, System.IO.Abstractions.IDirectoryInfo businessPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_DataCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IDirectoryInfo)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the nearest root package
  
<a name='Cmf_Common_Cli_Commands_New_DataCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IDirectoryInfo)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
the package version
  
<a name='Cmf_Common_Cli_Commands_New_DataCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IDirectoryInfo)_businessPackage'></a>
`businessPackage` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
business package where the process rules should be built.
  
  
<a name='Cmf_Common_Cli_Commands_New_DataCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)'></a>
## DataCommand.GenerateArgs(IDirectoryInfo, IDirectoryInfo, List&lt;string&gt;, JsonDocument) Method
generates additional arguments for the templating engine  
```csharp
protected override System.Collections.Generic.List<string> GenerateArgs(System.IO.Abstractions.IDirectoryInfo projectRoot, System.IO.Abstractions.IDirectoryInfo workingDir, System.Collections.Generic.List<string> args, System.Text.Json.JsonDocument projectConfig);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_DataCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectRoot'></a>
`projectRoot` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the project root
  
<a name='Cmf_Common_Cli_Commands_New_DataCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the current feature root (project root if no features exist)
  
<a name='Cmf_Common_Cli_Commands_New_DataCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_args'></a>
`args` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the base arguments: output, package name, version, id segment and tenant
  
<a name='Cmf_Common_Cli_Commands_New_DataCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectConfig'></a>
`projectConfig` [System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/System.Text.Json.JsonDocument 'System.Text.Json.JsonDocument')  
a JsonDocument with the .project-config.json content
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the complete list of arguments
  
  
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand'></a>
## FeatureCommand Class
new feature command  
```csharp
public class FeatureCommand : Cmf.Common.Cli.Commands.TemplateCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [TemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_TemplateCommand 'Cmf.Common.Cli.Commands.TemplateCommand') &#129106; FeatureCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand_FeatureCommand()'></a>
## FeatureCommand.FeatureCommand() Constructor
constructor  
```csharp
public FeatureCommand();
```
  
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand_FeatureCommand(System_IO_Abstractions_IFileSystem)'></a>
## FeatureCommand.FeatureCommand(IFileSystem) Constructor
constructor  
```csharp
public FeatureCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand_FeatureCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand_Configure(System_CommandLine_Command)'></a>
## FeatureCommand.Configure(Command) Method
configure the command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
  
  
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string)'></a>
## FeatureCommand.Execute(IDirectoryInfo, string, string) Method
execute the command  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo workingDir, string packageName, string version);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
  
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string)_packageName'></a>
`packageName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Commands_New_FeatureCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_string)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
  
  
<a name='Cmf_Common_Cli_Commands_New_HelpCommand'></a>
## HelpCommand Class
Generates Help/Documentation package structure  
```csharp
public class HelpCommand : Cmf.Common.Cli.Commands.UILayerTemplateCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [TemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_TemplateCommand 'Cmf.Common.Cli.Commands.TemplateCommand') &#129106; [LayerTemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_LayerTemplateCommand 'Cmf.Common.Cli.Commands.LayerTemplateCommand') &#129106; [UILayerTemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_UILayerTemplateCommand 'Cmf.Common.Cli.Commands.UILayerTemplateCommand') &#129106; HelpCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_HelpCommand()'></a>
## HelpCommand.HelpCommand() Constructor
constructor for System.IO filesystem  
```csharp
public HelpCommand();
```
  
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_HelpCommand(System_IO_Abstractions_IFileSystem)'></a>
## HelpCommand.HelpCommand(IFileSystem) Constructor
constructor  
```csharp
public HelpCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_HelpCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_Configure(System_CommandLine_Command)'></a>
## HelpCommand.Configure(Command) Method
configure the command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
base command
  
  
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo)'></a>
## HelpCommand.Execute(IDirectoryInfo, string, IFileInfo) Method
Execute the command  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo workingDir, string version, System.IO.Abstractions.IFileInfo documentationPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
nearest root package
  
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
package version
  
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo)_documentationPackage'></a>
`documentationPackage` [System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
The MES documentation package path
  
  
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)'></a>
## HelpCommand.GenerateArgs(IDirectoryInfo, IDirectoryInfo, List&lt;string&gt;, JsonDocument) Method
generates additional arguments for the templating engine  
```csharp
protected override System.Collections.Generic.List<string> GenerateArgs(System.IO.Abstractions.IDirectoryInfo projectRoot, System.IO.Abstractions.IDirectoryInfo workingDir, System.Collections.Generic.List<string> args, System.Text.Json.JsonDocument projectConfig);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectRoot'></a>
`projectRoot` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the project root
  
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the current feature root (project root if no features exist)
  
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_args'></a>
`args` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the base arguments: output, package name, version, id segment and tenant
  
<a name='Cmf_Common_Cli_Commands_New_HelpCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectConfig'></a>
`projectConfig` [System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/System.Text.Json.JsonDocument 'System.Text.Json.JsonDocument')  
a JsonDocument with the .project-config.json content
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the complete list of arguments
  
  
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand'></a>
## HTMLCommand Class
Generates Help/Documentation package structure  
```csharp
public class HTMLCommand : Cmf.Common.Cli.Commands.UILayerTemplateCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [TemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_TemplateCommand 'Cmf.Common.Cli.Commands.TemplateCommand') &#129106; [LayerTemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_LayerTemplateCommand 'Cmf.Common.Cli.Commands.LayerTemplateCommand') &#129106; [UILayerTemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_UILayerTemplateCommand 'Cmf.Common.Cli.Commands.UILayerTemplateCommand') &#129106; HTMLCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_HTMLCommand()'></a>
## HTMLCommand.HTMLCommand() Constructor
constructor for System.IO filesystem  
```csharp
public HTMLCommand();
```
  
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_HTMLCommand(System_IO_Abstractions_IFileSystem)'></a>
## HTMLCommand.HTMLCommand(IFileSystem) Constructor
constructor  
```csharp
public HTMLCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_HTMLCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_Configure(System_CommandLine_Command)'></a>
## HTMLCommand.Configure(Command) Method
configure the command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
base command
  
  
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo)'></a>
## HTMLCommand.Execute(IDirectoryInfo, string, IFileInfo) Method
Execute the command  
```csharp
public void Execute(System.IO.Abstractions.IDirectoryInfo workingDir, string version, System.IO.Abstractions.IFileInfo htmlPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
nearest root package
  
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
package version
  
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_Execute(System_IO_Abstractions_IDirectoryInfo_string_System_IO_Abstractions_IFileInfo)_htmlPackage'></a>
`htmlPackage` [System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
The MES Presentation HTML package path
  
  
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)'></a>
## HTMLCommand.GenerateArgs(IDirectoryInfo, IDirectoryInfo, List&lt;string&gt;, JsonDocument) Method
generates additional arguments for the templating engine  
```csharp
protected override System.Collections.Generic.List<string> GenerateArgs(System.IO.Abstractions.IDirectoryInfo projectRoot, System.IO.Abstractions.IDirectoryInfo workingDir, System.Collections.Generic.List<string> args, System.Text.Json.JsonDocument projectConfig);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectRoot'></a>
`projectRoot` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the project root
  
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the current feature root (project root if no features exist)
  
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_args'></a>
`args` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the base arguments: output, package name, version, id segment and tenant
  
<a name='Cmf_Common_Cli_Commands_New_HTMLCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectConfig'></a>
`projectConfig` [System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/System.Text.Json.JsonDocument 'System.Text.Json.JsonDocument')  
a JsonDocument with the .project-config.json content
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the complete list of arguments
  
  
<a name='Cmf_Common_Cli_Commands_New_IoTCommand'></a>
## IoTCommand Class
Generates IoT package structure  
```csharp
public class IoTCommand : Cmf.Common.Cli.Commands.LayerTemplateCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [TemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_TemplateCommand 'Cmf.Common.Cli.Commands.TemplateCommand') &#129106; [LayerTemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_LayerTemplateCommand 'Cmf.Common.Cli.Commands.LayerTemplateCommand') &#129106; IoTCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_New_IoTCommand_IoTCommand()'></a>
## IoTCommand.IoTCommand() Constructor
constructor  
```csharp
public IoTCommand();
```
  
<a name='Cmf_Common_Cli_Commands_New_IoTCommand_IoTCommand(System_IO_Abstractions_IFileSystem)'></a>
## IoTCommand.IoTCommand(IFileSystem) Constructor
constructor  
```csharp
public IoTCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_IoTCommand_IoTCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the filesystem implementation
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_New_IoTCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)'></a>
## IoTCommand.GenerateArgs(IDirectoryInfo, IDirectoryInfo, List&lt;string&gt;, JsonDocument) Method
generates additional arguments for the templating engine  
```csharp
protected override System.Collections.Generic.List<string> GenerateArgs(System.IO.Abstractions.IDirectoryInfo projectRoot, System.IO.Abstractions.IDirectoryInfo workingDir, System.Collections.Generic.List<string> args, System.Text.Json.JsonDocument projectConfig);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_IoTCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectRoot'></a>
`projectRoot` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the project root
  
<a name='Cmf_Common_Cli_Commands_New_IoTCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the current feature root (project root if no features exist)
  
<a name='Cmf_Common_Cli_Commands_New_IoTCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_args'></a>
`args` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the base arguments: output, package name, version, id segment and tenant
  
<a name='Cmf_Common_Cli_Commands_New_IoTCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectConfig'></a>
`projectConfig` [System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/System.Text.Json.JsonDocument 'System.Text.Json.JsonDocument')  
a JsonDocument with the .project-config.json content
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the complete list of arguments
  
  
<a name='Cmf_Common_Cli_Commands_New_TestCommand'></a>
## TestCommand Class
Generates the Test layer structure  
```csharp
public class TestCommand : Cmf.Common.Cli.Commands.LayerTemplateCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [BaseCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_BaseCommand 'Cmf.Common.Cli.Commands.BaseCommand') &#129106; [TemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_TemplateCommand 'Cmf.Common.Cli.Commands.TemplateCommand') &#129106; [LayerTemplateCommand](Cmf_Common_Cli_Commands.md#Cmf_Common_Cli_Commands_LayerTemplateCommand 'Cmf.Common.Cli.Commands.LayerTemplateCommand') &#129106; TestCommand  
### Constructors
<a name='Cmf_Common_Cli_Commands_New_TestCommand_TestCommand()'></a>
## TestCommand.TestCommand() Constructor
constructor for System.IO filesystem  
```csharp
public TestCommand();
```
  
<a name='Cmf_Common_Cli_Commands_New_TestCommand_TestCommand(System_IO_Abstractions_IFileSystem)'></a>
## TestCommand.TestCommand(IFileSystem) Constructor
constructor  
```csharp
public TestCommand(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_TestCommand_TestCommand(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Methods
<a name='Cmf_Common_Cli_Commands_New_TestCommand_Configure(System_CommandLine_Command)'></a>
## TestCommand.Configure(Command) Method
configure the command  
```csharp
public override void Configure(System.CommandLine.Command cmd);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_TestCommand_Configure(System_CommandLine_Command)_cmd'></a>
`cmd` [System.CommandLine.Command](https://docs.microsoft.com/en-us/dotnet/api/System.CommandLine.Command 'System.CommandLine.Command')  
base command
  
  
<a name='Cmf_Common_Cli_Commands_New_TestCommand_Execute(string)'></a>
## TestCommand.Execute(string) Method
Execute the command  
```csharp
public void Execute(string version);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_TestCommand_Execute(string)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
the package version
  
  
<a name='Cmf_Common_Cli_Commands_New_TestCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)'></a>
## TestCommand.GenerateArgs(IDirectoryInfo, IDirectoryInfo, List&lt;string&gt;, JsonDocument) Method
generates additional arguments for the templating engine  
```csharp
protected override System.Collections.Generic.List<string> GenerateArgs(System.IO.Abstractions.IDirectoryInfo projectRoot, System.IO.Abstractions.IDirectoryInfo workingDir, System.Collections.Generic.List<string> args, System.Text.Json.JsonDocument projectConfig);
```
#### Parameters
<a name='Cmf_Common_Cli_Commands_New_TestCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectRoot'></a>
`projectRoot` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the project root
  
<a name='Cmf_Common_Cli_Commands_New_TestCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_workingDir'></a>
`workingDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
the current feature root (project root if no features exist)
  
<a name='Cmf_Common_Cli_Commands_New_TestCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_args'></a>
`args` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the base arguments: output, package name, version, id segment and tenant
  
<a name='Cmf_Common_Cli_Commands_New_TestCommand_GenerateArgs(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string__System_Text_Json_JsonDocument)_projectConfig'></a>
`projectConfig` [System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/System.Text.Json.JsonDocument 'System.Text.Json.JsonDocument')  
a JsonDocument with the .project-config.json content
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
the complete list of arguments
  
  

#### [cmf](index.md 'index')
## Cmf.Common.Cli.Builders Namespace
### Classes
<a name='Cmf_Common_Cli_Builders_CmdCommand'></a>
## CmdCommand Class
```csharp
public class CmdCommand : Cmf.Common.Cli.Builders.ProcessCommand,
Cmf.Common.Cli.Builders.IBuildCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand') &#129106; CmdCommand  

Implements [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')  
### Properties
<a name='Cmf_Common_Cli_Builders_CmdCommand_Args'></a>
## CmdCommand.Args Property
Gets or sets the arguments.  
```csharp
public string[] Args { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')
The arguments.  
  
<a name='Cmf_Common_Cli_Builders_CmdCommand_Command'></a>
## CmdCommand.Command Property
Gets or sets the command.  
```csharp
public string Command { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The command.  
  
<a name='Cmf_Common_Cli_Builders_CmdCommand_DisplayName'></a>
## CmdCommand.DisplayName Property
Gets or sets the display name.  
```csharp
public string DisplayName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The display name.  

Implements [DisplayName](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_DisplayName 'Cmf.Common.Cli.Builders.IBuildCommand.DisplayName')  
  
### Methods
<a name='Cmf_Common_Cli_Builders_CmdCommand_GetSteps()'></a>
## CmdCommand.GetSteps() Method
Gets the steps.  
```csharp
public override Cmf.Common.Cli.Builders.ProcessBuildStep[] GetSteps();
```
#### Returns
[ProcessBuildStep](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessBuildStep 'Cmf.Common.Cli.Builders.ProcessBuildStep')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
  
#### See Also
- [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand')
- [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')
  
<a name='Cmf_Common_Cli_Builders_ConsistencyCheckCommand'></a>
## ConsistencyCheckCommand Class
Checks the consistency of packages under a root  
```csharp
public class ConsistencyCheckCommand :
Cmf.Common.Cli.Builders.IBuildCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; ConsistencyCheckCommand  

Implements [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')  
### Properties
<a name='Cmf_Common_Cli_Builders_ConsistencyCheckCommand_DisplayName'></a>
## ConsistencyCheckCommand.DisplayName Property
Gets or sets the display name.  
```csharp
public string DisplayName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The display name.  

Implements [DisplayName](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_DisplayName 'Cmf.Common.Cli.Builders.IBuildCommand.DisplayName')  
  
<a name='Cmf_Common_Cli_Builders_ConsistencyCheckCommand_FileSystem'></a>
## ConsistencyCheckCommand.FileSystem Property
Virtual File System  
```csharp
public System.IO.Abstractions.IFileSystem FileSystem { get; set; }
```
#### Property Value
[System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')
  
<a name='Cmf_Common_Cli_Builders_ConsistencyCheckCommand_WorkingDirectory'></a>
## ConsistencyCheckCommand.WorkingDirectory Property
Hook to start search root algorithm  
```csharp
public System.IO.Abstractions.IDirectoryInfo WorkingDirectory { get; set; }
```
#### Property Value
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')
  
### Methods
<a name='Cmf_Common_Cli_Builders_ConsistencyCheckCommand_Exec()'></a>
## ConsistencyCheckCommand.Exec() Method
Find Root Package, check dependencies, enforce consistency of package version  
```csharp
public System.Threading.Tasks.Task Exec();
```
#### Returns
[System.Threading.Tasks.Task](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task 'System.Threading.Tasks.Task')  

Implements [Exec()](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_Exec() 'Cmf.Common.Cli.Builders.IBuildCommand.Exec()')  
  
#### See Also
- [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand')
- [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')
  
<a name='Cmf_Common_Cli_Builders_DotnetCommand'></a>
## DotnetCommand Class
```csharp
public class DotnetCommand : Cmf.Common.Cli.Builders.ProcessCommand,
Cmf.Common.Cli.Builders.IBuildCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand') &#129106; DotnetCommand  

Implements [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')  
### Properties
<a name='Cmf_Common_Cli_Builders_DotnetCommand_Args'></a>
## DotnetCommand.Args Property
Gets or sets the arguments.  
```csharp
public string[] Args { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')
The arguments.  
  
<a name='Cmf_Common_Cli_Builders_DotnetCommand_Command'></a>
## DotnetCommand.Command Property
Gets or sets the command.  
```csharp
public string Command { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The command.  
  
<a name='Cmf_Common_Cli_Builders_DotnetCommand_Configuration'></a>
## DotnetCommand.Configuration Property
Gets or sets the configuration.  
```csharp
public string Configuration { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The configuration.  
  
<a name='Cmf_Common_Cli_Builders_DotnetCommand_DisplayName'></a>
## DotnetCommand.DisplayName Property
Gets or sets the display name.  
```csharp
public string DisplayName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The display name.  

Implements [DisplayName](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_DisplayName 'Cmf.Common.Cli.Builders.IBuildCommand.DisplayName')  
  
<a name='Cmf_Common_Cli_Builders_DotnetCommand_NuGetConfig'></a>
## DotnetCommand.NuGetConfig Property
Gets or sets the nu get configuration.  
```csharp
public System.IO.Abstractions.IFileInfo NuGetConfig { get; set; }
```
#### Property Value
[System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')
The nu get configuration.  
  
<a name='Cmf_Common_Cli_Builders_DotnetCommand_OutputDirectory'></a>
## DotnetCommand.OutputDirectory Property
Gets or sets the output directory.  
```csharp
public System.IO.Abstractions.IDirectoryInfo OutputDirectory { get; set; }
```
#### Property Value
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')
The output directory.  
  
<a name='Cmf_Common_Cli_Builders_DotnetCommand_Solution'></a>
## DotnetCommand.Solution Property
Gets or sets the solution.  
```csharp
public System.IO.Abstractions.IFileInfo Solution { get; set; }
```
#### Property Value
[System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')
The solution.  
  
### Methods
<a name='Cmf_Common_Cli_Builders_DotnetCommand_GetSteps()'></a>
## DotnetCommand.GetSteps() Method
Gets the steps.  
```csharp
public override Cmf.Common.Cli.Builders.ProcessBuildStep[] GetSteps();
```
#### Returns
[ProcessBuildStep](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessBuildStep 'Cmf.Common.Cli.Builders.ProcessBuildStep')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
  
#### See Also
- [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand')
- [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')
  
<a name='Cmf_Common_Cli_Builders_ExecuteCommand_T_'></a>
## ExecuteCommand&lt;T&gt; Class
```csharp
public class ExecuteCommand<T> :
Cmf.Common.Cli.Builders.IBuildCommand
    where T : Cmf.Common.Cli.Commands.BaseCommand
```
#### Type parameters
<a name='Cmf_Common_Cli_Builders_ExecuteCommand_T__T'></a>
`T`  
  

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; ExecuteCommand&lt;T&gt;  

Implements [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')  
### Properties
<a name='Cmf_Common_Cli_Builders_ExecuteCommand_T__Command'></a>
## ExecuteCommand&lt;T&gt;.Command Property
Gets or sets the command.  
```csharp
public T Command { get; set; }
```
#### Property Value
[T](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ExecuteCommand_T__T 'Cmf.Common.Cli.Builders.ExecuteCommand&lt;T&gt;.T')
The command.  
  
<a name='Cmf_Common_Cli_Builders_ExecuteCommand_T__DisplayName'></a>
## ExecuteCommand&lt;T&gt;.DisplayName Property
Gets or sets the display name.  
```csharp
public string DisplayName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The display name.  

Implements [DisplayName](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_DisplayName 'Cmf.Common.Cli.Builders.IBuildCommand.DisplayName')  
  
<a name='Cmf_Common_Cli_Builders_ExecuteCommand_T__Execute'></a>
## ExecuteCommand&lt;T&gt;.Execute Property
Gets or sets the execute.  
```csharp
public System.Action<T> Execute { get; set; }
```
#### Property Value
[System.Action&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Action-1 'System.Action`1')[T](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ExecuteCommand_T__T 'Cmf.Common.Cli.Builders.ExecuteCommand&lt;T&gt;.T')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Action-1 'System.Action`1')
The execute.  
  
### Methods
<a name='Cmf_Common_Cli_Builders_ExecuteCommand_T__Exec()'></a>
## ExecuteCommand&lt;T&gt;.Exec() Method
Executes this instance.  
```csharp
public System.Threading.Tasks.Task Exec();
```
#### Returns
[System.Threading.Tasks.Task](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task 'System.Threading.Tasks.Task')  

Implements [Exec()](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_Exec() 'Cmf.Common.Cli.Builders.IBuildCommand.Exec()')  
  
#### See Also
- [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')
  
<a name='Cmf_Common_Cli_Builders_GitCommand'></a>
## GitCommand Class
Execute a git command  
```csharp
public class GitCommand : Cmf.Common.Cli.Builders.ProcessCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand') &#129106; GitCommand  
### Properties
<a name='Cmf_Common_Cli_Builders_GitCommand_Args'></a>
## GitCommand.Args Property
Gets or sets the arguments.  
```csharp
public string[] Args { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')
The arguments.  
  
<a name='Cmf_Common_Cli_Builders_GitCommand_Command'></a>
## GitCommand.Command Property
Gets or sets the command.  
```csharp
public string Command { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The command.  
  
### Methods
<a name='Cmf_Common_Cli_Builders_GitCommand_GetSteps()'></a>
## GitCommand.GetSteps() Method
Gets the steps.  
```csharp
public override Cmf.Common.Cli.Builders.ProcessBuildStep[] GetSteps();
```
#### Returns
[ProcessBuildStep](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessBuildStep 'Cmf.Common.Cli.Builders.ProcessBuildStep')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
  
  
<a name='Cmf_Common_Cli_Builders_GulpCommand'></a>
## GulpCommand Class
```csharp
public class GulpCommand : Cmf.Common.Cli.Builders.ProcessCommand,
Cmf.Common.Cli.Builders.IBuildCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand') &#129106; GulpCommand  

Implements [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')  
### Properties
<a name='Cmf_Common_Cli_Builders_GulpCommand_Args'></a>
## GulpCommand.Args Property
Gets or sets the arguments.  
```csharp
public string[] Args { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')
The arguments.  
  
<a name='Cmf_Common_Cli_Builders_GulpCommand_DisplayName'></a>
## GulpCommand.DisplayName Property
Gets or sets the display name.  
```csharp
public string DisplayName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The display name.  

Implements [DisplayName](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_DisplayName 'Cmf.Common.Cli.Builders.IBuildCommand.DisplayName')  
  
<a name='Cmf_Common_Cli_Builders_GulpCommand_GulpFile'></a>
## GulpCommand.GulpFile Property
Gets or sets the gulp file.  
```csharp
public string GulpFile { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The gulp file.  
  
<a name='Cmf_Common_Cli_Builders_GulpCommand_GulpJS'></a>
## GulpCommand.GulpJS Property
Gets or sets the gulp js.  
```csharp
public string GulpJS { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The gulp js.  
  
<a name='Cmf_Common_Cli_Builders_GulpCommand_Task'></a>
## GulpCommand.Task Property
Gets or sets the task.  
```csharp
public string Task { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The task.  
  
### Methods
<a name='Cmf_Common_Cli_Builders_GulpCommand_GetSteps()'></a>
## GulpCommand.GetSteps() Method
Gets the steps.  
```csharp
public override Cmf.Common.Cli.Builders.ProcessBuildStep[] GetSteps();
```
#### Returns
[ProcessBuildStep](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessBuildStep 'Cmf.Common.Cli.Builders.ProcessBuildStep')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
  
#### See Also
- [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand')
- [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')
  
<a name='Cmf_Common_Cli_Builders_JSONValidatorCommand'></a>
## JSONValidatorCommand Class
Validator for json files  
```csharp
public class JSONValidatorCommand :
Cmf.Common.Cli.Builders.IBuildCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; JSONValidatorCommand  

Implements [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')  
### Properties
<a name='Cmf_Common_Cli_Builders_JSONValidatorCommand_DisplayName'></a>
## JSONValidatorCommand.DisplayName Property
Gets or sets the display name.  
```csharp
public string DisplayName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The display name.  

Implements [DisplayName](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_DisplayName 'Cmf.Common.Cli.Builders.IBuildCommand.DisplayName')  
  
<a name='Cmf_Common_Cli_Builders_JSONValidatorCommand_FilesToValidate'></a>
## JSONValidatorCommand.FilesToValidate Property
Gets or sets the command.  
```csharp
public System.Collections.Generic.List<Cmf.Common.Cli.Objects.FileToPack> FilesToValidate { get; set; }
```
#### Property Value
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[FileToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_FileToPack 'Cmf.Common.Cli.Objects.FileToPack')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')
The command.  
  
### Methods
<a name='Cmf_Common_Cli_Builders_JSONValidatorCommand_Exec()'></a>
## JSONValidatorCommand.Exec() Method
Search all the json files and validate them  
```csharp
public System.Threading.Tasks.Task Exec();
```
#### Returns
[System.Threading.Tasks.Task](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task 'System.Threading.Tasks.Task')  

Implements [Exec()](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_Exec() 'Cmf.Common.Cli.Builders.IBuildCommand.Exec()')  
  
#### See Also
- [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand')
- [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')
  
<a name='Cmf_Common_Cli_Builders_NPMCommand'></a>
## NPMCommand Class
```csharp
public class NPMCommand : Cmf.Common.Cli.Builders.ProcessCommand,
Cmf.Common.Cli.Builders.IBuildCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand') &#129106; NPMCommand  

Implements [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')  
### Properties
<a name='Cmf_Common_Cli_Builders_NPMCommand_Args'></a>
## NPMCommand.Args Property
Gets or sets the arguments.  
```csharp
public string[] Args { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')
The arguments.  
  
<a name='Cmf_Common_Cli_Builders_NPMCommand_Command'></a>
## NPMCommand.Command Property
Gets or sets the command.  
```csharp
public string Command { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The command.  
  
<a name='Cmf_Common_Cli_Builders_NPMCommand_DisplayName'></a>
## NPMCommand.DisplayName Property
Gets or sets the display name.  
```csharp
public string DisplayName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The display name.  

Implements [DisplayName](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_DisplayName 'Cmf.Common.Cli.Builders.IBuildCommand.DisplayName')  
  
### Methods
<a name='Cmf_Common_Cli_Builders_NPMCommand_GetSteps()'></a>
## NPMCommand.GetSteps() Method
Gets the steps.  
```csharp
public override Cmf.Common.Cli.Builders.ProcessBuildStep[] GetSteps();
```
#### Returns
[ProcessBuildStep](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessBuildStep 'Cmf.Common.Cli.Builders.ProcessBuildStep')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
  
#### See Also
- [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand')
- [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')
  
<a name='Cmf_Common_Cli_Builders_NPXCommand'></a>
## NPXCommand Class
run npx command  
```csharp
public class NPXCommand : Cmf.Common.Cli.Builders.ProcessCommand,
Cmf.Common.Cli.Builders.IBuildCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [ProcessCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessCommand 'Cmf.Common.Cli.Builders.ProcessCommand') &#129106; NPXCommand  

Implements [IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')  
### Properties
<a name='Cmf_Common_Cli_Builders_NPXCommand_Args'></a>
## NPXCommand.Args Property
Gets or sets the arguments.  
```csharp
public string[] Args { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')
The arguments.  
  
<a name='Cmf_Common_Cli_Builders_NPXCommand_Command'></a>
## NPXCommand.Command Property
Gets or sets the command.  
```csharp
public string Command { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The command.  
  
<a name='Cmf_Common_Cli_Builders_NPXCommand_DisplayName'></a>
## NPXCommand.DisplayName Property
Gets or sets the display name.  
```csharp
public string DisplayName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The display name.  

Implements [DisplayName](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand_DisplayName 'Cmf.Common.Cli.Builders.IBuildCommand.DisplayName')  
  
### Methods
<a name='Cmf_Common_Cli_Builders_NPXCommand_GetSteps()'></a>
## NPXCommand.GetSteps() Method
Gets the steps.  
```csharp
public override Cmf.Common.Cli.Builders.ProcessBuildStep[] GetSteps();
```
#### Returns
[ProcessBuildStep](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessBuildStep 'Cmf.Common.Cli.Builders.ProcessBuildStep')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
  
  
<a name='Cmf_Common_Cli_Builders_ProcessBuildStep'></a>
## ProcessBuildStep Class
```csharp
public class ProcessBuildStep
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; ProcessBuildStep  
### Properties
<a name='Cmf_Common_Cli_Builders_ProcessBuildStep_Args'></a>
## ProcessBuildStep.Args Property
Gets or sets the arguments.  
```csharp
public string[] Args { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')
The arguments.  
  
<a name='Cmf_Common_Cli_Builders_ProcessBuildStep_Command'></a>
## ProcessBuildStep.Command Property
Gets or sets the command.  
```csharp
public string Command { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The command.  
  
<a name='Cmf_Common_Cli_Builders_ProcessBuildStep_WorkingDirectory'></a>
## ProcessBuildStep.WorkingDirectory Property
Gets or sets the working directory.  
```csharp
public System.IO.Abstractions.IDirectoryInfo WorkingDirectory { get; set; }
```
#### Property Value
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')
The working directory.  
  
  
<a name='Cmf_Common_Cli_Builders_ProcessCommand'></a>
## ProcessCommand Class
```csharp
public abstract class ProcessCommand
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; ProcessCommand  

Derived  
&#8627; [CmdCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_CmdCommand 'Cmf.Common.Cli.Builders.CmdCommand')  
&#8627; [DotnetCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_DotnetCommand 'Cmf.Common.Cli.Builders.DotnetCommand')  
&#8627; [GitCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_GitCommand 'Cmf.Common.Cli.Builders.GitCommand')  
&#8627; [GulpCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_GulpCommand 'Cmf.Common.Cli.Builders.GulpCommand')  
&#8627; [NPMCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_NPMCommand 'Cmf.Common.Cli.Builders.NPMCommand')  
&#8627; [NPXCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_NPXCommand 'Cmf.Common.Cli.Builders.NPXCommand')  
### Fields
<a name='Cmf_Common_Cli_Builders_ProcessCommand_fileSystem'></a>
## ProcessCommand.fileSystem Field
the underlying file system  
```csharp
protected IFileSystem fileSystem;
```
#### Field Value
[System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')
  
### Properties
<a name='Cmf_Common_Cli_Builders_ProcessCommand_WorkingDirectory'></a>
## ProcessCommand.WorkingDirectory Property
Gets or sets the working directory.  
```csharp
public System.IO.Abstractions.IDirectoryInfo WorkingDirectory { get; set; }
```
#### Property Value
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')
The working directory.  
  
### Methods
<a name='Cmf_Common_Cli_Builders_ProcessCommand_Exec()'></a>
## ProcessCommand.Exec() Method
Executes this instance.  
```csharp
public System.Threading.Tasks.Task Exec();
```
#### Returns
[System.Threading.Tasks.Task](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task 'System.Threading.Tasks.Task')  
  
<a name='Cmf_Common_Cli_Builders_ProcessCommand_GetSteps()'></a>
## ProcessCommand.GetSteps() Method
Gets the steps.  
```csharp
public abstract Cmf.Common.Cli.Builders.ProcessBuildStep[] GetSteps();
```
#### Returns
[ProcessBuildStep](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ProcessBuildStep 'Cmf.Common.Cli.Builders.ProcessBuildStep')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
  
  
### Interfaces
<a name='Cmf_Common_Cli_Builders_IBuildCommand'></a>
## IBuildCommand Interface
```csharp
public interface IBuildCommand
```

Derived  
&#8627; [CmdCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_CmdCommand 'Cmf.Common.Cli.Builders.CmdCommand')  
&#8627; [ConsistencyCheckCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ConsistencyCheckCommand 'Cmf.Common.Cli.Builders.ConsistencyCheckCommand')  
&#8627; [DotnetCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_DotnetCommand 'Cmf.Common.Cli.Builders.DotnetCommand')  
&#8627; [ExecuteCommand&lt;T&gt;](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_ExecuteCommand_T_ 'Cmf.Common.Cli.Builders.ExecuteCommand&lt;T&gt;')  
&#8627; [GulpCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_GulpCommand 'Cmf.Common.Cli.Builders.GulpCommand')  
&#8627; [JSONValidatorCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_JSONValidatorCommand 'Cmf.Common.Cli.Builders.JSONValidatorCommand')  
&#8627; [NPMCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_NPMCommand 'Cmf.Common.Cli.Builders.NPMCommand')  
&#8627; [NPXCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_NPXCommand 'Cmf.Common.Cli.Builders.NPXCommand')  
### Properties
<a name='Cmf_Common_Cli_Builders_IBuildCommand_DisplayName'></a>
## IBuildCommand.DisplayName Property
Gets or sets the display name.  
```csharp
string DisplayName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The display name.  
  
### Methods
<a name='Cmf_Common_Cli_Builders_IBuildCommand_Exec()'></a>
## IBuildCommand.Exec() Method
Executes this instance.  
```csharp
System.Threading.Tasks.Task Exec();
```
#### Returns
[System.Threading.Tasks.Task](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task 'System.Threading.Tasks.Task')  
  
  

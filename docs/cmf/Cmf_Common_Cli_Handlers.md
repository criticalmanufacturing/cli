#### [cmf](index.md 'index')
## Cmf.Common.Cli.Handlers Namespace
### Classes
<a name='Cmf_Common_Cli_Handlers_BusinessPackageTypeHandler'></a>
## BusinessPackageTypeHandler Class
```csharp
public class BusinessPackageTypeHandler : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; BusinessPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_BusinessPackageTypeHandler_BusinessPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## BusinessPackageTypeHandler.BusinessPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [BusinessPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_BusinessPackageTypeHandler 'Cmf.Common.Cli.Handlers.BusinessPackageTypeHandler') class.  
```csharp
public BusinessPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_BusinessPackageTypeHandler_BusinessPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
### Methods
<a name='Cmf_Common_Cli_Handlers_BusinessPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)'></a>
## BusinessPackageTypeHandler.Bump(string, string, Dictionary&lt;string,object&gt;) Method
Bumps the specified CMF package.  
```csharp
public override void Bump(string version, string buildNr, System.Collections.Generic.Dictionary<string,object> bumpInformation=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_BusinessPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Handlers_BusinessPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version for build Nr.
  
<a name='Cmf_Common_Cli_Handlers_BusinessPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_bumpInformation'></a>
`bumpInformation` [System.Collections.Generic.Dictionary&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[,](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')  
The bump information.
  

Implements [Bump(string, string, Dictionary<string,object>)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Bump(string, string, System.Collections.Generic.Dictionary&lt;string,object&gt;)')  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_DatabasePackageTypeHandler'></a>
## DatabasePackageTypeHandler Class
```csharp
public class DatabasePackageTypeHandler : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; DatabasePackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_DatabasePackageTypeHandler_DatabasePackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## DatabasePackageTypeHandler.DatabasePackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [DatabasePackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_DatabasePackageTypeHandler 'Cmf.Common.Cli.Handlers.DatabasePackageTypeHandler') class.  
```csharp
public DatabasePackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_DatabasePackageTypeHandler_DatabasePackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandler'></a>
## DataPackageTypeHandler Class
```csharp
public class DataPackageTypeHandler : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; DataPackageTypeHandler  

Derived  
&#8627; [IoTDataPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_IoTDataPackageTypeHandler 'Cmf.Common.Cli.Handlers.IoTDataPackageTypeHandler')  
### Constructors
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandler_DataPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## DataPackageTypeHandler.DataPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [DataPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_DataPackageTypeHandler 'Cmf.Common.Cli.Handlers.DataPackageTypeHandler') class.  
```csharp
public DataPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandler_DataPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
### Methods
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandler_CopyInstallDependencies(System_IO_Abstractions_IDirectoryInfo)'></a>
## DataPackageTypeHandler.CopyInstallDependencies(IDirectoryInfo) Method
Copies the install dependencies.  
```csharp
protected override void CopyInstallDependencies(System.IO.Abstractions.IDirectoryInfo packageOutputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandler_CopyInstallDependencies(System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2'></a>
## DataPackageTypeHandlerV2 Class
```csharp
public class DataPackageTypeHandlerV2 : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; DataPackageTypeHandlerV2  
### Constructors
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2_DataPackageTypeHandlerV2(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## DataPackageTypeHandlerV2.DataPackageTypeHandlerV2(CmfPackage) Constructor
Initializes a new instance of the [DataPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_DataPackageTypeHandler 'Cmf.Common.Cli.Handlers.DataPackageTypeHandler') class.  
```csharp
public DataPackageTypeHandlerV2(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2_DataPackageTypeHandlerV2(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
### Methods
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2_GenerateDeploymentFrameworkManifest(System_IO_Abstractions_IDirectoryInfo)'></a>
## DataPackageTypeHandlerV2.GenerateDeploymentFrameworkManifest(IDirectoryInfo) Method
Generates the deployment framework manifest.  
```csharp
internal override void GenerateDeploymentFrameworkManifest(System.IO.Abstractions.IDirectoryInfo packageOutputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2_GenerateDeploymentFrameworkManifest(System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2_GenerateHostConfigFile(System_IO_Abstractions_IDirectoryInfo)'></a>
## DataPackageTypeHandlerV2.GenerateHostConfigFile(IDirectoryInfo) Method
Generates the host configuration file.  
```csharp
private void GenerateHostConfigFile(System.IO.Abstractions.IDirectoryInfo packageOutputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2_GenerateHostConfigFile(System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
  
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)'></a>
## DataPackageTypeHandlerV2.Pack(IDirectoryInfo, IDirectoryInfo) Method
Pack a Data package  
```csharp
public override void Pack(System.IO.Abstractions.IDirectoryInfo packageOutputDir, System.IO.Abstractions.IDirectoryInfo outputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
source directory
  
<a name='Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
output directory
  

Implements [Pack(IDirectoryInfo, IDirectoryInfo)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Pack(System.IO.Abstractions.IDirectoryInfo, System.IO.Abstractions.IDirectoryInfo)')  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_ExportedObjectsPackageTypeHandler'></a>
## ExportedObjectsPackageTypeHandler Class
```csharp
public class ExportedObjectsPackageTypeHandler : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; ExportedObjectsPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_ExportedObjectsPackageTypeHandler_ExportedObjectsPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## ExportedObjectsPackageTypeHandler.ExportedObjectsPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [ExportedObjectsPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_ExportedObjectsPackageTypeHandler 'Cmf.Common.Cli.Handlers.ExportedObjectsPackageTypeHandler') class.  
```csharp
public ExportedObjectsPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_ExportedObjectsPackageTypeHandler_ExportedObjectsPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_GenericPackageTypeHandler'></a>
## GenericPackageTypeHandler Class
```csharp
public class GenericPackageTypeHandler : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; GenericPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_GenericPackageTypeHandler_GenericPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## GenericPackageTypeHandler.GenericPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [GenericPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_GenericPackageTypeHandler 'Cmf.Common.Cli.Handlers.GenericPackageTypeHandler') class.  
```csharp
public GenericPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_GenericPackageTypeHandler_GenericPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_HelpPackageTypeHandler'></a>
## HelpPackageTypeHandler Class
```csharp
public class HelpPackageTypeHandler : Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; [PresentationPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler 'Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler') &#129106; HelpPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_HelpPackageTypeHandler_HelpPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## HelpPackageTypeHandler.HelpPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [HelpPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_HelpPackageTypeHandler 'Cmf.Common.Cli.Handlers.HelpPackageTypeHandler') class.  
```csharp
public HelpPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_HelpPackageTypeHandler_HelpPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
  
  
#### See Also
- [PresentationPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler 'Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_HtmlPackageTypeHandler'></a>
## HtmlPackageTypeHandler Class
```csharp
public class HtmlPackageTypeHandler : Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; [PresentationPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler 'Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler') &#129106; HtmlPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_HtmlPackageTypeHandler_HtmlPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## HtmlPackageTypeHandler.HtmlPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [HtmlPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_HtmlPackageTypeHandler 'Cmf.Common.Cli.Handlers.HtmlPackageTypeHandler') class.  
```csharp
public HtmlPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_HtmlPackageTypeHandler_HtmlPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
  
  
#### See Also
- [PresentationPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler 'Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_IoTDataPackageTypeHandler'></a>
## IoTDataPackageTypeHandler Class
```csharp
public class IoTDataPackageTypeHandler : Cmf.Common.Cli.Handlers.DataPackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; [DataPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_DataPackageTypeHandler 'Cmf.Common.Cli.Handlers.DataPackageTypeHandler') &#129106; IoTDataPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_IoTDataPackageTypeHandler_IoTDataPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## IoTDataPackageTypeHandler.IoTDataPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [DataPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_DataPackageTypeHandler 'Cmf.Common.Cli.Handlers.DataPackageTypeHandler') class.  
```csharp
public IoTDataPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_IoTDataPackageTypeHandler_IoTDataPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
### Methods
<a name='Cmf_Common_Cli_Handlers_IoTDataPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)'></a>
## IoTDataPackageTypeHandler.Bump(string, string, Dictionary&lt;string,object&gt;) Method
Bumps the specified CMF package.  
```csharp
public override void Bump(string version, string buildNr, System.Collections.Generic.Dictionary<string,object> bumpInformation=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_IoTDataPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Handlers_IoTDataPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version for build Nr.
  
<a name='Cmf_Common_Cli_Handlers_IoTDataPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_bumpInformation'></a>
`bumpInformation` [System.Collections.Generic.Dictionary&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[,](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')  
The bump information.
  

Implements [Bump(string, string, Dictionary<string,object>)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Bump(string, string, System.Collections.Generic.Dictionary&lt;string,object&gt;)')  
  
#### See Also
- [DataPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_DataPackageTypeHandler 'Cmf.Common.Cli.Handlers.DataPackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler'></a>
## IoTPackageTypeHandler Class
```csharp
public class IoTPackageTypeHandler : Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; [PresentationPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler 'Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler') &#129106; IoTPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_IoTPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## IoTPackageTypeHandler.IoTPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [IoTPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_IoTPackageTypeHandler 'Cmf.Common.Cli.Handlers.IoTPackageTypeHandler') class.  
```csharp
public IoTPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_IoTPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
### Methods
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)'></a>
## IoTPackageTypeHandler.Bump(string, string, Dictionary&lt;string,object&gt;) Method
Bumps the specified CMF package.  
```csharp
public override void Bump(string version, string buildNr, System.Collections.Generic.Dictionary<string,object> bumpInformation=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version for build Nr.
  
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_bumpInformation'></a>
`bumpInformation` [System.Collections.Generic.Dictionary&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[,](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')  
The bump information.
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  

Implements [Bump(string, string, Dictionary<string,object>)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Bump(string, string, System.Collections.Generic.Dictionary&lt;string,object&gt;)')  
  
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_CopyInstallDependencies(System_IO_Abstractions_IDirectoryInfo)'></a>
## IoTPackageTypeHandler.CopyInstallDependencies(IDirectoryInfo) Method
Copies the install dependencies.  
```csharp
protected override void CopyInstallDependencies(System.IO.Abstractions.IDirectoryInfo packageOutputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_CopyInstallDependencies(System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
  
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)'></a>
## IoTPackageTypeHandler.Pack(IDirectoryInfo, IDirectoryInfo) Method
Packs the specified package output dir.  
```csharp
public override void Pack(System.IO.Abstractions.IDirectoryInfo packageOutputDir, System.IO.Abstractions.IDirectoryInfo outputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
<a name='Cmf_Common_Cli_Handlers_IoTPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The output dir.
  

Implements [Pack(IDirectoryInfo, IDirectoryInfo)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Pack(System.IO.Abstractions.IDirectoryInfo, System.IO.Abstractions.IDirectoryInfo)')  
  
#### See Also
- [PresentationPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler 'Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler'></a>
## PackageTypeHandler Class
```csharp
public abstract class PackageTypeHandler :
Cmf.Common.Cli.Interfaces.IPackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; PackageTypeHandler  

Derived  
&#8627; [BusinessPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_BusinessPackageTypeHandler 'Cmf.Common.Cli.Handlers.BusinessPackageTypeHandler')  
&#8627; [DatabasePackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_DatabasePackageTypeHandler 'Cmf.Common.Cli.Handlers.DatabasePackageTypeHandler')  
&#8627; [DataPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_DataPackageTypeHandler 'Cmf.Common.Cli.Handlers.DataPackageTypeHandler')  
&#8627; [DataPackageTypeHandlerV2](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_DataPackageTypeHandlerV2 'Cmf.Common.Cli.Handlers.DataPackageTypeHandlerV2')  
&#8627; [ExportedObjectsPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_ExportedObjectsPackageTypeHandler 'Cmf.Common.Cli.Handlers.ExportedObjectsPackageTypeHandler')  
&#8627; [GenericPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_GenericPackageTypeHandler 'Cmf.Common.Cli.Handlers.GenericPackageTypeHandler')  
&#8627; [PresentationPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler 'Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler')  
&#8627; [ReportingPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_ReportingPackageTypeHandler 'Cmf.Common.Cli.Handlers.ReportingPackageTypeHandler')  
&#8627; [RootPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_RootPackageTypeHandler 'Cmf.Common.Cli.Handlers.RootPackageTypeHandler')  
&#8627; [TestPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_TestPackageTypeHandler 'Cmf.Common.Cli.Handlers.TestPackageTypeHandler')  

Implements [IPackageTypeHandler](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler')  
### Constructors
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_PackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## PackageTypeHandler.PackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') class.  
```csharp
public PackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_PackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_PackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IFileSystem)'></a>
## PackageTypeHandler.PackageTypeHandler(CmfPackage, IFileSystem) Constructor
Initializes a new instance of the [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') class.  
```csharp
public PackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage, System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_PackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IFileSystem)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_PackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Fields
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_BuildSteps'></a>
## PackageTypeHandler.BuildSteps Field
Gets or sets the build steps.  
```csharp
protected IBuildCommand[] BuildSteps;
```
#### Field Value
[IBuildCommand](Cmf_Common_Cli_Builders.md#Cmf_Common_Cli_Builders_IBuildCommand 'Cmf.Common.Cli.Builders.IBuildCommand')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_CmfPackage'></a>
## PackageTypeHandler.CmfPackage Field
The CMF package  
```csharp
protected CmfPackage CmfPackage;
```
#### Field Value
[CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_DFPackageType'></a>
## PackageTypeHandler.DFPackageType Field
The df package type  
```csharp
protected PackageType DFPackageType;
```
#### Field Value
[PackageType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackageType 'Cmf.Common.Cli.Enums.PackageType')
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_FilesToPack'></a>
## PackageTypeHandler.FilesToPack Field
The files to pack  
```csharp
protected List<FileToPack> FilesToPack;
```
#### Field Value
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[FileToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_FileToPack 'Cmf.Common.Cli.Objects.FileToPack')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_fileSystem'></a>
## PackageTypeHandler.fileSystem Field
the underlying file system  
```csharp
protected IFileSystem fileSystem;
```
#### Field Value
[System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')
  
### Properties
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_DefaultContentToIgnore'></a>
## PackageTypeHandler.DefaultContentToIgnore Property
Gets or sets the default content to ignore.  
```csharp
public System.Collections.Generic.List<string> DefaultContentToIgnore { get; }
```
#### Property Value
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')
The default content to ignore.  

Implements [DefaultContentToIgnore](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_DefaultContentToIgnore 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.DefaultContentToIgnore')  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_DependenciesFolder'></a>
## PackageTypeHandler.DependenciesFolder Property
Where should the dependencies go, relative to the cmfpackage.json file  
```csharp
public System.IO.Abstractions.IDirectoryInfo DependenciesFolder { get; set; }
```
#### Property Value
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')
  
### Methods
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_Build()'></a>
## PackageTypeHandler.Build() Method
Builds this instance.  
```csharp
public virtual void Build();
```

Implements [Build()](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Build() 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Build()')  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)'></a>
## PackageTypeHandler.Bump(string, string, Dictionary&lt;string,object&gt;) Method
Bumps the specified version.  
```csharp
public virtual void Bump(string version, string buildNr, System.Collections.Generic.Dictionary<string,object> bumpInformation=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version for build Nr.
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_bumpInformation'></a>
`bumpInformation` [System.Collections.Generic.Dictionary&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[,](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')  
The bump information.
  

Implements [Bump(string, string, Dictionary<string,object>)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Bump(string, string, System.Collections.Generic.Dictionary&lt;string,object&gt;)')  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_CopyInstallDependencies(System_IO_Abstractions_IDirectoryInfo)'></a>
## PackageTypeHandler.CopyInstallDependencies(IDirectoryInfo) Method
Copies the install dependencies.  
```csharp
protected virtual void CopyInstallDependencies(System.IO.Abstractions.IDirectoryInfo packageOutputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_CopyInstallDependencies(System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_FinalArchive(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)'></a>
## PackageTypeHandler.FinalArchive(IDirectoryInfo, IDirectoryInfo) Method
Final Archive the package  
```csharp
internal virtual void FinalArchive(System.IO.Abstractions.IDirectoryInfo packageOutputDir, System.IO.Abstractions.IDirectoryInfo outputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_FinalArchive(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The pack directory.
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_FinalArchive(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The Output directory.
  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_GenerateDeploymentFrameworkManifest(System_IO_Abstractions_IDirectoryInfo)'></a>
## PackageTypeHandler.GenerateDeploymentFrameworkManifest(IDirectoryInfo) Method
Generates the deployment framework manifest.  
```csharp
internal virtual void GenerateDeploymentFrameworkManifest(System.IO.Abstractions.IDirectoryInfo packageOutputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_GenerateDeploymentFrameworkManifest(System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_GetContentToIgnore(Cmf_Common_Cli_Objects_ContentToPack_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string_)'></a>
## PackageTypeHandler.GetContentToIgnore(ContentToPack, IDirectoryInfo, List&lt;string&gt;) Method
Gets the content to ignore.  
```csharp
private System.Collections.Generic.List<string> GetContentToIgnore(Cmf.Common.Cli.Objects.ContentToPack contentToPack, System.IO.Abstractions.IDirectoryInfo packDirectory, System.Collections.Generic.List<string> defaultContentToIgnore);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_GetContentToIgnore(Cmf_Common_Cli_Objects_ContentToPack_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string_)_contentToPack'></a>
`contentToPack` [ContentToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_ContentToPack 'Cmf.Common.Cli.Objects.ContentToPack')  
The content to pack.
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_GetContentToIgnore(Cmf_Common_Cli_Objects_ContentToPack_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string_)_packDirectory'></a>
`packDirectory` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The pack directory.
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_GetContentToIgnore(Cmf_Common_Cli_Objects_ContentToPack_System_IO_Abstractions_IDirectoryInfo_System_Collections_Generic_List_string_)_defaultContentToIgnore'></a>
`defaultContentToIgnore` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
The default content to ignore.
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_GetContentToPack(System_IO_Abstractions_IDirectoryInfo)'></a>
## PackageTypeHandler.GetContentToPack(IDirectoryInfo) Method
Get Content To pack  
```csharp
internal virtual System.Collections.Generic.List<Cmf.Common.Cli.Objects.FileToPack> GetContentToPack(System.IO.Abstractions.IDirectoryInfo packageOutputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_GetContentToPack(System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The pack directory.
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[FileToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_FileToPack 'Cmf.Common.Cli.Objects.FileToPack')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)'></a>
## PackageTypeHandler.Pack(IDirectoryInfo, IDirectoryInfo) Method
Packs the specified package output dir.  
```csharp
public virtual void Pack(System.IO.Abstractions.IDirectoryInfo packageOutputDir, System.IO.Abstractions.IDirectoryInfo outputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The output dir.
  

Implements [Pack(IDirectoryInfo, IDirectoryInfo)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Pack(System.IO.Abstractions.IDirectoryInfo, System.IO.Abstractions.IDirectoryInfo)')  
  
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_RestoreDependencies(System_Uri__)'></a>
## PackageTypeHandler.RestoreDependencies(Uri[]) Method
Restore the the current package's dependencies to the dependencies folder  
```csharp
public virtual void RestoreDependencies(System.Uri[] repoUris);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PackageTypeHandler_RestoreDependencies(System_Uri__)_repoUris'></a>
`repoUris` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
The Uris for the package repos
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
thrown when a repo uri is not available or in an incorrect format

Implements [RestoreDependencies(Uri[])](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_RestoreDependencies(System_Uri__) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.RestoreDependencies(System.Uri[])')  
  
  
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler'></a>
## PresentationPackageTypeHandler Class
```csharp
public class PresentationPackageTypeHandler : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; PresentationPackageTypeHandler  

Derived  
&#8627; [HelpPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_HelpPackageTypeHandler 'Cmf.Common.Cli.Handlers.HelpPackageTypeHandler')  
&#8627; [HtmlPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_HtmlPackageTypeHandler 'Cmf.Common.Cli.Handlers.HtmlPackageTypeHandler')  
&#8627; [IoTPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_IoTPackageTypeHandler 'Cmf.Common.Cli.Handlers.IoTPackageTypeHandler')  
### Constructors
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_PresentationPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## PresentationPackageTypeHandler.PresentationPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [PresentationPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler 'Cmf.Common.Cli.Handlers.PresentationPackageTypeHandler') class.  
```csharp
public PresentationPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_PresentationPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
### Methods
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)'></a>
## PresentationPackageTypeHandler.Bump(string, string, Dictionary&lt;string,object&gt;) Method
Bumps the specified version.  
```csharp
public override void Bump(string version, string buildNr, System.Collections.Generic.Dictionary<string,object> bumpInformation=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version for build Nr.
  
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_bumpInformation'></a>
`bumpInformation` [System.Collections.Generic.Dictionary&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[,](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')  
The bump information.
  

Implements [Bump(string, string, Dictionary<string,object>)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Bump(string, string, System.Collections.Generic.Dictionary&lt;string,object&gt;)')  
  
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_GeneratePresentationConfigFile(System_IO_Abstractions_IDirectoryInfo)'></a>
## PresentationPackageTypeHandler.GeneratePresentationConfigFile(IDirectoryInfo) Method
Generates the presentation configuration file.  
```csharp
private void GeneratePresentationConfigFile(System.IO.Abstractions.IDirectoryInfo packageOutputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_GeneratePresentationConfigFile(System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
  
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)'></a>
## PresentationPackageTypeHandler.Pack(IDirectoryInfo, IDirectoryInfo) Method
Packs the specified package output dir.  
```csharp
public override void Pack(System.IO.Abstractions.IDirectoryInfo packageOutputDir, System.IO.Abstractions.IDirectoryInfo outputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
<a name='Cmf_Common_Cli_Handlers_PresentationPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The output dir.
  

Implements [Pack(IDirectoryInfo, IDirectoryInfo)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Pack(System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IDirectoryInfo) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Pack(System.IO.Abstractions.IDirectoryInfo, System.IO.Abstractions.IDirectoryInfo)')  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_ReportingPackageTypeHandler'></a>
## ReportingPackageTypeHandler Class
```csharp
public class ReportingPackageTypeHandler : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; ReportingPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_ReportingPackageTypeHandler_ReportingPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## ReportingPackageTypeHandler.ReportingPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [ReportingPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_ReportingPackageTypeHandler 'Cmf.Common.Cli.Handlers.ReportingPackageTypeHandler') class.  
```csharp
public ReportingPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_ReportingPackageTypeHandler_ReportingPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_RootPackageTypeHandler'></a>
## RootPackageTypeHandler Class
```csharp
public class RootPackageTypeHandler : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; RootPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_RootPackageTypeHandler_RootPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## RootPackageTypeHandler.RootPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [RootPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_RootPackageTypeHandler 'Cmf.Common.Cli.Handlers.RootPackageTypeHandler') class.  
```csharp
public RootPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_RootPackageTypeHandler_RootPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  
<a name='Cmf_Common_Cli_Handlers_TestPackageTypeHandler'></a>
## TestPackageTypeHandler Class
```csharp
public class TestPackageTypeHandler : Cmf.Common.Cli.Handlers.PackageTypeHandler
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler') &#129106; TestPackageTypeHandler  
### Constructors
<a name='Cmf_Common_Cli_Handlers_TestPackageTypeHandler_TestPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## TestPackageTypeHandler.TestPackageTypeHandler(CmfPackage) Constructor
Initializes a new instance of the [TestPackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_TestPackageTypeHandler 'Cmf.Common.Cli.Handlers.TestPackageTypeHandler') class.  
```csharp
public TestPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_TestPackageTypeHandler_TestPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
  
### Methods
<a name='Cmf_Common_Cli_Handlers_TestPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)'></a>
## TestPackageTypeHandler.Bump(string, string, Dictionary&lt;string,object&gt;) Method
Bumps the specified CMF package.  
```csharp
public override void Bump(string version, string buildNr, System.Collections.Generic.Dictionary<string,object> bumpInformation=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_TestPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Handlers_TestPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version for build Nr.
  
<a name='Cmf_Common_Cli_Handlers_TestPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_bumpInformation'></a>
`bumpInformation` [System.Collections.Generic.Dictionary&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[,](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')  
The bump information.
  

Implements [Bump(string, string, Dictionary<string,object>)](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_) 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler.Bump(string, string, System.Collections.Generic.Dictionary&lt;string,object&gt;)')  
  
<a name='Cmf_Common_Cli_Handlers_TestPackageTypeHandler_GenerateDeploymentFrameworkManifest(System_IO_Abstractions_IDirectoryInfo)'></a>
## TestPackageTypeHandler.GenerateDeploymentFrameworkManifest(IDirectoryInfo) Method
Generates the deployment framework manifest.  
```csharp
internal override void GenerateDeploymentFrameworkManifest(System.IO.Abstractions.IDirectoryInfo packageOutputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Handlers_TestPackageTypeHandler_GenerateDeploymentFrameworkManifest(System_IO_Abstractions_IDirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
#### See Also
- [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')
  

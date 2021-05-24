#### [cmf](index.md 'index')
## Cmf.Common.Cli.Interfaces Namespace
### Interfaces
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler'></a>
## IPackageTypeHandler Interface
```csharp
public interface IPackageTypeHandler
```

Derived  
&#8627; [PackageTypeHandler](Cmf_Common_Cli_Handlers.md#Cmf_Common_Cli_Handlers_PackageTypeHandler 'Cmf.Common.Cli.Handlers.PackageTypeHandler')  
### Properties
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler_DefaultContentToIgnore'></a>
## IPackageTypeHandler.DefaultContentToIgnore Property
Gets or sets the default content to ignore.  
```csharp
System.Collections.Generic.List<string> DefaultContentToIgnore { get; }
```
#### Property Value
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')
The default content to ignore.  
  
### Methods
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Build()'></a>
## IPackageTypeHandler.Build() Method
Builds this instance.  
```csharp
void Build();
```
  
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)'></a>
## IPackageTypeHandler.Bump(string, string, Dictionary&lt;string,object&gt;) Method
Bumps the specified version.  
```csharp
void Bump(string version, string buildNr, System.Collections.Generic.Dictionary<string,object> bumpInformation=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version for build Nr.
  
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Bump(string_string_System_Collections_Generic_Dictionary_string_object_)_bumpInformation'></a>
`bumpInformation` [System.Collections.Generic.Dictionary&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[,](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.Dictionary-2 'System.Collections.Generic.Dictionary`2')  
The bump information.
  
  
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Pack(System_IO_DirectoryInfo_System_IO_DirectoryInfo)'></a>
## IPackageTypeHandler.Pack(DirectoryInfo, DirectoryInfo) Method
Packs the specified package output dir.  
```csharp
void Pack(System.IO.DirectoryInfo packageOutputDir, System.IO.DirectoryInfo outputDir);
```
#### Parameters
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Pack(System_IO_DirectoryInfo_System_IO_DirectoryInfo)_packageOutputDir'></a>
`packageOutputDir` [System.IO.DirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.DirectoryInfo 'System.IO.DirectoryInfo')  
The package output dir.
  
<a name='Cmf_Common_Cli_Interfaces_IPackageTypeHandler_Pack(System_IO_DirectoryInfo_System_IO_DirectoryInfo)_outputDir'></a>
`outputDir` [System.IO.DirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.DirectoryInfo 'System.IO.DirectoryInfo')  
The output dir.
  
  
  

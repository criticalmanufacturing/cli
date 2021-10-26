#### [cmf](index.md 'index')
## Cmf.Common.Cli.Objects Namespace
### Classes
<a name='Cmf_Common_Cli_Objects_CmfPackage'></a>
## CmfPackage Class
```csharp
public class CmfPackage :
System.IEquatable<Cmf.Common.Cli.Objects.CmfPackage>
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; CmfPackage  

Implements [System.IEquatable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable-1 'System.IEquatable`1')[CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable-1 'System.IEquatable`1')  
### Constructors
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage()'></a>
## CmfPackage.CmfPackage() Constructor
initialize an empty CmfPackage  
```csharp
public CmfPackage();
```
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)'></a>
## CmfPackage.CmfPackage(string, string, string, string, PackageType, string, string, Nullable&lt;bool&gt;, Nullable&lt;bool&gt;, string, Nullable&lt;bool&gt;, DependencyCollection, List&lt;Step&gt;, List&lt;ContentToPack&gt;, List&lt;string&gt;, DependencyCollection) Constructor
Initializes a new instance of the [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage') class.  
```csharp
public CmfPackage(string name, string packageId, string version, string description, Cmf.Common.Cli.Enums.PackageType packageType, string targetDirectory, string targetLayer, System.Nullable<bool> isInstallable, System.Nullable<bool> isUniqueInstall, string keywords, System.Nullable<bool> isToSetDefaultSteps, Cmf.Common.Cli.Objects.DependencyCollection dependencies, System.Collections.Generic.List<Cmf.Common.Cli.Objects.Step> steps, System.Collections.Generic.List<Cmf.Common.Cli.Objects.ContentToPack> contentToPack, System.Collections.Generic.List<string> xmlInjection, Cmf.Common.Cli.Objects.DependencyCollection testPackages=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_packageId'></a>
`packageId` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The package identifier.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_description'></a>
`description` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The description.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_packageType'></a>
`packageType` [PackageType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackageType 'Cmf.Common.Cli.Enums.PackageType')  
Type of the package.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_targetDirectory'></a>
`targetDirectory` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The target directory.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_targetLayer'></a>
`targetLayer` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The target layer.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_isInstallable'></a>
`isInstallable` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
The is installable.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_isUniqueInstall'></a>
`isUniqueInstall` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
The is unique install.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_keywords'></a>
`keywords` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The keywords.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_isToSetDefaultSteps'></a>
`isToSetDefaultSteps` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
The is to set default steps.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_dependencies'></a>
`dependencies` [DependencyCollection](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_DependencyCollection 'Cmf.Common.Cli.Objects.DependencyCollection')  
The dependencies.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_steps'></a>
`steps` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[Step](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step 'Cmf.Common.Cli.Objects.Step')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
The steps.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_contentToPack'></a>
`contentToPack` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[ContentToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_ContentToPack 'Cmf.Common.Cli.Objects.ContentToPack')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
The content to pack.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_xmlInjection'></a>
`xmlInjection` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
The XML injection.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(string_string_string_string_Cmf_Common_Cli_Enums_PackageType_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Nullable_bool__Cmf_Common_Cli_Objects_DependencyCollection_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step__System_Collections_Generic_List_Cmf_Common_Cli_Objects_ContentToPack__System_Collections_Generic_List_string__Cmf_Common_Cli_Objects_DependencyCollection)_testPackages'></a>
`testPackages` [DependencyCollection](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_DependencyCollection 'Cmf.Common.Cli.Objects.DependencyCollection')  
The test Packages.
  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(System_IO_Abstractions_IFileSystem)'></a>
## CmfPackage.CmfPackage(IFileSystem) Constructor
Initialize an empty CmfPackage with a specific file system  
```csharp
public CmfPackage(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_CmfPackage(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
  
### Fields
<a name='Cmf_Common_Cli_Objects_CmfPackage_FileInfo'></a>
## CmfPackage.FileInfo Field
The file information  
```csharp
private IFileInfo FileInfo;
```
#### Field Value
[System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_IsToSetDefaultValues'></a>
## CmfPackage.IsToSetDefaultValues Field
The skip set default values  
```csharp
private bool IsToSetDefaultValues;
```
#### Field Value
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
  
### Properties
<a name='Cmf_Common_Cli_Objects_CmfPackage_ContentToPack'></a>
## CmfPackage.ContentToPack Property
Gets or sets the content to pack.  
```csharp
public System.Collections.Generic.List<Cmf.Common.Cli.Objects.ContentToPack> ContentToPack { get; set; }
```
#### Property Value
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[ContentToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_ContentToPack 'Cmf.Common.Cli.Objects.ContentToPack')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')
The content to pack.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Dependencies'></a>
## CmfPackage.Dependencies Property
Gets or sets the dependencies.  
```csharp
public Cmf.Common.Cli.Objects.DependencyCollection Dependencies { get; set; }
```
#### Property Value
[DependencyCollection](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_DependencyCollection 'Cmf.Common.Cli.Objects.DependencyCollection')
The dependencies.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Description'></a>
## CmfPackage.Description Property
Gets or sets the description.  
```csharp
public string Description { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The description.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_IsInstallable'></a>
## CmfPackage.IsInstallable Property
Gets or sets a value indicating whether this instance is installable.  
```csharp
public System.Nullable<bool> IsInstallable { get; set; }
```
#### Property Value
[System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')
`true` if this instance is installable; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_IsToSetDefaultSteps'></a>
## CmfPackage.IsToSetDefaultSteps Property
Gets or sets the set default steps.  
```csharp
public System.Nullable<bool> IsToSetDefaultSteps { get; set; }
```
#### Property Value
[System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')
The set default steps.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_IsUniqueInstall'></a>
## CmfPackage.IsUniqueInstall Property
Gets or sets the is unique install.  
```csharp
public System.Nullable<bool> IsUniqueInstall { get; set; }
```
#### Property Value
[System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')
The is unique install.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Keywords'></a>
## CmfPackage.Keywords Property
Gets or sets the is root package.  
```csharp
public string Keywords { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The is root package.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Location'></a>
## CmfPackage.Location Property
The location of the package  
```csharp
public Cmf.Common.Cli.Enums.PackageLocation Location { get; set; }
```
#### Property Value
[PackageLocation](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackageLocation 'Cmf.Common.Cli.Enums.PackageLocation')
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Name'></a>
## CmfPackage.Name Property
Gets the name.  
```csharp
public string Name { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The name.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_PackageId'></a>
## CmfPackage.PackageId Property
Gets or sets the package identifier.  
```csharp
public string PackageId { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The package identifier.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_PackageName'></a>
## CmfPackage.PackageName Property
Gets the name of the package.  
```csharp
internal string PackageName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The name of the package.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_PackageType'></a>
## CmfPackage.PackageType Property
Gets or sets the type of the package.  
```csharp
public Cmf.Common.Cli.Enums.PackageType PackageType { get; set; }
```
#### Property Value
[PackageType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackageType 'Cmf.Common.Cli.Enums.PackageType')
The type of the package.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Steps'></a>
## CmfPackage.Steps Property
Gets or sets the steps.  
```csharp
public System.Collections.Generic.List<Cmf.Common.Cli.Objects.Step> Steps { get; set; }
```
#### Property Value
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[Step](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step 'Cmf.Common.Cli.Objects.Step')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')
The steps.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_TargetDirectory'></a>
## CmfPackage.TargetDirectory Property
Gets or sets the target directory where the package contents should be installed.  
This is used when the package is installed using Deployment Framework and ignored when it is installed using Environment Manager.  
```csharp
public string TargetDirectory { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The target directory.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_TargetLayer'></a>
## CmfPackage.TargetLayer Property
Gets or sets the target layer, which means the container in which the packages contents should be installed.  
This is used when the package is installed using Environment Manager and ignored when it is installed using Deployment Framework.  
```csharp
public string TargetLayer { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The target layer.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_TestPackages'></a>
## CmfPackage.TestPackages Property
Gets or sets the Test Package Id.  
```csharp
public Cmf.Common.Cli.Objects.DependencyCollection TestPackages { get; set; }
```
#### Property Value
[DependencyCollection](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_DependencyCollection 'Cmf.Common.Cli.Objects.DependencyCollection')
The Test Package Id.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Uri'></a>
## CmfPackage.Uri Property
The Uri of the package  
```csharp
public System.Uri Uri { get; set; }
```
#### Property Value
[System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Version'></a>
## CmfPackage.Version Property
Gets or sets the version.  
```csharp
public string Version { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The version.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_XmlInjection'></a>
## CmfPackage.XmlInjection Property
Gets or sets the deployment framework UI file.  
```csharp
public System.Collections.Generic.List<string> XmlInjection { get; set; }
```
#### Property Value
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')
The deployment framework UI file.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_ZipPackageName'></a>
## CmfPackage.ZipPackageName Property
Gets the name of the zip package.  
```csharp
internal string ZipPackageName { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The name of the zip package.  
  
### Methods
<a name='Cmf_Common_Cli_Objects_CmfPackage_Equals(Cmf_Common_Cli_Objects_CmfPackage)'></a>
## CmfPackage.Equals(CmfPackage) Method
Indicates whether the current object is equal to another object of the same type.  
```csharp
public bool Equals(Cmf.Common.Cli.Objects.CmfPackage other);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_Equals(Cmf_Common_Cli_Objects_CmfPackage)_other'></a>
`other` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
An object to compare with this object.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
[true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool') if the current object is equal to the [other](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage_Equals(Cmf_Common_Cli_Objects_CmfPackage)_other 'Cmf.Common.Cli.Objects.CmfPackage.Equals(Cmf.Common.Cli.Objects.CmfPackage).other') parameter; otherwise, [false](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool').  
            
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Equals(object)'></a>
## CmfPackage.Equals(object) Method
Equalses the specified object.  
```csharp
public override bool Equals(object obj);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_Equals(object)_obj'></a>
`obj` [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')  
The object.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_FromManifest(string_bool_System_IO_Abstractions_IFileSystem)'></a>
## CmfPackage.FromManifest(string, bool, IFileSystem) Method
Create a CmfPackage object from a DF package manifest  
```csharp
public static Cmf.Common.Cli.Objects.CmfPackage FromManifest(string manifest, bool setDefaultValues=false, System.IO.Abstractions.IFileSystem fileSystem=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_FromManifest(string_bool_System_IO_Abstractions_IFileSystem)_manifest'></a>
`manifest` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
the manifest content
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_FromManifest(string_bool_System_IO_Abstractions_IFileSystem)_setDefaultValues'></a>
`setDefaultValues` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
should set default values
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_FromManifest(string_bool_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
#### Returns
[CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
a CmfPackage
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_GetFileInfo()'></a>
## CmfPackage.GetFileInfo() Method
Gets or sets the file information.  
```csharp
public System.IO.Abstractions.IFileInfo GetFileInfo();
```
#### Returns
[System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
The file information.  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_GetHashCode()'></a>
## CmfPackage.GetHashCode() Method
Gets the hash code.  
```csharp
public override int GetHashCode();
```
#### Returns
[System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')  
#### Exceptions
[System.NotImplementedException](https://docs.microsoft.com/en-us/dotnet/api/System.NotImplementedException 'System.NotImplementedException')  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_IsRootPackage()'></a>
## CmfPackage.IsRootPackage() Method
Determines whether [is root package].  
```csharp
public bool IsRootPackage();
```
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if [is root package] [the specified CMF package]; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Load(System_IO_Abstractions_IFileInfo_bool_System_IO_Abstractions_IFileSystem)'></a>
## CmfPackage.Load(IFileInfo, bool, IFileSystem) Method
Loads the specified file.  
```csharp
public static Cmf.Common.Cli.Objects.CmfPackage Load(System.IO.Abstractions.IFileInfo file, bool setDefaultValues=false, System.IO.Abstractions.IFileSystem fileSystem=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_Load(System_IO_Abstractions_IFileInfo_bool_System_IO_Abstractions_IFileSystem)_file'></a>
`file` [System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
The file.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Load(System_IO_Abstractions_IFileInfo_bool_System_IO_Abstractions_IFileSystem)_setDefaultValues'></a>
`setDefaultValues` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_Load(System_IO_Abstractions_IFileInfo_bool_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
#### Returns
[CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_LoadDependencies(System_Uri___bool)'></a>
## CmfPackage.LoadDependencies(Uri[], bool) Method
Builds a dependency tree by attaching the CmfPackage objects to the parent's dependencies  
Can run recursively and fetch packages from a DF repository.  
Supports cycles  
```csharp
public Cmf.Common.Cli.Objects.CmfPackage LoadDependencies(System.Uri[] repoUris, bool recurse=false);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_LoadDependencies(System_Uri___bool)_repoUris'></a>
`repoUris` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
the address of the package repositories (currently only folders are supported)
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_LoadDependencies(System_Uri___bool)_recurse'></a>
`recurse` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
should we run recursively
  
#### Returns
[CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
this CmfPackage for chaining, but the method itself is mutable
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SaveCmfPackage()'></a>
## CmfPackage.SaveCmfPackage() Method
Saves the CMF package.  
```csharp
public void SaveCmfPackage();
```
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetDefaultValues(string_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step_)'></a>
## CmfPackage.SetDefaultValues(string, string, string, Nullable&lt;bool&gt;, Nullable&lt;bool&gt;, string, List&lt;Step&gt;) Method
Sets the defaults.  
```csharp
public void SetDefaultValues(string name=null, string targetDirectory=null, string targetLayer=null, System.Nullable<bool> isInstallable=null, System.Nullable<bool> isUniqueInstall=null, string keywords=null, System.Collections.Generic.List<Cmf.Common.Cli.Objects.Step> steps=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetDefaultValues(string_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step_)_name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetDefaultValues(string_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step_)_targetDirectory'></a>
`targetDirectory` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The target directory.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetDefaultValues(string_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step_)_targetLayer'></a>
`targetLayer` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The target layer container.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetDefaultValues(string_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step_)_isInstallable'></a>
`isInstallable` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
The is installable.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetDefaultValues(string_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step_)_isUniqueInstall'></a>
`isUniqueInstall` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
The is unique install.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetDefaultValues(string_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step_)_keywords'></a>
`keywords` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The keywords.
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetDefaultValues(string_string_string_System_Nullable_bool__System_Nullable_bool__string_System_Collections_Generic_List_Cmf_Common_Cli_Objects_Step_)_steps'></a>
`steps` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[Step](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step 'Cmf.Common.Cli.Objects.Step')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
The steps.
  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetFileInfo(System_IO_Abstractions_IFileInfo)'></a>
## CmfPackage.SetFileInfo(IFileInfo) Method
Gets or sets the file information.  
```csharp
public void SetFileInfo(System.IO.Abstractions.IFileInfo value);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetFileInfo(System_IO_Abstractions_IFileInfo)_value'></a>
`value` [System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
The file information.
  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetVersion(string)'></a>
## CmfPackage.SetVersion(string) Method
Sets the version.  
```csharp
public void SetVersion(string version);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackage_SetVersion(string)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
#### Exceptions
[System.NotImplementedException](https://docs.microsoft.com/en-us/dotnet/api/System.NotImplementedException 'System.NotImplementedException')  
  
<a name='Cmf_Common_Cli_Objects_CmfPackage_ValidatePackage()'></a>
## CmfPackage.ValidatePackage() Method
Validates the package.  
```csharp
private void ValidatePackage();
```
  
#### See Also
- [System.IEquatable<Cmf.Common.Cli.Objects.CmfPackage>](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable<Cmf.Common.Cli.Objects.CmfPackage> 'System.IEquatable<Cmf.Common.Cli.Objects.CmfPackage>')
  
<a name='Cmf_Common_Cli_Objects_CmfPackageCollection'></a>
## CmfPackageCollection Class
```csharp
public class CmfPackageCollection : System.Collections.Generic.List<Cmf.Common.Cli.Objects.CmfPackage>
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1') &#129106; CmfPackageCollection  
### Methods
<a name='Cmf_Common_Cli_Objects_CmfPackageCollection_GetDependency(Cmf_Common_Cli_Objects_Dependency)'></a>
## CmfPackageCollection.GetDependency(Dependency) Method
Gets the dependency.  
```csharp
public Cmf.Common.Cli.Objects.CmfPackage GetDependency(Cmf.Common.Cli.Objects.Dependency dependency);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_CmfPackageCollection_GetDependency(Cmf_Common_Cli_Objects_Dependency)_dependency'></a>
`dependency` [Dependency](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency 'Cmf.Common.Cli.Objects.Dependency')  
The dependency.
  
#### Returns
[CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
  
#### See Also
- [System.Collections.Generic.List<Objects.CmfPackage>](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List<Objects.CmfPackage> 'System.Collections.Generic.List<Objects.CmfPackage>')
  
<a name='Cmf_Common_Cli_Objects_ContentToPack'></a>
## ContentToPack Class
```csharp
public class ContentToPack :
System.IEquatable<Cmf.Common.Cli.Objects.ContentToPack>
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; ContentToPack  

Implements [System.IEquatable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable-1 'System.IEquatable`1')[ContentToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_ContentToPack 'Cmf.Common.Cli.Objects.ContentToPack')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable-1 'System.IEquatable`1')  
### Properties
<a name='Cmf_Common_Cli_Objects_ContentToPack_Action'></a>
## ContentToPack.Action Property
Gets or sets the action to be applied to the content  
Default is "pack"  
```csharp
public System.Nullable<Cmf.Common.Cli.Enums.PackAction> Action { get; set; }
```
#### Property Value
[System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[PackAction](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackAction 'Cmf.Common.Cli.Enums.PackAction')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')
  
<a name='Cmf_Common_Cli_Objects_ContentToPack_ContentType'></a>
## ContentToPack.ContentType Property
Gets or sets the type of the content.  
Default value = Generic  
```csharp
public System.Nullable<Cmf.Common.Cli.Enums.ContentType> ContentType { get; set; }
```
#### Property Value
[System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[ContentType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_ContentType 'Cmf.Common.Cli.Enums.ContentType')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')
The type of the content.  
  
<a name='Cmf_Common_Cli_Objects_ContentToPack_IgnoreFiles'></a>
## ContentToPack.IgnoreFiles Property
Gets or sets the ignore file.  
```csharp
public System.Collections.Generic.List<string> IgnoreFiles { get; set; }
```
#### Property Value
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')
The ignore file.  
  
<a name='Cmf_Common_Cli_Objects_ContentToPack_Source'></a>
## ContentToPack.Source Property
Gets or sets the source.  
```csharp
public string Source { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The source.  
  
<a name='Cmf_Common_Cli_Objects_ContentToPack_Target'></a>
## ContentToPack.Target Property
Gets or sets the target.  
```csharp
public string Target { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The target.  
  
### Methods
<a name='Cmf_Common_Cli_Objects_ContentToPack_Equals(Cmf_Common_Cli_Objects_ContentToPack)'></a>
## ContentToPack.Equals(ContentToPack) Method
Indicates whether the current object is equal to another object of the same type.  
```csharp
public bool Equals(Cmf.Common.Cli.Objects.ContentToPack other);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_ContentToPack_Equals(Cmf_Common_Cli_Objects_ContentToPack)_other'></a>
`other` [ContentToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_ContentToPack 'Cmf.Common.Cli.Objects.ContentToPack')  
An object to compare with this object.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
[true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool') if the current object is equal to the [other](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_ContentToPack_Equals(Cmf_Common_Cli_Objects_ContentToPack)_other 'Cmf.Common.Cli.Objects.ContentToPack.Equals(Cmf.Common.Cli.Objects.ContentToPack).other') parameter; otherwise, [false](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool').  
            
  
#### See Also
- [System.IEquatable<Cmf.Common.Cli.Objects.ContentToPack>](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable<Cmf.Common.Cli.Objects.ContentToPack> 'System.IEquatable<Cmf.Common.Cli.Objects.ContentToPack>')
  
<a name='Cmf_Common_Cli_Objects_Dependency'></a>
## Dependency Class
```csharp
public class Dependency :
System.IEquatable<Cmf.Common.Cli.Objects.Dependency>
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; Dependency  

Implements [System.IEquatable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable-1 'System.IEquatable`1')[Dependency](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency 'Cmf.Common.Cli.Objects.Dependency')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable-1 'System.IEquatable`1')  
### Constructors
<a name='Cmf_Common_Cli_Objects_Dependency_Dependency()'></a>
## Dependency.Dependency() Constructor
Initializes a new instance of the [Dependency](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency 'Cmf.Common.Cli.Objects.Dependency') class.  
```csharp
public Dependency();
```
  
<a name='Cmf_Common_Cli_Objects_Dependency_Dependency(string_string)'></a>
## Dependency.Dependency(string, string) Constructor
Initializes a new instance of the [Dependency](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency 'Cmf.Common.Cli.Objects.Dependency') class.  
```csharp
public Dependency(string id, string version);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_Dependency_Dependency(string_string)_id'></a>
`id` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The identifier.
  
<a name='Cmf_Common_Cli_Objects_Dependency_Dependency(string_string)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
#### Exceptions
[System.ArgumentNullException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentNullException 'System.ArgumentNullException')  
id  
            or  
            version
  
### Properties
<a name='Cmf_Common_Cli_Objects_Dependency_CmfPackage'></a>
## Dependency.CmfPackage Property
The CmfPackage that satisfies this dependency  
```csharp
public Cmf.Common.Cli.Objects.CmfPackage CmfPackage { get; set; }
```
#### Property Value
[CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')
  
<a name='Cmf_Common_Cli_Objects_Dependency_Id'></a>
## Dependency.Id Property
Gets or sets the identifier.  
```csharp
public string Id { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The identifier.  
  
<a name='Cmf_Common_Cli_Objects_Dependency_IsMissing'></a>
## Dependency.IsMissing Property
Is this package missing, i.e. we could not find it anywhere to satisfy this dependency  
```csharp
public bool IsMissing { get; }
```
#### Property Value
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
  
<a name='Cmf_Common_Cli_Objects_Dependency_Mandatory'></a>
## Dependency.Mandatory Property
Gets or sets a value indicating whether this [Dependency](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency 'Cmf.Common.Cli.Objects.Dependency') is mandatory.  
```csharp
public bool Mandatory { get; set; }
```
#### Property Value
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
`true` if mandatory; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Objects_Dependency_Version'></a>
## Dependency.Version Property
Gets or sets the version.  
```csharp
public string Version { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The version.  
  
### Methods
<a name='Cmf_Common_Cli_Objects_Dependency_Equals(Cmf_Common_Cli_Objects_Dependency)'></a>
## Dependency.Equals(Dependency) Method
Indicates whether the current object is equal to another object of the same type.  
```csharp
public bool Equals(Cmf.Common.Cli.Objects.Dependency other);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_Dependency_Equals(Cmf_Common_Cli_Objects_Dependency)_other'></a>
`other` [Dependency](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency 'Cmf.Common.Cli.Objects.Dependency')  
An object to compare with this object.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
[true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool') if the current object is equal to the [other](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency_Equals(Cmf_Common_Cli_Objects_Dependency)_other 'Cmf.Common.Cli.Objects.Dependency.Equals(Cmf.Common.Cli.Objects.Dependency).other') parameter; otherwise, [false](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool').  
            
  
#### See Also
- [System.IEquatable&lt;&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable-1 'System.IEquatable`1')
  
<a name='Cmf_Common_Cli_Objects_DependencyCollection'></a>
## DependencyCollection Class
```csharp
public class DependencyCollection : System.Collections.Generic.List<Cmf.Common.Cli.Objects.Dependency>
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[Dependency](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency 'Cmf.Common.Cli.Objects.Dependency')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1') &#129106; DependencyCollection  
### Methods
<a name='Cmf_Common_Cli_Objects_DependencyCollection_Contains(Cmf_Common_Cli_Objects_Dependency_bool)'></a>
## DependencyCollection.Contains(Dependency, bool) Method
Determines whether this instance contains the object.  
```csharp
public bool Contains(Cmf.Common.Cli.Objects.Dependency dependency, bool ignoreVersion);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_DependencyCollection_Contains(Cmf_Common_Cli_Objects_Dependency_bool)_dependency'></a>
`dependency` [Dependency](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency 'Cmf.Common.Cli.Objects.Dependency')  
The dependency.
  
<a name='Cmf_Common_Cli_Objects_DependencyCollection_Contains(Cmf_Common_Cli_Objects_Dependency_bool)_ignoreVersion'></a>
`ignoreVersion` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [ignore version].
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if [contains] [the specified dependency]; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Objects_DependencyCollection_Contains(Cmf_Common_Cli_Objects_Dependency)'></a>
## DependencyCollection.Contains(Dependency) Method
Gets the dependency.  
```csharp
public bool Contains(Cmf.Common.Cli.Objects.Dependency dependency);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_DependencyCollection_Contains(Cmf_Common_Cli_Objects_Dependency)_dependency'></a>
`dependency` [Dependency](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Dependency 'Cmf.Common.Cli.Objects.Dependency')  
The dependency.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if [contains] [the specified dependency]; otherwise, `false`.  
            
  
#### See Also
- [System.Collections.Generic.List<Cmf.Common.Cli.Objects.Dependency>](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List<Cmf.Common.Cli.Objects.Dependency> 'System.Collections.Generic.List<Cmf.Common.Cli.Objects.Dependency>')
  
<a name='Cmf_Common_Cli_Objects_FileToPack'></a>
## FileToPack Class
```csharp
public class FileToPack
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; FileToPack  
### Constructors
<a name='Cmf_Common_Cli_Objects_FileToPack_FileToPack()'></a>
## FileToPack.FileToPack() Constructor
Initializes a new instance of the [FileToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_FileToPack 'Cmf.Common.Cli.Objects.FileToPack') class.  
```csharp
public FileToPack();
```
  
<a name='Cmf_Common_Cli_Objects_FileToPack_FileToPack(System_IO_Abstractions_IFileInfo_System_IO_Abstractions_IFileInfo_Cmf_Common_Cli_Objects_ContentToPack)'></a>
## FileToPack.FileToPack(IFileInfo, IFileInfo, ContentToPack) Constructor
Initializes a new instance of the [FileToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_FileToPack 'Cmf.Common.Cli.Objects.FileToPack') class.  
```csharp
public FileToPack(System.IO.Abstractions.IFileInfo source, System.IO.Abstractions.IFileInfo target, Cmf.Common.Cli.Objects.ContentToPack contentToPack);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_FileToPack_FileToPack(System_IO_Abstractions_IFileInfo_System_IO_Abstractions_IFileInfo_Cmf_Common_Cli_Objects_ContentToPack)_source'></a>
`source` [System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
The source.
  
<a name='Cmf_Common_Cli_Objects_FileToPack_FileToPack(System_IO_Abstractions_IFileInfo_System_IO_Abstractions_IFileInfo_Cmf_Common_Cli_Objects_ContentToPack)_target'></a>
`target` [System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
The target.
  
<a name='Cmf_Common_Cli_Objects_FileToPack_FileToPack(System_IO_Abstractions_IFileInfo_System_IO_Abstractions_IFileInfo_Cmf_Common_Cli_Objects_ContentToPack)_contentToPack'></a>
`contentToPack` [ContentToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_ContentToPack 'Cmf.Common.Cli.Objects.ContentToPack')  
The content to pack.
  
  
### Properties
<a name='Cmf_Common_Cli_Objects_FileToPack_ContentToPack'></a>
## FileToPack.ContentToPack Property
Gets or sets the content to pack.  
```csharp
public Cmf.Common.Cli.Objects.ContentToPack ContentToPack { get; set; }
```
#### Property Value
[ContentToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_ContentToPack 'Cmf.Common.Cli.Objects.ContentToPack')
The content to pack.  
  
<a name='Cmf_Common_Cli_Objects_FileToPack_Source'></a>
## FileToPack.Source Property
Gets or sets the source.  
```csharp
public System.IO.Abstractions.IFileInfo Source { get; set; }
```
#### Property Value
[System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')
The source.  
  
<a name='Cmf_Common_Cli_Objects_FileToPack_Target'></a>
## FileToPack.Target Property
Gets or sets the target.  
```csharp
public System.IO.Abstractions.IFileInfo Target { get; set; }
```
#### Property Value
[System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')
The target.  
  
  
<a name='Cmf_Common_Cli_Objects_Step'></a>
## Step Class
```csharp
public class Step :
System.IEquatable<Cmf.Common.Cli.Objects.Step>
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; Step  

Implements [System.IEquatable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable-1 'System.IEquatable`1')[Step](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step 'Cmf.Common.Cli.Objects.Step')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable-1 'System.IEquatable`1')  
### Constructors
<a name='Cmf_Common_Cli_Objects_Step_Step()'></a>
## Step.Step() Constructor
Initializes a new instance of the [Step](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step 'Cmf.Common.Cli.Objects.Step') class.  
```csharp
public Step();
```
  
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType__string_string_string_string_System_Nullable_bool__string_System_Nullable_Cmf_Common_Cli_Enums_MessageType_)'></a>
## Step.Step(Nullable&lt;StepType&gt;, string, string, string, string, Nullable&lt;bool&gt;, string, Nullable&lt;MessageType&gt;) Constructor
Initializes a new instance of the [Step](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step 'Cmf.Common.Cli.Objects.Step') class.  
```csharp
public Step(System.Nullable<Cmf.Common.Cli.Enums.StepType> type, string title, string onExecute, string contentPath, string file, System.Nullable<bool> tagFile, string targetDatabase, System.Nullable<Cmf.Common.Cli.Enums.MessageType> messageType);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType__string_string_string_string_System_Nullable_bool__string_System_Nullable_Cmf_Common_Cli_Enums_MessageType_)_type'></a>
`type` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[StepType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_StepType 'Cmf.Common.Cli.Enums.StepType')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
The type.
  
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType__string_string_string_string_System_Nullable_bool__string_System_Nullable_Cmf_Common_Cli_Enums_MessageType_)_title'></a>
`title` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The title.
  
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType__string_string_string_string_System_Nullable_bool__string_System_Nullable_Cmf_Common_Cli_Enums_MessageType_)_onExecute'></a>
`onExecute` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The on execute.
  
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType__string_string_string_string_System_Nullable_bool__string_System_Nullable_Cmf_Common_Cli_Enums_MessageType_)_contentPath'></a>
`contentPath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The content path.
  
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType__string_string_string_string_System_Nullable_bool__string_System_Nullable_Cmf_Common_Cli_Enums_MessageType_)_file'></a>
`file` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The file.
  
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType__string_string_string_string_System_Nullable_bool__string_System_Nullable_Cmf_Common_Cli_Enums_MessageType_)_tagFile'></a>
`tagFile` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
The tag file.
  
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType__string_string_string_string_System_Nullable_bool__string_System_Nullable_Cmf_Common_Cli_Enums_MessageType_)_targetDatabase'></a>
`targetDatabase` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The target database.
  
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType__string_string_string_string_System_Nullable_bool__string_System_Nullable_Cmf_Common_Cli_Enums_MessageType_)_messageType'></a>
`messageType` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[MessageType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_MessageType 'Cmf.Common.Cli.Enums.MessageType')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
Type of the message.
  
#### Exceptions
[System.ArgumentNullException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentNullException 'System.ArgumentNullException')  
type
  
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType_)'></a>
## Step.Step(Nullable&lt;StepType&gt;) Constructor
Initializes a new instance of the [Step](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step 'Cmf.Common.Cli.Objects.Step') class.  
```csharp
public Step(System.Nullable<Cmf.Common.Cli.Enums.StepType> type);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_Step_Step(System_Nullable_Cmf_Common_Cli_Enums_StepType_)_type'></a>
`type` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[StepType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_StepType 'Cmf.Common.Cli.Enums.StepType')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
The type.
  
#### Exceptions
[System.ArgumentNullException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentNullException 'System.ArgumentNullException')  
type
  
### Properties
<a name='Cmf_Common_Cli_Objects_Step_ContentPath'></a>
## Step.ContentPath Property
Gets or sets the content path.  
```csharp
public string ContentPath { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The content path.  
  
<a name='Cmf_Common_Cli_Objects_Step_File'></a>
## Step.File Property
Gets or sets the file.  
```csharp
public string File { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The file.  
  
<a name='Cmf_Common_Cli_Objects_Step_MessageType'></a>
## Step.MessageType Property
Gets or sets the type of the message.  
```csharp
public System.Nullable<Cmf.Common.Cli.Enums.MessageType> MessageType { get; set; }
```
#### Property Value
[System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[MessageType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_MessageType 'Cmf.Common.Cli.Enums.MessageType')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')
The type of the message.  
  
<a name='Cmf_Common_Cli_Objects_Step_OnExecute'></a>
## Step.OnExecute Property
Gets or sets the on execute.  
```csharp
public string OnExecute { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The on execute.  
  
<a name='Cmf_Common_Cli_Objects_Step_TagFile'></a>
## Step.TagFile Property
Gets or sets a value indicating whether [tag file].  
```csharp
public System.Nullable<bool> TagFile { get; set; }
```
#### Property Value
[System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')
`true` if [tag file]; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Objects_Step_TargetDatabase'></a>
## Step.TargetDatabase Property
Gets or sets the target database.  
```csharp
public string TargetDatabase { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The target database.  
  
<a name='Cmf_Common_Cli_Objects_Step_Title'></a>
## Step.Title Property
Gets or sets the title.  
```csharp
public string Title { get; set; }
```
#### Property Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The title.  
  
<a name='Cmf_Common_Cli_Objects_Step_Type'></a>
## Step.Type Property
Gets or sets the type.  
```csharp
public System.Nullable<Cmf.Common.Cli.Enums.StepType> Type { get; set; }
```
#### Property Value
[System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[StepType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_StepType 'Cmf.Common.Cli.Enums.StepType')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')
The type.  
  
### Methods
<a name='Cmf_Common_Cli_Objects_Step_Equals(Cmf_Common_Cli_Objects_Step)'></a>
## Step.Equals(Step) Method
Indicates whether the current object is equal to another object of the same type.  
```csharp
public bool Equals(Cmf.Common.Cli.Objects.Step other);
```
#### Parameters
<a name='Cmf_Common_Cli_Objects_Step_Equals(Cmf_Common_Cli_Objects_Step)_other'></a>
`other` [Step](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step 'Cmf.Common.Cli.Objects.Step')  
An object to compare with this object.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
[true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool') if the current object is equal to the [other](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_Step_Equals(Cmf_Common_Cli_Objects_Step)_other 'Cmf.Common.Cli.Objects.Step.Equals(Cmf.Common.Cli.Objects.Step).other') parameter; otherwise, [false](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool').  
            
  
#### See Also
- [System.IEquatable<Cmf.Common.Cli.Objects.Step>](https://docs.microsoft.com/en-us/dotnet/api/System.IEquatable<Cmf.Common.Cli.Objects.Step> 'System.IEquatable<Cmf.Common.Cli.Objects.Step>')
  

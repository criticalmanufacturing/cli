#### [cmf](index.md 'index')
## Cmf.Common.Cli.Factories Namespace
### Classes
<a name='Cmf_Common_Cli_Factories_PackageTypeFactory'></a>
## PackageTypeFactory Class
```csharp
public static class PackageTypeFactory
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; PackageTypeFactory  
### Methods
<a name='Cmf_Common_Cli_Factories_PackageTypeFactory_GetPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage_bool)'></a>
## PackageTypeFactory.GetPackageTypeHandler(CmfPackage, bool) Method
Gets the package type handler.  
```csharp
public static Cmf.Common.Cli.Interfaces.IPackageTypeHandler GetPackageTypeHandler(Cmf.Common.Cli.Objects.CmfPackage cmfPackage, bool setDefaultValues=false);
```
#### Parameters
<a name='Cmf_Common_Cli_Factories_PackageTypeFactory_GetPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage_bool)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
<a name='Cmf_Common_Cli_Factories_PackageTypeFactory_GetPackageTypeHandler(Cmf_Common_Cli_Objects_CmfPackage_bool)_setDefaultValues'></a>
`setDefaultValues` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [set default values].
  
#### Returns
[IPackageTypeHandler](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler')  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
  
<a name='Cmf_Common_Cli_Factories_PackageTypeFactory_GetPackageTypeHandler(System_IO_FileInfo_bool)'></a>
## PackageTypeFactory.GetPackageTypeHandler(FileInfo, bool) Method
Gets the package type handler.  
```csharp
public static Cmf.Common.Cli.Interfaces.IPackageTypeHandler GetPackageTypeHandler(System.IO.FileInfo file, bool setDefaultValues=false);
```
#### Parameters
<a name='Cmf_Common_Cli_Factories_PackageTypeFactory_GetPackageTypeHandler(System_IO_FileInfo_bool)_file'></a>
`file` [System.IO.FileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.FileInfo 'System.IO.FileInfo')  
The file.
  
<a name='Cmf_Common_Cli_Factories_PackageTypeFactory_GetPackageTypeHandler(System_IO_FileInfo_bool)_setDefaultValues'></a>
`setDefaultValues` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
#### Returns
[IPackageTypeHandler](Cmf_Common_Cli_Interfaces.md#Cmf_Common_Cli_Interfaces_IPackageTypeHandler 'Cmf.Common.Cli.Interfaces.IPackageTypeHandler')  
  
  

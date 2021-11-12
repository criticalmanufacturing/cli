#### [cmf](index.md 'index')
## Cmf.Common.Cli.Utilities Namespace
### Classes
<a name='Cmf_Common_Cli_Utilities_CliException'></a>
## CliException Class
```csharp
public class CliException : System.Exception
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [System.Exception](https://docs.microsoft.com/en-us/dotnet/api/System.Exception 'System.Exception') &#129106; CliException  
### Constructors
<a name='Cmf_Common_Cli_Utilities_CliException_CliException()'></a>
## CliException.CliException() Constructor
Initializes a new instance of the [CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException') class.  
```csharp
public CliException();
```
  
<a name='Cmf_Common_Cli_Utilities_CliException_CliException(string_System_Exception)'></a>
## CliException.CliException(string, Exception) Constructor
Initializes a new instance of the [CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException') class.  
```csharp
public CliException(string message, System.Exception innerException);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_CliException_CliException(string_System_Exception)_message'></a>
`message` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The error message that explains the reason for the exception.
  
<a name='Cmf_Common_Cli_Utilities_CliException_CliException(string_System_Exception)_innerException'></a>
`innerException` [System.Exception](https://docs.microsoft.com/en-us/dotnet/api/System.Exception 'System.Exception')  
The exception that is the cause of the current exception, or a null reference ([Nothing](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/Nothing 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/Nothing') in Visual Basic) if no inner exception is specified.
  
  
<a name='Cmf_Common_Cli_Utilities_CliException_CliException(string)'></a>
## CliException.CliException(string) Constructor
Initializes a new instance of the [CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException') class.  
```csharp
public CliException(string message);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_CliException_CliException(string)_message'></a>
`message` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The message that describes the error.
  
  
<a name='Cmf_Common_Cli_Utilities_CliException_CliException(System_Runtime_Serialization_SerializationInfo_System_Runtime_Serialization_StreamingContext)'></a>
## CliException.CliException(SerializationInfo, StreamingContext) Constructor
Initializes a new instance of the [CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException') class.  
```csharp
protected CliException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_CliException_CliException(System_Runtime_Serialization_SerializationInfo_System_Runtime_Serialization_StreamingContext)_info'></a>
`info` [System.Runtime.Serialization.SerializationInfo](https://docs.microsoft.com/en-us/dotnet/api/System.Runtime.Serialization.SerializationInfo 'System.Runtime.Serialization.SerializationInfo')  
The [System.Runtime.Serialization.SerializationInfo](https://docs.microsoft.com/en-us/dotnet/api/System.Runtime.Serialization.SerializationInfo 'System.Runtime.Serialization.SerializationInfo') that holds the serialized object data about the exception being thrown.
  
<a name='Cmf_Common_Cli_Utilities_CliException_CliException(System_Runtime_Serialization_SerializationInfo_System_Runtime_Serialization_StreamingContext)_context'></a>
`context` [System.Runtime.Serialization.StreamingContext](https://docs.microsoft.com/en-us/dotnet/api/System.Runtime.Serialization.StreamingContext 'System.Runtime.Serialization.StreamingContext')  
The [System.Runtime.Serialization.StreamingContext](https://docs.microsoft.com/en-us/dotnet/api/System.Runtime.Serialization.StreamingContext 'System.Runtime.Serialization.StreamingContext') that contains contextual information about the source or destination.
  
  
#### See Also
- [System.Exception](https://docs.microsoft.com/en-us/dotnet/api/System.Exception 'System.Exception')
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods'></a>
## ExtensionMethods Class
```csharp
public static class ExtensionMethods
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; ExtensionMethods  
### Methods
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_Element(System_Xml_Linq_XContainer_System_Xml_Linq_XName_bool)'></a>
## ExtensionMethods.Element(XContainer, XName, bool) Method
Gets the first (in document order) child element with the specified [System.Xml.Linq.XName](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.Linq.XName 'System.Xml.Linq.XName').  
```csharp
public static System.Xml.Linq.XElement Element(this System.Xml.Linq.XContainer element, System.Xml.Linq.XName name, bool ignoreCase);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_Element(System_Xml_Linq_XContainer_System_Xml_Linq_XName_bool)_element'></a>
`element` [System.Xml.Linq.XContainer](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.Linq.XContainer 'System.Xml.Linq.XContainer')  
The element.
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_Element(System_Xml_Linq_XContainer_System_Xml_Linq_XName_bool)_name'></a>
`name` [System.Xml.Linq.XName](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.Linq.XName 'System.Xml.Linq.XName')  
The [System.Xml.Linq.XName](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.Linq.XName 'System.Xml.Linq.XName') to match.
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_Element(System_Xml_Linq_XContainer_System_Xml_Linq_XName_bool)_ignoreCase'></a>
`ignoreCase` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
If set to `true` case will be ignored whilst searching for the [System.Xml.Linq.XElement](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.Linq.XElement 'System.Xml.Linq.XElement').
  
#### Returns
[System.Xml.Linq.XElement](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.Linq.XElement 'System.Xml.Linq.XElement')  
A [System.Xml.Linq.XElement](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.Linq.XElement 'System.Xml.Linq.XElement') that matches the specified [System.Xml.Linq.XName](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.Linq.XName 'System.Xml.Linq.XName'), or null.  
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_GetPackageJsonFile(System_IO_Abstractions_IDirectoryInfo)'></a>
## ExtensionMethods.GetPackageJsonFile(IDirectoryInfo) Method
Gets the package json file.  
```csharp
public static object GetPackageJsonFile(this System.IO.Abstractions.IDirectoryInfo packDirectory);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_GetPackageJsonFile(System_IO_Abstractions_IDirectoryInfo)_packDirectory'></a>
`packDirectory` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The pack directory.
  
#### Returns
[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')  
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_GetPropertyValueFromTokenName(object_string)'></a>
## ExtensionMethods.GetPropertyValueFromTokenName(object, string) Method
Gets the name of the property value from token.  
```csharp
public static object GetPropertyValueFromTokenName(this object obj, string token);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_GetPropertyValueFromTokenName(object_string)_obj'></a>
`obj` [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')  
The object.
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_GetPropertyValueFromTokenName(object_string)_token'></a>
`token` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The token.
  
#### Returns
[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')  
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_GetTokenName(string)'></a>
## ExtensionMethods.GetTokenName(string) Method
Gets the name of the token.  
```csharp
private static string GetTokenName(this string token);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_GetTokenName(string)_token'></a>
`token` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The token.
  
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_Has(System_Collections_Generic_IEnumerable_object__object)'></a>
## ExtensionMethods.Has(IEnumerable&lt;object&gt;, object) Method
Determines whether [has] [the specified object].  
```csharp
public static bool Has(this System.Collections.Generic.IEnumerable<object> objects, object obj);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_Has(System_Collections_Generic_IEnumerable_object__object)_objects'></a>
`objects` [System.Collections.Generic.IEnumerable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IEnumerable-1 'System.Collections.Generic.IEnumerable`1')[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IEnumerable-1 'System.Collections.Generic.IEnumerable`1')  
The objects.
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_Has(System_Collections_Generic_IEnumerable_object__object)_obj'></a>
`obj` [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')  
The object.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if [has] [the specified object]; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource__System_Func_TSource_bool_)'></a>
## ExtensionMethods.HasAny&lt;TSource&gt;(IEnumerable&lt;TSource&gt;, Func&lt;TSource,bool&gt;) Method
Determines whether a sequence contains any elements.  
```csharp
public static bool HasAny<TSource>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource,bool> predicate=null);
```
#### Type parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource__System_Func_TSource_bool_)_TSource'></a>
`TSource`  
The type of the source.
  
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource__System_Func_TSource_bool_)_source'></a>
`source` [System.Collections.Generic.IEnumerable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IEnumerable-1 'System.Collections.Generic.IEnumerable`1')[TSource](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource__System_Func_TSource_bool_)_TSource 'Cmf.Common.Cli.Utilities.ExtensionMethods.HasAny&lt;TSource&gt;(System.Collections.Generic.IEnumerable&lt;TSource&gt;, System.Func&lt;TSource,bool&gt;).TSource')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IEnumerable-1 'System.Collections.Generic.IEnumerable`1')  
The source.
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource__System_Func_TSource_bool_)_predicate'></a>
`predicate` [System.Func&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Func-2 'System.Func`2')[TSource](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource__System_Func_TSource_bool_)_TSource 'Cmf.Common.Cli.Utilities.ExtensionMethods.HasAny&lt;TSource&gt;(System.Collections.Generic.IEnumerable&lt;TSource&gt;, System.Func&lt;TSource,bool&gt;).TSource')[,](https://docs.microsoft.com/en-us/dotnet/api/System.Func-2 'System.Func`2')[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Func-2 'System.Func`2')  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the specified source has any; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource_)'></a>
## ExtensionMethods.HasAny&lt;TSource&gt;(IEnumerable&lt;TSource&gt;) Method
Determines whether a sequence contains any elements.  
```csharp
public static bool HasAny<TSource>(this System.Collections.Generic.IEnumerable<TSource> source);
```
#### Type parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource_)_TSource'></a>
`TSource`  
The type of the source.
  
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource_)_source'></a>
`source` [System.Collections.Generic.IEnumerable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IEnumerable-1 'System.Collections.Generic.IEnumerable`1')[TSource](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_ExtensionMethods_HasAny_TSource_(System_Collections_Generic_IEnumerable_TSource_)_TSource 'Cmf.Common.Cli.Utilities.ExtensionMethods.HasAny&lt;TSource&gt;(System.Collections.Generic.IEnumerable&lt;TSource&gt;).TSource')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IEnumerable-1 'System.Collections.Generic.IEnumerable`1')  
The source.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the specified source has any; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IgnoreCaseEquals(string_string)'></a>
## ExtensionMethods.IgnoreCaseEquals(string, string) Method
Ignores the case equals.  
```csharp
public static bool IgnoreCaseEquals(this string str, string value);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IgnoreCaseEquals(string_string)_str'></a>
`str` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The string.
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IgnoreCaseEquals(string_string)_value'></a>
`value` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The value.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IsDirectory(System_Uri)'></a>
## ExtensionMethods.IsDirectory(Uri) Method
Determines whether this instance is directory.  
```csharp
public static bool IsDirectory(this System.Uri uri);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IsDirectory(System_Uri)_uri'></a>
`uri` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
The URI.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the specified URI is directory; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IsList(object)'></a>
## ExtensionMethods.IsList(object) Method
Determines whether this instance is list.  
```csharp
public static bool IsList(this object obj);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IsList(object)_obj'></a>
`obj` [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')  
The object.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the specified object is list; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IsNullOrEmpty(object)'></a>
## ExtensionMethods.IsNullOrEmpty(object) Method
Determines whether [is null or empty].  
```csharp
public static bool IsNullOrEmpty(this object obj);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IsNullOrEmpty(object)_obj'></a>
`obj` [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')  
The object.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if [is null or empty] [the specified object]; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IsToken(string)'></a>
## ExtensionMethods.IsToken(string) Method
Determines whether the specified value is token.  
```csharp
private static bool IsToken(this string value);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_IsToken(string)_value'></a>
`value` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The value.
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the specified value is token; otherwise, `false`.  
            
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_LoadCmfPackagesFromSubDirectories(System_IO_Abstractions_IDirectoryInfo_Cmf_Common_Cli_Enums_PackageType_bool)'></a>
## ExtensionMethods.LoadCmfPackagesFromSubDirectories(IDirectoryInfo, PackageType, bool) Method
Gets the CMF package files from sub directories.  
```csharp
public static Cmf.Common.Cli.Objects.CmfPackageCollection LoadCmfPackagesFromSubDirectories(this System.IO.Abstractions.IDirectoryInfo directory, Cmf.Common.Cli.Enums.PackageType packageType=Cmf.Common.Cli.Enums.PackageType.None, bool setDefaultValues=false);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_LoadCmfPackagesFromSubDirectories(System_IO_Abstractions_IDirectoryInfo_Cmf_Common_Cli_Enums_PackageType_bool)_directory'></a>
`directory` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The directory.
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_LoadCmfPackagesFromSubDirectories(System_IO_Abstractions_IDirectoryInfo_Cmf_Common_Cli_Enums_PackageType_bool)_packageType'></a>
`packageType` [PackageType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackageType 'Cmf.Common.Cli.Enums.PackageType')  
Type of the package.
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_LoadCmfPackagesFromSubDirectories(System_IO_Abstractions_IDirectoryInfo_Cmf_Common_Cli_Enums_PackageType_bool)_setDefaultValues'></a>
`setDefaultValues` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
#### Returns
[CmfPackageCollection](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackageCollection 'Cmf.Common.Cli.Objects.CmfPackageCollection')  
  
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_ToCamelCase(string)'></a>
## ExtensionMethods.ToCamelCase(string) Method
Converts to camelcase.  
```csharp
public static string ToCamelCase(this string str);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_ExtensionMethods_ToCamelCase(string)_str'></a>
`str` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The string.
  
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities'></a>
## FileSystemUtilities Class
```csharp
public static class FileSystemUtilities
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; FileSystemUtilities  
### Methods
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyDirectory(string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool)'></a>
## FileSystemUtilities.CopyDirectory(string, string, IFileSystem, List&lt;string&gt;, bool, bool) Method
Directories copy.  
```csharp
public static void CopyDirectory(string sourceDirName, string destDirName, System.IO.Abstractions.IFileSystem fileSystem, System.Collections.Generic.List<string> contentToIgnore=null, bool copySubDirs=true, bool isCopyDependencies=false);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyDirectory(string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool)_sourceDirName'></a>
`sourceDirName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Name of the source dir.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyDirectory(string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool)_destDirName'></a>
`destDirName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Name of the dest dir.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyDirectory(string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyDirectory(string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool)_contentToIgnore'></a>
`contentToIgnore` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
The exclusions.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyDirectory(string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool)_copySubDirs'></a>
`copySubDirs` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [copy sub dirs].
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyDirectory(string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool)_isCopyDependencies'></a>
`isCopyDependencies` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [is copy dependencies].
  
#### Exceptions
[System.IO.DirectoryNotFoundException](https://docs.microsoft.com/en-us/dotnet/api/System.IO.DirectoryNotFoundException 'System.IO.DirectoryNotFoundException')  
Source directory does not exist or could not be found: "  
            + sourceDirName
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyInstallDependenciesFiles(System_IO_Abstractions_IDirectoryInfo_Cmf_Common_Cli_Enums_PackageType_System_IO_Abstractions_IFileSystem)'></a>
## FileSystemUtilities.CopyInstallDependenciesFiles(IDirectoryInfo, PackageType, IFileSystem) Method
Copies the install dependencies.  
```csharp
public static void CopyInstallDependenciesFiles(System.IO.Abstractions.IDirectoryInfo packageOutputDir, Cmf.Common.Cli.Enums.PackageType packageType, System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyInstallDependenciesFiles(System_IO_Abstractions_IDirectoryInfo_Cmf_Common_Cli_Enums_PackageType_System_IO_Abstractions_IFileSystem)_packageOutputDir'></a>
`packageOutputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package output dir.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyInstallDependenciesFiles(System_IO_Abstractions_IDirectoryInfo_Cmf_Common_Cli_Enums_PackageType_System_IO_Abstractions_IFileSystem)_packageType'></a>
`packageType` [PackageType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackageType 'Cmf.Common.Cli.Enums.PackageType')  
Type of the package.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyInstallDependenciesFiles(System_IO_Abstractions_IDirectoryInfo_Cmf_Common_Cli_Enums_PackageType_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyStream(System_IO_Stream_System_IO_Stream)'></a>
## FileSystemUtilities.CopyStream(Stream, Stream) Method
Copies the contents of input to output. Doesn't close either stream.  
```csharp
public static void CopyStream(System.IO.Stream input, System.IO.Stream output);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyStream(System_IO_Stream_System_IO_Stream)_input'></a>
`input` [System.IO.Stream](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Stream 'System.IO.Stream')  
The input.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_CopyStream(System_IO_Stream_System_IO_Stream)_output'></a>
`output` [System.IO.Stream](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Stream 'System.IO.Stream')  
The output.
  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFileContentFromPackage(string_string)'></a>
## FileSystemUtilities.GetFileContentFromPackage(string, string) Method
Get File Content From package  
```csharp
public static string GetFileContentFromPackage(string packageFile, string filename);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFileContentFromPackage(string_string)_packageFile'></a>
`packageFile` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFileContentFromPackage(string_string)_filename'></a>
`filename` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFilesToPack(Cmf_Common_Cli_Objects_ContentToPack_string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool_System_Collections_Generic_List_Cmf_Common_Cli_Objects_FileToPack_)'></a>
## FileSystemUtilities.GetFilesToPack(ContentToPack, string, string, IFileSystem, List&lt;string&gt;, bool, bool, List&lt;FileToPack&gt;) Method
Gets the files to pack.  
```csharp
public static System.Collections.Generic.List<Cmf.Common.Cli.Objects.FileToPack> GetFilesToPack(Cmf.Common.Cli.Objects.ContentToPack contentToPack, string sourceDirName, string destDirName, System.IO.Abstractions.IFileSystem fileSystem, System.Collections.Generic.List<string> contentToIgnore=null, bool copySubDirs=true, bool isCopyDependencies=false, System.Collections.Generic.List<Cmf.Common.Cli.Objects.FileToPack> filesToPack=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFilesToPack(Cmf_Common_Cli_Objects_ContentToPack_string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool_System_Collections_Generic_List_Cmf_Common_Cli_Objects_FileToPack_)_contentToPack'></a>
`contentToPack` [ContentToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_ContentToPack 'Cmf.Common.Cli.Objects.ContentToPack')  
The content to pack.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFilesToPack(Cmf_Common_Cli_Objects_ContentToPack_string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool_System_Collections_Generic_List_Cmf_Common_Cli_Objects_FileToPack_)_sourceDirName'></a>
`sourceDirName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Name of the source dir.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFilesToPack(Cmf_Common_Cli_Objects_ContentToPack_string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool_System_Collections_Generic_List_Cmf_Common_Cli_Objects_FileToPack_)_destDirName'></a>
`destDirName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Name of the dest dir.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFilesToPack(Cmf_Common_Cli_Objects_ContentToPack_string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool_System_Collections_Generic_List_Cmf_Common_Cli_Objects_FileToPack_)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFilesToPack(Cmf_Common_Cli_Objects_ContentToPack_string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool_System_Collections_Generic_List_Cmf_Common_Cli_Objects_FileToPack_)_contentToIgnore'></a>
`contentToIgnore` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
The content to ignore.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFilesToPack(Cmf_Common_Cli_Objects_ContentToPack_string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool_System_Collections_Generic_List_Cmf_Common_Cli_Objects_FileToPack_)_copySubDirs'></a>
`copySubDirs` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [copy sub dirs].
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFilesToPack(Cmf_Common_Cli_Objects_ContentToPack_string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool_System_Collections_Generic_List_Cmf_Common_Cli_Objects_FileToPack_)_isCopyDependencies'></a>
`isCopyDependencies` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [is copy dependencies].
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetFilesToPack(Cmf_Common_Cli_Objects_ContentToPack_string_string_System_IO_Abstractions_IFileSystem_System_Collections_Generic_List_string__bool_bool_System_Collections_Generic_List_Cmf_Common_Cli_Objects_FileToPack_)_filesToPack'></a>
`filesToPack` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[FileToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_FileToPack 'Cmf.Common.Cli.Objects.FileToPack')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
The files to pack.
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[FileToPack](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_FileToPack 'Cmf.Common.Cli.Objects.FileToPack')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
#### Exceptions
[System.IO.DirectoryNotFoundException](https://docs.microsoft.com/en-us/dotnet/api/System.IO.DirectoryNotFoundException 'System.IO.DirectoryNotFoundException')  
$"Source directory does not exist or could not be found: {sourceDirName}
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetManifestFromPackage(string)'></a>
## FileSystemUtilities.GetManifestFromPackage(string) Method
Get Manifest File From package  
```csharp
public static System.Xml.Linq.XDocument GetManifestFromPackage(string packageFile);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetManifestFromPackage(string)_packageFile'></a>
`packageFile` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
#### Returns
[System.Xml.Linq.XDocument](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.Linq.XDocument 'System.Xml.Linq.XDocument')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetOutputDir(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_bool)'></a>
## FileSystemUtilities.GetOutputDir(CmfPackage, IDirectoryInfo, bool) Method
Gets the output dir.  
```csharp
public static System.IO.Abstractions.IDirectoryInfo GetOutputDir(Cmf.Common.Cli.Objects.CmfPackage cmfPackage, System.IO.Abstractions.IDirectoryInfo outputDir, bool force);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetOutputDir(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_bool)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetOutputDir(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_bool)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The output dir.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetOutputDir(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_bool)_force'></a>
`force` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [force].
  
#### Returns
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageOutputDir(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IFileSystem)'></a>
## FileSystemUtilities.GetPackageOutputDir(CmfPackage, IDirectoryInfo, IFileSystem) Method
Gets the package output dir.  
```csharp
public static System.IO.Abstractions.IDirectoryInfo GetPackageOutputDir(Cmf.Common.Cli.Objects.CmfPackage cmfPackage, System.IO.Abstractions.IDirectoryInfo packageDirectory, System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageOutputDir(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IFileSystem)_cmfPackage'></a>
`cmfPackage` [CmfPackage](Cmf_Common_Cli_Objects.md#Cmf_Common_Cli_Objects_CmfPackage 'Cmf.Common.Cli.Objects.CmfPackage')  
The CMF package.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageOutputDir(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IFileSystem)_packageDirectory'></a>
`packageDirectory` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
The package directory.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageOutputDir(Cmf_Common_Cli_Objects_CmfPackage_System_IO_Abstractions_IDirectoryInfo_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
#### Returns
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageRoot(System_IO_Abstractions_IFileSystem_string)'></a>
## FileSystemUtilities.GetPackageRoot(IFileSystem, string) Method
Gets the package root.  
```csharp
public static System.IO.Abstractions.IDirectoryInfo GetPackageRoot(System.IO.Abstractions.IFileSystem fileSystem, string workingDir=null);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageRoot(System_IO_Abstractions_IFileSystem_string)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageRoot(System_IO_Abstractions_IFileSystem_string)_workingDir'></a>
`workingDir` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
#### Returns
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
Cannot find package root. Are you in a valid package directory?
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageRootByType(string_Cmf_Common_Cli_Enums_PackageType_System_IO_Abstractions_IFileSystem)'></a>
## FileSystemUtilities.GetPackageRootByType(string, PackageType, IFileSystem) Method
Gets the package root of type package root.  
```csharp
public static System.IO.Abstractions.IDirectoryInfo GetPackageRootByType(string directoryName, Cmf.Common.Cli.Enums.PackageType packageType, System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageRootByType(string_Cmf_Common_Cli_Enums_PackageType_System_IO_Abstractions_IFileSystem)_directoryName'></a>
`directoryName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The current working directory
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageRootByType(string_Cmf_Common_Cli_Enums_PackageType_System_IO_Abstractions_IFileSystem)_packageType'></a>
`packageType` [PackageType](Cmf_Common_Cli_Enums.md#Cmf_Common_Cli_Enums_PackageType 'Cmf.Common.Cli.Enums.PackageType')  
Type of the package.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetPackageRootByType(string_Cmf_Common_Cli_Enums_PackageType_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
#### Returns
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
Cannot find project root. Are you in a valid project directory?
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetProjectRoot(System_IO_Abstractions_IFileSystem_bool)'></a>
## FileSystemUtilities.GetProjectRoot(IFileSystem, bool) Method
Gets the project root.  
```csharp
public static System.IO.Abstractions.IDirectoryInfo GetProjectRoot(System.IO.Abstractions.IFileSystem fileSystem, bool throwException=false);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetProjectRoot(System_IO_Abstractions_IFileSystem_bool)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_GetProjectRoot(System_IO_Abstractions_IFileSystem_bool)_throwException'></a>
`throwException` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
#### Returns
[System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
#### Exceptions
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
Cannot find project root. Are you in a valid project directory?
[CliException](Cmf_Common_Cli_Utilities.md#Cmf_Common_Cli_Utilities_CliException 'Cmf.Common.Cli.Utilities.CliException')  
Cannot find project root. Are you in a valid project directory?
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_ReadEnvironmentConfig(string_System_IO_Abstractions_IFileSystem)'></a>
## FileSystemUtilities.ReadEnvironmentConfig(string, IFileSystem) Method
Reads the environment configuration.  
```csharp
public static System.Text.Json.JsonDocument ReadEnvironmentConfig(string envConfigName, System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_ReadEnvironmentConfig(string_System_IO_Abstractions_IFileSystem)_envConfigName'></a>
`envConfigName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Name of the env configuration.
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_ReadEnvironmentConfig(string_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
#### Returns
[System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/System.Text.Json.JsonDocument 'System.Text.Json.JsonDocument')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_ReadProjectConfig(System_IO_Abstractions_IFileSystem)'></a>
## FileSystemUtilities.ReadProjectConfig(IFileSystem) Method
Reads the project configuration.  
```csharp
public static System.Text.Json.JsonDocument ReadProjectConfig(System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_ReadProjectConfig(System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
  
#### Returns
[System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/System.Text.Json.JsonDocument 'System.Text.Json.JsonDocument')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_ReadToString(System_IO_Abstractions_IFileInfo)'></a>
## FileSystemUtilities.ReadToString(IFileInfo) Method
Reads to string.  
```csharp
public static string ReadToString(this System.IO.Abstractions.IFileInfo fi);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_ReadToString(System_IO_Abstractions_IFileInfo)_fi'></a>
`fi` [System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
The fi.
  
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_ReadToStringList(System_IO_Abstractions_IFileInfo)'></a>
## FileSystemUtilities.ReadToStringList(IFileInfo) Method
Reads to string list.  
```csharp
public static System.Collections.Generic.List<string> ReadToStringList(this System.IO.Abstractions.IFileInfo fi);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_FileSystemUtilities_ReadToStringList(System_IO_Abstractions_IFileInfo)_fi'></a>
`fi` [System.IO.Abstractions.IFileInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileInfo 'System.IO.Abstractions.IFileInfo')  
The fi.
  
#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')  
  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities'></a>
## GenericUtilities Class
```csharp
public static class GenericUtilities
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; GenericUtilities  
### Methods
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetCurrentPresentationVersion(string_string_string)'></a>
## GenericUtilities.GetCurrentPresentationVersion(string, string, string) Method
Get current version based on string, for  
the format 1.0.0-1234  
where 1.0.0 will be the version  
and the 1234 will be the build number  
```csharp
public static void GetCurrentPresentationVersion(string source, out string version, out string buildNr);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetCurrentPresentationVersion(string_string_string)_source'></a>
`source` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Source information to be parsed
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetCurrentPresentationVersion(string_string_string)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Version Number
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetCurrentPresentationVersion(string_string_string)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Build Number
  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetEmbeddedResourceContent(string)'></a>
## GenericUtilities.GetEmbeddedResourceContent(string) Method
Read Embedded Resource file content and return it.  
e.g. GetEmbeddedResourceContent("BuildScrips/cleanNodeModules.ps1")  
NOTE: Don't forget to set the BuildAction for your resource as EmbeddedResource. Resources must be in the [root]/resources folder  
```csharp
public static string GetEmbeddedResourceContent(string resourceName);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetEmbeddedResourceContent(string)_resourceName'></a>
`resourceName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
the path of the embedded resource inside the [root}/resources folder
  
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
the resource content  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetPackageFromRepository(System_IO_Abstractions_IDirectoryInfo_System_Uri_bool_string_string_System_IO_Abstractions_IFileSystem)'></a>
## GenericUtilities.GetPackageFromRepository(IDirectoryInfo, Uri, bool, string, string, IFileSystem) Method
Get Package from Repository  
```csharp
public static bool GetPackageFromRepository(System.IO.Abstractions.IDirectoryInfo outputDir, System.Uri repoUri, bool force, string packageId, string packageVersion, System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetPackageFromRepository(System_IO_Abstractions_IDirectoryInfo_System_Uri_bool_string_string_System_IO_Abstractions_IFileSystem)_outputDir'></a>
`outputDir` [System.IO.Abstractions.IDirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IDirectoryInfo 'System.IO.Abstractions.IDirectoryInfo')  
Target directory for the package
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetPackageFromRepository(System_IO_Abstractions_IDirectoryInfo_System_Uri_bool_string_string_System_IO_Abstractions_IFileSystem)_repoUri'></a>
`repoUri` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
Repository Uri
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetPackageFromRepository(System_IO_Abstractions_IDirectoryInfo_System_Uri_bool_string_string_System_IO_Abstractions_IFileSystem)_force'></a>
`force` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetPackageFromRepository(System_IO_Abstractions_IDirectoryInfo_System_Uri_bool_string_string_System_IO_Abstractions_IFileSystem)_packageId'></a>
`packageId` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Package Identifier
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetPackageFromRepository(System_IO_Abstractions_IDirectoryInfo_System_Uri_bool_string_string_System_IO_Abstractions_IFileSystem)_packageVersion'></a>
`packageVersion` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Package Version
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_GetPackageFromRepository(System_IO_Abstractions_IDirectoryInfo_System_Uri_bool_string_string_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_JsonObjectToUri(dynamic)'></a>
## GenericUtilities.JsonObjectToUri(dynamic) Method
Converts a JsonObject to an Uri  
```csharp
public static System.Uri? JsonObjectToUri(dynamic value);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_JsonObjectToUri(dynamic)_value'></a>
`value` [dynamic](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/using-type-dynamic 'https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/using-type-dynamic')  
  
#### Returns
[System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_RetrieveNewPresentationVersion(string_string_string)'></a>
## GenericUtilities.RetrieveNewPresentationVersion(string, string, string) Method
Will create a new version based on the old and new inputs  
```csharp
public static string RetrieveNewPresentationVersion(string currentVersion, string version, string buildNr);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_RetrieveNewPresentationVersion(string_string_string)_currentVersion'></a>
`currentVersion` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_RetrieveNewPresentationVersion(string_string_string)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_RetrieveNewPresentationVersion(string_string_string)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
the new version  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_RetrieveNewVersion(string_string_string)'></a>
## GenericUtilities.RetrieveNewVersion(string, string, string) Method
Will create a new version based on the old and new inputs  
```csharp
public static string RetrieveNewVersion(string currentVersion, string version, string buildNr);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_RetrieveNewVersion(string_string_string)_currentVersion'></a>
`currentVersion` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_RetrieveNewVersion(string_string_string)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
<a name='Cmf_Common_Cli_Utilities_GenericUtilities_RetrieveNewVersion(string_string_string)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
  
#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
the new version  
  
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities'></a>
## IoTUtilities Class
```csharp
public static class IoTUtilities
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; IoTUtilities  
### Methods
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTCustomPackages(string_string_string_string_System_IO_Abstractions_IFileSystem)'></a>
## IoTUtilities.BumpIoTCustomPackages(string, string, string, string, IFileSystem) Method
Bumps the iot custom packages.  
```csharp
public static void BumpIoTCustomPackages(string packagePath, string version, string buildNr, string packageNames, System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTCustomPackages(string_string_string_string_System_IO_Abstractions_IFileSystem)_packagePath'></a>
`packagePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The package path.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTCustomPackages(string_string_string_string_System_IO_Abstractions_IFileSystem)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTCustomPackages(string_string_string_string_System_IO_Abstractions_IFileSystem)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version of the build (v-b).
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTCustomPackages(string_string_string_string_System_IO_Abstractions_IFileSystem)_packageNames'></a>
`packageNames` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The package names.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTCustomPackages(string_string_string_string_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTMasterData(string_string_string_System_IO_Abstractions_IFileSystem_string_bool)'></a>
## IoTUtilities.BumpIoTMasterData(string, string, string, IFileSystem, string, bool) Method
Bumps the io t master data.  
```csharp
public static void BumpIoTMasterData(string automationWorkflowFileGroup, string version, string buildNr, System.IO.Abstractions.IFileSystem fileSystem, string packageNames=null, bool onlyCustomization=true);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTMasterData(string_string_string_System_IO_Abstractions_IFileSystem_string_bool)_automationWorkflowFileGroup'></a>
`automationWorkflowFileGroup` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The automation workflow file group.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTMasterData(string_string_string_System_IO_Abstractions_IFileSystem_string_bool)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTMasterData(string_string_string_System_IO_Abstractions_IFileSystem_string_bool)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version of the build (v-b).
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTMasterData(string_string_string_System_IO_Abstractions_IFileSystem_string_bool)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTMasterData(string_string_string_System_IO_Abstractions_IFileSystem_string_bool)_packageNames'></a>
`packageNames` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The package names.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpIoTMasterData(string_string_string_System_IO_Abstractions_IFileSystem_string_bool)_onlyCustomization'></a>
`onlyCustomization` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
if set to `true` [only customization].
  
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpWorkflowFiles(string_string_string_string_string_System_IO_Abstractions_IFileSystem)'></a>
## IoTUtilities.BumpWorkflowFiles(string, string, string, string, string, IFileSystem) Method
Bumps the workflow files.  
```csharp
public static void BumpWorkflowFiles(string group, string version, string buildNr, string workflowName, string packageNames, System.IO.Abstractions.IFileSystem fileSystem);
```
#### Parameters
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpWorkflowFiles(string_string_string_string_string_System_IO_Abstractions_IFileSystem)_group'></a>
`group` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The group.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpWorkflowFiles(string_string_string_string_string_System_IO_Abstractions_IFileSystem)_version'></a>
`version` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpWorkflowFiles(string_string_string_string_string_System_IO_Abstractions_IFileSystem)_buildNr'></a>
`buildNr` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The version of the build (v-b).
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpWorkflowFiles(string_string_string_string_string_System_IO_Abstractions_IFileSystem)_workflowName'></a>
`workflowName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Name of the workflow.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpWorkflowFiles(string_string_string_string_string_System_IO_Abstractions_IFileSystem)_packageNames'></a>
`packageNames` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The package names.
  
<a name='Cmf_Common_Cli_Utilities_IoTUtilities_BumpWorkflowFiles(string_string_string_string_string_System_IO_Abstractions_IFileSystem)_fileSystem'></a>
`fileSystem` [System.IO.Abstractions.IFileSystem](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Abstractions.IFileSystem 'System.IO.Abstractions.IFileSystem')  
the underlying file system
  
  
  

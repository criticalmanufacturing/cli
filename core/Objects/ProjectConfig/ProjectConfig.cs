using System;
using System.Text.Json.Serialization;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using NuGet.Versioning;

namespace Cmf.CLI.Core.Objects;

public class ProjectConfig
{
	public string ProjectName { get; set; }
	public RepositoryType? RepositoryType { get; set; }
	public BaseLayer? BaseLayer { get; set; }
	[Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
	public Uri NPMRegistry { get; set; }
	[Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
	public Uri NuGetRegistry { get; set; }
	public string Tenant { get; set; }
	[Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<NuGetVersion>))]
	public NuGetVersion MESVersion { get; set; }
	public string NGXSchematicsVersion { get; set; }
	[Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<NuGetVersion>))]
	public NuGetVersion NugetVersion { get; set; }
	[Newtonsoft.Json.JsonConverter(typeof(VersionStringConverter<NuGetVersion>))]
	public NuGetVersion TestScenariosNugetVersion { get; set; }
	[Newtonsoft.Json.JsonConverter(typeof(UriConverter))]
	public Uri CIRepo { get; set; }
}
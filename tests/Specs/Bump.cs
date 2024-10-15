using Cmf.CLI.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Handlers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace tests.Specs;

public class Bump
{
    [Theory]
    [InlineData("'", "1.0.0")]
    [InlineData("\"", "1.0.0")]
    [InlineData("'", "")]
    public void Bump_MetadataWithAnyQuoteType(string quoteType, string version)
    {
        // files
        string cmfPackageJson = $"help/{CliConstants.CmfPackageFileName}";
        string npmPackageJson = "/help/package.json";
        string metadataTS =
            "/help/src/packages/cmf.docs.area.cmf.custom.help/src/cmf.docs.area.cmf.custom.help.metadata.ts";

        string bumpVersion = "1.0.1";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                MockUnixSupport.Path(@"c:\.project-config.json"), new MockFileData(
                    @"{
              ""MESVersion"": ""9.0.0""
            }")
            },
            {
                cmfPackageJson, new MockFileData(
                    @$"{{
                      ""packageId"": ""Cmf.Custom.Help"",
                      ""version"": ""{version}"",
                      ""description"": ""Cmf Custom Cmf.Custom.Help Package"",
                      ""packageType"": ""Help"",
                      ""isInstallable"": true,
                      ""isUniqueInstall"": false,
                      ""contentToPack"": [
                        {{
                          ""source"": ""src/packages/*"",
                          ""target"": ""node_modules"",
                          ""ignoreFiles"": [
                            "".npmignore""
                          ]
                        }}
                      ]
                }}")
            },
            {
                npmPackageJson, new MockFileData(
                    @$"{{
                      ""name"": ""cmf.docs.area"",
                      ""version"": ""{version}"",
                      ""description"": ""Help customization package"",
                      ""private"": true,
                      ""scripts"": {{
                        ""preinstall"": ""node npm.preinstall.js"",
                        ""postinstall"": ""node npm.postinstall.js""
                      }},
                      ""repository"": {{
                        ""type"": ""git"",
                        ""url"": ""https://url/git""
                      }}
                }}")
            },
            {
                metadataTS, new MockFileData(
                    @$"
                (...)
                function applyConfig (packageName: string) {{
                  const config: PackageMetadata = {{
                    version: {quoteType}{version}{quoteType},
                (...)
            ")
            }
        });

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IProjectConfigService>(new ProjectConfigService())
            .BuildServiceProvider();
        ExecutionContext.Initialize(fileSystem);

        IFileInfo cmfpackageFile = fileSystem.FileInfo.New(cmfPackageJson);
        IPackageTypeHandler packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfpackageFile);
        packageTypeHandler.Bump(bumpVersion, "");

        string cmfPackageVersion = (packageTypeHandler as HelpGulpPackageTypeHandler).CmfPackage.Version;
        dynamic packageFile = JsonConvert.DeserializeObject(fileSystem.File.ReadAllText(npmPackageJson));
        string packageFileVersion = packageFile.version;
        string metadataFile = fileSystem.File.ReadAllText(metadataTS);

        cmfPackageVersion.Should().Be(bumpVersion);
        packageFileVersion.Should().Be(bumpVersion);
        metadataFile.Should().Contain($"version: \"{bumpVersion}\"");
    }

    [Theory]
    [InlineData("1.1.0", ".")]
    [InlineData("1.1.0", "Cmf.Custom.Business")]
    public void Bump_BusinessFromDifferentPaths(string version, string runPath)
    {
        // files
        KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.0.0");
        KeyValuePair<string, string> packageBusiness = new("Cmf.Custom.Business", "1.0.0");
        KeyValuePair<string, string> packageTest = new("Cmf.Custom.Tests", "1.0.0");        
        string testsCmfPackageJson = $"Cmf.Custom.Tests/{CliConstants.CmfPackageFileName}";
        string businessAssemblyInfo = "Cmf.Custom.Business/Cmf.Custom.Common/Properties/AssemblyInfo.cs";
        string testAssemblyInfo = "Cmf.Custom.Tests/Cmf.Custom.E2ETests/Properties/AssemblyInfo.cs";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            // project config file
            { ".project-config.json", new MockFileData("") },

            // root cmfpackage file
            {
                $"cmfpackage.json", new MockFileData(
                    @$"{{
                ""packageId"": ""{packageRoot.Key}"",
                ""version"": ""{packageRoot.Value}"",
                ""description"": ""This package deploys Critical Manufacturing Customization"",
                ""packageType"": ""Root"",
                ""isInstallable"": true,
                ""isUniqueInstall"": false
            }}")
            }
        });

        // business cmfpackage file
        fileSystem.AddFile("Cmf.Custom.Business/cmfpackage.json", new MockFileData(
            @$"{{
              ""packageId"": ""{packageBusiness.Key}"",
              ""version"": ""{packageBusiness.Value}"",
              ""description"": ""Cmf Custom Business Package"",
              ""packageType"": ""Business"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
                {{
                  ""source"": ""Release/*.dll"",
                  ""target"": """",
                  ""ignoreFiles"": [
                    "".cmfpackageignore""
                  ]
                }}
              ]
            }}"));

        // business sln
        fileSystem.AddFile("Cmf.Custom.Business/Business.sln", new MockFileData(
            @$"Microsoft Visual Studio Solution File, Format Version 12.00
                # Visual Studio Version 17
                VisualStudioVersion = 17.3.32825.248
                MinimumVisualStudioVersion = 10.0.40219.1
                Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""Cmf.Custom.Common"", ""Cmf.Custom.Common\Cmf.Custom.Common.csproj"", ""{{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}""
                EndProject
                Global
	                GlobalSection(SolutionConfigurationPlatforms) = preSolution
		                Debug|Any CPU = Debug|Any CPU
		                Release|Any CPU = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(ProjectConfigurationPlatforms) = postSolution
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.Build.0 = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.Build.0 = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(SolutionProperties) = preSolution
		                HideSolutionNode = FALSE
	                EndGlobalSection
	                GlobalSection(ExtensibilityGlobals) = postSolution
		                SolutionGuid = {{B6998FE2-5739-49CF-939B-73DB4B5DAF2E}}
	                EndGlobalSection
                EndGlobal"));

        // common csproj
        fileSystem.AddFile("Cmf.Custom.Business/Cmf.Custom.Common/Cmf.Custom.Common.csproj", new MockFileData(
            @$"<Project Sdk=""Microsoft.NET.Sdk"">
                      <PropertyGroup>
                        <TargetFramework>net6.0</TargetFramework>
                        <ImplicitUsings>enable</ImplicitUsings>
                        <Nullable>enable</Nullable>
                      </PropertyGroup>
                </Project>"));

        // class file
        fileSystem.AddFile("Cmf.Custom.Business/Cmf.Custom.Common/TestClass.cs", new MockFileData(
            @$"namespace Cmf.Custom.Actions
                {{
                    public class Class1
                    {{
                    }}
                }}"));

        // assembly info file
        fileSystem.AddFile("Cmf.Custom.Business/Cmf.Custom.Common/Properties/AssemblyInfo.cs", new MockFileData(
            @$"using System.Reflection;
                         using System.Runtime.CompilerServices;
                         using System.Runtime.InteropServices;

                         [assembly: AssemblyTitle(""Cmf.Custom.Tests.Biz"")]
                         [assembly: AssemblyDescription("""")]
                         [assembly: AssemblyConfiguration("""")]
                         [assembly: AssemblyCompany("""")]
                         [assembly: AssemblyProduct(""Cmf.Custom.Tests.Biz"")]
                         [assembly: AssemblyCopyright(""Copyright ©  2020"")]
                         [assembly: AssemblyTrademark("""")]
                         [assembly: AssemblyCulture("""")]

                         [assembly: ComVisible(false)]

                         [assembly: Guid(""756fb0df-23db-4581-a056-9fff0e36e993"")]

                         // [assembly: AssemblyVersion(""1.0.*"")]
                         [assembly: AssemblyVersion(""1.0.0.0"")]
                         [assembly: AssemblyFileVersion(""1.0.0.0"")]"));

        // tests cmfpackage file
        fileSystem.AddFile("Cmf.Custom.Tests/cmfpackage.json", new MockFileData(
            @$"{{
              ""packageId"": ""{packageTest.Key}"",
              ""version"": ""{packageTest.Value}"",
              ""description"": ""Cmf Custom Test Package"",
              ""packageType"": ""Tests"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
                {{
                  ""source"": ""Release/*.dll"",
                  ""target"": """",
                  ""ignoreFiles"": [
                    "".cmfpackageignore""
                  ]
                }}
              ]
            }}"));

        // test sln
        fileSystem.AddFile("Cmf.Custom.Tests/Tests.sln", new MockFileData(
            @$"Microsoft Visual Studio Solution File, Format Version 12.00
                # Visual Studio Version 17
                VisualStudioVersion = 17.3.32825.248
                MinimumVisualStudioVersion = 10.0.40219.1
                Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""Cmf.Custom.E2ETests"", ""Cmf.Custom.Tests\Cmf.Custom.E2ETests.csproj"", ""{{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}""
                EndProject
                Global
	                GlobalSection(SolutionConfigurationPlatforms) = preSolution
		                Debug|Any CPU = Debug|Any CPU
		                Release|Any CPU = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(ProjectConfigurationPlatforms) = postSolution
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.Build.0 = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.Build.0 = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(SolutionProperties) = preSolution
		                HideSolutionNode = FALSE
	                EndGlobalSection
	                GlobalSection(ExtensibilityGlobals) = postSolution
		                SolutionGuid = {{B6998FE2-5739-49CF-939B-73DB4B5DAF2E}}
	                EndGlobalSection
                EndGlobal"));

        // common csproj
        fileSystem.AddFile("Cmf.Custom.Tests/Cmf.Custom.E2ETests/Cmf.Custom.E2ETests.csproj", new MockFileData(
            @$"<Project Sdk=""Microsoft.NET.Sdk"">
                      <PropertyGroup>
                        <TargetFramework>net6.0</TargetFramework>
                        <ImplicitUsings>enable</ImplicitUsings>
                        <Nullable>enable</Nullable>
                      </PropertyGroup>
                </Project>"));

        // class file
        fileSystem.AddFile("Cmf.Custom.Tests/Cmf.Custom.E2ETests/TestClass.cs", new MockFileData(
            @$"namespace Cmf.Custom.Actions
                {{
                    public class Class1
                    {{
                    }}
                }}"));

        // assembly info file
        fileSystem.AddFile("Cmf.Custom.Tests/Cmf.Custom.E2ETests/Properties/AssemblyInfo.cs", new MockFileData(
            @$"using System.Reflection;
                         using System.Runtime.CompilerServices;
                         using System.Runtime.InteropServices;

                         [assembly: AssemblyTitle(""Cmf.Custom.Tests.Biz"")]
                         [assembly: AssemblyDescription("""")]
                         [assembly: AssemblyConfiguration("""")]
                         [assembly: AssemblyCompany("""")]
                         [assembly: AssemblyProduct(""Cmf.Custom.Tests.Biz"")]
                         [assembly: AssemblyCopyright(""Copyright ©  2020"")]
                         [assembly: AssemblyTrademark("""")]
                         [assembly: AssemblyCulture("""")]

                         [assembly: ComVisible(false)]

                         [assembly: Guid(""756fb0df-23db-4581-a056-9fff0e36e993"")]

                         // [assembly: AssemblyVersion(""1.0.*"")]
                         [assembly: AssemblyVersion(""1.0.0.0"")]
                         [assembly: AssemblyFileVersion(""1.0.0.0"")]"));

        
        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IProjectConfigService>(new ProjectConfigService())
            .BuildServiceProvider();

        ExecutionContext.Initialize(fileSystem);

        IFileInfo cmfPackageFile = fileSystem.FileInfo.New($"Cmf.Custom.Business/{CliConstants.CmfPackageFileName}");

        BusinessPackageTypeHandler packageTypeHandler =
            PackageTypeFactory.GetPackageTypeHandler(cmfPackageFile) as BusinessPackageTypeHandler;

        fileSystem.Directory.SetCurrentDirectory(runPath);
        
        packageTypeHandler!.Bump(version, "");

        fileSystem.Directory.SetCurrentDirectory("..");
        string businessPackageVersion = packageTypeHandler.CmfPackage.Version;
        dynamic testPackageFile = JsonConvert.DeserializeObject(fileSystem.File.ReadAllText(testsCmfPackageJson));
        string testPackageVersion = testPackageFile!.version;
        string businessAssemblyInfoFile = fileSystem.File.ReadAllText(businessAssemblyInfo);
        string testAssemblyInfoFile = fileSystem.File.ReadAllText(testAssemblyInfo);
        
        businessPackageVersion.Should().Be(version);
        testPackageVersion!.Should().Be(packageTest.Value);
        businessAssemblyInfoFile.Should().ContainAll($"[assembly: AssemblyVersion(\"{version}.0\")]", $"[assembly: AssemblyFileVersion(\"{version}.0\")]");
        testAssemblyInfoFile.Should().ContainAll($"[assembly: AssemblyVersion(\"1.0.0.0\")]", $"[assembly: AssemblyFileVersion(\"1.0.0.0\")]");
    }
    
        [Theory]
    [InlineData("1.1.0", ".")]
    [InlineData("1.1.0", "Cmf.Custom.Tests")]
    public void Bump_TestsFromDifferentPaths(string version, string runPath)
    {
        // files
        KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.0.0");
        KeyValuePair<string, string> packageBusiness = new("Cmf.Custom.Business", "1.0.0");
        KeyValuePair<string, string> packageTest = new("Cmf.Custom.Tests", "1.0.0");        
        string businessCmfPackageJson = $"Cmf.Custom.Business/{CliConstants.CmfPackageFileName}";
        string businessAssemblyInfo = "Cmf.Custom.Business/Cmf.Custom.Common/Properties/AssemblyInfo.cs";
        string testAssemblyInfo = "Cmf.Custom.Tests/Cmf.Custom.E2ETests/Properties/AssemblyInfo.cs";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            // project config file
            { ".project-config.json", new MockFileData("") },

            // root cmfpackage file
            {
                $"cmfpackage.json", new MockFileData(
                    @$"{{
                ""packageId"": ""{packageRoot.Key}"",
                ""version"": ""{packageRoot.Value}"",
                ""description"": ""This package deploys Critical Manufacturing Customization"",
                ""packageType"": ""Root"",
                ""isInstallable"": true,
                ""isUniqueInstall"": false
            }}")
            }
        });

        // business cmfpackage file
        fileSystem.AddFile("Cmf.Custom.Business/cmfpackage.json", new MockFileData(
            @$"{{
              ""packageId"": ""{packageBusiness.Key}"",
              ""version"": ""{packageBusiness.Value}"",
              ""description"": ""Cmf Custom Business Package"",
              ""packageType"": ""Business"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
                {{
                  ""source"": ""Release/*.dll"",
                  ""target"": """",
                  ""ignoreFiles"": [
                    "".cmfpackageignore""
                  ]
                }}
              ]
            }}"));

        // business sln
        fileSystem.AddFile("Cmf.Custom.Business/Business.sln", new MockFileData(
            @$"Microsoft Visual Studio Solution File, Format Version 12.00
                # Visual Studio Version 17
                VisualStudioVersion = 17.3.32825.248
                MinimumVisualStudioVersion = 10.0.40219.1
                Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""Cmf.Custom.Common"", ""Cmf.Custom.Common\Cmf.Custom.Common.csproj"", ""{{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}""
                EndProject
                Global
	                GlobalSection(SolutionConfigurationPlatforms) = preSolution
		                Debug|Any CPU = Debug|Any CPU
		                Release|Any CPU = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(ProjectConfigurationPlatforms) = postSolution
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.Build.0 = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.Build.0 = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(SolutionProperties) = preSolution
		                HideSolutionNode = FALSE
	                EndGlobalSection
	                GlobalSection(ExtensibilityGlobals) = postSolution
		                SolutionGuid = {{B6998FE2-5739-49CF-939B-73DB4B5DAF2E}}
	                EndGlobalSection
                EndGlobal"));

        // common csproj
        fileSystem.AddFile("Cmf.Custom.Business/Cmf.Custom.Common/Cmf.Custom.Common.csproj", new MockFileData(
            @$"<Project Sdk=""Microsoft.NET.Sdk"">
                      <PropertyGroup>
                        <TargetFramework>net6.0</TargetFramework>
                        <ImplicitUsings>enable</ImplicitUsings>
                        <Nullable>enable</Nullable>
                      </PropertyGroup>
                </Project>"));

        // class file
        fileSystem.AddFile("Cmf.Custom.Business/Cmf.Custom.Common/TestClass.cs", new MockFileData(
            @$"namespace Cmf.Custom.Actions
                {{
                    public class Class1
                    {{
                    }}
                }}"));

        // assembly info file
        fileSystem.AddFile("Cmf.Custom.Business/Cmf.Custom.Common/Properties/AssemblyInfo.cs", new MockFileData(
            @$"using System.Reflection;
                         using System.Runtime.CompilerServices;
                         using System.Runtime.InteropServices;

                         [assembly: AssemblyTitle(""Cmf.Custom.Tests.Biz"")]
                         [assembly: AssemblyDescription("""")]
                         [assembly: AssemblyConfiguration("""")]
                         [assembly: AssemblyCompany("""")]
                         [assembly: AssemblyProduct(""Cmf.Custom.Tests.Biz"")]
                         [assembly: AssemblyCopyright(""Copyright ©  2020"")]
                         [assembly: AssemblyTrademark("""")]
                         [assembly: AssemblyCulture("""")]

                         [assembly: ComVisible(false)]

                         [assembly: Guid(""756fb0df-23db-4581-a056-9fff0e36e993"")]

                         // [assembly: AssemblyVersion(""1.0.*"")]
                         [assembly: AssemblyVersion(""1.0.0.0"")]
                         [assembly: AssemblyFileVersion(""1.0.0.0"")]"));

        // tests cmfpackage file
        fileSystem.AddFile("Cmf.Custom.Tests/cmfpackage.json", new MockFileData(
            @$"{{
              ""packageId"": ""{packageTest.Key}"",
              ""version"": ""{packageTest.Value}"",
              ""description"": ""Cmf Custom Test Package"",
              ""packageType"": ""Tests"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
                {{
                  ""source"": ""Release/*.dll"",
                  ""target"": """",
                  ""ignoreFiles"": [
                    "".cmfpackageignore""
                  ]
                }}
              ]
            }}"));

        // test sln
        fileSystem.AddFile("Cmf.Custom.Tests/Tests.sln", new MockFileData(
            @$"Microsoft Visual Studio Solution File, Format Version 12.00
                # Visual Studio Version 17
                VisualStudioVersion = 17.3.32825.248
                MinimumVisualStudioVersion = 10.0.40219.1
                Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""Cmf.Custom.E2ETests"", ""Cmf.Custom.Tests\Cmf.Custom.E2ETests.csproj"", ""{{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}""
                EndProject
                Global
	                GlobalSection(SolutionConfigurationPlatforms) = preSolution
		                Debug|Any CPU = Debug|Any CPU
		                Release|Any CPU = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(ProjectConfigurationPlatforms) = postSolution
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{6C3D698D-7F9E-4E39-A571-DEB63582CA52}}.Release|Any CPU.Build.0 = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		                {{22BF79FE-7D2A-4F61-A115-DAB07DE05686}}.Release|Any CPU.Build.0 = Release|Any CPU
	                EndGlobalSection
	                GlobalSection(SolutionProperties) = preSolution
		                HideSolutionNode = FALSE
	                EndGlobalSection
	                GlobalSection(ExtensibilityGlobals) = postSolution
		                SolutionGuid = {{B6998FE2-5739-49CF-939B-73DB4B5DAF2E}}
	                EndGlobalSection
                EndGlobal"));

        // common csproj
        fileSystem.AddFile("Cmf.Custom.Tests/Cmf.Custom.E2ETests/Cmf.Custom.E2ETests.csproj", new MockFileData(
            @$"<Project Sdk=""Microsoft.NET.Sdk"">
                      <PropertyGroup>
                        <TargetFramework>net6.0</TargetFramework>
                        <ImplicitUsings>enable</ImplicitUsings>
                        <Nullable>enable</Nullable>
                      </PropertyGroup>
                </Project>"));

        // class file
        fileSystem.AddFile("Cmf.Custom.Tests/Cmf.Custom.E2ETests/TestClass.cs", new MockFileData(
            @$"namespace Cmf.Custom.Actions
                {{
                    public class Class1
                    {{
                    }}
                }}"));

        // assembly info file
        fileSystem.AddFile("Cmf.Custom.Tests/Cmf.Custom.E2ETests/Properties/AssemblyInfo.cs", new MockFileData(
            @$"using System.Reflection;
                         using System.Runtime.CompilerServices;
                         using System.Runtime.InteropServices;

                         [assembly: AssemblyTitle(""Cmf.Custom.Tests.Biz"")]
                         [assembly: AssemblyDescription("""")]
                         [assembly: AssemblyConfiguration("""")]
                         [assembly: AssemblyCompany("""")]
                         [assembly: AssemblyProduct(""Cmf.Custom.Tests.Biz"")]
                         [assembly: AssemblyCopyright(""Copyright ©  2020"")]
                         [assembly: AssemblyTrademark("""")]
                         [assembly: AssemblyCulture("""")]

                         [assembly: ComVisible(false)]

                         [assembly: Guid(""756fb0df-23db-4581-a056-9fff0e36e993"")]

                         // [assembly: AssemblyVersion(""1.0.*"")]
                         [assembly: AssemblyVersion(""1.0.0.0"")]
                         [assembly: AssemblyFileVersion(""1.0.0.0"")]"));

        
        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton<IProjectConfigService>(new ProjectConfigService())
            .BuildServiceProvider();

        ExecutionContext.Initialize(fileSystem);

        IFileInfo cmfPackageFile = fileSystem.FileInfo.New($"Cmf.Custom.Tests/{CliConstants.CmfPackageFileName}");

        TestPackageTypeHandler packageTypeHandler =
            PackageTypeFactory.GetPackageTypeHandler(cmfPackageFile) as TestPackageTypeHandler;

        fileSystem.Directory.SetCurrentDirectory(runPath);
        
        packageTypeHandler!.Bump(version, "");

        fileSystem.Directory.SetCurrentDirectory("..");
        string testPackageVersion = packageTypeHandler.CmfPackage.Version;
        dynamic businessPackageFile = JsonConvert.DeserializeObject(fileSystem.File.ReadAllText(businessCmfPackageJson));
        string businessPackageVersion = businessPackageFile!.version;
        string testAssemblyInfoFile = fileSystem.File.ReadAllText(testAssemblyInfo);
        string businessAssemblyInfoFile = fileSystem.File.ReadAllText(businessAssemblyInfo);
        
        testPackageVersion!.Should().Be(version);
        businessPackageVersion.Should().Be(packageBusiness.Value);
        testAssemblyInfoFile.Should().ContainAll($"[assembly: AssemblyVersion(\"{version}.0\")]", $"[assembly: AssemblyFileVersion(\"{version}.0\")]");
        businessAssemblyInfoFile.Should().ContainAll($"[assembly: AssemblyVersion(\"1.0.0.0\")]", $"[assembly: AssemblyFileVersion(\"1.0.0.0\")]");
    }
}
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests.Objects
{
    internal static class MockPackage
    {
        internal static readonly MockFileSystem Grafana = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path(@"c:\.project-config.json"), new MockFileData(
                @"{
                    ""ProjectName"": ""MockGrafana"",
                    ""Tenant"": ""MockTenant"",
                    ""MESVersion"": ""10.2.1"",
                    ""NGXSchematicsVersion"": ""1.3.3"",
                    ""RepositoryType"": ""App"",
                }")
            },
            { MockUnixSupport.Path(@"c:\grafana\1.1.0\dashboards\dashboards.yaml"), new MockFileData(
                @"{
                apiVersion: 1
                providers:
                  - name: Default   
                    folder: CoolApp
                    type: file
                    options:
                      path:  /etc/grafana/provisioning/dashboards
                      foldersFromFilesStructure: true
                }")},
            { MockUnixSupport.Path(@"c:\grafana\1.1.0\datasources\datasources.yaml"), new MockFileData(
                @"{
                apiVersion: 1
                datasources:
                  ~
                }")},
            { MockUnixSupport.Path(@"c:\grafana\cmfpackage.json"), new MockFileData(
            $@"{{
                ""packageId"": ""Cmf.Custom.Grafana"",
                ""version"": ""1.1.0"",
                ""description"": ""Cmf Custom Grafana Package"",
                ""packageType"": ""Generic"",
                ""targetLayer"": ""grafana"",
                ""isInstallable"": true,
                ""isUniqueInstall"": true,
	            ""steps"": [
		            {{
			            ""order"": ""1"",
			            ""type"": ""DeployFiles"",
			            ""ContentPath"": ""**/**""
		            }}
	            ],
                ""buildsteps"": [],
                ""contentToPack"": [
                {{
                    ""source"": ""{MockUnixSupport.Path("1.1.0\\*").Replace("\\", "\\\\")}"",
                    ""target"": """"
                }}
              ]
            }}")}});

        internal static readonly MockFileSystem Html = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path(@"c:\.project-config.json"), new MockFileData(
            @"{
              ""MESVersion"": ""9.0.0""
            }")
            },
            { MockUnixSupport.Path(@"c:\ui\src\packages\customization.common\package.json"), new MockFileData(
            @"{
              ""name"": ""customization.package""
            }")},
            { MockUnixSupport.Path(@"c:\ui\src\packages\customization.common\customization.common.js"), new MockFileData(string.Empty)},
            { MockUnixSupport.Path(@"c:\ui\package.json"), new MockFileData(
            @"{
              ""name"": ""customization.package""
            }")},
            { MockUnixSupport.Path(@"c:\ui\cmfpackage.json"), new MockFileData(
            $@"{{
              ""packageId"": ""Cmf.Custom.HTML"",
              ""version"": ""1.1.0"",
              ""description"": ""Cmf Custom HTML Package"",
              ""packageType"": ""Html"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
                {{
                  ""source"": ""{MockUnixSupport.Path("src\\packages\\*").Replace("\\", "\\\\")}"",
                  ""target"": ""node_modules"",
                  ""ignoreFiles"": [
                    "".npmignore""
                  ]
                }}
              ]
            }}")}});

        internal static readonly MockFileSystem Html_OnlyLBOs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
          { MockUnixSupport.Path(@"c:\Libs\LBOs\TypeScript\APIReference.js"), new MockFileData(@"dummy")},
          { MockUnixSupport.Path(@"c:\Libs\LBOs\TypeScript\cmf.lbos.js"), new MockFileData(@"dummy")},
          { MockUnixSupport.Path(@"c:\Libs\LBOs\TypeScript\package.json"), new MockFileData(@"{
              ""name"": ""cmf.lbos""
            }")},
          { MockUnixSupport.Path(@"c:\.project-config.json"), new MockFileData(
            @"{
              ""MESVersion"": ""9.0.0""
            }")
          },
          { MockUnixSupport.Path(@"c:\ui\package.json"), new MockFileData(
            @"{
              ""name"": ""customization.package""
            }")},
          { MockUnixSupport.Path(@"c:\ui\cmfpackage.json"), new MockFileData(
            $@"{{
              ""packageId"": ""Cmf.Custom.HTML"",
              ""version"": ""1.1.0"",
              ""description"": ""Cmf Custom HTML Package"",
              ""packageType"": ""Html"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
                {{
                  ""source"": ""{MockUnixSupport.Path("src\\packages\\*").Replace("\\", "\\\\")}"",
                  ""target"": ""node_modules"",
                  ""ignoreFiles"": [
                    "".npmignore""
                  ]
                }},
                {{
                  ""source"": ""{MockUnixSupport.Path(@"..\Libs\LBOs\TypeScript\APIReference*").Replace("\\", "\\\\")}"",
                  ""target"": ""node_modules/cmf.lbos""
                }},
                {{
                  ""source"": ""{MockUnixSupport.Path(@"..\Libs\LBOs\TypeScript\cmf.lbos.*").Replace("\\", "\\\\")}"",
                  ""target"": ""node_modules/cmf.lbos""
                }},
                {{
                  ""source"": ""{MockUnixSupport.Path(@"..\Libs\LBOs\TypeScript\package.json").Replace("\\", "\\\\")}"",
                  ""target"": ""node_modules/cmf.lbos""
                }}
              ]
            }}")}});

        internal static readonly MockFileSystem Html_MissingDeclaredContent = new MockFileSystem(new Dictionary<string, MockFileData>
        {
          { MockUnixSupport.Path(@"c:\.project-config.json"), new MockFileData(
            @"{
              ""MESVersion"": ""9.0.0""
            }")
          },
          { MockUnixSupport.Path(@"c:\ui\package.json"), new MockFileData(
            @"{
              ""name"": ""customization.package""
            }")
          },
          { MockUnixSupport.Path(@"c:\ui\cmfpackage.json"), new MockFileData(
            $@"{{
              ""packageId"": ""Cmf.Custom.HTML"",
              ""version"": ""1.1.0"",
              ""description"": ""Cmf Custom HTML Package"",
              ""packageType"": ""Html"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
                {{
                  ""source"": ""{MockUnixSupport.Path("src\\packages\\*").Replace("\\", "\\\\")}"",
                  ""target"": ""node_modules"",
                  ""ignoreFiles"": [
                    "".npmignore""
                  ]
                }}
              ]
           }}")
          }
        });

        internal static readonly MockFileSystem Html_EmptyContentToPack = new MockFileSystem(new Dictionary<string, MockFileData>
        {
          { MockUnixSupport.Path(@"c:\ui\package.json"), new MockFileData(
            @"{
              ""name"": ""customization.package""
            }")
          },
          { MockUnixSupport.Path(@"c:\ui\cmfpackage.json"), new MockFileData(
            $@"{{
              ""packageId"": ""Cmf.Custom.HTML"",
              ""version"": ""1.1.0"",
              ""description"": ""Cmf Custom HTML Package"",
              ""packageType"": ""Html"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
              ]
           }}")
          }
        });

        internal static readonly MockFileSystem Root_Empty = new MockFileSystem(new Dictionary<string, MockFileData>
        {
         { MockUnixSupport.Path(@"c:\repo\cmfpackage.json"), new MockFileData(
            $@"{{
              ""packageId"": ""Cmf.Custom.Root"",
              ""version"": ""1.1.0"",
              ""description"": ""Cmf Custom Root Package"",
              ""packageType"": ""Root"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""dependencies"": [
                {{ ""id"": ""Cmf.Environment"", ""version"": ""0.0.0"" }}
              ]
           }}")
          }
        });

        internal static readonly MockFileSystem ProductDatabase_Empty = new MockFileSystem(new Dictionary<string, MockFileData>
        {
         { MockUnixSupport.Path(@"c:\repo\cmfpackage.json"), new MockFileData(
            $@"{{
              ""packageId"": ""Cmf.Custom.ProductDatabase"",
              ""version"": ""1.1.0"",
              ""description"": ""Cmf Custom ProductDatabase Package"",
              ""packageType"": ""ProductDatabase"",
              ""isInstallable"": true,
              ""isUniqueInstall"": false,
              ""contentToPack"": [
                {{
                  ""source"": ""{MockUnixSupport.Path("src\\packages\\*").Replace("\\", "\\\\")}"",
                  ""target"": ""test""
                }}
              ]
           }}")
          }
        });
    }
}

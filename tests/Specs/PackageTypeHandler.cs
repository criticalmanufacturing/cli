using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Handlers;
using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using tests.Objects;
using Xunit;
using System.Collections.Generic;
using FluentAssertions;

namespace tests.Specs
{
    public class PackageTypeHandler
    {
        public PackageTypeHandler()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<IProjectConfigService>(new ProjectConfigService())
                .BuildServiceProvider();
        }
        
        [Fact]
        public void GetContentToPack_WithNonExistentIgnoreFiles()
        {
            var fileSystem = MockPackage.Html;

            ExecutionContext.Initialize(fileSystem);

            fileSystem.Directory.SetCurrentDirectory(MockUnixSupport.Path("c:\\ui"));

            StringWriter standardOutput = (new Logging()).GetLogStringWriter();

            string exceptionMessage = string.Empty;
            try
            {
                var cmfPackage = fileSystem.FileInfo.New(CliConstants.CmfPackageFileName);
                var packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage) as PresentationPackageTypeHandler;

                packageTypeHandler.GetContentToPack(fileSystem.DirectoryInfo.New("output"));
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }

            Assert.Equal(string.Empty, exceptionMessage);
            Assert.Contains($"{MockUnixSupport.Path(@"c:\ui\src\packages\customization.common\")}.npmignore not found!", standardOutput.ToString().Trim());
        }

        /// <summary>
        /// NotImplementedPackageTypeHandler
        /// Checks whether it throws a controlled exception when it does not have the desired Package Type Handler implemented.
        /// </summary>
        [Fact]
        public void NotImplementedPackageTypeHandler()
        {
            var fileSystem = MockPackage.ProductDatabase_Empty;

            ExecutionContext.Initialize(fileSystem);

            fileSystem.Directory.SetCurrentDirectory(MockUnixSupport.Path("c:\\repo"));

            string exceptionMessage = string.Empty;
            try
            {
                var cmfPackage = fileSystem.FileInfo.New(CliConstants.CmfPackageFileName);
                var packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage);
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }

            Assert.False(string.IsNullOrWhiteSpace(exceptionMessage));
            Assert.Equal(string.Format(CoreMessages.PackageTypeHandlerNotImplemented, "ProductDatabase"), exceptionMessage);
        }

        [Fact]
        public void GetContentToPack_OrderCheck()
        {
            KeyValuePair<string, string> packageRoot = new("Cmf.Custom.Package", "1.1.0");
            KeyValuePair<string, string> packageData = new("Cmf.Custom.Data", "1.1.0");


            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageRoot.Key}"",
                  ""version"": ""{packageRoot.Value}"",
                  ""packageType"": ""Root"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": false,
                  ""dependencies"": [
                    {{
                         ""id"": ""{packageData.Key}"",
                        ""version"": ""{packageData.Value}""
                    }}
                  ]
                }}")},
                { "/repo/Cmf.Custom.Data/cmfpackage.json", new MockFileData(
                @$"{{
                  ""packageId"": ""{packageData.Key}"",
                  ""version"": ""{packageData.Value}"",
                  ""packageType"": ""Data"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {{
                        ""source"": ""{MockUnixSupport.Path("folder\\*").Replace("\\", "\\\\")}"",
                        ""target"": """"
                    }}
                  ]
                }}")}
            });

            int numberOfFiles = 12;
            for (int i = 0; i < numberOfFiles; i++)
            {
                fileSystem.AddFile($"/repo/Cmf.Custom.Data/folder/file_{i:000}.txt", new MockFileData($"file-content_{i:000}"));
            }

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = fileSystem.FileInfo.New("/repo/Cmf.Custom.Data/cmfpackage.json");
            var packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage) as DataPackageTypeHandlerV2;

            var contentToPack = packageTypeHandler.GetContentToPack(fileSystem.DirectoryInfo.New("output"));


            for(int i = 0;i< numberOfFiles; i++)
            {
                contentToPack[i].Source.FullName.Should().EndWith($"file_{i:000}.txt");
            }

        }

        [Fact]
        public void DatabasePackageTypeHandler_OnlyOnlineTarget_AddsOnlyOnlineStep()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @"{
                  ""packageId"": ""Cmf.Custom.Database"",
                  ""version"": ""1.0.0"",
                  ""packageType"": ""Database"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {
                      ""source"": ""ONLINE/UpgradeScripts/$(version)/*"",
                      ""target"": ""Online/""
                    }
                  ],
                  ""steps"": []
                }")}
            });

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
            
            // Act
            var handler = new DatabasePackageTypeHandler(cmfPackage);
            
            // Assert
            cmfPackage.Steps.Should().NotBeNull();
            cmfPackage.Steps.Should().HaveCount(1);
            cmfPackage.Steps[0].ContentPath.Should().Be("Online/*.sql");
            cmfPackage.Steps[0].TargetDatabase.Should().Be("$(Product.Database.Online)");
            cmfPackage.Steps[0].Type.Should().Be(Core.Enums.StepType.RunSql);
        }

        [Fact]
        public void DatabasePackageTypeHandler_OnlyODSTarget_AddsOnlyODSStep()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @"{
                  ""packageId"": ""Cmf.Custom.Database"",
                  ""version"": ""1.0.0"",
                  ""packageType"": ""Database"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {
                      ""source"": ""ODS/UpgradeScripts/$(version)/*"",
                      ""target"": ""ODS/""
                    }
                  ],
                  ""steps"": []
                }")}
            });

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
            
            // Act
            var handler = new DatabasePackageTypeHandler(cmfPackage);
            
            // Assert
            cmfPackage.Steps.Should().NotBeNull();
            cmfPackage.Steps.Should().HaveCount(1);
            cmfPackage.Steps[0].ContentPath.Should().Be("ODS/*.sql");
            cmfPackage.Steps[0].TargetDatabase.Should().Be("$(Product.Database.Ods)");
            cmfPackage.Steps[0].Type.Should().Be(Core.Enums.StepType.EnqueueSql);
        }

        [Fact]
        public void DatabasePackageTypeHandler_OnlyDWHTarget_AddsOnlyDWHStep()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @"{
                  ""packageId"": ""Cmf.Custom.Database"",
                  ""version"": ""1.0.0"",
                  ""packageType"": ""Database"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {
                      ""source"": ""DWH/UpgradeScripts/$(version)/*"",
                      ""target"": ""DWH/""
                    }
                  ],
                  ""steps"": []
                }")}
            });

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
            
            // Act
            var handler = new DatabasePackageTypeHandler(cmfPackage);
            
            // Assert
            cmfPackage.Steps.Should().NotBeNull();
            cmfPackage.Steps.Should().HaveCount(1);
            cmfPackage.Steps[0].ContentPath.Should().Be("DWH/*.sql");
            cmfPackage.Steps[0].TargetDatabase.Should().Be("$(Product.Database.Dwh)");
            cmfPackage.Steps[0].Type.Should().Be(Core.Enums.StepType.EnqueueSql);
        }

        [Fact]
        public void DatabasePackageTypeHandler_AllThreeTargets_AddsAllSteps()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @"{
                  ""packageId"": ""Cmf.Custom.Database"",
                  ""version"": ""1.0.0"",
                  ""packageType"": ""Database"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {
                      ""source"": ""ONLINE/UpgradeScripts/$(version)/*"",
                      ""target"": ""Online/""
                    },
                    {
                      ""source"": ""ODS/UpgradeScripts/$(version)/*"",
                      ""target"": ""ODS/""
                    },
                    {
                      ""source"": ""DWH/UpgradeScripts/$(version)/*"",
                      ""target"": ""DWH/""
                    }
                  ],
                  ""steps"": []
                }")}
            });

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
            
            // Act
            var handler = new DatabasePackageTypeHandler(cmfPackage);
            
            // Assert
            cmfPackage.Steps.Should().NotBeNull();
            cmfPackage.Steps.Should().HaveCount(3);
            
            // Verify Online step
            cmfPackage.Steps.Should().Contain(s => 
                s.ContentPath == "Online/*.sql" && 
                s.TargetDatabase == "$(Product.Database.Online)" &&
                s.Type == Core.Enums.StepType.RunSql);
            
            // Verify ODS step
            cmfPackage.Steps.Should().Contain(s => 
                s.ContentPath == "ODS/*.sql" && 
                s.TargetDatabase == "$(Product.Database.Ods)" &&
                s.Type == Core.Enums.StepType.EnqueueSql);
            
            // Verify DWH step
            cmfPackage.Steps.Should().Contain(s => 
                s.ContentPath == "DWH/*.sql" && 
                s.TargetDatabase == "$(Product.Database.Dwh)" &&
                s.Type == Core.Enums.StepType.EnqueueSql);
        }

        [Fact]
        public void DatabasePackageTypeHandler_EmptyContentToPack_AddsNoSteps()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @"{
                  ""packageId"": ""Cmf.Custom.Database"",
                  ""version"": ""1.0.0"",
                  ""packageType"": ""Database"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [],
                  ""steps"": []
                }")}
            });

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
            
            // Act
            var handler = new DatabasePackageTypeHandler(cmfPackage);
            
            // Assert
            cmfPackage.Steps.Should().NotBeNull();
            cmfPackage.Steps.Should().BeEmpty();
        }

        [Fact]
        public void DatabasePackageTypeHandler_TargetWithoutTrailingSlash_AddsCorrectStep()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @"{
                  ""packageId"": ""Cmf.Custom.Database"",
                  ""version"": ""1.0.0"",
                  ""packageType"": ""Database"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {
                      ""source"": ""ONLINE/UpgradeScripts/$(version)/*"",
                      ""target"": ""Online""
                    }
                  ],
                  ""steps"": []
                }")}
            });

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
            
            // Act
            var handler = new DatabasePackageTypeHandler(cmfPackage);
            
            // Assert
            cmfPackage.Steps.Should().NotBeNull();
            cmfPackage.Steps.Should().HaveCount(1);
            cmfPackage.Steps[0].ContentPath.Should().Be("Online/*.sql");
        }

        [Fact]
        public void DatabasePackageTypeHandler_FalsePositiveTargets_DoesNotMatch()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @"{
                  ""packageId"": ""Cmf.Custom.Database"",
                  ""version"": ""1.0.0"",
                  ""packageType"": ""Database"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {
                      ""source"": ""OnlineBackup/*"",
                      ""target"": ""OnlineBackup/""
                    },
                    {
                      ""source"": ""ODSArchive/*"",
                      ""target"": ""ODSArchive/""
                    },
                    {
                      ""source"": ""DWHTemp/*"",
                      ""target"": ""DWHTemp/""
                    }
                  ],
                  ""steps"": []
                }")}
            });

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
            
            // Act
            var handler = new DatabasePackageTypeHandler(cmfPackage);
            
            // Assert
            cmfPackage.Steps.Should().NotBeNull();
            cmfPackage.Steps.Should().BeEmpty();
        }

        [Fact]
        public void DatabasePackageTypeHandler_CaseInsensitiveMatching_AddsCorrectSteps()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @"{
                  ""packageId"": ""Cmf.Custom.Database"",
                  ""version"": ""1.0.0"",
                  ""packageType"": ""Database"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {
                      ""source"": ""ONLINE/UpgradeScripts/$(version)/*"",
                      ""target"": ""online/""
                    },
                    {
                      ""source"": ""ODS/UpgradeScripts/$(version)/*"",
                      ""target"": ""ods/""
                    },
                    {
                      ""source"": ""DWH/UpgradeScripts/$(version)/*"",
                      ""target"": ""dwh/""
                    }
                  ],
                  ""steps"": []
                }")}
            });

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
            
            // Act
            var handler = new DatabasePackageTypeHandler(cmfPackage);
            
            // Assert
            cmfPackage.Steps.Should().NotBeNull();
            cmfPackage.Steps.Should().HaveCount(3);
        }

        [Fact]
        public void DatabasePackageTypeHandler_MixedValidAndInvalidTargets_AddsOnlyValidSteps()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfpackage.json", new MockFileData(
                @"{
                  ""packageId"": ""Cmf.Custom.Database"",
                  ""version"": ""1.0.0"",
                  ""packageType"": ""Database"",
                  ""isInstallable"": true,
                  ""isUniqueInstall"": true,
                  ""contentToPack"": [
                    {
                      ""source"": ""ONLINE/UpgradeScripts/$(version)/*"",
                      ""target"": ""Online/""
                    },
                    {
                      ""source"": ""OnlineBackup/*"",
                      ""target"": ""OnlineBackup/""
                    },
                    {
                      ""source"": ""OtherFolder/*"",
                      ""target"": ""OtherFolder/""
                    }
                  ],
                  ""steps"": []
                }")}
            });

            ExecutionContext.Initialize(fileSystem);
            var cmfPackage = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"), setDefaultValues: true);
            
            // Act
            var handler = new DatabasePackageTypeHandler(cmfPackage);
            
            // Assert
            cmfPackage.Steps.Should().NotBeNull();
            cmfPackage.Steps.Should().HaveCount(1);
            cmfPackage.Steps[0].ContentPath.Should().Be("Online/*.sql");
        }
    }
}

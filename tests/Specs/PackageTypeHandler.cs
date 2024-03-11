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
    }
}

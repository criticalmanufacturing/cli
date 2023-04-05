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
    }
}

using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Factories;
using Cmf.Common.Cli.Handlers;
using Cmf.Common.Cli.Objects;
using Xunit;
using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using tests.Objects;

namespace tests.Specs
{
    public class PackageTypeHandler
    {
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
                var cmfPackage = fileSystem.FileInfo.FromFileName(CliConstants.CmfPackageFileName);
                var packageTypeHandler = PackageTypeFactory.GetPackageTypeHandler(cmfPackage) as PresentationPackageTypeHandler;

                packageTypeHandler.GetContentToPack(fileSystem.DirectoryInfo.FromDirectoryName("output"));
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }

            Assert.Equal(string.Empty, exceptionMessage);
            Assert.Contains($"{ MockUnixSupport.Path(@"c:\ui\src\packages\customization.common\") }.npmignore not found!", standardOutput.ToString().Trim());
        }
    }
}

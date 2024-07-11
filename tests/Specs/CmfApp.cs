using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Objects.CmfApp;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests.Specs
{
    public class CmfApp
    {
        [Fact]
        public void CmfApp_Manifest()
        {
            var appId = "App Id";
            var version = "App version";
            var appName = "App name";
            var author = "App author";
            var description = "App description";
            var targetFramework = "10.0.0";
            var licensedApplication = "Test application";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/repo/cmfapp.json", new MockFileData(
                    @$"{{
                        ""id"": ""{appId}"",
                        ""name"": ""{appName}"",
                        ""author"": ""{author}"",
                        ""description"": ""{description}"",
                        ""licensedApplication"": ""{licensedApplication}"",
                        ""icon"": """"
                }}")},
                { ".project-config.json", new MockFileData(
                    @$"{{
                        ""MESVersion"": ""{targetFramework}""
                    }}")
                },
            });

            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<IProjectConfigService>(new ProjectConfigService())
                .BuildServiceProvider();

            ExecutionContext.Initialize(fileSystem);
            IFileInfo cmfappFile = fileSystem.FileInfo.New($"repo/{CliConstants.CmfAppFileName}");

            string message = string.Empty;
            Cmf.CLI.Core.Objects.CmfApp.CmfApp cmfAppDataObject = null;
            try
            {
                // Reading cmfapp
                cmfAppDataObject = Cmf.CLI.Core.Objects.CmfApp.CmfApp.Load(cmfappFile, null, version);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            Assert.Equal(string.Empty, message);
            Assert.NotNull(cmfAppDataObject);

            CmfAppV1 app = cmfAppDataObject.App;

            Assert.Equal(app.Id, appId);
            Assert.Equal(app.Version, version);
            Assert.Equal(app.Name, appName);
            Assert.Equal(app.Author, author);
            Assert.Equal(app.Description, description);
            Assert.Equal(app.Framework.Version, $"^{targetFramework}");
            Assert.Equal(app.LicensedApplication.Name, licensedApplication);
        }
    }
}

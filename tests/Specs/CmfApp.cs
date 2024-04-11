using Cmf.CLI.Constants;
using Cmf.CLI.Core.Objects;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;
using Xunit;
using Cmf.CLI.Core.Objects.CmfApp;
using System.IO;
using System.Diagnostics;
using Cmf.CLI.Utilities;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Cmf.Common.Cli.TestUtilities;

namespace tests.Specs
{
    public class CmfApp
    {
        [Fact]
        public void CmfApp_Manifest()
        {
            var appId = "App Id";
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
                  ""targetFramework"": ""{targetFramework}"",
                  ""licensedApplication"": ""{licensedApplication}"",
                  ""icon"": """"
                }}")}
            });

            ExecutionContext.Initialize(fileSystem);
            IFileInfo cmfappFile = fileSystem.FileInfo.New($"repo/{CliConstants.CmfAppFileName}");

            string message = string.Empty;
            Cmf.CLI.Core.Objects.CmfApp.CmfApp cmfAppDataObject = null;
            try
            {
                // Reading cmfapp
                cmfAppDataObject = Cmf.CLI.Core.Objects.CmfApp.CmfApp.Load(cmfappFile);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            Assert.Equal(string.Empty, message);
            Assert.NotNull(cmfAppDataObject);

            CmfAppV1 app = cmfAppDataObject.Content.App;

            Assert.Equal(app.Id, appId);
            Assert.Equal(app.Name, appName);
            Assert.Equal(app.Author, author);
            Assert.Equal(app.Description, description);
            Assert.Equal(app.Framework.Version, targetFramework);
            Assert.Equal(app.LicensedApplication.Name, licensedApplication);
        }
    }
}

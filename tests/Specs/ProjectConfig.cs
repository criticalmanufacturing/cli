using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Cmf.CLI.Core.Objects;
using FluentAssertions;
using Xunit;

namespace tests.Specs;

public class ProjectConfig
{
    const string projCfgTpl = @"{
          ""ProjectName"": ""ExampleProject"",
          ""NPMRegistry"": ""http://npmrepo/"",
          ""NuGetRegistry"": ""https://nuget-repo/"",
          ""AzureDevopsCollectionURL"": ""https://azuredevops.server/ExampleCollection"",
          ""AgentPool"": ""ExamplePool"",

          ""RepositoryURL"": ""https://azuredevops.server/ExampleCollection/ExampleProject/_git/ExampleRepo"",
          ""EnvironmentName"": ""ExampleIntegration"",
          ""DefaultDomain"": ""AD"",
          ""RESTPort"": ""443"",
          ""Tenant"": ""ExampleClient"",
          ""MESVersion"": ""9.0.5"",
          ""DevTasksVersion"": ""{DEV_TASKS_VERSION}"",
          ""HTMLStarterVersion"": ""8.0.0"",
          ""YoGeneratorVersion"": ""8.1.1"",
          ""NugetVersion"": ""9.0.5"",
          ""TestScenariosNugetVersion"": ""9.0.5"",
          ""IsSslEnabled"": ""True"",
          ""vmHostname"": ""exampleintegration.int.local"",
          ""DBReplica1"": ""SERV\\INSTANCE"",
          ""DBReplica2"": ""SERV\\INSTANCE"",
          ""DBServerOnline"": ""SERV\\INSTANCE"",
          ""DBServerODS"": ""SERV\\INSTANCE"",
          ""DBServerDWH"": ""SERV\\INSTANCE"",
          ""ReportServerURI"": ""http://serv/Reports"",
          ""AlwaysOn"": ""False"",
          ""InstallationPath"": """",
          ""DBBackupPath"": """",
          ""TemporaryPath"": """",
          ""HTMLPort"": ""443"",
          ""GatewayPort"": ""443"",
          ""ReleaseEnvironmentConfig"": ""ExampleIntegration_Development_parameters.json""
        }";
    
    [Theory]
    [InlineData("9.0.0")]
    [InlineData("9.0.0-5")]
    [InlineData("9.0.0-pre.5")]
    public void ClassicProjectConfig(string devTasksVersion)
    {
        var projConfig = projCfgTpl.Replace("{DEV_TASKS_VERSION}", devTasksVersion);
        
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/.project-config.json", new MockFileData(projConfig)}
        });

        var pc = (new ProjectConfigService()).Load(fileSystem);

        pc.Should().NotBeNull("project config was not loaded");
        pc.HTMLPort.Should().Be(443, "could not load HTML port");
        pc.AlwaysOn.Should().BeFalse("AlwaysOn should be false");
        pc.IsSslEnabled.Should().BeTrue("IsSslEnabled should be true");
    }
}
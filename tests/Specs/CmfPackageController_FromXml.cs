using System;
using System.Linq;
using System.Xml.Linq;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace tests.Specs;

public class CmfPackageController_FromXml
{
    [Fact]
    public void FromXml_ShouldParseStepMessageType_FromManifestAttribute()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="DeployFiles" contentPath="*.example" messageType="ImportObject" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().MessageType.Should().Be(MessageType.ImportObject);
    }

    [Fact]
    public void FromXml_ShouldKeepStepMessageTypeNull_WhenAttributeIsMissing()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="DeployFiles" contentPath="*.example" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().MessageType.Should().BeNull();
    }

    [Fact]
    public void FromXml_ShouldParseDeeBasePathAndImportXmlObjectPath_FromManifestAttributes()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="MasterData" contentPath="*.xlsx" deeBasePath="./dees" importXMLObjectPath="./xml" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().DeeBasePath.Should().Be("./dees");
        pkg.Steps.Single().ImportXMLObjectPath.Should().Be("./xml");
    }

    [Theory]
    [InlineData("DeployFiles", StepType.DeployFiles)]
    [InlineData("RunSql", StepType.RunSql)]
    [InlineData("MasterData", StepType.MasterData)]
    [InlineData("EnqueueXmla", StepType.EnqueueXmla)]
    [InlineData("DatabaseAccount", StepType.DatabaseAccount)]
    public void FromXml_ShouldParseStepType_WhenTypeAttributeIsKnown(string typeValue, StepType expectedType)
    {
        var xml = XDocument.Parse(
            $"""
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="{typeValue}" contentPath="*" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().Type.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("UnknownType")]
    [InlineData("")]
    [InlineData(null)]
    public void FromXml_ShouldFallbackToGenericStepType_WhenTypeAttributeIsUnknownOrMissing(string typeValue)
    {
        var typeAttr = typeValue != null ? $"type=\"{typeValue}\"" : "";
        var xml = XDocument.Parse(
            $"""
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step {typeAttr} contentPath="*" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().Type.Should().Be(StepType.Generic);
    }

    [Fact]
    public void FromXml_ShouldFallbackToGenericStepType_WhenTypeAttributeIsMissing()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step contentPath="*" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().Type.Should().Be(StepType.Generic);
    }

    [Fact]
    public void FromXml_ShouldParseIsToForceInstall_FromManifestElement()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <isInstallable>true</isInstallable>
              <isUniqueInstall>false</isUniqueInstall>
              <IsToForceInstall>true</IsToForceInstall>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.IsToForceInstall.Should().BeTrue();
    }

    [Fact]
    public void FromXml_ShouldParseForceRerunAfterDatabaseRestore_FromManifestElement()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <isInstallable>true</isInstallable>
              <isUniqueInstall>false</isUniqueInstall>
              <IsToForceInstall>true</IsToForceInstall>
              <forceRerunAfterDatabaseRestore>true</forceRerunAfterDatabaseRestore>
            </deploymentPackage>
            """);

      var pkg = CmfPackageController.FromXml(xml);

      pkg.ForceRerunAfterDatabaseRestore.Should().BeTrue();
    }

    [Fact]
    public void FromXml_ShouldParseFileAttribute_ForTransformFileStep()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="TransformFile" file="./config/app.config" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().File.Should().Be("./config/app.config");
    }

    [Fact]
    public void FromXml_ShouldParseContentAttribute_ForAutomationSyncStep()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="AutomationBusinessScenariosSync" content="./scenarios" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().Content.Should().Be("./scenarios");
    }

    [Fact]
    public void FromXml_ShouldParseUserKeyAndQuota_ForClickHouseAccountStep()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="ClickHouseAccount" userKey="myUser" quota="1000" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().UserKey.Should().Be("myUser");
        pkg.Steps.Single().Quota.Should().Be("1000");
    }

    [Fact]
    public void FromXml_ShouldParseIdAndMessageType_ForCreateIntegrationEntriesStep()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="CreateIntegrationEntries" id="entry1" messageType="ImportObject" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().Id.Should().Be("entry1");
        pkg.Steps.Single().MessageType.Should().Be(MessageType.ImportObject);
    }

    [Fact]
    public void FromXml_ShouldParseUseMachineNameAndAccount_ForDatabaseAccountStep()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="DatabaseAccount" useMachineName="true" account="sa" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().UseMachineName.Should().BeTrue();
        pkg.Steps.Single().Account.Should().Be("sa");
    }

    [Fact]
    public void FromXml_ShouldParseGenericStepHandlersAndPatchAttributes()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="Generic" id="step1" onInitialize="InitHandler" onPrepare="PrepHandler" scriptHandler="ScriptH" patchId="p001" replaceTokens="true" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        var step = pkg.Steps.Single();
        step.Id.Should().Be("step1");
        step.OnInitialize.Should().Be("InitHandler");
        step.OnPrepare.Should().Be("PrepHandler");
        step.ScriptHandler.Should().Be("ScriptH");
        step.PatchId.Should().Be("p001");
        step.ReplaceTokens.Should().BeTrue();
    }

    [Fact]
    public void FromXml_ShouldParseInstallIfDbDoesntExistAttributes()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="InstallIfDbDoesntExist" sourcePackages="pkg1,pkg2" targetDb="myDb" replaceTokens="false" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        var step = pkg.Steps.Single();
        step.SourcePackages.Should().Be("pkg1,pkg2");
        step.TargetDb.Should().Be("myDb");
        step.ReplaceTokens.Should().BeFalse();
    }

    [Fact]
    public void FromXml_ShouldParseCreateInCollection_ForMasterDataStep()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="MasterData" contentPath="*.xlsx" createInCollection="true" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().CreateInCollection.Should().BeTrue();
    }

    [Fact]
    public void FromXml_ShouldParseRunClickhouseSqlAttributes()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="RunClickhouseSql" conditionPath="./cond.sql" patchId="p001" patchDescription="init patch" replaceTokens="true" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        var step = pkg.Steps.Single();
        step.ConditionPath.Should().Be("./cond.sql");
        step.PatchId.Should().Be("p001");
        step.PatchDescription.Should().Be("init patch");
        step.ReplaceTokens.Should().BeTrue();
    }

    [Fact]
    public void FromXml_ShouldParseDatabaseType_ForRunSqlStep()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="RunSql" databaseType="Online" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().DatabaseType.Should().Be("Online");
    }

    [Fact]
    public void FromXml_ShouldParseConfigPathAndValue_ForUpdateConfigurationStep()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="UpdateConfiguration" configPath="./appsettings.json" value="newValue" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().ConfigPath.Should().Be("./appsettings.json");
        pkg.Steps.Single().Value.Should().Be("newValue");
    }

    [Fact]
    public void FromXml_ShouldDefaultForceRerunAfterDatabaseRestoreToFalse_WhenMissing()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.ForceRerunAfterDatabaseRestore.Should().BeFalse();
    }

    [Fact]
    public void FromXml_ShouldParseUpgradeStrategy_FromManifestElement()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <upgradeStrategy>CumulativeByMinor</upgradeStrategy>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.UpgradeStrategy.Should().Be("CumulativeByMinor");
    }

    [Fact]
    public void FromXml_ShouldParseManifestVersion_FromManifestElement()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <manifestVersion>1</manifestVersion>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.ManifestVersion.Should().Be(1);
    }

    [Fact]
    public void FromXml_ShouldDefaultManifestVersionToZero_WhenMissing()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.ManifestVersion.Should().Be(0);
    }

    [Fact]
    public void FromXml_ShouldParseMinSqlCompatibility_FromManifestElement()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <minSqlCompatibility>150</minSqlCompatibility>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.MinSqlCompatibility.Should().Be(150);
    }

    [Fact]
    public void FromXml_ShouldParseTargetLayerDirectory_FromManifestElement()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <targetLayerDirectory>some/layer/dir</targetLayerDirectory>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.TargetLayerDirectory.Should().Be("some/layer/dir");
    }

    [Fact]
    public void FromXml_ShouldParseBuildDate_FromManifestElement()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <buildDate>25/12/2024</buildDate>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.BuildDate.Should().Be(new DateTime(2024, 12, 25));
    }

    [Fact]
    public void FromXml_ShouldParseExtendedMetadata_FromSystemNameVersionAndCustomMetadata()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <systemName>MyApp</systemName>
              <systemVersion>11.1.0</systemVersion>
              <metadata>
                <customKey>customValue</customKey>
              </metadata>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.ExtendedMetadata.Should().ContainKey("ApplicationName");
        pkg.ExtendedMetadata["ApplicationName"].Should().Be("MyApp");
        pkg.ExtendedMetadata.Should().ContainKey("ApplicationVersion");
        pkg.ExtendedMetadata["ApplicationVersion"].Should().Be("11.1.0");
        pkg.ExtendedMetadata.Should().ContainKey("customKey");
        pkg.ExtendedMetadata["customKey"].Should().Be("customValue");
    }

    [Fact]
    public void FromXml_ShouldParsePackageDemands_WithAttributesOnly()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <packageDemands>
                <packageDemand type="SQLServer" version="14.0.1000.169" description="Microsoft SQL Server 2017"/>
              </packageDemands>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.PackageDemands.Should().ContainSingle();
        pkg.PackageDemands[0]["type"]!.Value<string>().Should().Be("SQLServer");
        pkg.PackageDemands[0]["version"]!.Value<string>().Should().Be("14.0.1000.169");
        pkg.PackageDemands[0]["description"]!.Value<string>().Should().Be("Microsoft SQL Server 2017");
    }

    [Fact]
    public void FromXml_ShouldParsePackageDemands_WithInnerChildElements()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <packageDemands>
                <packageDemand type="Generic">
                  <Option name="opt1" value="v1"/>
                  <Option name="opt2" value="v2"/>
                </packageDemand>
              </packageDemands>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.PackageDemands.Should().ContainSingle();
        pkg.PackageDemands[0]["type"]!.Value<string>().Should().Be("Generic");
        var options = pkg.PackageDemands[0]["Option"] as JArray;
        options.Should().HaveCount(2);
        options![0]["name"]!.Value<string>().Should().Be("opt1");
        options![1]["name"]!.Value<string>().Should().Be("opt2");
    }

    [Fact]
    public void FromXml_ShouldReturnEmptyPackageDemands_WhenNoDemandsDeclared()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.PackageDemands.Should().BeEmpty();
    }

    [Fact]
    public void FromXml_ShouldParseStepInnerElements_WithSingleRoleElement()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="DatabaseAccount" targetDatabase="$(Product.Database.Online)" useMachineName="false" account="ReadOnlyUser">
                  <Role name="db_datareader" />
                </step>
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        var step = pkg.Steps.Should().ContainSingle().Subject;
        step.Elements.Should().ContainSingle();
        step.Elements.Single().Name.LocalName.Should().Be("Role");
        step.Elements.Single().Attribute("name")?.Value.Should().Be("db_datareader");
    }

    [Fact]
    public void FromXml_ShouldParseStepInnerElements_WithMultipleObjectElements()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="GrantPermissions" targetDatabase="$(Product.Database.Online)" useMachineName="false" account="ReadOnlyUser">
                  <Object type="udtt" preset="all" />
                  <Object type="function" preset="all" />
                  <Object type="procedure" preset="readonly" />
                </step>
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        var step = pkg.Steps.Should().ContainSingle().Subject;
        step.Type.Should().Be(StepType.GrantPermissions);
        step.Elements.Should().HaveCount(3);
        step.Elements.Select(e => e.Attribute("type")?.Value).Should().BeEquivalentTo(new[] { "udtt", "function", "procedure" });
    }

    [Fact]
    public void FromXml_ShouldLeaveStepElementsNull_WhenStepHasNoInnerElements()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="DeployFiles" contentPath="*.example" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().Elements.Should().BeNull();
    }

    [Fact]
    public void ToJson_ShouldGroupStepInnerElements_ByElementName()
    {
        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="GrantPermissions" targetDatabase="$(Product.Database.Online)" useMachineName="false" account="ReadOnlyUser">
                  <Object type="udtt" preset="all" />
                  <Object type="function" preset="all" />
                </step>
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);
        var json = new CmfPackageController(pkg, null).ToJson();
        var jsonObject = JObject.Parse(json);

        var stepObject = (JObject)jsonObject["deployment"]["steps"][0];
        var objects = (JArray)stepObject["Object"];
        objects.Should().HaveCount(2);
        ((JObject)objects[0]["Object"])["type"]!.Value<string>().Should().Be("udtt");
        ((JObject)objects[1]["Object"])["type"]!.Value<string>().Should().Be("function");
    }

    [Fact]
    public void FromXml_ShouldThrowException_WhenStrictStepParsingEnabledAndStepHasUnknownAttribute()
    {
        Environment.SetEnvironmentVariable("cmf_cli_internal_strict_step_parsing", "1");
        try
        {
            var xml = XDocument.Parse(
                """
                <deploymentPackage>
                  <packageId>Cmf.Custom.Data</packageId>
                  <version>1.0.0</version>
                  <steps>
                    <step type="DeployFiles" contentPath="*.example" notARealAttribute="oops" />
                  </steps>
                </deploymentPackage>
                """);

            Action act = () => CmfPackageController.FromXml(xml);

            act.Should().Throw<CliException>().WithMessage("*CLI encountered unknown metadata*");
        }
        finally
        {
            Environment.SetEnvironmentVariable("cmf_cli_internal_strict_step_parsing", null);
        }
    }

    [Fact]
    public void FromXml_ShouldThrowException_WhenStrictStepParsingEnabledAndStepTypeIsInvalid()
    {
        Environment.SetEnvironmentVariable("cmf_cli_internal_strict_step_parsing", "1");
        try
        {
            var xml = XDocument.Parse(
                """
                <deploymentPackage>
                  <packageId>Cmf.Custom.Data</packageId>
                  <version>1.0.0</version>
                  <steps>
                    <step type="NotARealStepType" contentPath="*.example" />
                  </steps>
                </deploymentPackage>
                """);

            Action act = () => CmfPackageController.FromXml(xml);

            act.Should().Throw<CliException>().WithMessage("*CLI encountered unknown metadata*");
        }
        finally
        {
            Environment.SetEnvironmentVariable("cmf_cli_internal_strict_step_parsing", null);
        }
    }

    [Fact]
    public void FromXml_ShouldDefaultToGenericAndNotThrow_WhenStrictStepParsingDisabled()
    {
        Environment.SetEnvironmentVariable("cmf_cli_internal_strict_step_parsing", null);

        var xml = XDocument.Parse(
            """
            <deploymentPackage>
              <packageId>Cmf.Custom.Data</packageId>
              <version>1.0.0</version>
              <steps>
                <step type="NotARealStepType" contentPath="*.example" notARealAttribute="oops" />
              </steps>
            </deploymentPackage>
            """);

        var pkg = CmfPackageController.FromXml(xml);

        pkg.Steps.Should().ContainSingle();
        pkg.Steps.Single().Type.Should().Be(StepType.Generic);
    }
}

using System.Linq;
using System.Xml.Linq;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Services;
using FluentAssertions;
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
}

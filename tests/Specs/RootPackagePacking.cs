using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Cmf.CLI.Commands;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Handlers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;
using PackageHandler = Cmf.CLI.Handlers.PackageTypeHandler;

namespace tests.Specs;

public class RootPackagePacking
{
    public RootPackagePacking()
    {
        ExecutionContext.ServiceProvider = new ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>()
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData("10.0.0", "Customization", "Root", true)]
    [InlineData("10.2.5", "Customization", "Root", true)]
    [InlineData("11.0.0", "Customization", "Root", false)]
    [InlineData("12.0.0-beta.2", "Customization", "Root", false)]
    [InlineData("10.2.5", "App", "Root", false)]
    [InlineData("11.0.0", "App", "Root", false)]
    [InlineData("12.0.0", "App", "Root", false)]
    [InlineData("10.2.5", "Customization", "Generic", false)]
    [InlineData("11.0.0", "Customization", "Generic", false)]
    public void VirtualDependencies_PopulateOnlyRootsAndUseNormalManifestGeneration(string mesVersion, string repositoryType, string packageType, bool hasMetadata)
    {
        var fileSystem = CreateFileSystem(mesVersion, repositoryType, packageType);
        var source = fileSystem.File.ReadAllText("/repo/cmfpackage.json");
        var handler = (PackageHandler)PackageTypeFactory.GetPackageTypeHandler(fileSystem.FileInfo.New("/repo/cmfpackage.json"));
        var package = handler.CmfPackage;
        var originalDependency = package.Dependencies.Single();

        // Virtual dependencies must be visible in memory before serialization, without accumulating duplicates.
        package.SetVirtualDependencies();
        package.SetVirtualDependencies();

        package.Dependencies.Should().Contain(originalDependency);
        var virtualDependencies = package.Dependencies.Where(d => d.Id != "Custom.Dependency").ToArray();
        virtualDependencies.Should().HaveCount(packageType == "Root" ? (hasMetadata ? 2 : 1) : 0);
        foreach (var dependency in virtualDependencies)
        {
            dependency.Version.Should().Be(mesVersion);
            dependency.Mandatory.Should().BeFalse();
            dependency.Conditional.Should().BeFalse();
            dependency.IsIgnorable.Should().BeTrue();
        }

        handler.GenerateDeploymentFrameworkManifest(fileSystem.DirectoryInfo.New("/repo/packed"));

        var manifest = XDocument.Parse(fileSystem.File.ReadAllText("/repo/packed/manifest.xml"));
        var dependencies = manifest.Descendants("dependency").ToArray();
        dependencies.Should().ContainSingle(d => (string)d.Attribute("id") == "Custom.Dependency");
        var environment = dependencies.Where(d => (string)d.Attribute("id") == "Cmf.Environment").ToArray();
        environment.Should().HaveCount(packageType == "Root" ? 1 : 0);
        var metadata = dependencies.Where(d => (string)d.Attribute("id") == "CriticalManufacturing.DeploymentMetadata").ToArray();
        metadata.Should().HaveCount(hasMetadata ? 1 : 0);
        foreach (var dependency in environment.Concat(metadata))
        {
            ((string)dependency.Attribute("version")).Should().Be(mesVersion);
            ((string)dependency.Attribute("mandatory")).Should().Be("false");
            ((string)dependency.Attribute("conditional")).Should().Be("false");
            ((string)dependency.Attribute("isIgnorable")).Should().Be("true");
        }
        fileSystem.File.ReadAllText("/repo/cmfpackage.json").Should().Be(source);
        dependencies.Select(d => (string)d.Attribute("id")).Should().Equal(package.Dependencies.Select(d => d.Id));
    }

    [Theory]
    [InlineData("10.2.5", "Customization", true)]
    [InlineData("11.0.0", "Customization", false)]
    [InlineData("12.0.0", "Customization", false)]
    [InlineData("10.2.5", "App", false)]
    public void VirtualDependencies_HandleExistingDependenciesCaseInsensitively(string mesVersion, string repositoryType, bool hasMetadata)
    {
        var fileSystem = CreateFileSystem(mesVersion, repositoryType, "Root", """
            { "id": "cmf.environment", "version": "10.0.1", "mandatory": true },
            { "id": "criticalmanufacturing.deploymentmetadata", "version": "10.0.1", "mandatory": true }
            """);
        var source = fileSystem.File.ReadAllText("/repo/cmfpackage.json");
        var handler = (RootPackageTypeHandler)PackageTypeFactory.GetPackageTypeHandler(fileSystem.FileInfo.New("/repo/cmfpackage.json"));
        var originalEnvironment = handler.CmfPackage.Dependencies.First();

        handler.CmfPackage.SetVirtualDependencies();
        handler.CmfPackage.SetVirtualDependencies();
        handler.CmfPackage.Dependencies.First().Should().BeSameAs(originalEnvironment);
        handler.CmfPackage.Dependencies.Should().HaveCount(hasMetadata ? 2 : 1);

        handler.GenerateDeploymentFrameworkManifest(fileSystem.DirectoryInfo.New("/repo/packed"));

        var manifest = XDocument.Parse(fileSystem.File.ReadAllText("/repo/packed/manifest.xml"));
        var dependencies = manifest.Descendants("dependency").ToArray();
        var environment = dependencies.Should().ContainSingle(d => (string)d.Attribute("id") == "cmf.environment").Subject;
        ((string)environment.Attribute("version")).Should().Be("10.0.1");
        ((string)environment.Attribute("mandatory")).Should().Be("true");
        dependencies.Count(d => (string)d.Attribute("id") == "criticalmanufacturing.deploymentmetadata").Should().Be(hasMetadata ? 1 : 0);
        dependencies.Should().HaveCount(hasMetadata ? 2 : 1);
        fileSystem.File.ReadAllText("/repo/cmfpackage.json").Should().Be(source);
    }

    [Theory]
    [InlineData("10.2.5", true)]
    [InlineData("11.0.0", false)]
    [InlineData("12.0.0", false)]
    public void Pack_IncludesDynamicDependenciesInArchive(string mesVersion, bool hasMetadata)
    {
        var fileSystem = CreateFileSystem(mesVersion, "Customization", "Root", "");
        var source = fileSystem.File.ReadAllText("/repo/cmfpackage.json");
        var package = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"));

        package.ValidatePackage();
        new PackCommand(fileSystem).Execute(package, fileSystem.DirectoryInfo.New("/repo/output"), false, false);

        package.Dependencies.Should().ContainSingle(d => d.Id == "Cmf.Environment");
        package.Dependencies.Count(d => d.Id == "CriticalManufacturing.DeploymentMetadata").Should().Be(hasMetadata ? 1 : 0);

        using var archive = new ZipArchive(fileSystem.File.OpenRead("/repo/output/Custom.Root.1.0.0.zip"));
        using var reader = new StreamReader(archive.GetEntry("manifest.xml").Open());
        var manifest = XDocument.Parse(reader.ReadToEnd());
        var dependencies = manifest.Descendants("dependency").ToArray();
        dependencies.Should().ContainSingle(d => (string)d.Attribute("id") == "Cmf.Environment");
        dependencies.Count(d => (string)d.Attribute("id") == "CriticalManufacturing.DeploymentMetadata").Should().Be(hasMetadata ? 1 : 0);
        fileSystem.File.ReadAllText("/repo/cmfpackage.json").Should().Be(source);
    }

    [Fact]
    public void Pack_DryRunPopulatesVirtualDependenciesWithoutWritingFiles()
    {
        var fileSystem = CreateFileSystem("10.2.5", "Customization", "Root");
        var originalFiles = fileSystem.AllFiles.ToArray();
        var source = fileSystem.File.ReadAllText("/repo/cmfpackage.json");
        var package = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"));
        var originalDependency = package.Dependencies.Single();

        new PackCommand(fileSystem).Execute(package, fileSystem.DirectoryInfo.New("/repo/output"), false, true);

        fileSystem.AllFiles.Should().BeEquivalentTo(originalFiles);
        package.Dependencies.Should().HaveCount(3);
        package.Dependencies.Should().Contain(originalDependency);
        package.Dependencies.Should().ContainSingle(d => d.Id == "Cmf.Environment");
        package.Dependencies.Should().ContainSingle(d => d.Id == "CriticalManufacturing.DeploymentMetadata");
        fileSystem.File.ReadAllText("/repo/cmfpackage.json").Should().Be(source);
    }

    [Fact]
    public void VirtualDependencies_WithoutMesVersionPreserveExistingDependencies()
    {
        var fileSystem = CreateFileSystem(null, "Customization", "Root");
        var handler = (RootPackageTypeHandler)PackageTypeFactory.GetPackageTypeHandler(fileSystem.FileInfo.New("/repo/cmfpackage.json"));
        var originalDependencies = handler.CmfPackage.Dependencies.ToArray();

        handler.CmfPackage.SetVirtualDependencies();

        handler.CmfPackage.Dependencies.Should().Equal(originalDependencies);

        handler.GenerateDeploymentFrameworkManifest(fileSystem.DirectoryInfo.New("/repo/packed"));

        var manifest = XDocument.Parse(fileSystem.File.ReadAllText("/repo/packed/manifest.xml"));
        manifest.Descendants("dependency").Should().ContainSingle(d => (string)d.Attribute("id") == "Custom.Dependency");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VirtualDependencies_InitializeMissingOrNullCollection(bool explicitNull)
    {
        var fileSystem = CreateFileSystem("10.2.5", "Customization", "Root");
        var source = JObject.Parse(fileSystem.File.ReadAllText("/repo/cmfpackage.json"));
        if (explicitNull)
        {
            source["dependencies"] = JValue.CreateNull();
        }
        else
        {
            source.Remove("dependencies");
        }
        fileSystem.File.WriteAllText("/repo/cmfpackage.json", source.ToString());
        var package = CmfPackage.Load(fileSystem.FileInfo.New("/repo/cmfpackage.json"));

        new PackCommand(fileSystem).Execute(package, fileSystem.DirectoryInfo.New("/repo/output"), false, true);

        package.Dependencies.Should().HaveCount(2);
        package.Dependencies.Should().ContainSingle(d => d.Id == "Cmf.Environment");
        package.Dependencies.Should().ContainSingle(d => d.Id == "CriticalManufacturing.DeploymentMetadata");
        fileSystem.File.ReadAllText("/repo/cmfpackage.json").Should().Be(source.ToString());
    }

    private static MockFileSystem CreateFileSystem(string mesVersion, string repositoryType, string packageType,
        string dependencies = """{ "id": "Custom.Dependency", "version": "1.0.0" }""")
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/repo/.project-config.json"] = new MockFileData($$"""
                { "MESVersion": {{(mesVersion == null ? "null" : $"\"{mesVersion}\"")}}, "RepositoryType": "{{repositoryType}}" }
                """),
            ["/repo/cmfpackage.json"] = new MockFileData($$"""
                {
                  "packageId": "Custom.Root",
                  "version": "1.0.0",
                  "packageType": "{{packageType}}",
                  "dependencies": [{{dependencies}}]
                }
                """)
        }, "/repo");
        fileSystem.Directory.CreateDirectory("/repo/packed");
        fileSystem.Directory.CreateDirectory("/repo/output");
        ExecutionContext.Initialize(fileSystem);
        return fileSystem;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Cmf.CLI.Commands.New.IoT;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace tests.Specs;

public class IoTNodeTypings
{
    [Theory]
    [InlineData("driver", "10.2.7", "18")]
    [InlineData("driver", "11.0.0", "20")]
    [InlineData("driver", "12.0.0", "24")]
    [InlineData("driver-dotnet", "11.0.0", "20")]
    [InlineData("driver-dotnet", "12.0.0", "24")]
    [InlineData("driver-dotnet-framework", "11.0.0", "20")]
    [InlineData("driver-dotnet-framework", "12.0.0", "24")]
    [InlineData("taskLibrary", "10.2.7", "18")]
    [InlineData("taskLibrary", "11.0.0", "20")]
    [InlineData("taskLibrary", "12.0.0", "24")]
    [InlineData("businessScenario", "11.1.0", "20")]
    [InlineData("businessScenario", "12.0.0", "24")]
    public void Generate_MatchesNodeTypingsToProjectRuntime(string generator, string mesVersion, string nodeMajor)
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/repo/.project-config.json"] = new($$"""
                { "MESVersion": "{{mesVersion}}", "NPMRegistry": "https://registry.example/" }
                """),
            ["/repo/IoT/cmfpackage.json"] = new("""
                { "packageId": "Cmf.Custom.IoT.Packages", "version": "1.0.0", "packageType": "IoT" }
                """)
        }, "/repo/IoT");
        IFileSystem physicalFileSystem = new FileSystem();
        var templateFeed = physicalFileSystem.Path.Join(AppContext.BaseDirectory, "resources", "template_feed");
        foreach (var template in physicalFileSystem.Directory.EnumerateFiles(templateFeed, "*", SearchOption.AllDirectories))
        {
            fileSystem.AddFile(template, new MockFileData(physicalFileSystem.File.ReadAllBytes(template)));
        }

        using var services = new ServiceCollection()
            .AddSingleton<IProjectConfigService, ProjectConfigService>()
            .AddSingleton<IVersionService>(new VersionService("cmf-node-typings-tests"))
            .AddSingleton<IDependencyVersionService, DependencyVersionService>()
            .BuildServiceProvider();
        var previousServices = ExecutionContext.ServiceProvider;
        var previousFileSystem = ExecutionContext.Instance?.FileSystem;
        var previousConsole = AnsiConsole.Console;
        var console = new Spectre.Console.Testing.TestConsole();
        console.Profile.Capabilities.Interactive = true;
        for (var i = 0; i < 5; i++)
        {
            console.Input.PushTextWithEnter("");
        }
        if (generator == "taskLibrary")
        {
            console.Input.PushTextWithEnter(" ");
        }
        for (var i = 0; i < 8; i++)
        {
            console.Input.PushTextWithEnter("");
        }

        try
        {
            ExecutionContext.ServiceProvider = services;
            AnsiConsole.Console = console;
            var directory = fileSystem.DirectoryInfo.New("/repo/IoT");
            switch (generator)
            {
                case "driver": new GenerateDriverCommand(fileSystem).Execute(directory); break;
                case "driver-dotnet": new GenerateDotnetDriverCommand(fileSystem).Execute(directory); break;
                case "driver-dotnet-framework": new GenerateDotnetFrameworkDriverCommand(fileSystem).Execute(directory); break;
                case "taskLibrary": new GenerateTaskLibraryCommand(fileSystem).Execute(directory); break;
                case "businessScenario": new GenerateBusinessScenarioCommand(fileSystem).Execute(directory); break;
                default: throw new ArgumentOutOfRangeException(nameof(generator));
            }

            var packages = fileSystem.Directory.GetFiles("/repo/IoT/src", "package.json", SearchOption.AllDirectories)
                .Select(path => JObject.Parse(fileSystem.File.ReadAllText(path)))
                .Where(package => package["devDependencies"]?["@types/node"] != null);
            var package = packages.Should().ContainSingle().Subject;
            package["devDependencies"]["@types/node"].Value<string>().Should().Be($"^{nodeMajor}.0.0");
        }
        finally
        {
            AnsiConsole.Console = previousConsole;
            ExecutionContext.ServiceProvider = previousServices;
            if (previousFileSystem != null)
            {
                ExecutionContext.Initialize(previousFileSystem);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI;
using Cmf.CLI.Commands;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using tests.Mocks;
using Xunit;
using ExecutionContext = Cmf.CLI.Core.Objects.ExecutionContext;

namespace tests.Specs;

public class Plugins : IDisposable
{
    private readonly LogLevel originalLevel = Log.Level;
    private readonly Spectre.Console.IAnsiConsole originalConsole = Log.AnsiConsole;
    private readonly string originalLogLevelVariable = Environment.GetEnvironmentVariable("cmf_cli_loglevel");
    private readonly string originalSystemDebugVariable = Environment.GetEnvironmentVariable("SYSTEM_DEBUG");
    private readonly List<string> tempDirectories = new();

    public void Dispose()
    {
        Log.Level = originalLevel;
        Log.AnsiConsole = originalConsole;
        Environment.SetEnvironmentVariable("cmf_cli_loglevel", originalLogLevelVariable);
        Environment.SetEnvironmentVariable("SYSTEM_DEBUG", originalSystemDebugVariable);
        tempDirectories.ForEach(dir => Directory.Delete(dir, recursive: true));
    }

    [Fact]
    public void PluginsSuccess()
    {
        var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
        repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton(repositoryAuthStoreMock.Object)
            .BuildServiceProvider();

        var content = @"{""objects"":[{""package"":{""name"":""@criticalmanufacturing/portal"",""scope"":""criticalmanufacturing"",""version"":""1.6.0"",""description"":""Critical Manufacturing Portal CLI"",""keywords"":[""cmf"",""cmf-cli-plugin""],""date"":""2022-09-27T09:12:30.603Z"",""links"":{""npm"":""https://www.npmjs.com/package/%40criticalmanufacturing%2Fportal"",""homepage"":""https://github.com/criticalmanufacturing/portal-sdk#readme"",""repository"":""https://github.com/criticalmanufacturing/portal-sdk"",""bugs"":""https://github.com/criticalmanufacturing/portal-sdk/issues""},""author"":{""name"":""Critical Manufacturing, SA""},""publisher"":{""username"":""x1"",""email"":""x1@example.com""},""maintainers"":[{""username"":""x2"",""email"":""x2@example.com""},{""username"":""x3"",""email"":""x3@example.com""},{""username"":""x4"",""email"":""x4@example.com""},{""username"":""x5"",""email"":""x5@example.com""},{""username"":""x6"",""email"":""x6@example.com""},{""username"":""x7"",""email"":""x7@example.com""},{""username"":""x8"",""email"":""x8@example.com""},{""username"":""x9"",""email"":""x9@example.com""}]},""score"":{""final"":0.22965059277610983,""detail"":{""quality"":0.36747990161041577,""popularity"":0.007828444646624137,""maintenance"":0.3333333333333333}},""searchScore"":2.6164612e-10}],""total"":1,""time"":""Tue Oct 25 2022 15:43:09 GMT+0000 (Coordinated Universal Time)""}";
        var search = $"{CoreConstants.NpmJsUrl.TrimEnd('/')}/-/v1/search?text=+keywords:cmf-cli-plugin";

        // mock HttpClient
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.AbsoluteUri == search),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();

                response.Content = new StringContent(content);
                response.StatusCode = HttpStatusCode.OK;

                return response;
            });
        var httpClient = new HttpClient(httpMessageHandler.Object);

        var npmClient = new NPMClient(httpClient);

        var plugins = npmClient.FindPlugins(null);

        plugins.Should().ContainSingle();
        plugins[0].IsOfficial.Should().BeTrue();
        plugins[0].Name = "@criticalmanufacturing/portal";
        plugins[0].Registry = "https://registry.npmjs.com/";
        plugins[0].Link.AbsoluteUri.Should().Be("https://www.npmjs.com/package/%40criticalmanufacturing%2Fportal");
    }

    [Fact]
    public void PluginsError()
    {
        var repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
        repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).Returns(Task.FromResult(new CmfAuthFile()));

        ExecutionContext.ServiceProvider = (new ServiceCollection())
            .AddSingleton(repositoryAuthStoreMock.Object)
            .BuildServiceProvider();

        var search = $"{CoreConstants.NpmJsUrl.TrimEnd('/')}/-/v1/search?text=+keywords:cmf-cli-plugin";

        // mock HttpClient
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.AbsoluteUri == search),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                
                response.StatusCode = HttpStatusCode.InternalServerError;

                return response;
            });
        var httpClient = new HttpClient(httpMessageHandler.Object);

        var npmClient = new NPMClient(httpClient);

        var logWriter = (new Logging()).GetLogStringWriter();
        
        var plugins = npmClient.FindPlugins(null);

        plugins.Should().BeEmpty();
        logWriter.ToString().Should().Contain($"Search request to {new Uri(CoreConstants.NpmJsUrl).AbsoluteUri} failed: {HttpStatusCode.InternalServerError}");
    }

    [Fact]
    public async Task InvokePluginsAsync()
    {
        Environment.SetEnvironmentVariable("PATH", $"{Environment.CurrentDirectory}/bin:{Environment.GetEnvironmentVariable("PATH")}");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path("bin/cmf-pipeline"), new MockFileData("dummy") }
        });
        
        var args = new string[] { "pipeline", "--help" };
            
        var rootCommand = await StartupModule.Configure(
            packageId: "root-command-test",
            envVarPrefix: "root_command_test",
            description: "Root Command Test",
            args: args
        );
        
        BaseCommand.AddPluginCommands(fileSystem, rootCommand);
        
        var console = new TestConsole();
        var parseResult = rootCommand.Parse(args);
        var result = await parseResult.InvokeAsync(console);

        if (result != 0)
        {
            throw new Exception(console.Error.ToString());
        }

        Assert.NotEmpty(console.Out.ToString());
        Assert.NotEmpty(rootCommand.Subcommands);
    }

    [Fact]
    public void AddPluginCommands_ReturnsDiscoveredPlugins()
    {
        Environment.SetEnvironmentVariable("PATH", $"{Environment.CurrentDirectory}/bin:{Environment.GetEnvironmentVariable("PATH")}");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { MockUnixSupport.Path(OperatingSystem.IsWindows() ? "bin/cmf-pipeline.exe" : "bin/cmf-pipeline"), new MockFileData("dummy") }
        });
        var rootCommand = new RootCommand();

        var plugins = BaseCommand.AddPluginCommands(fileSystem, rootCommand);

        plugins.Should().ContainKey("pipeline");
        rootCommand.Subcommands.Should().Contain(cmd => cmd.Name == "pipeline");
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    [InlineData("-?")]
    [InlineData("--version")]
    [InlineData("--loglevel", "Debug")]
    [InlineData("build", "--help")]
    [InlineData("run", "--", "--flag")]
    public void TryExecutePlugin_ForwardsArgumentsVerbatim(params string[] pluginArgs)
    {
        var plugin = CreateEchoPlugin();
        var plugins = new Dictionary<string, PluginCommand> { { "echo", plugin } };

        var output = CaptureConsoleOutput(() =>
            Program.TryExecutePlugin(plugins, ["echo", .. pluginArgs]).Should().BeTrue());

        output.Should().Be(string.Join(Environment.NewLine, pluginArgs));
    }

    [Theory]
    [InlineData(new[] { "--", "--help" }, new[] { "--help" })]
    [InlineData(new[] { "--", "--", "x" }, new[] { "--", "x" })]
    [InlineData(new[] { "--" }, new string[0])]
    public void TryExecutePlugin_DropsSeparatorAfterPluginName(string[] pluginArgs, string[] expected)
    {
        var plugins = new Dictionary<string, PluginCommand> { { "echo", CreateEchoPlugin() } };

        var output = CaptureConsoleOutput(() =>
            Program.TryExecutePlugin(plugins, ["echo", .. pluginArgs]).Should().BeTrue());

        output.Should().Be(string.Join(Environment.NewLine, expected));
    }

    [Theory]
    [InlineData]
    [InlineData("build", "--help")]
    public void TryExecutePlugin_IgnoresNonPluginCommands(params string[] args)
    {
        var plugins = new Dictionary<string, PluginCommand> { { "echo", CreateEchoPlugin() } };

        var output = CaptureConsoleOutput(() => Program.TryExecutePlugin(plugins, args).Should().BeFalse());

        output.Should().BeEmpty();
    }

    [Fact]
    public void TryExecutePlugin_ThrowsWhenPluginFails()
    {
        var plugins = new Dictionary<string, PluginCommand> { { "echo", CreateEchoPlugin(exitCode: 3) } };

        var act = () => Program.TryExecutePlugin(plugins, ["echo", "--help"]);

        act.Should().Throw<CliException>().Where(e => (int)e.ErrorCode == 3);
    }

    [Fact]
    public void TryExecutePlugin_InheritedConsole_DoesNotRelayOutput()
    {
        // the plugin writes straight to the process' stdout, so nothing goes through Console.Out
        var plugins = new Dictionary<string, PluginCommand> { { "echo", CreateEchoPlugin(inheritConsoleOutput: true) } };

        var output = CaptureConsoleOutput(() =>
            Program.TryExecutePlugin(plugins, ["echo", "hello"]).Should().BeTrue());

        output.Should().BeEmpty();
    }

    [Fact]
    public void TryExecutePlugin_InheritedConsole_ThrowsWhenPluginFails()
    {
        var plugins = new Dictionary<string, PluginCommand> { { "echo", CreateEchoPlugin(exitCode: 3, inheritConsoleOutput: true) } };

        var act = () => Program.TryExecutePlugin(plugins, ["echo", "--help"]);

        act.Should().Throw<CliException>().Where(e => (int)e.ErrorCode == 3);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TryExecutePlugin_LogsConsoleMode(bool inheritConsoleOutput)
    {
        var logWriter = new Logging().GetLogStringWriter();
        var plugins = new Dictionary<string, PluginCommand> { { "echo", CreateEchoPlugin(inheritConsoleOutput: inheritConsoleOutput) } };
        ClearLogLevelVariables();
        Environment.SetEnvironmentVariable("cmf_cli_loglevel", "Debug");

        CaptureConsoleOutput(() => Program.TryExecutePlugin(plugins, ["echo"]));

        var mode = inheritConsoleOutput ? "inherited" : "relayed";
        logWriter.ToString().Should().Contain($"stdout {mode}, stderr {mode}");
    }

    [Theory]
    [InlineData("cmf_cli_loglevel", "Debug")]
    [InlineData("SYSTEM_DEBUG", "true")]
    public void TryExecutePlugin_AppliesLogLevelFromEnvironment(string variable, string value)
    {
        var logWriter = new Logging().GetLogStringWriter();
        var plugins = new Dictionary<string, PluginCommand> { { "echo", CreateEchoPlugin() } };
        ClearLogLevelVariables();
        Log.Level = LogLevel.Information;

        Environment.SetEnvironmentVariable(variable, value);
        CaptureConsoleOutput(() => Program.TryExecutePlugin(plugins, ["echo"]));

        Log.Level.Should().Be(LogLevel.Debug);
        logWriter.ToString().Should().Contain("Child process exited with code 0");
    }

    [Fact]
    public void TryExecutePlugin_DoesNotApplyLogLevelFromPluginArguments()
    {
        var plugins = new Dictionary<string, PluginCommand> { { "echo", CreateEchoPlugin() } };
        ClearLogLevelVariables();
        Log.Level = LogLevel.Information;

        var output = CaptureConsoleOutput(() => Program.TryExecutePlugin(plugins, ["echo", "--loglevel", "Debug"]));

        output.Should().Be(string.Join(Environment.NewLine, "--loglevel", "Debug"));
        Log.Level.Should().Be(LogLevel.Verbose);
    }

    [Fact, Trait("TestCategory", "Integration")]
    public void Execute_InTerminal_PluginCanUseSpectreMultiSelectionPrompt()
    {
        if (!CanRunInTerminal())
        {
            return;
        }
        var pluginDir = CreateSpectreMultiSelectPlugin();

        // select Apple, move down twice, select Cherry, submit
        var output = RunInTerminal($"exec '{CmfExecutable}' multiselect", pluginDir,
            waitFor: "Pick fruits", " ", "\u001b[B", "\u001b[B", " ", "\r");

        output.Should().Contain("Interactive: True");
        output.Should().Contain("Selected: Apple, Cherry");
    }

    [Fact, Trait("TestCategory", "Integration")]
    public void Execute_WithRedirectedOutput_PluginIsNotInteractive()
    {
        if (!CanRunInTerminal())
        {
            return;
        }
        var pluginDir = CreateSpectreMultiSelectPlugin();

        // cmf's stdout is a pipe, so the plugin's stdout is relayed and Spectre.Console can't prompt
        var output = RunInTerminal($"'{CmfExecutable}' multiselect | cat", pluginDir);

        output.Should().Contain("Interactive: False");
        output.Should().NotContain("Selected:");
    }

    private static string CmfExecutable => Path.Combine(AppContext.BaseDirectory, "cmf");

    /// <summary>
    /// The terminal tests need a pseudo-terminal, which is provided by util-linux's script
    /// </summary>
    private static bool CanRunInTerminal() =>
        OperatingSystem.IsLinux() && File.Exists(CmfExecutable) &&
        (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator).Any(dir => File.Exists(Path.Combine(dir, "script")));

    /// <summary>
    /// Builds a cmf-multiselect plugin which reports whether its console is interactive and shows a Spectre.Console MultiSelectionPrompt
    /// </summary>
    /// <returns>the directory containing the plugin launcher</returns>
    private string CreateSpectreMultiSelectPlugin()
    {
        var dir = Directory.CreateTempSubdirectory("cmf-plugin-tests").FullName;
        tempDirectories.Add(dir);
        var projectDir = Directory.CreateDirectory(Path.Combine(dir, "src")).FullName;
        var outputDir = Path.Combine(dir, "out");
        var spectreVersion = typeof(Spectre.Console.AnsiConsole).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0];

        File.WriteAllText(Path.Combine(projectDir, "multiselect.csproj"), $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net{Environment.Version.Major}.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Spectre.Console" Version="{spectreVersion}" />
              </ItemGroup>
            </Project>
            """);
        File.WriteAllText(Path.Combine(projectDir, "Program.cs"), """
            using Spectre.Console;

            AnsiConsole.WriteLine($"Interactive: {AnsiConsole.Console.Profile.Capabilities.Interactive}");
            var selected = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
                .Title("Pick fruits")
                .AddChoices("Apple", "Banana", "Cherry"));
            AnsiConsole.WriteLine($"Selected: {string.Join(", ", selected)}");
            """);

        var build = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true };
        new[] { "build", projectDir, "--configuration", "Release", "--output", outputDir, "--nologo" }.ToList().ForEach(build.ArgumentList.Add);
        using (var process = Process.Start(build)!)
        {
            var buildOutput = process.StandardOutput.ReadToEndAsync();
            var buildError = process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            process.ExitCode.Should().Be(0, $"the test plugin should build: {buildOutput.Result}{buildError.Result}");
        }

        var pluginDir = Directory.CreateDirectory(Path.Combine(dir, "bin")).FullName;
        var launcher = Path.Combine(pluginDir, "cmf-multiselect");
        File.WriteAllText(launcher, $"#!/bin/sh\nexec dotnet '{Path.Combine(outputDir, "multiselect.dll")}' \"$@\"\n");
        File.SetUnixFileMode(launcher, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return pluginDir;
    }

    /// <summary>
    /// Runs a shell command in a pseudo-terminal, optionally typing keys once some text is shown
    /// </summary>
    /// <returns>everything written to the terminal</returns>
    private static string RunInTerminal(string command, string pluginDir, string waitFor = null, params string[] keys)
    {
        var ps = new ProcessStartInfo("script") { RedirectStandardInput = true, RedirectStandardOutput = true };
        new[] { "--quiet", "--return", "--command", command, "/dev/null" }.ToList().ForEach(ps.ArgumentList.Add);
        ps.Environment["PATH"] = $"{pluginDir}{Path.PathSeparator}{Environment.GetEnvironmentVariable("PATH")}";
        ps.Environment["TERM"] = "xterm-256color";
        ps.Environment["cmf_cli_skip_version_check"] = "true";

        using var process = Process.Start(ps)!;
        var output = new StringBuilder();
        string Output() { lock (output) { return output.ToString(); } }
        var reader = Task.Run(async () =>
        {
            var buffer = new char[1024];
            int read;
            while ((read = await process.StandardOutput.ReadAsync(buffer)) > 0)
            {
                lock (output) { output.Append(buffer, 0, read); }
            }
        });

        try
        {
            if (waitFor != null)
            {
                SpinWait.SpinUntil(() => Output().Contains(waitFor) || process.HasExited, TimeSpan.FromSeconds(60));
                Output().Should().Contain(waitFor);
                foreach (var key in keys)
                {
                    // give the prompt time to switch the terminal to raw mode and handle the previous key
                    Thread.Sleep(500);
                    process.StandardInput.Write(key);
                    process.StandardInput.Flush();
                }
            }

            process.WaitForExit(TimeSpan.FromSeconds(60)).Should().BeTrue($"the command should finish, got: {Output()}");
            reader.Wait(TimeSpan.FromSeconds(10));
            return Output();
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
    }

    private static void ClearLogLevelVariables()
    {
        Environment.SetEnvironmentVariable("cmf_cli_loglevel", null);
        Environment.SetEnvironmentVariable("SYSTEM_DEBUG", null);
    }

    /// <summary>
    /// Creates a plugin which prints each of its arguments on a separate line
    /// </summary>
    private PluginCommand CreateEchoPlugin(int exitCode = 0, bool inheritConsoleOutput = false)
    {
        var dir = Directory.CreateTempSubdirectory("cmf-plugin-tests").FullName;
        tempDirectories.Add(dir);
        string path;
        if (OperatingSystem.IsWindows())
        {
            path = Path.Combine(dir, "cmf-echo.cmd");
            File.WriteAllText(path, $"@echo off\r\n:loop\r\nif \"%~1\"==\"\" goto end\r\necho %~1\r\nshift\r\ngoto loop\r\n:end\r\nexit /b {exitCode}\r\n");
        }
        else
        {
            path = Path.Combine(dir, "cmf-echo");
            File.WriteAllText(path, $"#!/bin/sh\nprintf '%s\\n' \"$@\"\nexit {exitCode}\n");
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        return new PluginCommand("echo", path) { InheritConsoleOutput = inheritConsoleOutput };
    }

    private static string CaptureConsoleOutput(Action action)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            action();
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return writer.ToString().TrimEnd();
    }
}

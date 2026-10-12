using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Cmf.CLI.Builders;
using FluentAssertions;
using Xunit;

namespace tests.Specs;

public class PythonCommands
{
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private static readonly string Python = IsWindows ? "python.exe" : "python3";

    [Fact]
    public void PythonCommand_WithoutVirtualEnvironment_UsesSystemPython()
    {
        var command = new PythonCommand
        {
            DisplayName = "Create virtual environment",
            Module = "venv",
            Args = new[] { ".venv" },
            WorkingDirectory = new MockFileSystem().DirectoryInfo.New(".")
        };

        var step = command.GetSteps().Should().ContainSingle().Subject;

        step.Command.Should().Be(Python, "the system Python should be used without a virtual environment");
        step.Args.Should().Equal("-m", "venv", ".venv");
    }

    [Fact]
    public void PythonCommand_WithVirtualEnvironment_UsesEnvironmentPython()
    {
        var command = new PythonCommand
        {
            DisplayName = "Install package",
            Module = "pip",
            Args = new[] { "install", "mkdocs" },
            VirtualEnvironment = ".venv",
            WorkingDirectory = new MockFileSystem().DirectoryInfo.New(".")
        };

        var step = command.GetSteps().Should().ContainSingle().Subject;

        step.Command.Should().Be(IsWindows
            ? Path.Join(command.WorkingDirectory.FullName, ".venv", "Scripts", "python.exe")
            : Path.Join(command.WorkingDirectory.FullName, ".venv", "bin", "python"), "the virtual environment Python should be used");
        step.Args.Should().Equal("-m", "pip", "install", "mkdocs");
    }

    [Fact]
    public void PythonCommand_WithVirtualEnvironment_SetsActivationEnvironment()
    {
        var command = new PythonCommand
        {
            VirtualEnvironment = ".venv",
            EnvironmentVariables = new() { { "CUSTOM_VARIABLE", "value" } },
            WorkingDirectory = new MockFileSystem().DirectoryInfo.New(".")
        };

        var step = command.GetSteps().Should().ContainSingle().Subject;
        var virtualEnvironmentPath = command.GetVirtualEnvironmentPath();
        var virtualEnvironmentBin = Path.Join(virtualEnvironmentPath, IsWindows ? "Scripts" : "bin");
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

        step.EnvironmentVariables["PYTHONHOME"].Should().BeNull();
        step.EnvironmentVariables["VIRTUAL_ENV"].Should().Be(virtualEnvironmentPath);
        step.EnvironmentVariables["VIRTUAL_ENV_PROMPT"].Should().Be(".venv");
        step.EnvironmentVariables["PATH"].Should().Be(string.IsNullOrEmpty(path)
            ? virtualEnvironmentBin
            : virtualEnvironmentBin + Path.PathSeparator + path);
        step.EnvironmentVariables["CUSTOM_VARIABLE"].Should().Be("value");
    }

    [Fact]
    public void PythonCommand_WithNamedVirtualEnvironment_UsesDirectoryNameAsPrompt()
    {
        var command = new PythonCommand
        {
            VirtualEnvironment = "build-env",
            WorkingDirectory = new MockFileSystem().DirectoryInfo.New(".")
        };

        var step = command.GetSteps().Should().ContainSingle().Subject;

        step.EnvironmentVariables["VIRTUAL_ENV_PROMPT"].Should().Be("build-env");
    }
}

using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Runtime.InteropServices;
using Cmf.CLI.Builders;
using FluentAssertions;
using Xunit;

namespace tests.Specs;

public class MkDocsCommand
{
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void WithoutVirtualEnvironment_UsesSystemMkDocs()
    {
        var command = new Cmf.CLI.Builders.MkDocsCommand
        {
            DisplayName = "MkDocs build",
            Command = "build",
            WorkingDirectory = new MockFileSystem().DirectoryInfo.New(".")
        };

        var step = command.GetSteps().Should().ContainSingle().Subject;

        step.Command.Should().Be(IsWindows ? "mkdocs.exe" : "mkdocs",
            "the system MkDocs should be used without a virtual environment");
        step.Args.Should().Equal("build");
    }

    [Theory]
    [InlineData(".venv")]
    [InlineData("../shared venv")]
    public void WithRelativeVirtualEnvironment_ResolvesFromWorkingDirectory(string virtualEnvironment)
    {
        var fileSystem = new MockFileSystem();
        var workingDirectory = fileSystem.DirectoryInfo.New("help package");
        var command = new Cmf.CLI.Builders.MkDocsCommand
        {
            DisplayName = "MkDocs build",
            Command = "build",
            VirtualEnvironment = virtualEnvironment,
            WorkingDirectory = workingDirectory
        };

        var step = command.GetSteps().Should().ContainSingle().Subject;

        step.Command.Should().Be(fileSystem.Path.GetFullPath(fileSystem.Path.Combine(
            workingDirectory.FullName, virtualEnvironment,
            IsWindows ? "Scripts" : "bin", IsWindows ? "mkdocs.exe" : "mkdocs")));
        step.Args.Should().Equal("build");
    }

    [Fact]
    public void WithAbsoluteVirtualEnvironment_PreservesEnvironmentPath()
    {
        var fileSystem = new MockFileSystem();
        var virtualEnvironment = fileSystem.Path.GetFullPath("shared venv");
        var command = new Cmf.CLI.Builders.MkDocsCommand
        {
            Command = "build",
            Args = ["--strict"],
            VirtualEnvironment = virtualEnvironment,
            WorkingDirectory = fileSystem.DirectoryInfo.New("help package")
        };

        var step = command.GetSteps().Should().ContainSingle().Subject;

        step.Command.Should().Be(fileSystem.Path.Combine(
            virtualEnvironment, IsWindows ? "Scripts" : "bin", IsWindows ? "mkdocs.exe" : "mkdocs"));
        step.Args.Should().Equal("build", "--strict");
    }
}

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

    [Fact]
    public void WithVirtualEnvironment_UsesEnvironmentMkDocs()
    {
        var command = new Cmf.CLI.Builders.MkDocsCommand
        {
            DisplayName = "MkDocs build",
            Command = "build",
            VirtualEnvironment = ".venv",
            WorkingDirectory = new MockFileSystem().DirectoryInfo.New(".")
        };

        var step = command.GetSteps().Should().ContainSingle().Subject;

        step.Command.Should().Be(IsWindows
            ? ".venv\\Scripts\\mkdocs.exe"
            : ".venv/bin/mkdocs", "the virtual environment MkDocs should be used");
        step.Args.Should().Equal("build");
    }
}

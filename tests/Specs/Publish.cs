using System;
using Xunit;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using Moq;
using Cmf.CLI.Commands;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.IO;

namespace tests.Specs;

public class Publish
{
    [Theory]
    [InlineData("testhost", @"\\testhost\files\share")]
    [InlineData("example.com", "https://example.com/repository")]
    [InlineData("", "/local/path/to/repository")]
    public void Repository_Arg_ParsedCorrectly(string expectedHost, string inputRepository)
    {
        string inputFile = "/test/testPackage.zip";

        var publishCommand = new PublishCommand();
        var cmd = new Command("publish");
        publishCommand.Configure(cmd);

        IFileInfo _file = null;
        Uri _repository = null;
        cmd.Handler = CommandHandler.Create<IFileInfo, Uri>((
            file, repository) => {
                _file = file;
                _repository = repository;
            }
        );

        var console = new TestConsole();
        cmd.Invoke(new[] {
            inputFile, "--repository", inputRepository
        }, console);

        Assert.Equal(inputFile, _file.FullName);
        Assert.Equal(inputRepository, _repository.OriginalString);
        Assert.Equal(expectedHost, _repository.Host);
    }
}

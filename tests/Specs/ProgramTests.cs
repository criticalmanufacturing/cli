using Cmf.CLI;
using Cmf.CLI.Utilities;
using Cmf.CLI.Core.Enums;
using System;
using System.CommandLine;
using System.Threading.Tasks;
using Xunit;


namespace tests.Specs
{
    public class ProgramTests
    {
        [Fact]
        public async Task InvokeCommandAsync_PropagatesCliExceptionsToApplicationHandler()
        {
            var exception = new CliException("Expected command error", ErrorCode.InvalidArgument);
            var command = new RootCommand();
            command.SetAction((_, _) => Task.FromException<int>(exception));

            var actual = await Assert.ThrowsAsync<CliException>(() =>
                Program.InvokeCommandAsync(command.Parse(Array.Empty<string>())));

            Assert.Same(exception, actual);
            Assert.Equal(ErrorCode.InvalidArgument, actual.ErrorCode);
        }

        [Fact]
        public async Task InvokeCommandAsync_PropagatesUnexpectedExceptionsToApplicationHandler()
        {
            var exception = new InvalidOperationException("Unexpected command error");
            var command = new RootCommand();
            command.SetAction((_, _) => Task.FromException<int>(exception));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Program.InvokeCommandAsync(command.Parse(Array.Empty<string>())));

            Assert.Same(exception, actual);
        }

        [Theory]
        [InlineData(ErrorCode.Success)]
        [InlineData(ErrorCode.InvalidArgument)]
        public async Task InvokeCommandAsync_PreservesCommandExitCode(ErrorCode exitCode)
        {
            var command = new RootCommand();
            command.SetAction((_, _) => Task.FromResult((int)exitCode));

            var result = await Program.InvokeCommandAsync(command.Parse(Array.Empty<string>()));

            Assert.Equal((int)exitCode, result);
        }

        [Fact]
        public async Task InvokeCommandAsync_ParseErrorsReturnFailureWithoutExecutingCommand()
        {
            var command = new RootCommand();
            command.Add(new Argument<string>("requiredValue"));
            var invoked = false;
            command.SetAction((_, _) =>
            {
                invoked = true;
                return Task.FromResult(0);
            });

            var result = await Program.InvokeCommandAsync(command.Parse(Array.Empty<string>()));

            Assert.NotEqual(0, result);
            Assert.False(invoked);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(11)]
        public void Should_NotThrow_For_Valid_Versions(int version)
        {
            Program.ValidateMesVersion(version);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(9)]
        public void Should_Throw_For_Invalid_Versions(int version)
        {
            Assert.Throws<CliException>(() => Program.ValidateMesVersion(version));
        }

        [Fact]
        public void Should_NotThrow_When_Version_Is_Null()
        {
            Program.ValidateMesVersion(null);
        }
    }
}

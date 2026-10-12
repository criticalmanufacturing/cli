using System;
using System.Runtime.CompilerServices;
using Cmf.CLI.Core;
using FluentAssertions;
using Spectre.Console;
using Xunit;

namespace tests.Specs
{
    public class ExceptionLogging : IDisposable
    {
        private readonly IAnsiConsole originalConsole = Log.AnsiConsole;
        private readonly LogLevel originalLevel = Log.Level;
        private readonly Spectre.Console.Testing.TestConsole console = new();

        public ExceptionLogging()
        {
            Log.AnsiConsole = console;
        }

        public void Dispose()
        {
            Log.AnsiConsole = originalConsole;
            Log.Level = originalLevel;
            console.Dispose();
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Verbose)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        public void Exception_AtEveryLogLevel_PrintsStackTraceAndInnerException(LogLevel level)
        {
            Log.Level = level;

            Log.Exception(CreateException());

            console.Output.Should().Contain("Outer failure")
                .And.Contain("Inner failure")
                .And.Contain(nameof(InvalidOperationException))
                .And.Contain(nameof(ThrowForLogging));
        }

        private static Exception CreateException() => Xunit.Assert.Throws<InvalidOperationException>(ThrowForLogging);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowForLogging() => throw new InvalidOperationException(
            "Outer failure [details]", new ArgumentException("Inner failure"));
    }
}

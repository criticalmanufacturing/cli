using System.CommandLine;
using System.IO;

namespace tests
{
    /// <summary>
    /// Test console implementation for System.CommandLine beta5+
    /// Replaces the removed IConsole/TestConsole from beta4
    /// </summary>
    public class TestConsole
    {
        private readonly StringWriter _out;
        private readonly StringWriter _error;

        public TestConsole()
        {
            _out = new StringWriter();
            _error = new StringWriter();
        }

        public StringWriter Out => _out;
        public StringWriter Error => _error;
    }

    /// <summary>
    /// Command wrapper to support beta4-style Invoke(args, console) pattern
    /// </summary>
    public class CommandWrapper
    {
        private readonly Command _command;

        public CommandWrapper(Command command)
        {
            _command = command;
        }

        public int Invoke(string[] args, TestConsole console)
        {
            var parseResult = _command.Parse(args);
            return parseResult.Invoke(console);
        }

        public async System.Threading.Tasks.Task<int> InvokeAsync(string[] args, TestConsole console)
        {
            var parseResult = _command.Parse(args);
            return await parseResult.InvokeAsync(console);
        }
    }

    /// <summary>
    /// Extension methods for ParseResult to support TestConsole
    /// </summary>
    public static class ParseResultExtensions
    {
        public static int Invoke(this ParseResult parseResult, TestConsole console)
        {
            // In beta5, we need to redirect Console.Out and Console.Error temporarily
            var originalOut = System.Console.Out;
            var originalError = System.Console.Error;

            try
            {
                System.Console.SetOut(console.Out);
                System.Console.SetError(console.Error);
                return parseResult.Invoke();
            }
            finally
            {
                System.Console.SetOut(originalOut);
                System.Console.SetError(originalError);
            }
        }

        public static async System.Threading.Tasks.Task<int> InvokeAsync(this ParseResult parseResult, TestConsole console)
        {
            // In beta5, we need to redirect Console.Out and Console.Error temporarily
            var originalOut = System.Console.Out;
            var originalError = System.Console.Error;

            try
            {
                System.Console.SetOut(console.Out);
                System.Console.SetError(console.Error);
                return await parseResult.InvokeAsync();
            }
            finally
            {
                System.Console.SetOut(originalOut);
                System.Console.SetError(originalError);
            }
        }
    }
}

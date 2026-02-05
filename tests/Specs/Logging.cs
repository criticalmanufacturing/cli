using Cmf.CLI;
using Spectre.Console;
using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using Cmf.CLI.Core;
using Xunit;
using Assert = tests.AssertWithMessage;

namespace tests.Specs
{
    public class Logging
    {
        private StringWriter _writer = null;
        
        public Logging()
        {
            GetLogStringWriter();
        }

        /// <summary>
        /// create a test console and return its string writer, which will contain all the console content
        /// </summary>
        /// <returns></returns>
        public StringWriter GetLogStringWriter()
        {
            _writer = new StringWriter();
            Environment.SetEnvironmentVariable("cmf_cli_loglevel", null);

            // In beta5, Option.Parse() doesn't exist - use a temporary command to parse and trigger default value
            var tempCmd = new Command("temp");
            tempCmd.Add(LoggerHelpers.LogLevelOption);
            tempCmd.Parse(Array.Empty<string>());

            Log.AnsiConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Yes,
                // we should probably add some color testing but it's low priority, to avoid a lot of trimming we disable the colours here
                ColorSystem = (ColorSystemSupport)ColorSystem.NoColors,
                Out = new AnsiConsoleOutput(_writer),
                Interactive = InteractionSupport.No,
                Enrichment = new ProfileEnrichment
                {
                    UseDefaultEnrichers = false,
                },
            });

            return _writer;
        }

        [Fact]
        public void LogDebug_WhenVerbose()
        {
            Assert.Equal(LogLevel.Verbose, Log.Level, "Default log level is not Verbose");
            Log.Debug("testing");
            var text = _writer.ToString();
            Assert.Equal(string.Empty, text.Trim(), "Debug statement should not have been printed");
        }

        [Fact]
        public void LogDebug_WhenDebug_Assign()
        {
            // System.Environment.SetEnvironmentVariable("cmf:cli:loglevel", "debug");
            // --loglevel debug
            Log.Level = LogLevel.Debug;

            // var root = Program.Main(new string[] { "--loglevel", "debug", "ls" });
            Assert.Equal(LogLevel.Debug, Log.Level, "Log level is not Debug");
            Log.Debug("testing");
            var text = _writer.ToString();
            Assert.Equal("testing", text.Trim(), "Debug statement should not have been printed");
        }

        [Fact]
        public void LogDebug_WhenDebug_Option()
        {
            // System.Environment.SetEnvironmentVariable("cmf:cli:loglevel", "debug");
            // In beta5, Option.Parse() doesn't exist - use a temporary command to parse
            var tempCmd = new Command("temp");
            tempCmd.Add(LoggerHelpers.LogLevelOption);
            var parseResult = tempCmd.Parse(new[] { "--loglevel", "debug" });
            parseResult.GetValue(LoggerHelpers.LogLevelOption); // trigger the custom parser
            // Log.Level = LogLevel.Debug;

            // var root = Program.Main(new string[] { "--loglevel", "debug", "ls" });
            Assert.Equal(LogLevel.Debug, Log.Level, "Log level is not Debug");
            Log.Debug("testing");
            var text = _writer.ToString();
            Assert.Equal("testing", text.Trim(), "Debug statement should not have been printed");
        }

        [Fact]
        public void LogDebug_WhenDebug_Environment()
        {
            System.Environment.SetEnvironmentVariable("cmf_cli_loglevel", "debug");
            // In beta5, Option.Parse() doesn't exist - use a temporary command to parse and trigger default value
            var tempCmd = new Command("temp");
            tempCmd.Add(LoggerHelpers.LogLevelOption);
            var parseResult = tempCmd.Parse(Array.Empty<string>());
            parseResult.GetValue(LoggerHelpers.LogLevelOption); // trigger the custom parser to read env var
            // --loglevel debug
            // Log.Level = LogLevel.Debug;

            // var root = Program.Main(new string[] { "--loglevel", "debug", "ls" });
            Assert.Equal(LogLevel.Debug, Log.Level, "Log level is not Debug");
            Log.Debug("testing");
            var text = _writer.ToString();
            Assert.Equal("testing", text.Trim(), "Debug statement should not have been printed");
        }

        [Theory]
        [InlineData(LogLevel.Debug, new string[] { "debug", "verbose", "information", "warning", "error" }, new string[0])]
        [InlineData(LogLevel.Verbose, new string[] { "verbose", "information", "warning", "error" }, new string[] { "debug" })]
        [InlineData(LogLevel.Information, new string[] { "information", "warning", "error" }, new string[] { "debug", "verbose" })]
        [InlineData(LogLevel.Warning, new string[] { "warning", "error" }, new string[] { "debug", "verbose", "information" })]
        [InlineData(LogLevel.Error, new[] { "error" }, new string[] { "debug", "verbose", "information", "warning" })]
        public void LogLevelTest(LogLevel level, string[] expected, string[] notExpected)
        {
            Log.Level = level;

            Log.Debug("debug");
            Log.Verbose("verbose");
            Log.Information("information");
            Log.Warning("warning");
            Log.Error("error");

            var text = _writer.ToString();
            foreach (var exp in expected)
            {
                Assert.Contains(exp, text);
            }
            foreach (var exp in notExpected)
            {
                Assert.DoesNotContain(exp, text);
            }
        }
    }
}

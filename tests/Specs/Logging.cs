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
        private AnsiConsoleFactory factory = new AnsiConsoleFactory();
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
            Program.logLevelOption.Parse(""); // reset to default log level
            Log.AnsiConsole = factory.Create(new AnsiConsoleSettings
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
            Program.logLevelOption.Parse("--loglevel debug");
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
            Program.logLevelOption.Parse(""); // invoke rootCommand option parser to set environment value
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

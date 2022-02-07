using Cmf.Common.Cli;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spectre.Console;
using Spectre.Console.Advanced;
using Spectre.Console.Testing;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests.Specs
{
    [TestClass]
    public class Logging
    {
        private AnsiConsoleFactory factory = new AnsiConsoleFactory();
        private StringWriter _writer = null;
        [TestInitialize]
        public void Setup_Logging() 
        {
            _writer = new StringWriter();
            Environment.SetEnvironmentVariable("cmf:cli:loglevel", null);
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
        }

        [TestMethod]
        public void LogDebug_WhenVerbose()
        {
            Assert.AreEqual(LogLevel.Verbose, Log.Level, "Default log level is not Verbose");
            Log.Debug("testing");
            var text = _writer.ToString();
            Assert.AreEqual(string.Empty, text.Trim(), "Debug statement should not have been printed");
        }

        [TestMethod]
        public void LogDebug_WhenDebug_Assign()
        {
            // System.Environment.SetEnvironmentVariable("cmf:cli:loglevel", "debug");
            // --loglevel debug
            Log.Level = LogLevel.Debug;

            // var root = Program.Main(new string[] { "--loglevel", "debug", "ls" });
            Assert.AreEqual(LogLevel.Debug, Log.Level, "Log level is not Debug");
            Log.Debug("testing");
            var text = _writer.ToString();
            Assert.AreEqual("testing", text.Trim(), "Debug statement should not have been printed");
        }

        [TestMethod]
        public void LogDebug_WhenDebug_Option()
        {
            // System.Environment.SetEnvironmentVariable("cmf:cli:loglevel", "debug");
            Program.logLevelOption.Parse("--loglevel debug");
            // Log.Level = LogLevel.Debug;

            // var root = Program.Main(new string[] { "--loglevel", "debug", "ls" });
            Assert.AreEqual(LogLevel.Debug, Log.Level, "Log level is not Debug");
            Log.Debug("testing");
            var text = _writer.ToString();
            Assert.AreEqual("testing", text.Trim(), "Debug statement should not have been printed");
        }

        [TestMethod]
        public void LogDebug_WhenDebug_Environment()
        {
            System.Environment.SetEnvironmentVariable("cmf:cli:loglevel", "debug");
            Program.logLevelOption.Parse(""); // invoke rootCommand option parser to set environment value
            // --loglevel debug
            // Log.Level = LogLevel.Debug;

            // var root = Program.Main(new string[] { "--loglevel", "debug", "ls" });
            Assert.AreEqual(LogLevel.Debug, Log.Level, "Log level is not Debug");
            Log.Debug("testing");
            var text = _writer.ToString();
            Assert.AreEqual("testing", text.Trim(), "Debug statement should not have been printed");
        }

        [TestMethod]
        public void LogDebug()
        {
            LogLevelTest(LogLevel.Debug, new string[] { "debug", "verbose", "information", "warning", "error" }, new string[0]);
        }

        [TestMethod]
        public void LogVerbose()
        {
            LogLevelTest(LogLevel.Verbose, new string[] { "verbose", "information", "warning", "error" }, new string[] { "debug" });
        }

        [TestMethod]
        public void LogInformation()
        {
            LogLevelTest(LogLevel.Information, new string[] { "information", "warning", "error" }, new string[] { "debug", "verbose" });
        }

        [TestMethod]
        public void LogWarning()
        {
            LogLevelTest(LogLevel.Warning, new string[] { "warning", "error" }, new string[] { "debug", "verbose", "information" });
        }

        [TestMethod]
        public void LogError()
        {
            LogLevelTest(LogLevel.Error, new[] { "error" }, new string[] { "debug", "verbose", "information", "warning" });
        }

        //[Theory]
        //[InlineData(LogLevel.Debug, new string[] { "debug", "verbose", "information", "warning", "error" }, new string[0])]
        //[InlineData(LogLevel.Verbose, new string[] { "verbose", "information", "warning", "error" }, new string[] { "debug" })]
        //[InlineData(LogLevel.Information, new string[] { "information", "warning", "error" }, new string[] { "debug", "verbose" })]
        //[InlineData(LogLevel.Warning, new string[] { "warning", "error" }, new string[] { "debug", "verbose", "information" })]
        //[InlineData(LogLevel.Error, new[] { "error" }, new string[] { "debug", "verbose", "information", "warning" })]
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
                Assert.IsTrue(text.Contains(exp));
            }
            foreach (var exp in notExpected)
            {
                Assert.IsFalse(text.Contains(exp));
            }
        }
    }
}

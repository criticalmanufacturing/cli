using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Core
{
    public static class LoggerHelpers
    {
        private static LogLevel ParseLogLevel(ArgumentResult argResult = null)
        {
            string loglevelStr = "verbose";

            if (argResult?.Tokens.Count > 0)
            {
                loglevelStr = argResult.Tokens[0].Value;
            }
            else if (Environment.GetEnvironmentVariable("cmf_cli_loglevel") != null)
            {
                loglevelStr = Environment.GetEnvironmentVariable("cmf_cli_loglevel");
            }

            if (Environment.GetEnvironmentVariable("SYSTEM_DEBUG")?.ToBool() ?? false)
            {
                loglevelStr = "debug";
            }

            var loglevel = LogLevel.Verbose;
            if (Enum.TryParse(typeof(LogLevel), loglevelStr, ignoreCase: true, out var loglevelObj))
            {
                loglevel = (LogLevel)loglevelObj;
            }

            Log.Level = loglevel;
            return loglevel;
        }

        /// <summary>
        /// Log verbosity option that can be used in root commands
        /// </summary>
        public static readonly Option<LogLevel> LogLevelOption = new("--loglevel", "-l")
        {
            Description = "Log Verbosity",
            DefaultValueFactory = _ => ParseLogLevel(),
            CustomParser = argResult => ParseLogLevel(argResult)
        };
    }
}
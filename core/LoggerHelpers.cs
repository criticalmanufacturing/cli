using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Core
{
    public static class LoggerHelpers
    {

        /// <summary>
        /// Parse log level based on input option or environment variables
        /// </summary>
        private static ParseArgument<LogLevel> parseLogLevel = argResult =>
        {
            var loglevel = LogLevel.Verbose;
            string loglevelStr = "verbose";
            if (argResult.Tokens.Any())
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

            if (Enum.TryParse(typeof(LogLevel), loglevelStr, ignoreCase: true, out var loglevelObj))
            {
                loglevel = (LogLevel)loglevelObj!;
            }
            Log.Level = loglevel;
            return loglevel;
        };

        /// <summary>
        /// Log verbosity option that can be used in root commands
        /// </summary>
        public static Option<LogLevel> LogLevelOption = new Option<LogLevel>(
            aliases: new[] { "--loglevel", "-l" },
            description: "Log Verbosity",
            parseArgument: parseLogLevel,
            isDefault: true
        );
    }
}
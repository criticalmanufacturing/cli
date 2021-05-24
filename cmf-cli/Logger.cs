using System;

namespace Cmf.Common.Cli
{
    /// <summary>
    ///
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// The level
        /// </summary>
        public static LogLevel Level = LogLevel.Verbose;

        /// <summary>
        /// Debugs the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Debug(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Write(msg);
            Console.ResetColor();
        }

        /// <summary>
        /// Verboses the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Verbose(string msg)
        {
            Console.ResetColor();
            Write(msg);
            Console.ResetColor();
        }

        /// <summary>
        /// Informations the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Information(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Write(msg);
            Console.ResetColor();
        }

        /// <summary>
        /// Warnings the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Warning(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Write(msg);
            Console.ResetColor();
        }

        /// <summary>
        /// Errors the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(msg);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private static void Write(string msg)
        {
            Console.WriteLine($"  {msg}");
        }
    }

    /// <summary>
    ///
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// The debug
        /// </summary>
        Debug,

        /// <summary>
        /// The verbose
        /// </summary>
        Verbose,

        /// <summary>
        /// The information
        /// </summary>
        Information,

        /// <summary>
        /// The warning
        /// </summary>
        Warning,

        /// <summary>
        /// The error
        /// </summary>
        Error
    }
}
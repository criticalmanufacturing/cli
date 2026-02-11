using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Cmf.CLI.Core
{
    /// <summary>
    ///
    /// </summary>
    public static class Log
    {
        public static IAnsiConsole AnsiConsole = Spectre.Console.AnsiConsole.Console;

        /// <summary>
        /// The default log level
        /// </summary>
        public static LogLevel Level = LogLevel.Verbose;

        /// <summary>
        /// Log the message with Debug verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Debug(string msg)
        {
            if (Level <= LogLevel.Debug)
                Write($"[gray]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the message with Verbose verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Verbose(string msg)
        {
            if (Level <= LogLevel.Verbose)
                Write($"[white]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the message with Information verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Information(string msg)
        {
            if (Level <= LogLevel.Information)
                Write($"[green]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the message with Warning verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Warning(string msg)
        {
            if (Level <= LogLevel.Warning)
                Write($"[yellow]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the message with Error verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Error(string msg)
        {
            if (Level <= LogLevel.Error)
                Write($"[red]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the Exception
        /// </summary>
        /// <param name="e">an exception</param>
        public static void Exception(Exception e)
        {
            AnsiConsole.WriteException(e);
        }

        /// <summary>
        /// Logs the progress of an operation, single line with a spinner
        /// </summary>
        /// <param name="msg">initial message to print</param>
        public static void Status(string msg, Action<StatusContext>? action = null)
        {
            AnsiConsole.Status().Start(msg, ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                if (action != null)
                {
                    action(new StatusContext(ctx));
                }
            });
        }

        /// <summary>
        /// renders a renderable Spectre object (currently we use it for trees)
        /// This abstracts from the logging library (up to a point)
        /// </summary>
        /// <param name="renderable"></param>
        public static void Render(IRenderable renderable)
        {
            AnsiConsole.Write(renderable);
        }

        /// <summary>
        /// Renders a table with the given columns and rows
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        public static void RenderTable(string[] columns, List<string[]> rows)
        {
            // Create a table
            var table = new Table();

            foreach (var col in columns)
            {
                table.AddColumn(EscapeMarkup(col));
            }

            foreach (var row in rows)
            {
                var _row = new string[row.Length];
                for (int i = 0; i < row.Length; i++)
                {
                    _row[i] = EscapeMarkup(row[i]);
                }
                table.AddRow(_row);
            };

            // Render the table to the console
            Render(table);
        }

        /// <summary>
        /// Abstracts from the logging library
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private static void Write(string msg) => AnsiConsole.MarkupLine($"  {msg}");

        /// <summary>
        /// Escapes Spectre Console markup
        /// Markup is enclosed in [] and escaped as [[]]
        /// This is a very naive operation that will likely need extending in the future
        /// </summary>
        /// <param name="msg">message to be escaped</param>
        /// <returns>escaped message</returns>
        private static string EscapeMarkup(string? msg) => (msg ?? string.Empty).Replace("[", "[[").Replace("]", "]]");
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

    /// <summary>
    /// Wrapper for the SpectreConsole StatusContext object so we do not pollute all classes with Spectre imports
    /// </summary>
    public class StatusContext
    {
        private Spectre.Console.StatusContext context;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="context">the StatusContext object</param>
        public StatusContext(Spectre.Console.StatusContext context)
        {
            this.context = context;
        }
        
        //
        // Summary:
        //     Sets the status message.
        //
        // Parameters:
        //
        //   status:
        //     The status message.
        //
        // Returns:
        //     The same instance so that multiple calls can be chained.
        public StatusContext Status(string status)
        {
            context.Status = status;
            return this;
        }
    }
}
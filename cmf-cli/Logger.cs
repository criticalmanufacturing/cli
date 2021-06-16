using System;
using System.CommandLine;
using System.ComponentModel;
using System.Linq;

namespace Cmf.Common.Cli
{
    /// <summary>
    ///
    /// </summary>
    public static class Log
    {
        //private static IConsole console;

        //private static bool ConsoleSupportsColor => (Log.console.GetType() is System.Console);

        //private static bool ConsoleSupportsCursos => (Log.console.GetType() is System.Console);

        //public static void SetConsole(IConsole console) => Log.console = console;

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

        private static BackgroundWorker worker = null;
        private static string curMsg = string.Empty;
        
        /// <summary>
        /// Logs the progress of an operation, single line with a spinner
        /// TODO: this needs a better API and implementation, it's a mess
        /// </summary>
        /// <param name="msg">message to print</param>
        /// <param name="end">should stop the spinner and clear the console line</param>
        public static void Progress(string msg, bool end = false)
        {
            if (Console.LargestWindowHeight > 0) // no console available
            {
                curMsg = msg;
                var spinnerPosition = Console.CursorLeft;
                if (worker == null)
                {
                    worker = new BackgroundWorker();
                    worker.DoWork += delegate (object _, DoWorkEventArgs e)
                    {
                        Console.CursorVisible = false;
                        while (!worker.CancellationPending)
                        {

                            char[] spinChars = new char[] { '|', '/', '-', '\\' };
                            foreach (char spinChar in spinChars)
                            {
                                Console.CursorLeft = spinnerPosition;
                                Console.Write(spinChar);
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write($" {curMsg}{string.Join("", Enumerable.Repeat<char>(' ', Console.BufferWidth - curMsg.Length - 3).ToArray())}");
                                Console.ResetColor();
                                System.Threading.Thread.Sleep(50);
                            }
                        }
                        e.Cancel = true;
                        e.Result = "done";
                    };
                    worker.WorkerSupportsCancellation = true;
                    worker.RunWorkerAsync();
                }
                if (end)
                {
                    worker.CancelAsync();
                    // according to docs, Cancel should trigger RunWorkerCompleted, but in .NET 5 I could not do anything complex there
                    // such as wiping the last line. This needs more debugging.
                    // So the thread sleep here allows for the worker to finish printing
                    System.Threading.Thread.Sleep(100);
                    // this clears the last line
                    Console.CursorVisible = true;
                    int currentLineCursor = Console.CursorTop;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.BufferWidth));
                    Console.SetCursorPosition(0, currentLineCursor);
                    // this allows the console to reset before we actually continue printing stuff
                    System.Threading.Thread.Sleep(100);
                }
            }
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
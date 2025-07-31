using System;
using System.Threading.Tasks;

namespace Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Common
{
    /// <summary>
    /// IoT Logger Handling class
    /// </summary>
    public class IoTLogger
    {
        private Func<object, Task<object>> m_LogHandler = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">NodeJs Logger callback function</param>
        public IoTLogger(Func<object, Task<object>> logger)
        {
            m_LogHandler = logger;
        }

        /// <summary>Log message using the JS connection</summary>
        /// <param name="verbosity">Verbosity of the log message</param>
        /// <param name="text">Log message</param>
        /// <param name="args">Parameters to format the message</param>
        public void Log(string verbosity, string text, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                m_LogHandler?.Invoke(new { verbosity = verbosity, message = text });
            }
            else
            {
                m_LogHandler?.Invoke(new { verbosity = verbosity, message = string.Format(text, args) });
            }
        }

        public void Error(string text, params object[] args) { Log("error", text, args); }
        public void Info(string text, params object[] args) { Log("info", text, args); }
        public void Warning(string text, params object[] args) { Log("warning", text, args); }
        public void Debug(string text, params object[] args) { Log("debug", text, args); }
    }
}

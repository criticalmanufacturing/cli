using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    ///
    /// </summary>
    public interface IProcessStartInfoCLI : IDisposable
    {
        public Process? Start();
        public Process? Process { get; set; }

        public string FileName { get; set; }
        public string WorkingDirectory { get; set; }
        public string Arguments { get; set; }
        public bool UseShellExecute { get; set; }
        public bool RedirectStandardOutput { get; set; }
        public bool RedirectStandardError { get; set; }
        public StringDictionary EnvironmentVariables { get; }

        public void AddEventOutDataReceived(DataReceivedEventHandler handler);
        public void AddEvenErrorDataReceived(DataReceivedEventHandler handler);
        public void BeginOutputReadLine();
        public void BeginErrorReadLine();
        public void WaitForExit();

        public int ExitCode { get; }
    }
}
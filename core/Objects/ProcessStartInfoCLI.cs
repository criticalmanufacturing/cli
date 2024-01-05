using System.Collections.Specialized;
using System.Diagnostics;

namespace Cmf.CLI.Core.Objects
{
    /// <summary>
    ///
    /// </summary>
    public class ProcessStartInfoCLI : IProcessStartInfoCLI
    {
        private ProcessStartInfo ps;
        public Process Process { get; set; }

        public ProcessStartInfoCLI()
        {
            this.ps = new ProcessStartInfo();
        }

        public ProcessStartInfoCLI(string fileName)
        {
            this.ps = new ProcessStartInfo(fileName);
        }

        public ProcessStartInfoCLI(string fileName, string arguments)
        {
            this.ps = new ProcessStartInfo(fileName, arguments);
        }

        public Process Start()
        {
            return StartProcess();
        }

        public void AddEvenErrorDataReceived(DataReceivedEventHandler handler)
        {
            this.Process.ErrorDataReceived += handler;
        }

        public void AddEventOutDataReceived(DataReceivedEventHandler handler)
        {
            this.Process.OutputDataReceived += handler;
        }

        protected Process StartProcess()
        {
            this.Process = System.Diagnostics.Process.Start(this.ps);
            return this.Process;
        }

        public void BeginOutputReadLine()
        {
            this.Process.BeginOutputReadLine();
        }

        public void BeginErrorReadLine()
        {
            this.Process.BeginErrorReadLine();
        }

        public void WaitForExit()
        {
            this.Process.WaitForExit();
        }

        public void Dispose()
        {
            this.Process.Dispose();
        }

        public string FileName
        {
            get => this.ps.FileName;
            set => this.ps.FileName = value;
        }
        public string WorkingDirectory
        {
            get => this.ps.WorkingDirectory;
            set => this.ps.WorkingDirectory = value;
        }
        public string Arguments
        {
            get => this.ps.Arguments;
            set => this.ps.Arguments = value;
        }
        public bool UseShellExecute
        {
            get => this.ps.UseShellExecute;
            set => this.ps.UseShellExecute = value;
        }
        public bool RedirectStandardOutput
        {
            get => this.ps.RedirectStandardOutput;
            set => this.ps.RedirectStandardOutput = value;
        }
        public bool RedirectStandardError
        {
            get => this.ps.RedirectStandardError;
            set => this.ps.RedirectStandardError = value;
        }
        public StringDictionary EnvironmentVariables
        {
            get => this.ps.EnvironmentVariables;
        }

        public int ExitCode { get => this.Process.ExitCode; }

    }
}
using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="BaseCommand" />
    public abstract class PowershellCommand : BaseCommand
    {
        /// <summary>
        /// Gets the a remote pwsh runspace.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <returns></returns>
        protected static Runspace GetRunspace(string hostname)
        {
            // Create a WSManConnectionInfo object using the default constructor
            // to connect to the "localHost". The WSManConnectionInfo object can
            // also specify connections to remote computers.
            var host = $"http://{hostname}:5985/WSMAN";
            Uri uri = new(host);
            WSManConnectionInfo connectionInfo = new(uri);

            // Set the OperationTimeout property. The OperationTimeout is used to tell
            // Windows PowerShell how long to wait (in milliseconds) before timing out
            // for any operation. This includes sending input data to the remote computer,
            // receiving output data from the remote computer, and more. The user can
            // change this timeout depending on whether the connection is to a computer
            // in the data center or across a slow WAN.
            connectionInfo.OperationTimeout = 4 * 60 * 1000; // 4 minutes.

            // Set the OpenTimeout property. OpenTimeout is used to tell Windows PowerShell
            // how long to wait (in milliseconds) before timing out while establishing a
            // remote connection. The user can change this timeout depending on whether the
            // connection is to a computer in the data center or across a slow WAN.
            connectionInfo.OpenTimeout = 1 * 60 * 1000; // 1 minute.

            // Create a remote runspace using the connection information.
            Runspace remoteRunspace = RunspaceFactory.CreateRunspace(connectionInfo);
            // Establish the connection by calling the Open() method to open the runspace.
            // The OpenTimeout value set previously will be applied while establishing
            // the connection. Establishing a remote connection involves sending and
            // receiving some data, so the OperationTimeout will also play a role in this process.
            // remoteRunspace.Open();
            return remoteRunspace;
        }

        /// <summary>
        /// Executes the PWSH script asynchronously.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="script">The script.</param>
        /// <param name="hostname">The hostname.</param>
        /// <returns></returns>
        protected async Task<PSDataCollection<PSObject>> ExecutePwshScriptAsync(IDictionary parameters = null, string script = null, string hostname = null)
        {
            using Runspace runspace = hostname != null ? GetRunspace(hostname) : null;
            runspace?.Open();
            using PowerShell invoker = runspace != null ? PowerShell.Create(runspace: runspace) : PowerShell.Create();

            const string getverbose = "$verbosepreference='continue';InformationPreference='continue';ProgressPreference='continue'";
            invoker.AddScript(string.Format(getverbose));
            invoker.Invoke();
            invoker.Commands.Clear();
            // specify the script code to run.
            invoker.AddScript(script ?? this.GetPowershellScript());
            // specify the parameters to pass into the script.
            if (parameters != null)
            {
                invoker.AddParameters(parameters: parameters);
            }

            EventHandler<DataAddedEventArgs> handler = (sender, args) =>
            {
                var record = (sender as IList)[args.Index];
                switch (record)
                {
                    case ErrorRecord errorRecord:
                        Console.WriteLine("[{0}] {1}{2}{3}",
                            errorRecord.GetType().Name[0],
                            errorRecord.Exception.Message,
                            Environment.NewLine,
                            errorRecord.ScriptStackTrace);
                        break;

                    case InformationalRecord informationalRecord:
                        Console.WriteLine("[{0}] {1}", informationalRecord?.GetType().Name[0], informationalRecord.Message);
                        break;

                    case ProgressRecord progressRecord:
                        Console.WriteLine("[{0}] {1}", progressRecord?.GetType().Name[0], progressRecord);
                        break;
                }
            };

            invoker.Streams.Error.DataAdded += handler;
            invoker.Streams.Verbose.DataAdded += handler;
            invoker.Streams.Progress.DataAdded += handler;
            invoker.Streams.Warning.DataAdded += handler;
            invoker.Streams.Debug.DataAdded += handler;
            invoker.Streams.Information.DataAdded += handler;

            // execute the script and await the result.
            var pipelineObjects = await invoker.InvokeAsync().ConfigureAwait(false);

            invoker.Streams.Error.DataAdded -= handler;
            invoker.Streams.Verbose.DataAdded -= handler;
            invoker.Streams.Progress.DataAdded -= handler;
            invoker.Streams.Warning.DataAdded -= handler;
            invoker.Streams.Debug.DataAdded -= handler;
            invoker.Streams.Information.DataAdded -= handler;

            runspace?.Close();

            return pipelineObjects;
        }

        /// <summary>
        /// Executes the PWSH script synchronously.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="script">The script.</param>
        /// <param name="hostname">The hostname.</param>
        /// <returns></returns>
        protected PSDataCollection<PSObject> ExecutePwshScriptSync(IDictionary parameters = null, string script = null, string hostname = null)
        {
            var task = this.ExecutePwshScriptAsync(parameters, script, hostname);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Gets the powershell script.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetPowershellScript();
    }
}
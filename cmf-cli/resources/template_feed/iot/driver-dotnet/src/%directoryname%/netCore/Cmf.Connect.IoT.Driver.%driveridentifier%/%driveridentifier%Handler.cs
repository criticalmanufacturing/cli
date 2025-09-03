using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Common;
using Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.DriverObjects;
using Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Extensions;

namespace Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>
{
    public class <%= $CLI_PARAM_Identifier %>Handler
    {
        // Event handlers in JS side
        private Func<object, Task<object>> m_ConnectedHandler = null;

        private Func<object, Task<object>> m_DisconnectedHandler = null;

        #region Constructors

        /// <summary>Create new instance of the <%= $CLI_PARAM_Identifier %> handler</summary>
        public <%= $CLI_PARAM_Identifier %>Handler()
        {
            try
            {
                string m_BaseLocation = Path.GetFullPath(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            }
            catch
            {
                Shared.Log.Error("Error creating <%= $CLI_PARAM_Identifier %>Handler!");
            }
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>Register a handler to receive the event occurrence</summary>
        /// <param name="inputValues">object with the following structure:
        ///   eventName (string): Name of the event to register
        ///   callback (function): Handler of the event
        /// </param>
        /// <returns>Boolean indicating result</returns>
        public async Task<object> RegisterEventHandler(dynamic inputValues)
        {
            var input = (IDictionary<string, object>)inputValues;

            var eventName = input.Get("eventName", "").ToLowerInvariant();
            var handler = input.Get<Func<object, Task<object>>>("callback", null);

            switch (eventName)
            {
                case "log": Shared.Log = new IoTLogger(handler); break;
                case "connect": m_ConnectedHandler = handler; break;
                case "disconnect": m_DisconnectedHandler = handler; break;

                default: Shared.Log.Error("Unknown event '{0}'", eventName); return (false);
            }

            Shared.Log.Info("Registered <%= $CLI_PARAM_Identifier %> driver event '{0}'", eventName);

            return (true);
        }

        /// <summary>Connect to the equipment</summary>
        /// <param name="inputValues">object containing all parameters. (TODO: document them)
        /// </param>
        /// <returns>Boolean indicating result</returns>
        public async Task<object> Connect(dynamic inputValues)
        {
            IDictionary<string, object> input = (IDictionary<string, object>)inputValues;

            Shared.Settings.Load(input);
            // Dump configuration for debug purposes
            Shared.Log.Debug("Communication parameters: {0}", Shared.Settings.Dump());

            // Add here your Connection Logic
            // ...

            return true;
        }

        /// <summary>Disconnect from the equipment</summary>
        /// <param name="inputValues">Not used</param>
        /// <returns>Boolean indicating result</returns>
        public async Task<object> Disconnect(dynamic inputValues)
        {
            IDictionary<string, object> input = (IDictionary<string, object>)inputValues;
            return true;
        }

        /// <summary>Register event to register in the Server</summary>
        /// <param name="inputValues">object with the following structure:
        ///   eventId: Id of the event to register the listener
        ///   callback: Method that will be called when the reply is received
        /// <returns>Boolean indicating result</returns>
        public async Task<object> RegisterEvent(dynamic inputValues)
        {
            IDictionary<string, object> input = (IDictionary<string, object>)inputValues;

            string eventId = input.Get<string>("eventId", "");
            Func<object, Task<object>> callback = input.Get<Func<object, Task<object>>>("callback", null);

            if (string.IsNullOrEmpty(eventId))
            {
                throw new Exception("Unable to identify the event to register");
            }

            Shared.EventDispatcher.RegisterEventHandler(eventId, callback);

            Shared.Log.Info($"Registered event '{eventId}' with success!");
            return (true);
        }

        /// <summary>Unregister event from the Server</summary>
        /// <param name="inputValues">object with the following structure:
        ///   eventId: Id of the event to unregister
        /// <returns>Boolean indicating result</returns>
        public async Task<object> UnregisterEvent(dynamic inputValues)
        {
            IDictionary<string, object> input = (IDictionary<string, object>)inputValues;

            string eventId = input.Get<string>("eventId", "");
            if (string.IsNullOrEmpty(eventId))
            {
                throw new Exception("Unable to identify the event to register");
            }

            Shared.EventDispatcher.UnregisterEventHandler(eventId);

            Shared.Log.Info($"Unregistered event '{eventId}' with success!");
            return (true);
        }

        //#if hasCommands
        /// <summary>Execute Command</summary>
        /// <param name="inputValues">object with the following structure:
        ///   commandId: String with CommandId
        /// <returns>Command reply data</returns>
        public async Task<object> ExecuteCommand(dynamic inputValues)
        {
            IDictionary<string, object> input = (IDictionary<string, object>)inputValues;

            // Add here the Execute Command Logic
            // ...
            return true;
        }
        //#endif

        #endregion Public Methods

        #region Private & Internal Methods

        #endregion Private & Internal Methods
    }
}

using Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.DriverObjects
{
    public class CommunicationSettings
    {
        /// <summary>Protocol used to communicate with the endpoints</summary>
        public string NetCoreSdkVersion { get; set; } = "";

        /// <summary>
        /// Load the settings from json format to the internal structure
        /// </summary>
        /// <param name="settings">List of settings</param>
        public void Load(IDictionary<string, object> settings)
        {
            NetCoreSdkVersion = settings.Get("netCoreSdkVersion", NetCoreSdkVersion);
        }

        /// <summary>
        /// Dump the configuration settings to log for future validation
        /// </summary>
        /// <returns></returns>
        public string Dump()
        {
            StringBuilder cfgDump = new StringBuilder();
            cfgDump.AppendLine();
            cfgDump.AppendLine($"NetCoreSdkVersion: '{NetCoreSdkVersion}'");

            return (cfgDump.ToString());
        }
    }
}

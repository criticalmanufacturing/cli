using Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Common;
using Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.DriverObjects;

namespace Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Common
{
    /// <summary>
    /// Static/Shared Instances
    /// </summary>
    public static class Shared
    {
        public static CommunicationSettings Settings = new CommunicationSettings();
        public static IoTLogger Log;
        public static EventDispatcher EventDispatcher = new EventDispatcher();
    }
}
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Extensions;

namespace Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.DriverObjects
{
    /// <summary>
    /// Class responsible to keep track of the listeners waiting for event notifications
    /// </summary>
    public class EventDispatcher
{
    private ConcurrentDictionary<string, Func<object, Task<object>>> m_EventListeners = new ConcurrentDictionary<string, Func<object, Task<object>>>(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Register a new listener (only one for each event id is supported)
    /// </summary>
    /// <param name="eventId">Id of the event to register</param>
    /// <param name="callback">Callback for the handler</param>
    public void RegisterEventHandler(string eventId, Func<object, Task<object>> callback)
    {
        this.m_EventListeners[eventId] = callback;
    }

    /// <summary>
    /// Unregister a previously registered event handler
    /// </summary>
    /// <param name="eventId">Id of the event to unregister</param>
    public void UnregisterEventHandler(string eventId)
    {
        this.m_EventListeners.TryRemove(eventId);
    }

    /// <summary>
    /// Report to any previously registered event handler about the event occurrence
    /// </summary>
    /// <param name="eventData">Event occurrence data</param>
    public void TriggerEvent(dynamic eventData)
    {
        // Replace object with your EventOccurrence Object
        // ...

        var callback = this.m_EventListeners.GetValue((string)eventData.EventId, null);
        if (callback != null)
        {
            callback(eventData);
        }
    }

    /// <summary>
    /// Checks if a specific eventid is registered on the driver.
    /// </summary>
    /// <param name="eventId">Id of the event to check</param>
    /// <returns></returns>
    public bool IsEventIdRegistered(string eventId)
    {
        return (this.m_EventListeners.ContainsKey(eventId));
    }
}

}

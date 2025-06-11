using System;
using System.Threading.Tasks;

namespace Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>
{
    public class Startup
	{
		/// <summary>
		/// Create a new a Handler and return an object containing methods to call
		/// </summary>
		/// <param name="obj">Not used</param>
		/// <returns></returns>
		public async Task<object> CreateHandler(object obj)
		{
			var handler = new <%= $CLI_PARAM_Identifier %>Handler();
			return new
			{
				RegisterEventHandler = (Func<object, Task<object>>)(
					async (input) =>
					{
						return await handler.RegisterEventHandler(input);
					}
				),

				//GetValues = (Func<object, Task<object>>)(
				//    async (input) =>
				//    {
				//        return await handler.GetValues(input);
				//    }
				//),
				//SetValues = (Func<object, Task<object>>)(
				//    async (input) =>
				//    {
				//        return await handler.SetValues(input);
				//    }
				//),

				//#if hasCommands
				ExecuteCommand = (Func<object, Task<object>>)(
					async (input) =>
					{
						return await handler.ExecuteCommand(input);
					}
				),
				//#endif

				RegisterEvent = (Func<object, Task<object>>)(
					async (input) =>
					{
						return await handler.RegisterEvent(input);
					}
				),
				UnregisterEvent = (Func<object, Task<object>>)(
					async (input) =>
					{
						return await handler.UnregisterEvent(input);
					}
				),
				Connect = (Func<object, Task<object>>)(
					async (input) =>
					{
						return await handler.Connect(input);
					}
				),
				Disconnect = (Func<object, Task<object>>)(
					async (input) =>
					{
						return await handler.Disconnect(input);
					}
				),
			};
		}
	}
}

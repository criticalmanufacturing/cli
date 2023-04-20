using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods;

public class ValidateLogger : IValidateLogger
{
	public void Warning(string message)
		=> Cmf.CLI.Core.Log.Warning(message);
}
namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods;

public static class ErrorMessages
{
	/// <summary>
	/// {0} -> StartMethod or EndMethod
	/// {1} -> class name
	/// {2} -> method name
	/// {3} -> current number of parameters
	/// {4} -> expected number of parameters
	/// </summary>
	public static string MethodHasIncorrectNumberOfParameters => "{0}, File: {1}, Method: {2}, Issue: Incorrect number of parameters parameters. Current: {3} :: Expected {4}.";

	/// <summary>
	/// {0} -> StartMethod or EndMethod
	/// {2} -> class name
	/// {3} -> method name
	/// </summary>
	public static string MethodHasNoParameters => "{0}, File: {1}, Method: {2}, Issue: No parameters.";

	/// <summary>
	/// {0} -> StartMethod or EndMethod
	/// {1} -> class name
	/// {2} -> method name
	/// {3} -> method name
	/// </summary>
	public static string MethodParameterMissingOrIncorrectlyNamed => "{0}, File: {1}, Method: {2}, Issue: Missing parameter {3}.";

	/// <summary>
	/// {0} -> class name
	/// {1} -> method name
	/// </summary>
	public static string StartMethodDoesNotContainMethodName => "StartMethod, File: {0}, Method: {1}, Issue: Does not contain a method name.";

	/// <summary>
	/// {0} -> class name
	/// {1} -> method name
	/// </summary>
	public static string StartMethodMethodNameIsIncorrect => "StartMethod, File: {0}, Method: {1}, Issue: Incorrect method name.";

	/// <summary>
	/// {0} -> class name
	/// {1} -> method name
	/// </summary>
	public static string InputOutputMethodDoNotCoincide => "File: {0}, Method: {1}, Issue: InputObject, OutputObject are different.";
}
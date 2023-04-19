namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods;

public static class ErrorMessages
{
    /// <summary>
    /// {0} -> StartMethod or EndMethod
    /// {1} -> namespace name
    /// {2} -> class name
    /// {3} -> method name
    /// {4} -> current number of parameters
    /// {5} -> expected number of parameters
    /// </summary>
    public static string MethodHasIncorrectNumberOfParameters => "{0}, File: {2}, Method: {3}, Issue: Incorrect number of parameters parameters. Current: {4} :: Expected {5}.";

    /// <summary>
    /// {0} -> StartMethod or EndMethod
    /// {1} -> namespace name
    /// {2} -> class name
    /// {3} -> method name
    /// </summary>
    public static string MethodHasNoParameters => "{0}, File: {2}, Method: {3}, Issue: No parameters.";

    /// <summary>
    /// {0} -> StartMethod or EndMethod
    /// {1} -> namespace name
    /// {2} -> class name
    /// {3} -> method name
    /// {4} -> method name
    /// </summary>
    public static string MethodParameterMissingOrIncorrectlyNamed => "{0}, File: {2}, Method: {3}, Issue: Missing parameter {4}.";

    /// <summary>
    /// {0} -> namespace name
    /// {1} -> class name
    /// {2} -> method name
    /// </summary>
    public static string StartMethodDoesNotContainMethodName => "StartMethod, File: {1}, Method: {2}, Issue: Does not contain a method name.";

    /// <summary>
    /// {0} -> namespace name
    /// {1} -> class name
    /// {2} -> method name
    /// </summary>
    public static string StartMethodMethodNameIsIncorrect => "StartMethod, File: {1}, Method: {2}, Issue: Incorrect method name.";
}
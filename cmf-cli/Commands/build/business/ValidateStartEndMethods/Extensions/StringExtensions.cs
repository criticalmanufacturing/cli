using System.Text;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class StringExtensions
{
    public static string ToLower(this string value, int numberOfChars)
    {
        int i = 0;
        StringBuilder sb = new StringBuilder();

        for (i = 0; i < numberOfChars && i < value.Length; i++)
        {
            sb.Append(char.ToLower(value[i]));
        }

        sb.Append(value.Substring(i));

        return sb.ToString();
    }

    public static bool EqualsToItselfOrNameOfItself(this string value1, string value2)
    {
        return value1.Equals(value2) || value1.Equals($"nameof({value2})");
    }
}
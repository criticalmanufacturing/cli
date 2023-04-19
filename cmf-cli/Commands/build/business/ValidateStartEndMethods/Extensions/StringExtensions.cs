using System.Text;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class StringExtensions
{
    public static string ToLower(this string value, int numberOfChars)
    {
		return ToUpperOrLower(value, numberOfChars, true);
    }

	public static string ToUpper(this string value, int numberOfChars)
	{
		return ToUpperOrLower(value, numberOfChars, false);
	}

    private static string ToUpperOrLower(string value, int numberOfChars, bool toLower)
    {
		int i = 0;
		StringBuilder sb = new StringBuilder();

		for (i = 0; i < numberOfChars && i < value.Length; i++)
		{
			if (toLower)
			{
				sb.Append(char.ToLower(value[i]));
			}
			else
			{
				sb.Append(char.ToUpper(value[i]));
			}			
		}

		sb.Append(value.Substring(i));

		return sb.ToString();
	}

	public static bool EqualsToItselfOrNameOfItself(this string value1, string value2)
    {
        return value1.Equals(value2) || value1.Equals($"nameof({value2})");
    }
}
using System;
using System.Globalization;

namespace Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>Compare two strings in insensitive case</summary>
        /// <param name="str">The first string</param>
        /// <param name="compare">The second string</param>
        /// <returns>True if they are equal, false otherwise</returns>
        internal static bool IsEqualTo(this string str, string compare)
        {
            if (str == null && compare == null)
                return (true);
            if (compare == null || str == null)
                return (false);

            return (String.Compare(str, compare, StringComparison.InvariantCultureIgnoreCase) == 0);
        }


        /// <summary>
        /// Convert a string text into a decimal value
        /// </summary>
        /// <param name="str">string value of the decimal value</param>
        /// <param name="Default">Default value to use if the string cannot be converted</param>
        /// <returns>The decimal value of the string or the default value is the string is unable to be converted</returns>
        internal static decimal ToDecimal(this string str, decimal Default = (decimal)0)
        {
            decimal dResult;
            if (!decimal.TryParse(str.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out dResult))
                dResult = Default;

            return (dResult);
        }

        /// <summary>
        /// Convert a string text into a float value
        /// </summary>
        /// <param name="str">string value of the float value</param>
        /// <param name="Default">Default value to use if the string cannot be converted</param>
        /// <returns>The float value of the string or the default value is the string is unable to be converted</returns>
        internal static float ToFloat(this string str, float Default = 0.0f)
        {
            return ((float)(ToDouble(str, Convert.ToDouble(Default))));
        }

        /// <summary>
        /// Convert a string text into a double value
        /// </summary>
        /// <param name="str">string value of the double value</param>
        /// <param name="Default">Default value to use if the string cannot be converted</param>
        /// <returns>The double value of the string or the default value is the string is unable to be converted</returns>
        internal static double ToDouble(this string str, double Default = 0.0d)
        {
            double dResult;
            if (!double.TryParse(str.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out dResult))
                dResult = Default;

            return (dResult);
            //return (Convert.ToDouble(ToDecimal(str, Convert.ToDecimal(Default))));
        }

        /// <summary>
        /// Convert a string text into an int value
        /// </summary>
        /// <param name="str">string value of the int value</param>
        /// <param name="Default">Default value to use if the string cannot be converted</param>
        /// <returns>The int value of the string or the default value is the string is unable to be converted</returns>
        internal static int ToInt(this string str, int Default = 0)
        {
            int iResult;
            if (!int.TryParse(str, out iResult))
                iResult = Default;

            return (iResult);
        }

        /// <summary>
        /// Convert a string text into a long value
        /// </summary>
        /// <param name="str">string value of the long value</param>
        /// <param name="Default">Default value to use if the string cannot be converted</param>
        /// <returns>The long value of the string or the default value is the string is unable to be converted</returns>
        internal static long ToLong(this string str, long Default = 0)
        {
            long lResult;
            if (!long.TryParse(str, out lResult))
                lResult = Default;

            return (lResult);
        }

        internal static bool IsInt(this string str)
        {
            int dummy;
            return (int.TryParse(str, out dummy));
        }
        internal static bool IsLong(this string str)
        {
            long dummy;
            return (long.TryParse(str, out dummy));
        }
        internal static bool IsDecimal(this string str)
        {
            decimal dummy;
            return (decimal.TryParse(str, out dummy));
        }
        internal static bool IsFloat(this string str)
        {
            float dummy;
            return (float.TryParse(str, out dummy));
        }
        internal static bool IsDouble(this string str)
        {
            double dummy;
            return (double.TryParse(str, out dummy));
        }

        /// <summary>
        /// Convert a string text into a boolean value
        /// </summary>
        /// <param name="str">string value of the boolean value</param>
        /// <param name="Default">Default value to use if the string cannot be converted</param>
        /// <returns>The boolean value of the string or the default value is the string is unable to be converted</returns>
        internal static bool ToBoolean(this string str, bool Default = false)
        {
            bool bResult;
            if (!Boolean.TryParse(str, out bResult))
            {
                // Check for some admissable values
                str = str.Trim().ToLower();
                if (str == "1" || str == "y" || str == "yes" || str == "true" || str == "t")
                    return (true);

                if (str == "0" || str == "n" || str == "no" || str == "false" || str == "f")
                    return (false);

                bResult = Default;
            }

            return (bResult);
        }
    }
}

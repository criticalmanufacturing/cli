using System;
using System.Collections.Generic;
using System.Linq;

namespace Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Extensions
{
    internal static class DictionaryExtensions
    {
        internal static bool HasKey<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            if (typeof(TKey) == typeof(string))
                return (dic.FirstOrDefault(d => d.Key.ToString().IsEqualTo(key.ToString())).Key != null);
            else
                return (dic.ContainsKey(key));
        }

        internal static T Get<T>(this IDictionary<string, object> dic, string key, T defaultValue)
        {
            if (!dic.HasKey(key))
                return (defaultValue);

            KeyValuePair<string, object> entry = dic.FirstOrDefault(d => d.Key.IsEqualTo(key));

            // Check nullable types and null value
            bool bCanBeNull = typeof(T).IsGenericParameter && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
            if (entry.Value == null && entry.Key != null && (typeof(T) == typeof(string) || bCanBeNull))
                return (default(T));

            if (entry.Value != null)
            {

                Type t = typeof(T);
                Type u = Nullable.GetUnderlyingType(t);

                try
                {
                    if (u != null)
                    {
                        return (T)Convert.ChangeType(entry.Value, u);
                    }
                    else
                    {
                        return (T)Convert.ChangeType(entry.Value, t);
                    }
                }
                catch (Exception ex)
                {
                    // Different types, return the default value
                }
            }

            return (defaultValue);
        }

        /// <summary>
        /// Same as Get method but safer to convert the default value using internal extensions
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key">Key to find</param>
        /// <param name="defaultValue">Default value to use if conversions fail</param>
        /// <returns></returns>
        public static bool GetValue(this IDictionary<string, object> dic, string key, bool defaultValue)
        {
            return (dic.Get(key, defaultValue).ToString().ToBoolean(defaultValue));
        }

        /// <summary>
        /// Same as Get method but safer to convert the default value using internal extensions
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key">Key to find</param>
        /// <param name="defaultValue">Default value to use if conversions fail</param>
        /// <returns></returns>
        public static int GetValue(this IDictionary<string, object> dic, string key, int defaultValue)
        {
            return (dic.Get(key, defaultValue).ToString().ToInt(defaultValue));
        }

        /// <summary>
        /// Same as Get method but safer to read enumerations
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key">Key to find</param>
        /// <param name="defaultValue">Default value to use if conversions fail</param>
        /// <returns></returns>
        public static T GetEnum<T>(this IDictionary<string, object> dic, string key, T defaultValue) where T : struct
        {
            if (Enum.TryParse<T>(dic.Get(key, defaultValue.ToString()), out T res))
            {
                return (res);
            }
            else
            {
                return (defaultValue);
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Extensions
{
    internal static class ConcurrentDictionaryExtension
    {
        // Either Add or overwrite
        internal static void AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }

        internal static bool TryRemove<K, V>(this ConcurrentDictionary<K, V> dictionary, K key)
        {
            V removed;
            return (dictionary.TryRemove(key, out removed));
        }

        internal static V GetValue<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V defaultValue)
        {
            dictionary.TryGetValue(key, out defaultValue);

            return (defaultValue);
        }

        public static bool Contains<K, V>(this ConcurrentDictionary<K, V> dictionary, Func<System.Collections.Generic.KeyValuePair<K, V>, bool> predicate)
        {
            return (dictionary.FirstOrDefault(predicate).Value != null);
        }

    }
}

using System.Collections.Generic;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class ListExtensions
{
    internal static bool ContainsKeyValue<TKey, TValue>(this IList<KeyValuePair<TKey, TValue>> list, TKey? key, TValue? value)
    {
        if (key is null || value is null)
            return false;

        return list.Contains(new KeyValuePair<TKey, TValue>(key, value));
    }
}
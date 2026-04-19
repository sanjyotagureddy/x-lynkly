namespace Lynkly.Shared.Kernel.Core.Helpers.Collections;

/// <summary>
/// Provides dictionary utility methods.
/// </summary>
public static class DictionaryHelper
{
    /// <summary>
    /// Gets an existing value or adds one if absent.
    /// </summary>
    public static TValue GetOrAdd<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> factory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(factory);

        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        var created = factory();
        dictionary[key] = created;
        return created;
    }

    /// <summary>
    /// Returns dictionary value or provided fallback.
    /// </summary>
    public static TValue? GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue? fallback = default)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        return dictionary.TryGetValue(key, out var value) ? value : fallback;
    }
}

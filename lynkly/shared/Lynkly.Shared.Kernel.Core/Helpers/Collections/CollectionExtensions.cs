namespace Lynkly.Shared.Kernel.Core.Helpers.Collections;

/// <summary>
/// Provides collection extension methods.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Determines whether sequence is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        if (source is null)
        {
            return true;
        }

        if (source is ICollection<T> collection)
        {
            return collection.Count == 0;
        }

        if (source is IReadOnlyCollection<T> readOnlyCollection)
        {
            return readOnlyCollection.Count == 0;
        }

        return !source.Any();
    }

    /// <summary>
    /// Filters null values from a nullable sequence.
    /// </summary>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.OfType<T>();
    }

    /// <summary>
    /// Executes an action for each item in the sequence.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
        }
    }
}

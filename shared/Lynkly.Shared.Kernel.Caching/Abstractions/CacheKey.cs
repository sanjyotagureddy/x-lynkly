namespace Lynkly.Shared.Kernel.Caching.Abstractions;

public readonly record struct CacheKey<TValue>
{
    public CacheKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Cache key cannot be null, empty, or whitespace.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static implicit operator string(CacheKey<TValue> key)
    {
        return key.Value;
    }

    public override string ToString()
    {
        return Value;
    }
}

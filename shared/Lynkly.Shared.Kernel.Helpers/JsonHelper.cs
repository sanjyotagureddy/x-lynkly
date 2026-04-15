using Newtonsoft.Json;

namespace Lynkly.Shared.Kernel.Helpers;

/// <summary>
/// Provides JSON serialization and deserialization utilities.
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DateParseHandling = DateParseHandling.DateTimeOffset
    };

    /// <summary>
    /// Creates serializer settings using defaults and optional custom configuration.
    /// </summary>
    public static JsonSerializerSettings CreateSettings(Action<JsonSerializerSettings>? configure = null)
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = DefaultSettings.NullValueHandling,
            DateParseHandling = DefaultSettings.DateParseHandling
        };

        configure?.Invoke(settings);
        return settings;
    }

    /// <summary>
    /// Serializes an object to JSON.
    /// </summary>
    public static string Serialize(object? value, JsonSerializerSettings? settings = null)
    {
        return JsonConvert.SerializeObject(value, settings ?? DefaultSettings);
    }

    /// <summary>
    /// Safely serializes an object to JSON and returns fallback on failure.
    /// </summary>
    public static string SafeSerialize(object? value, string fallback = "", JsonSerializerSettings? settings = null)
    {
        try
        {
            return Serialize(value, settings);
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    /// <summary>
    /// Deserializes JSON to a target type.
    /// </summary>
    public static T Deserialize<T>(string json, JsonSerializerSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON payload cannot be null, empty, or whitespace.", nameof(json));
        }

        var value = JsonConvert.DeserializeObject<T>(json, settings ?? DefaultSettings);

        if (value is null)
        {
            throw new JsonSerializationException("JSON deserialization returned null.");
        }

        return value;
    }

    /// <summary>
    /// Attempts to deserialize JSON to a target type.
    /// </summary>
    public static bool TryDeserialize<T>(string? json, out T? value, JsonSerializerSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            value = default;
            return false;
        }

        try
        {
            value = JsonConvert.DeserializeObject<T>(json, settings ?? DefaultSettings);
            return value is not null;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}

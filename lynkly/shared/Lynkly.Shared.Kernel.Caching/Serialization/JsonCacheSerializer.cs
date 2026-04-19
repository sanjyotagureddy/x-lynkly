using System.Text.Json;

namespace Lynkly.Shared.Kernel.Caching.Serialization;

internal sealed class JsonCacheSerializer : ICacheSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public byte[] Serialize<TValue>(TValue value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
    }

    public TValue? Deserialize<TValue>(byte[] payload)
    {
        return JsonSerializer.Deserialize<TValue>(payload, SerializerOptions);
    }
}

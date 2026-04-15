namespace Lynkly.Shared.Kernel.Caching.Serialization;

public interface ICacheSerializer
{
    byte[] Serialize<TValue>(TValue value);

    TValue? Deserialize<TValue>(byte[] payload);
}

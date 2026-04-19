using System.Security.Cryptography;
using Lynkly.Resolver.Domain.Links;
using Lynkly.Shared.Kernel.Core.Helpers.Encoding;
using Microsoft.Extensions.Options;

namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

internal sealed class HmacShortAliasGenerator : IShortAliasGenerator
{
    private const int AliasLength = 8;
    private readonly byte[] _keyBytes;

    public HmacShortAliasGenerator(IOptions<AliasGeneratorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var key = options.Value.HmacKey;
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(AliasGeneratorOptions.HmacKey));
        _keyBytes = EncodingHelper.Utf8Encode(key);
    }

    internal HmacShortAliasGenerator(string hmacKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hmacKey, nameof(hmacKey));
        _keyBytes = EncodingHelper.Utf8Encode(hmacKey);
    }

    public string Generate(TenantId tenantId, string originalUrl, int attempt)
    {
        ArgumentNullException.ThrowIfNull(originalUrl);
        ArgumentOutOfRangeException.ThrowIfNegative(attempt);

        var material = EncodingHelper.Utf8Encode($"{tenantId.Value:N}|{originalUrl}|{attempt}");
        var hashBytes = HMACSHA256.HashData(_keyBytes, material);
        var hash = Convert.ToHexString(hashBytes);
        return hash[..AliasLength].ToLowerInvariant();
    }
}

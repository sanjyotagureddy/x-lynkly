using System.Security.Cryptography;
using System.Text;
using Lynkly.Resolver.Domain.Links;

namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public sealed class Sha256ShortAliasGenerator : IShortAliasGenerator
{
    private const int AliasLength = 8;

    public string Generate(TenantId tenantId, string originalUrl, int attempt)
    {
        ArgumentNullException.ThrowIfNull(originalUrl);
        ArgumentOutOfRangeException.ThrowIfNegative(attempt);

        var material = $"{tenantId.Value:N}|{originalUrl}|{attempt}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        var hash = Convert.ToHexString(hashBytes);
        return hash[..AliasLength].ToLowerInvariant();
    }
}

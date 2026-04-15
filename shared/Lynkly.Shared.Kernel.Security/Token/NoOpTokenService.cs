using System.Security.Claims;

namespace Lynkly.Shared.Kernel.Security.Token;

internal sealed class NoOpTokenService : ITokenService
{
    public ClaimsPrincipal? ValidateToken(string token)
    {
        return null;
    }
}

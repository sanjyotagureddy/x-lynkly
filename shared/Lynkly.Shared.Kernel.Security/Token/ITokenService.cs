using System.Security.Claims;

namespace Lynkly.Shared.Kernel.Security.Token;

public interface ITokenService
{
    ClaimsPrincipal? ValidateToken(string token);
}

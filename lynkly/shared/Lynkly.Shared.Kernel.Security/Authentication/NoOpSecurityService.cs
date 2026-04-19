namespace Lynkly.Shared.Kernel.Security.Authentication;

internal sealed class NoOpSecurityService : ISecurityService
{
    public bool IsAuthenticated => false;
}

namespace Lynkly.Shared.Kernel.Security.Authentication;

public interface ISecurityService
{
    bool IsAuthenticated { get; }
}

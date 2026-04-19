namespace Lynkly.Shared.Kernel.Security.Authorization;

public interface IUserContext
{
    string? UserId { get; }

    string? UserName { get; }

    IReadOnlyCollection<string> Roles { get; }
}

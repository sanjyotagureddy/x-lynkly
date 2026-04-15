namespace Lynkly.Shared.Kernel.Security.Authorization;

internal sealed class NoOpUserContext : IUserContext
{
    public string? UserId => null;

    public string? UserName => null;

    public IReadOnlyCollection<string> Roles { get; } = Array.Empty<string>();
}

namespace Lynkly.Resolver.Application.Abstractions;

public interface IBlockedDomainChecker
{
    bool IsBlocked(string host);
}

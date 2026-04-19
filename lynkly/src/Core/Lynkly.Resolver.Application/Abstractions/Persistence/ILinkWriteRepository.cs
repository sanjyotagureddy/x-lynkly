using Lynkly.Resolver.Domain.Links;

namespace Lynkly.Resolver.Application.Abstractions.Persistence;

public interface ILinkWriteRepository
{
    Task<TenantId> GetOrCreateDefaultTenantIdAsync(CancellationToken cancellationToken);

    Task<bool> AliasExistsAsync(TenantId tenantId, string alias, CancellationToken cancellationToken);

    void Add(Link link, LinkAlias linkAlias);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace Lynkly.Resolver.Infrastructure.Persistence.Internal;

internal sealed class LinkWriteRepository(AppDbContext dbContext) : ILinkWriteRepository
{
    private const string DefaultTenantName = "default";

    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<TenantId> GetOrCreateDefaultTenantIdAsync(CancellationToken cancellationToken)
    {
        var existingTenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(tenant => tenant.Name == DefaultTenantName, cancellationToken);

        if (existingTenant is not null)
        {
            return existingTenant.Id;
        }

        var tenant = Tenant.Create(DefaultTenantName);
        _dbContext.Tenants.Add(tenant);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return tenant.Id;
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(tenant).State = EntityState.Detached;

            var concurrentTenant = await _dbContext.Tenants
                .FirstOrDefaultAsync(savedTenant => savedTenant.Name == DefaultTenantName, cancellationToken);
            if (concurrentTenant is not null)
            {
                return concurrentTenant.Id;
            }

            throw;
        }
    }

    public Task<bool> AliasExistsAsync(TenantId tenantId, string alias, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        var normalizedAlias = alias.Trim().ToLowerInvariant();

        return _dbContext.LinkAliases.AnyAsync(
            linkAlias => linkAlias.TenantId == tenantId && linkAlias.Alias == normalizedAlias,
            cancellationToken);
    }

    public void Add(Link link, LinkAlias linkAlias)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(linkAlias);

        _dbContext.Links.Add(link);
        _dbContext.LinkAliases.Add(linkAlias);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;
using Lynkly.Resolver.Infrastructure.Persistence.Models;

using Microsoft.EntityFrameworkCore;

namespace Lynkly.Resolver.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Link> Links => Set<Link>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<CustomDomain> CustomDomains => Set<CustomDomain>();

    public DbSet<LinkAlias> LinkAliases => Set<LinkAlias>();

    internal DbSet<LinkClickRecord> LinkClicks => Set<LinkClickRecord>();

    internal DbSet<LinkRollupRecord> LinkRollups => Set<LinkRollupRecord>();

    internal DbSet<OutboxMessageRecord> OutboxMessages => Set<OutboxMessageRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

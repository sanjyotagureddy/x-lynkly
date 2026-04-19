using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lynkly.Resolver.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    private static readonly ValueConverter<TenantId, Guid> TenantIdConverter = new(
        tenantId => tenantId.Value,
        value => new TenantId(value));

    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(tenant => tenant.Id)
            .HasName("pk_tenants");

        builder.Property(tenant => tenant.Id)
            .HasColumnName("tenant_id")
            .HasConversion(TenantIdConverter)
            .ValueGeneratedNever();

        builder.Property(tenant => tenant.Name)
            .HasColumnName("name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(tenant => tenant.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(tenant => tenant.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(tenant => tenant.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(tenant => tenant.Name)
            .HasDatabaseName("ux_tenants_name")
            .IsUnique();
    }
}
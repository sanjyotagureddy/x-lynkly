using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lynkly.Resolver.Infrastructure.Persistence;

internal sealed class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    private static readonly ValueConverter<LinkId, Guid> LinkIdConverter = new(
        linkId => linkId.Value,
        value => new LinkId(value));

    private static readonly ValueConverter<TenantId, Guid> TenantIdConverter = new(
        tenantId => tenantId.Value,
        value => new TenantId(value));

    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ToTable("links");

        builder.HasKey(link => link.Id)
            .HasName("pk_links");

        builder.Property(link => link.Id)
            .HasColumnName("link_id")
            .HasConversion(LinkIdConverter)
            .ValueGeneratedNever();

        builder.Property(link => link.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(TenantIdConverter)
            .IsRequired();

        builder.Property(link => link.DestinationUrl)
            .HasColumnName("destination_url")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(link => link.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(link => link.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(link => link.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.Property(link => link.ExpiresAtUtc)
            .HasColumnName("expires_at_utc");

        builder.HasIndex(link => link.Status)
            .HasDatabaseName("ix_links_status");

        builder.HasIndex(link => new { link.TenantId, link.CreatedAtUtc })
            .HasDatabaseName("ix_links_active_tenant_id_created_at_utc")
            .HasFilter("status = 0");

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(link => link.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

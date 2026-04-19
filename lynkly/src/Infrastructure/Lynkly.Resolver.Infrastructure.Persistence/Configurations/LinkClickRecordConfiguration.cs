using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;
using Lynkly.Resolver.Infrastructure.Persistence.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lynkly.Resolver.Infrastructure.Persistence.Configurations;

internal sealed class LinkClickRecordConfiguration : IEntityTypeConfiguration<LinkClickRecord>
{
    private static readonly ValueConverter<TenantId, Guid> TenantIdConverter = new(
        tenantId => tenantId.Value,
        value => new TenantId(value));

    private static readonly ValueConverter<LinkId, Guid> LinkIdConverter = new(
        linkId => linkId.Value,
        value => new LinkId(value));

    public void Configure(EntityTypeBuilder<LinkClickRecord> builder)
    {
        builder.ToTable("link_clicks");

        builder.HasKey(linkClick => linkClick.LinkClickId)
            .HasName("pk_link_clicks");

        builder.Property(linkClick => linkClick.LinkClickId)
            .HasColumnName("link_click_id")
            .ValueGeneratedNever();

        builder.Property(linkClick => linkClick.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(TenantIdConverter)
            .IsRequired();

        builder.Property(linkClick => linkClick.LinkId)
            .HasColumnName("link_id")
            .HasConversion(LinkIdConverter)
            .IsRequired();

        builder.Property(linkClick => linkClick.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .IsRequired();

        builder.Property(linkClick => linkClick.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("text");

        builder.Property(linkClick => linkClick.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text");

        builder.Property(linkClick => linkClick.Referrer)
            .HasColumnName("referrer")
            .HasColumnType("text");

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(linkClick => linkClick.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Link>()
            .WithMany()
            .HasForeignKey(linkClick => linkClick.LinkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(linkClick => new { linkClick.TenantId, linkClick.OccurredAtUtc })
            .HasDatabaseName("ix_link_clicks_tenant_id_occurred_at_utc");

        builder.HasIndex(linkClick => linkClick.LinkId)
            .HasDatabaseName("ix_link_clicks_link_id");
    }
}
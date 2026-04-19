using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;
using Lynkly.Resolver.Infrastructure.Persistence.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lynkly.Resolver.Infrastructure.Persistence.Configurations;

internal sealed class LinkRollupRecordConfiguration : IEntityTypeConfiguration<LinkRollupRecord>
{
  private static readonly ValueConverter<TenantId, Guid> TenantIdConverter = new(
    tenantId => tenantId.Value,
    value => new TenantId(value));

  private static readonly ValueConverter<LinkId, Guid> LinkIdConverter = new(
    linkId => linkId.Value,
    value => new LinkId(value));

  public void Configure(EntityTypeBuilder<LinkRollupRecord> builder)
  {
    builder.ToTable("link_rollups");

    builder.HasKey(linkRollup => new
      {
        linkRollup.TenantId,
        linkRollup.LinkId,
        linkRollup.BucketStartUtc,
        linkRollup.BucketKind
      })
      .HasName("pk_link_rollups");

    builder.Property(linkRollup => linkRollup.TenantId)
      .HasColumnName("tenant_id")
      .HasConversion(TenantIdConverter)
      .IsRequired();

    builder.Property(linkRollup => linkRollup.LinkId)
      .HasColumnName("link_id")
      .HasConversion(LinkIdConverter)
      .IsRequired();

    builder.Property(linkRollup => linkRollup.BucketStartUtc)
      .HasColumnName("bucket_start_utc")
      .IsRequired();

    builder.Property(linkRollup => linkRollup.BucketKind)
      .HasColumnName("bucket_kind")
      .HasConversion<int>()
      .IsRequired();

    builder.Property(linkRollup => linkRollup.TotalClicks)
      .HasColumnName("total_clicks")
      .IsRequired();

    builder.Property(linkRollup => linkRollup.UniqueVisitors)
      .HasColumnName("unique_visitors")
      .IsRequired();

    builder.Property(linkRollup => linkRollup.LastAccessedAtUtc)
      .HasColumnName("last_accessed_at_utc");

    builder.HasOne<Tenant>()
      .WithMany()
      .HasForeignKey(linkRollup => linkRollup.TenantId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne<Link>()
      .WithMany()
      .HasForeignKey(linkRollup => linkRollup.LinkId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(linkRollup => new { linkRollup.TenantId, linkRollup.BucketStartUtc })
      .HasDatabaseName("ix_link_rollups_tenant_id_bucket_start_utc");
  }
}
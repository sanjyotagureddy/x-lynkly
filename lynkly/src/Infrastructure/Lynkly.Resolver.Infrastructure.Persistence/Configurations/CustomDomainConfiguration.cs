using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lynkly.Resolver.Infrastructure.Persistence.Configurations;

internal sealed class CustomDomainConfiguration : IEntityTypeConfiguration<CustomDomain>
{
  private static readonly ValueConverter<CustomDomainId, Guid> CustomDomainIdConverter = new(
    customDomainId => customDomainId.Value,
    value => new CustomDomainId(value));

  private static readonly ValueConverter<TenantId, Guid> TenantIdConverter = new(
    tenantId => tenantId.Value,
    value => new TenantId(value));

  public void Configure(EntityTypeBuilder<CustomDomain> builder)
  {
    builder.ToTable("custom_domains");

    builder.HasKey(customDomain => customDomain.Id)
      .HasName("pk_custom_domains");

    builder.Property(customDomain => customDomain.Id)
      .HasColumnName("custom_domain_id")
      .HasConversion(CustomDomainIdConverter)
      .ValueGeneratedNever();

    builder.Property(customDomain => customDomain.TenantId)
      .HasColumnName("tenant_id")
      .HasConversion(TenantIdConverter)
      .IsRequired();

    builder.Property(customDomain => customDomain.DomainName)
      .HasColumnName("domain_name")
      .HasColumnType("text")
      .IsRequired();

    builder.Property(customDomain => customDomain.VerificationStatus)
      .HasColumnName("verification_status")
      .HasConversion<int>()
      .IsRequired();

    builder.Property(customDomain => customDomain.CreatedAtUtc)
      .HasColumnName("created_at_utc")
      .IsRequired();

    builder.Property(customDomain => customDomain.UpdatedAtUtc)
      .HasColumnName("updated_at_utc")
      .IsRequired();

    builder.HasOne<Tenant>()
      .WithMany()
      .HasForeignKey(customDomain => customDomain.TenantId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(customDomain => customDomain.DomainName)
      .HasDatabaseName("ux_custom_domains_domain_name")
      .IsUnique();

    builder.HasIndex(customDomain => customDomain.TenantId)
      .HasDatabaseName("ix_custom_domains_tenant_id");
  }
}
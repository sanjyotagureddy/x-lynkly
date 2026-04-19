using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lynkly.Resolver.Infrastructure.Persistence.Configurations;

internal sealed class LinkAliasConfiguration : IEntityTypeConfiguration<LinkAlias>
{
    private static readonly ValueConverter<LinkAliasId, Guid> LinkAliasIdConverter = new(
        linkAliasId => linkAliasId.Value,
        value => new LinkAliasId(value));

    private static readonly ValueConverter<CustomDomainId, Guid> CustomDomainIdConverter = new(
        customDomainId => customDomainId.Value,
        value => new CustomDomainId(value));

    private static readonly ValueConverter<TenantId, Guid> TenantIdConverter = new(
        tenantId => tenantId.Value,
        value => new TenantId(value));

    private static readonly ValueConverter<LinkId, Guid> LinkIdConverter = new(
        linkId => linkId.Value,
        value => new LinkId(value));

    public void Configure(EntityTypeBuilder<LinkAlias> builder)
    {
        builder.ToTable("link_aliases");

        builder.HasKey(linkAlias => linkAlias.Id)
            .HasName("pk_link_aliases");

        builder.Property(linkAlias => linkAlias.Id)
            .HasColumnName("link_alias_id")
            .HasConversion(LinkAliasIdConverter)
            .ValueGeneratedNever();

        builder.Property(linkAlias => linkAlias.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(TenantIdConverter)
            .IsRequired();

        builder.Property(linkAlias => linkAlias.CustomDomainId)
            .HasColumnName("custom_domain_id")
            .HasConversion(CustomDomainIdConverter);

        builder.Property(linkAlias => linkAlias.Alias)
            .HasColumnName("alias")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(linkAlias => linkAlias.LinkId)
            .HasColumnName("link_id")
            .HasConversion(LinkIdConverter)
            .IsRequired();

        builder.Property(linkAlias => linkAlias.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();

        builder.Property(linkAlias => linkAlias.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(linkAlias => linkAlias.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(linkAlias => linkAlias.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<CustomDomain>()
            .WithMany()
            .HasForeignKey(linkAlias => linkAlias.CustomDomainId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Link>()
            .WithMany()
            .HasForeignKey(linkAlias => linkAlias.LinkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(linkAlias => new { linkAlias.TenantId, linkAlias.CustomDomainId, linkAlias.Alias })
            .HasDatabaseName("ux_link_aliases_tenant_domain_alias")
            .IsUnique();

        builder.HasIndex(linkAlias => linkAlias.LinkId)
            .HasDatabaseName("ix_link_aliases_link_id");
    }
}

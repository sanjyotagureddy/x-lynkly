using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;
using Lynkly.Resolver.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Lynkly.Resolver.IntegrationTests;

public sealed class PersistenceModelConfigurationTests
{
    [Fact]
    public void AppDbContext_ModelContainsExpectedTables()
    {
        using var context = CreateContext();
        var tableNames = context.Model.GetEntityTypes()
            .Select(entityType => entityType.GetTableName())
            .Where(tableName => tableName is not null)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("links", tableNames);
        Assert.Contains("tenants", tableNames);
        Assert.Contains("custom_domains", tableNames);
        Assert.Contains("link_aliases", tableNames);
        Assert.Contains("link_clicks", tableNames);
        Assert.Contains("link_rollups", tableNames);
        Assert.Contains("outbox_messages", tableNames);
    }

    [Fact]
    public void LinkEntity_HasExpectedMappings()
    {
        using var context = CreateContext();
        var linkEntity = context.Model.FindEntityType(typeof(Link));

        Assert.NotNull(linkEntity);
        Assert.Equal("links", linkEntity!.GetTableName());
        Assert.Equal("pk_links", linkEntity.FindPrimaryKey()!.GetName());
        Assert.Equal("link_id", linkEntity.FindProperty(nameof(Link.Id))!.GetColumnName());
        Assert.Equal("destination_url", linkEntity.FindProperty(nameof(Link.DestinationUrl))!.GetColumnName());
        Assert.Equal("status", linkEntity.FindProperty(nameof(Link.Status))!.GetColumnName());
        Assert.Contains(linkEntity.GetIndexes(), index => index.GetDatabaseName() == "ix_links_status");
        Assert.Contains(linkEntity.GetIndexes(), index => index.GetDatabaseName() == "ix_links_active_tenant_id_created_at_utc");
    }

    [Fact]
    public void TenantEntity_HasExpectedMappings()
    {
        using var context = CreateContext();
        var tenantEntity = context.Model.FindEntityType(typeof(Tenant));

        Assert.NotNull(tenantEntity);
        Assert.Equal("tenants", tenantEntity!.GetTableName());
        Assert.Equal("pk_tenants", tenantEntity.FindPrimaryKey()!.GetName());
        Assert.Equal("tenant_id", tenantEntity.FindProperty(nameof(Tenant.Id))!.GetColumnName());
        Assert.Equal("name", tenantEntity.FindProperty(nameof(Tenant.Name))!.GetColumnName());
        Assert.Contains(tenantEntity.GetIndexes(), index => index.GetDatabaseName() == "ux_tenants_name" && index.IsUnique);
    }

    [Fact]
    public void LinkAliasEntity_HasExpectedUniqueIndexAndDeleteBehavior()
    {
        using var context = CreateContext();
        var linkAliasEntity = context.Model.FindEntityType(typeof(LinkAlias));

        Assert.NotNull(linkAliasEntity);
        Assert.Equal("link_aliases", linkAliasEntity!.GetTableName());
        Assert.Equal("pk_link_aliases", linkAliasEntity.FindPrimaryKey()!.GetName());
        Assert.Contains(linkAliasEntity.GetIndexes(), index => index.GetDatabaseName() == "ux_link_aliases_tenant_domain_alias" && index.IsUnique);
        Assert.Contains(linkAliasEntity.GetForeignKeys(), foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(Link) && foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(linkAliasEntity.GetForeignKeys(), foreignKey => foreignKey.PrincipalEntityType.ClrType.Name == nameof(CustomDomain) && foreignKey.DeleteBehavior == DeleteBehavior.SetNull);
    }

    [Fact]
    public void InternalPersistenceEntities_HaveExpectedTableMappings()
    {
        using var context = CreateContext();

        var outboxEntity = context.Model.GetEntityTypes().Single(entityType => entityType.GetTableName() == "outbox_messages");
        Assert.Equal("pk_outbox_messages", outboxEntity.FindPrimaryKey()!.GetName());
        Assert.Contains(outboxEntity.GetIndexes(), index => index.GetDatabaseName() == "ix_outbox_messages_processed_at_utc");
        Assert.Contains(outboxEntity.GetIndexes(), index => index.GetDatabaseName() == "ix_outbox_messages_occurred_at_utc");

        var linkClicksEntity = context.Model.GetEntityTypes().Single(entityType => entityType.GetTableName() == "link_clicks");
        Assert.Equal("pk_link_clicks", linkClicksEntity.FindPrimaryKey()!.GetName());
        Assert.Contains(linkClicksEntity.GetIndexes(), index => index.GetDatabaseName() == "ix_link_clicks_tenant_id_occurred_at_utc");
        Assert.Contains(linkClicksEntity.GetIndexes(), index => index.GetDatabaseName() == "ix_link_clicks_link_id");

        var linkRollupsEntity = context.Model.GetEntityTypes().Single(entityType => entityType.GetTableName() == "link_rollups");
        Assert.Equal("pk_link_rollups", linkRollupsEntity.FindPrimaryKey()!.GetName());
        Assert.Contains(linkRollupsEntity.GetIndexes(), index => index.GetDatabaseName() == "ix_link_rollups_tenant_id_bucket_start_utc");
    }

    private static AppDbContext CreateContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("LYNKLY_TEST_DB_CONNECTION")
            ?? "Host=localhost;Database=lynkly_test";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}

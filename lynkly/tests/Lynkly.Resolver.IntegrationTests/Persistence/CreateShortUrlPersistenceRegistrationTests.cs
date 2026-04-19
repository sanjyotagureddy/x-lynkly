using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Infrastructure.Persistence;
using Lynkly.Resolver.Infrastructure.Persistence.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.IntegrationTests.Persistence;

public sealed class CreateShortUrlPersistenceRegistrationTests
{
    [Fact]
    public void AddResolverPersistence_RegistersDbContextAndLinkWriteRepository()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:lynkly-db"] = "Host=localhost;Database=lynkly_test"
            })
            .Build();

        services.AddResolverPersistence(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<AppDbContext>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ILinkWriteRepository>());
    }
}

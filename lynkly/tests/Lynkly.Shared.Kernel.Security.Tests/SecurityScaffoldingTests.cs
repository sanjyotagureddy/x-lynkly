using Lynkly.Shared.Kernel.Security.Authentication;
using Lynkly.Shared.Kernel.Security.Authorization;
using Lynkly.Shared.Kernel.Security.Configuration;
using Lynkly.Shared.Kernel.Security.Extensions;
using Lynkly.Shared.Kernel.Security.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.Security.Tests;

public class SecurityScaffoldingTests
{
    [Fact]
    public void AddSecurity_Throws_WhenServicesAreNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(() =>
            SecurityServiceCollectionExtensions.AddSecurity(null!, configuration));
    }

    [Fact]
    public void AddSecurity_Throws_WhenConfigurationIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddSecurity(null!));
    }

    [Fact]
    public void AddSecurity_RegistersPlaceholderServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ISecurityService>());
        Assert.NotNull(provider.GetService<ITokenService>());
        Assert.NotNull(provider.GetService<IUserContext>());
    }

    [Fact]
    public void AddSecurity_DoesNotOverride_PreRegisteredImplementations()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var securityService = new TestSecurityService();
        var tokenService = new TestTokenService();
        var userContext = new TestUserContext();

        services.AddSingleton<ISecurityService>(securityService);
        services.AddSingleton<ITokenService>(tokenService);
        services.AddSingleton<IUserContext>(userContext);

        services.AddSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.Same(securityService, provider.GetRequiredService<ISecurityService>());
        Assert.Same(tokenService, provider.GetRequiredService<ITokenService>());
        Assert.Same(userContext, provider.GetRequiredService<IUserContext>());
    }

    [Fact]
    public void NoOpSecurityService_IsUnauthenticated()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        var security = provider.GetRequiredService<ISecurityService>();

        Assert.False(security.IsAuthenticated);
    }

    [Fact]
    public void NoOpTokenService_ReturnsNullPrincipal()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        var tokenService = provider.GetRequiredService<ITokenService>();

        Assert.Null(tokenService.ValidateToken("token-value"));
    }

    [Fact]
    public void NoOpUserContext_ExposesExpectedDefaults()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        var userContext = provider.GetRequiredService<IUserContext>();

        Assert.Null(userContext.UserId);
        Assert.Null(userContext.UserName);
        Assert.Empty(userContext.Roles);
    }

    [Fact]
    public void SecurityOptions_ExposeExpectedSectionNameAndValues()
    {
        var options = new SecurityOptions
        {
            Authority = "https://issuer.example",
            Audience = "lynkly-api"
        };

        Assert.Equal("Security", SecurityOptions.SectionName);
        Assert.Equal("https://issuer.example", options.Authority);
        Assert.Equal("lynkly-api", options.Audience);
    }

    private sealed class TestSecurityService : ISecurityService
    {
        public bool IsAuthenticated => true;
    }

    private sealed class TestTokenService : ITokenService
    {
        public System.Security.Claims.ClaimsPrincipal? ValidateToken(string token) => new();
    }

    private sealed class TestUserContext : IUserContext
    {
        public string? UserId => "user-1";
        public string? UserName => "test-user";
        public IReadOnlyCollection<string> Roles => new[] { "admin" };
    }
}

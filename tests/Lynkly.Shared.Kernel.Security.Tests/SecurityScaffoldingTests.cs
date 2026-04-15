using Lynkly.Shared.Kernel.Security.Authentication;
using Lynkly.Shared.Kernel.Security.Authorization;
using Lynkly.Shared.Kernel.Security.Extensions;
using Lynkly.Shared.Kernel.Security.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.Security.Tests;

public class SecurityScaffoldingTests
{
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
}

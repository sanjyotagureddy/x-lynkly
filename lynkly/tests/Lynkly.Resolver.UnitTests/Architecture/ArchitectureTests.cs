using NetArchTest.Rules;

namespace Lynkly.Resolver.UnitTests.Architecture;

public sealed class ArchitectureTests : ArchitectureTestBase
{
    [Fact]
    public void Application_Should_Not_Depend_OnApiOrInfrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Lynkly.Resolver.API",
                "Lynkly.Resolver.Infrastructure")
            .GetResult();

        AssertArchitectureRule(result, nameof(Application_Should_Not_Depend_OnApiOrInfrastructure));
    }

    [Fact]
    public void Persistence_Should_Not_Depend_OnApi()
    {
        var result = Types.InAssembly(PersistenceAssembly)
            .ShouldNot()
            .HaveDependencyOn("Lynkly.Resolver.API")
            .GetResult();

        AssertArchitectureRule(result, nameof(Persistence_Should_Not_Depend_OnApi));
    }

    [Fact]
    public void Persistence_Implementations_Should_Be_Internal_By_Default()
    {
        var result = Types.InAssembly(PersistenceAssembly)
            .That()
            .AreClasses()
            .And()
            .AreNotStatic()
            .And()
            .DoNotHaveName("AppDbContext")
            .Should()
            .NotBePublic()
            .GetResult();

        AssertArchitectureRule(result, nameof(Persistence_Implementations_Should_Be_Internal_By_Default));
    }

    [Fact]
    public void Api_Should_Not_Depend_OnDomain_Directly()
    {
        var result = Types.InAssembly(ApiAssembly)
            .ShouldNot()
            .HaveDependencyOn("Lynkly.Resolver.Domain")
            .GetResult();

        AssertArchitectureRule(result, nameof(Api_Should_Not_Depend_OnDomain_Directly));
    }
}

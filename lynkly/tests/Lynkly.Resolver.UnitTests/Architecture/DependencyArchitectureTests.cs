using NetArchTest.Rules;

namespace Lynkly.Resolver.UnitTests.Architecture;

public sealed class DependencyArchitectureTests : ArchitectureTestBase
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Application_Layer()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Lynkly.Resolver.Application")
            .GetResult();

        AssertArchitectureRule(result, nameof(Domain_Should_Not_Depend_On_Application_Layer));
    }

    [Fact]
    public void Api_Should_Not_Depend_On_Domain_Directly()
    {
        var result = Types.InAssembly(ApiAssembly)
            .ShouldNot()
            .HaveDependencyOn("Lynkly.Resolver.Domain")
            .GetResult();

        AssertArchitectureRule(result, nameof(Api_Should_Not_Depend_On_Domain_Directly));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Lynkly.Resolver.Infrastructure", "Lynkly.Resolver.API")
            .GetResult();

        AssertArchitectureRule(result, nameof(Application_Should_Not_Depend_On_Infrastructure_Or_Api));
    }

    [Fact]
    public void Persistence_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(PersistenceAssembly)
            .ShouldNot()
            .HaveDependencyOn("Lynkly.Resolver.API")
            .GetResult();

        AssertArchitectureRule(result, nameof(Persistence_Should_Not_Depend_On_Api));
    }

    [Fact]
    public void SharedKernelCore_Should_Not_Depend_On_Resolver_Assemblies()
    {
        var result = Types.InAssembly(SharedKernelCoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("Lynkly.Resolver")
            .GetResult();

        AssertArchitectureRule(result, nameof(SharedKernelCore_Should_Not_Depend_On_Resolver_Assemblies));
    }
}

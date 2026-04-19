using Lynkly.Shared.Kernel.Core.Domain;
using NetArchTest.Rules;

namespace Lynkly.Resolver.UnitTests.Architecture;

public sealed class DomainArchitectureTests : ArchitectureTestBase
{
    [Fact]
    public void Domain_Should_Not_Depend_OnInfrastructureOrApiOrFrameworks()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Lynkly.Resolver.API",
                "Lynkly.Resolver.Infrastructure",
                "Microsoft.EntityFrameworkCore",
                "MassTransit",
                "StackExchange.Redis")
            .GetResult();

        AssertArchitectureRule(result, nameof(Domain_Should_Not_Depend_OnInfrastructureOrApiOrFrameworks));
    }

    [Fact]
    public void Domain_Events_Should_End_With_DomainEvent()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        AssertArchitectureRule(result, nameof(Domain_Events_Should_End_With_DomainEvent));
    }
}

using System.Reflection;
using NetArchTest.Rules;

namespace Lynkly.Resolver.UnitTests.Architecture;

public abstract class ArchitectureTestBase
{
    protected static readonly Assembly DomainAssembly = Assembly.Load("Lynkly.Resolver.Domain");
    protected static readonly Assembly ApplicationAssembly = Assembly.Load("Lynkly.Resolver.Application");
    protected static readonly Assembly ApiAssembly = Assembly.Load("Lynkly.Resolver.API");
    protected static readonly Assembly PersistenceAssembly = Assembly.Load("Lynkly.Resolver.Infrastructure.Persistence");
    protected static readonly Assembly SharedKernelCoreAssembly = Assembly.Load("Lynkly.Shared.Kernel.Core");
    protected static readonly Assembly UnitTestsAssembly = Assembly.Load("Lynkly.Resolver.UnitTests");

    protected static void AssertArchitectureRule(TestResult result, string ruleName)
    {
        if (result.IsSuccessful)
        {
            return;
        }

        var failingTypes = result.FailingTypeNames ?? [];
        var message =
            $"Architecture rule '{ruleName}' failed for: {string.Join(", ", failingTypes)}";

        Assert.True(result.IsSuccessful, message);
    }
}

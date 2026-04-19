using Lynkly.Shared.Kernel.Core.Domain;
using NetArchTest.Rules;

namespace Lynkly.Resolver.UnitTests.Architecture;

public sealed class NamingConventionArchitectureTests : ArchitectureTestBase
{
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

    [Fact]
    public void Domain_ValueTypes_With_Value_Property_Should_End_With_Id()
    {
        var offenders = DomainAssembly
            .GetTypes()
            .Where(type => type.Namespace?.StartsWith("Lynkly.Resolver.Domain", StringComparison.Ordinal) == true)
            .Where(type => type.IsValueType)
            .Where(type => type.GetProperty("Value") is not null)
            .Where(type => !type.Name.EndsWith("Id", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .ToArray();

        Assert.True(offenders.Length == 0, $"Value-object IDs must end with 'Id'. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Persistence_Model_Records_Should_End_With_Record()
    {
        var offenders = PersistenceAssembly
            .GetTypes()
            .Where(type => type.Namespace?.Contains("Persistence.Models", StringComparison.Ordinal) == true)
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => !type.Name.EndsWith("Record", StringComparison.Ordinal) && !type.Name.EndsWith("Kind", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .ToArray();

        Assert.True(offenders.Length == 0, $"Persistence model types should end with 'Record' (or 'Kind' for enums). Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Unit_Test_Classes_Should_End_With_Tests()
    {
        var offenders = UnitTestsAssembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.GetMethods().Any(method => method.GetCustomAttributes(typeof(FactAttribute), true).Any()))
            .Where(type => !type.Name.EndsWith("Tests", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .ToArray();

        Assert.True(offenders.Length == 0, $"Test classes should end with 'Tests'. Offenders: {string.Join(", ", offenders)}");
    }
}

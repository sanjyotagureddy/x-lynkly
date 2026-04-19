namespace Lynkly.Resolver.Domain.Links;

public readonly record struct CustomDomainId(Guid Value)
{
    public static CustomDomainId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

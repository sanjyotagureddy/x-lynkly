namespace Lynkly.Resolver.Domain.Links;

public readonly record struct LinkAliasId(Guid Value)
{
    public static LinkAliasId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

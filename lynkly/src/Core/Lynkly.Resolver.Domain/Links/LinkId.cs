namespace Lynkly.Resolver.Domain.Links;

public readonly record struct LinkId(Guid Value)
{
    public static LinkId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

using Lynkly.Shared.Kernel.Core.Domain;

namespace Lynkly.Resolver.Domain.Links.Events;

public sealed record LinkCreatedDomainEvent(
    LinkId LinkId,
    TenantId TenantId,
    string DestinationUrl,
    DateTime OccurredOnUtc) : IDomainEvent;

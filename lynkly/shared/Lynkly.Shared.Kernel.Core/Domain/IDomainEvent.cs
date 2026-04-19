namespace Lynkly.Shared.Kernel.Core.Domain;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}

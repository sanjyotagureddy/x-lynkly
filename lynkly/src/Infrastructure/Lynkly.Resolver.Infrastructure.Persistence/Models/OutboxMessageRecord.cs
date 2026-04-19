namespace Lynkly.Resolver.Infrastructure.Persistence.Models;

internal sealed class OutboxMessageRecord
{
  private OutboxMessageRecord()
  {
    MessageType = string.Empty;
    Payload = string.Empty;
  }

  public Guid OutboxMessageId { get; private set; }

  public DateTimeOffset OccurredAtUtc { get; private set; }

  public string MessageType { get; private set; }

  public string Payload { get; private set; }

  public string? CorrelationId { get; private set; }

  public DateTimeOffset? ProcessedAtUtc { get; private set; }

  public string? Error { get; private set; }
}
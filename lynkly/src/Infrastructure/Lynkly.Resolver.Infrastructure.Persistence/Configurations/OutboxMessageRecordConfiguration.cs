using Lynkly.Resolver.Infrastructure.Persistence.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lynkly.Resolver.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageRecordConfiguration : IEntityTypeConfiguration<OutboxMessageRecord>
{
  public void Configure(EntityTypeBuilder<OutboxMessageRecord> builder)
  {
    builder.ToTable("outbox_messages");

    builder.HasKey(outboxMessage => outboxMessage.OutboxMessageId)
      .HasName("pk_outbox_messages");

    builder.Property(outboxMessage => outboxMessage.OutboxMessageId)
      .HasColumnName("outbox_message_id")
      .ValueGeneratedNever();

    builder.Property(outboxMessage => outboxMessage.OccurredAtUtc)
      .HasColumnName("occurred_at_utc")
      .IsRequired();

    builder.Property(outboxMessage => outboxMessage.MessageType)
      .HasColumnName("message_type")
      .HasColumnType("text")
      .IsRequired();

    builder.Property(outboxMessage => outboxMessage.Payload)
      .HasColumnName("payload")
      .HasColumnType("jsonb")
      .IsRequired();

    builder.Property(outboxMessage => outboxMessage.CorrelationId)
      .HasColumnName("correlation_id")
      .HasColumnType("text");

    builder.Property(outboxMessage => outboxMessage.ProcessedAtUtc)
      .HasColumnName("processed_at_utc");

    builder.Property(outboxMessage => outboxMessage.Error)
      .HasColumnName("error")
      .HasColumnType("text");

    builder.HasIndex(outboxMessage => outboxMessage.ProcessedAtUtc)
      .HasDatabaseName("ix_outbox_messages_processed_at_utc");

    builder.HasIndex(outboxMessage => outboxMessage.OccurredAtUtc)
      .HasDatabaseName("ix_outbox_messages_occurred_at_utc");
  }
}
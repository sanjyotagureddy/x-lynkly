using Lynkly.Resolver.Domain.Links;

namespace Lynkly.Resolver.Infrastructure.Persistence.Models;

internal sealed class LinkRollupRecord
{
  private LinkRollupRecord()
  {
  }

  public TenantId TenantId { get; private set; }

  public LinkId LinkId { get; private set; }

  public DateTimeOffset BucketStartUtc { get; private set; }

  public LinkRollupBucketKind BucketKind { get; private set; }

  public long TotalClicks { get; private set; }

  public long UniqueVisitors { get; private set; }

  public DateTimeOffset? LastAccessedAtUtc { get; private set; }
}
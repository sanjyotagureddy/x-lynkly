# Lynkly

Lynkly is a globally distributed URL shortener built on .NET 10, ASP.NET Core Minimal APIs, Clean Architecture, Vertical Slice Architecture (VSA), and Event-Driven Architecture (EDA).

The entire system is designed around one rule: keep redirects extremely fast, move analytics and side effects to async event processing.

## Table of Contents

1. [Product Goal](#product-goal)
2. [Architecture Principles](#architecture-principles)
3. [Approved Stack](#approved-stack)
4. [End-to-End System Flow](#end-to-end-system-flow)
5. [Core Events](#core-events)
6. [Data Model](#data-model)
7. [API Surface](#api-surface)
8. [Namespace and Shared Kernel](#namespace-and-shared-kernel)
9. [Project Structure (Clean Arch + VSA)](#project-structure-clean-arch--vsa)
10. [Scalability and Performance](#scalability-and-performance)
11. [Global Deployment Strategy](#global-deployment-strategy)
12. [Reliability and Failure Behavior](#reliability-and-failure-behavior)
13. [Retry and DLQ Policy](#retry-and-dlq-policy)
14. [Security and Abuse Prevention](#security-and-abuse-prevention)
15. [Observability and SLOs](#observability-and-slos)
16. [Testing Strategy and Coverage Policy](#testing-strategy-and-coverage-policy)
17. [Local Development](#local-development)
18. [Roadmap](#roadmap)
19. [Wiki](#wiki)

## Product Goal

Lynkly provides:

- Fast link creation.
- Ultra-low-latency redirect service.
- Asynchronous analytics pipeline.
- Tenant-aware link management for teams and SaaS.

Non-goals:

- Marketing automation suite.
- Heavy synchronous processing during redirect.
- Cross-region strong consistency for all read/write operations.

## Architecture Principles

- .NET 10 + ASP.NET Core Minimal APIs.
- Clean Architecture boundaries: Domain is framework-independent.
- Vertical Slice Architecture: each feature owns endpoint, contract, validator, handler, and tests.
- Event-Driven Architecture: business actions emit events, consumers process side effects asynchronously through a broker abstraction.
- Redirect-first design: redirect response is never blocked by analytics.
- Scale-out by default: stateless APIs, partition-friendly data model, and queue-based decoupling.

## Approved Stack

The canonical stack decisions live in [docs/architecture/tech-stack.md](docs/architecture/tech-stack.md). In short:

- Runtime and API: .NET 10 and ASP.NET Core Minimal APIs.
- Architecture: Clean Architecture plus Vertical Slice Architecture.
- Data: EF Core, PostgreSQL, and Redis.
- Messaging: RabbitMQ with MassTransit and outbox-based delivery.
- Resilience: Polly v8 through Microsoft.Extensions.Http.Resilience.
- Validation: FluentValidation.
- Auth: Keycloak with OAuth2 / OpenID Connect and JWT bearer tokens for management APIs.
- Testing: xUnit, Bogus, hand-written fakes, NSubstitute when needed, and Testcontainers for external dependencies.
- Observability: OpenTelemetry.

## Namespace and Shared Kernel

- Base namespace: `Lynkly.Resolver`.
- Feature and infrastructure projects should use the same root namespace pattern, for example `Lynkly.Resolver.Api` and `Lynkly.Resolver.Infrastructure`.
- Shared, reusable primitives should live in `Shared.Kernel`.
- `Shared.Kernel` should contain only stable, generic building blocks such as result types, domain events, guard clauses, strongly typed IDs, and common abstractions.
- Anything in `Shared.Kernel` should be written as if it could later be extracted into a NuGet package without dragging application-specific dependencies with it.
- Keep `Shared.Kernel` small and strict; if a type is only useful to Lynkly, it does not belong there.

## End-to-End System Flow

### 1) Create Short Link (Write Flow)

Request:

- POST /links

Flow:

```text
Client
  -> API Service
    -> Generate Slug (Snowflake + Base62)
      -> Store in DB (Links table)
        -> Publish Event: LinkCreated
          -> Return Short URL
```

### 2) Redirect (Critical Read Flow)

Request:

- GET /{slug}

Flow:

```text
User
  -> Redirect Handler
    -> Check Redis (slug -> URL)
      -> HIT: Redirect (302)
      -> MISS: Fetch DB -> Update Cache -> Redirect
    -> Publish Event: LinkAccessed (async, fire-and-forget)
```

Important:

- Redirect happens first.
- Event publish must not block redirect latency.

### 3) Event Processing (Analytics Pipeline)

```text
LinkAccessed Event
  -> Message Broker (RabbitMQ today, Kafka later)
    -> Analytics Consumer
      -> Store in DB (Clicks table)
        -> Update Aggregates (LinkStats)
```

### 4) Read Analytics

Request:

- GET /links/{id}/analytics

Flow:

```text
Client
  -> API Service
    -> Read LinkStats aggregates
      -> Return response
```

### One-View Mental Model

```text
[CREATE LINK]
Client -> API -> DB -> Event(LinkCreated)

[REDIRECT]
User -> Redirect -> Redis -> DB fallback -> Redirect
                               -> Event(LinkAccessed)

[ANALYTICS]
Event -> Message Broker -> Consumer -> DB (Clicks + Stats)

[READ ANALYTICS]
Client -> API -> Stats -> Response
```

Golden rules:

- Redirect path is hot path and must remain minimal.
- Analytics is asynchronous and eventually consistent.
- Events are the decoupling boundary for scale.
- Consumers must be idempotent and retry-safe.

## Core Events

### LinkCreated

Purpose:

- Audit trail.
- Future automation hooks (notifications, workflows, billing, etc).

Example payload:

```json
{
  "eventType": "LinkCreated",
  "slug": "abc123",
  "originalUrl": "https://example.com/page",
  "createdAt": "2026-04-13T12:00:00Z"
}
```

### LinkAccessed

Purpose:

- Core analytics signal.

Example payload:

```json
{
  "eventType": "LinkAccessed",
  "slug": "abc123",
  "timestamp": "2026-04-13T12:00:01Z",
  "ip": "203.0.113.1",
  "userAgent": "Mozilla/5.0",
  "referrer": "https://search.example"
}
```

## Data Model

### Links table

- LinkId (PK, time-sortable UUIDv7 or ULID)
- TenantId
- DestinationUrl
- CreatedAtUtc
- UpdatedAtUtc
- ExpiresAtUtc (nullable)
- Status

Status values:

- Active
- Disabled
- Archived
- Blocked

### LinkAliases table

- TenantId
- Optional DomainId
- Alias
- LinkId (FK)
- CreatedAtUtc

### CustomDomains table

- DomainId (PK)
- TenantId
- DomainName
- VerificationStatus
- CreatedAtUtc

### Clicks table

- Id
- LinkId
- TimestampUtc
- Ip
- UserAgent
- Referrer

### LinkStats table (aggregated)

- LinkId
- TotalClicks
- UniqueVisitors
- LastAccessedAtUtc
- TimeBucketed metrics (hour/day)

Design notes:

- Keep redirect lookup data in a dedicated alias table rather than the canonical link record.
- Use append-only Clicks + precomputed LinkStats for fast analytics reads.
- Partition Clicks and rollups by time.
- Prefer tenant-aware uniqueness for aliases and domains so custom branding and SaaS tenancy scale cleanly.
- Treat Blocked as an admin/abuse state that returns `410 Gone` on redirect and stays out of the hot redirect path through status-aware filtering.

## API Surface

- POST /api/v1/links
- GET /api/v1/links/{code}
- DELETE /api/v1/links/{code}
- GET /{code}
- GET /api/v1/links/{id}/analytics

Create link request example:

```json
{
  "destinationUrl": "https://example.com/some/long/path",
  "customAlias": "summer-sale",
  "expiresAtUtc": "2026-12-31T23:59:59Z"
}
```

## Project Structure (Clean Arch + VSA)

Recommended repository layout:

```text
Lynkly.sln
│
├── src/
│   │
│   ├── Services/
│   │   └── Lynkly.Resolver.API
│   │       ├── Endpoints/
│   │       ├── Middleware/
│   │       ├── Configurations/
│   │       └── Program.cs
│   │
│   ├── Core/
│   │   ├── Lynkly.Resolver.Domain
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   ├── Enums/
│   │   │   └── Rules/
│   │   │
│   │   └── Lynkly.Resolver.Application
│   │       ├── Interfaces/
│   │       ├── UseCases/
│   │       │   ├── CreateLink/
│   │       │   ├── ResolveLink/
│   │       │   └── GetAnalytics/
│   │       ├── DTOs/
│   │       └── Mappers/
│   │
│   ├── Infrastructure/
│   │   ├── Lynkly.Resolver.Persistence
│   │   │   ├── DbContext/
│   │   │   ├── Repositories/
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   │
│   │   ├── Lynkly.Resolver.Caching
│   │   │   ├── Redis/
│   │   │   ├── KeyStrategies/
│   │   │   └── CacheServices/
│   │   │
│   │   └── Lynkly.Resolver.Messaging
│   │       ├── Producers/
│   │       ├── Consumers/
│   │       ├── Events/
│   │       └── Configurations/
│   │
│   └── Workers/
│       └── Lynkly.Resolver.AnalyticsWorker
│           ├── Consumers/
│           ├── Processors/
│           └── BackgroundServices/
│
├── shared/
│   │
│   ├── Lynkly.Shared.Kernel.Core
│   │   ├── Helpers/
│   │   ├── Validation/
│   │   ├── Exceptions/
│   │   └── RequestContext/
│   │
│   ├── Lynkly.Shared.Kernel.Caching
│   ├── Lynkly.Shared.Kernel.Logging
│   ├── Lynkly.Shared.Kernel.MediatR
│   ├── Lynkly.Shared.Kernel.Messaging
│   ├── Lynkly.Shared.Kernel.Observability
│   ├── Lynkly.Shared.Kernel.Persistence
│   └── Lynkly.Shared.Kernel.Security
│
├── tests/
│   │
│   ├── Lynkly.Resolver.UnitTests
│   │   ├── Domain/
│   │   ├── Application/
│   │   └── Helpers/
│   │
│   └── Lynkly.Resolver.IntegrationTests
│       ├── API/
│       ├── Persistence/
│       └── Messaging/
│
├── docs/
│   │
│   ├── architecture/
│   ├── api/
│   ├── events/
│   ├── infra/
│   └── development/
│
├── .github/
│   └── copilot-instructions.md
│
├── docker/
│   ├── docker-compose.yml
│   └── services/
│
├── scripts/
│
├── .env
├── .gitignore
└── README.md
```

Notes:

- `Lynkly.Resolver.API` is the composition root and should stay thin.
- `Lynkly.Resolver.Application` should contain use cases only, not infrastructure concerns.
- `Lynkly.Resolver.Persistence`, `Lynkly.Resolver.Caching`, and `Lynkly.Resolver.Messaging` are infrastructure adapters behind interfaces.
- `Lynkly.Shared.Kernel.*` modules are intentionally reusable and should be kept NuGet-ready.
- `Lynkly.Resolver.AnalyticsWorker` owns asynchronous event consumption and aggregation.
- Docs, docker assets, and scripts should live outside source projects to keep the solution clean.

Vertical slice example:

- Features/Links/CreateLink/Endpoint.cs
- Features/Links/CreateLink/Command.cs
- Features/Links/CreateLink/Validator.cs
- Features/Links/CreateLink/Handler.cs
- Features/Links/CreateLink/Tests.cs

## Scalability and Performance

- Redis cache-aside for redirect lookups.
- Negative cache for unknown slug.
- Stateless API instances for horizontal scaling.
- Queue decoupling via a broker abstraction, with RabbitMQ as the first implementation and Kafka as a later drop-in option.
- Outbox pattern for reliable event publication.

Targets:

- Redirect availability: 99.99%.
- Redirect latency P95: < 30 ms (in-region service time).
- Cache hit ratio on hot traffic: > 95%.

## Global Deployment Strategy

- Active-active multi-region deployment.
- Geo-routing to nearest healthy region.
- Multi-AZ in every region.
- Regional Redis caches.
- Globally replicated metadata store.
- Kubernetes deployment (AKS/EKS/GKE).

## Reliability and Failure Behavior

- If Redis fails: fallback to DB, continue redirect, degrade latency but preserve availability.
- If the message broker is unavailable: redirect still succeeds; events buffered via outbox and retried.
- If analytics consumer lags: redirects unaffected; analytics freshness degrades temporarily.
- Use retries with backoff, circuit breakers, and DLQ.

## Retry and DLQ Policy

Retry classes:

- Transient failures: network timeout, broker connection reset, temporary DB throttling.
- Semi-transient failures: dependency warmup/startup windows.
- Non-retryable failures: schema mismatch, invalid payload, business rule violation.

Producer policy (API or outbox dispatcher -> message broker):

- Use exponential backoff with jitter.
- Base delay: 250 ms.
- Multiplier: x2.
- Max delay: 30 s.
- Max attempts per dispatch cycle: 8.
- If still failing: keep event in outbox as pending and continue scheduled retries.

Consumer policy (message broker -> Analytics handler):

- Attempt processing with bounded retries.
- Recommended max attempts: 5 total deliveries.
- Requeue with increasing delay between attempts.
- Acknowledge only after successful processing and idempotency check.
- On final failure: route message to DLQ.

DLQ handling rules:

- Every dead-lettered message must include reason, stack trace summary, first-seen time, and attempt count.
- Poison messages (always failing) stay in DLQ until fixed and replayed safely.
- Provide replay tooling to re-publish selected DLQ messages after remediation.
- Replayed messages must keep idempotency keys to avoid double counting analytics.

Operational guardrails:

- Alert when DLQ depth > 0 for critical events.
- Alert on retry storm thresholds (for example, retries/min beyond baseline).
- Track mean time to recover for DLQ incidents.
- Create runbooks for: broker outage, malformed event rollout, consumer deployment rollback.

Golden implementation rules:

- Never block redirect path because of queue publish issues.
- Prefer outbox durability over in-memory retries.
- Keep consumers idempotent using event ID or deterministic dedupe key.
- Treat DLQ as a first-class operational signal, not a passive backlog.

## Security and Abuse Prevention

- HTTPS everywhere.
- OAuth2/OIDC + JWT for management APIs.
- Strict URL validation and allow/deny policies.
- Rate limiting by IP, tenant, and token.
- Malware/phishing checks and takedown workflows.
- Secrets via managed vault.

## Observability and SLOs

Collect:

- Redirect latency P50/P95/P99.
- Cache hit/miss ratio.
- Redirect status/error rates.
- DB latency and throttling.
- Message broker queue depth, retry counts, DLQ counts.
- Consumer lag and analytics freshness.

Implementation:

- OpenTelemetry traces, metrics, structured logs.
- Correlation IDs across API, queue, and consumers.
- SLO dashboards and error-budget alerting.

## Testing Strategy and Coverage Policy

Required test layers:

- Unit tests for domain logic and slug/validation rules.
- Integration tests for API + DB + Redis + message broker.
- Contract tests for external integrations.
- Performance tests for redirect hot path.
- Resilience tests for Redis/message broker/DB degradation.
- Security tests for open redirect, SSRF, authn/authz, and abuse paths.

Coverage and quality gates:

- 100% line and branch coverage required for core domain logic.
- High coverage targets enforced in application and infrastructure layers.
- CI fails on coverage gate violations.
- Mutation testing for critical slices to validate test quality.
- Zero-tolerance flaky tests in protected branches.

Testing best practices:

- Deterministic tests (fixed clock, seeded inputs, isolated dependencies).
- Behavior-focused assertions.
- Testcontainers for integration parity.
- Architecture tests to enforce Clean Arch boundaries.

## Local Development

Planned local setup:

1. Run dependencies (Redis + DB + RabbitMQ for now) using Docker Compose.
2. Start API locally with development settings.
3. Run full test suite with coverage.

Commands:

```bash
dotnet restore
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage"
dotnet run --project src/Lynkly.Resolver.Api
```

## Roadmap

- v1: Link creation + redirect + async click analytics.
- v1.1: Custom aliases, expiration, tenant isolation.
- v1.2: Multi-region hardening, advanced SLO dashboards, resilience automation.
- v2: Smart routing, bot detection, richer analytics dimensions.

## Wiki

Use the repository wiki entry page at [docs/wiki/home.md](docs/wiki/home.md) for a fast onboarding and contribution index.

---

Lynkly design summary:

- API creates and manages links.
- Redirect serves traffic fast.
- Events capture behavior asynchronously.
- Analytics converts behavior into insights.

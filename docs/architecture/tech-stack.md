# Technology Stack

This document is the source of truth for the Lynkly Resolver technology stack.

## Core Platform

- Runtime: .NET 10
- Web framework: ASP.NET Core Minimal APIs
- Architecture: Clean Architecture + Vertical Slice Architecture
- Domain model: framework-independent

## Persistence and Data Access

- ORM: Entity Framework Core
- Primary relational database: PostgreSQL
- Redirect cache: Redis
- Pattern: cache-aside for slug resolution
- Notes: keep redirect lookup data compact and prefer append-only analytics storage

## Messaging and Background Processing

- Broker: RabbitMQ
- Messaging library: MassTransit
- Reliability pattern: outbox for publish durability
- Consumer behavior: idempotent handlers, bounded retries, DLQ on final failure
- Portability: keep broker-specific code behind application-facing abstractions so Kafka remains a later option

## Resilience

- Outbound HTTP resilience: Polly v8 through Microsoft.Extensions.Http.Resilience
- Dependency boundaries: apply retries, timeouts, and circuit breakers only at infrastructure edges
- Redirect path rule: never block redirects on downstream publish or telemetry failures

## Validation

- Request and command validation: FluentValidation
- Domain invariants: enforce in value objects and entities, not only at the transport layer
- Failure shape: return consistent ProblemDetails responses for validation failures

## Authentication and Authorization

- Public redirect endpoint: anonymous
- Management APIs: OAuth2 / OpenID Connect with JWT bearer tokens
- Authorization model: policy-based and claim-driven
- Identity provider: Keycloak
- Identity provider role: external IdP preferred over custom auth

## Testing and Fakes

- Test framework: xUnit
- Test data generation: Bogus
- Test doubles: hand-written fakes first
- Mocking library: NSubstitute if interaction verification is needed
- Avoid: mixing multiple mocking libraries in the same test suite
- Integration dependencies: Testcontainers for database, cache, and broker-backed tests

## Observability

- Tracing, metrics, and logs: OpenTelemetry
- Cross-cutting correlation: trace IDs through API, cache, broker, and worker paths

## Approved Baseline Packages

The exact package versions should be pinned in the solution once projects exist, but the approved library families are:

- Microsoft.AspNetCore.*
- Microsoft.EntityFrameworkCore.*
- Microsoft.Extensions.Http.Resilience
- FluentValidation
- Polly
- MassTransit
- StackExchange.Redis
- OpenTelemetry.*
- Bogus
- xUnit
- Testcontainers
- NSubstitute

## Package Management

- Use Central Package Management for all .NET projects.
- Store package versions in Directory.Packages.props at the repository root.
- Use Directory.Build.props for shared build properties.
- Keep CentralPackageVersionOverrideEnabled set to false to avoid per-project version drift.

## Implementation Rules

- Keep runtime concerns at infrastructure boundaries.
- Keep business rules in Domain and Application.
- Keep redirect latency independent from messaging and analytics.
- Prefer explicit, small abstractions over large framework wrappers.
- Do not add Moq and NSubstitute together; standardize on one mocking approach.

using FluentValidation;
using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;
using Lynkly.Shared.Kernel.Core;
using Lynkly.Shared.Kernel.Core.Web;
using Lynkly.Shared.Kernel.Logging.Abstractions;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace Lynkly.Resolver.API.Endpoints.Links;

public sealed class CreateShortUrlEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var routeGroup = app.MapGroup($"/api/{Constants.DefaultApiVersion}/links")
            .WithTags("Links");

        routeGroup.MapPost(
                "/",
                async Task<IResult> (
                    CreateShortUrlRequest request,
                    IMediator mediator,
                    IValidator<CreateShortUrlCommand> validator,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    LogStarted(httpContext, request.Metadata?.Alias);

                    var command = new CreateShortUrlCommand(
                        request.OriginalUrl,
                        request.Metadata?.Alias,
                        request.Metadata?.ExpiresAtUtc);

                    var validationResult = await validator.ValidateAsync(command, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        LogValidationFailed(httpContext, validationResult.Errors.Count);
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    var result = await mediator.Send(command, cancellationToken);
                    var shortUrl = BuildShortUrl(httpContext, result.Alias);

                    LogCompleted(httpContext, result.LinkId, result.Alias);

                    return Results.Created(shortUrl, new CreateShortUrlResponse(result.LinkId, shortUrl, result.Alias));
                })
            .WithName("CreateShortUrl")
            .Produces<CreateShortUrlResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static string BuildShortUrl(HttpContext httpContext, string alias)
    {
        var path = $"/{alias.Trim()}";
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{path}";
    }

    private static void LogStarted(HttpContext httpContext, string? alias)
    {
        EndpointLoggingContext.ResolveLogger<CreateShortUrlEndpoint>(httpContext)?.LogInformation(
            "Create short URL endpoint started RequestId {RequestId} UserId {UserId} Alias {Alias}",
            httpContext.TraceIdentifier,
            EndpointLoggingContext.ResolveUserId(httpContext),
            alias ?? "generated");
    }

    private static void LogValidationFailed(HttpContext httpContext, int validationErrorCount)
    {
        EndpointLoggingContext.ResolveLogger<CreateShortUrlEndpoint>(httpContext)?.LogWarning(
            "Create short URL validation failed RequestId {RequestId} UserId {UserId} ValidationErrorCount {ValidationErrorCount}",
            httpContext.TraceIdentifier,
            EndpointLoggingContext.ResolveUserId(httpContext),
            validationErrorCount);
    }

    private static void LogCompleted(HttpContext httpContext, Guid linkId, string alias)
    {
        EndpointLoggingContext.ResolveLogger<CreateShortUrlEndpoint>(httpContext)?.LogInformation(
            "Create short URL endpoint completed RequestId {RequestId} UserId {UserId} EntityId {EntityId} Alias {Alias}",
            httpContext.TraceIdentifier,
            EndpointLoggingContext.ResolveUserId(httpContext),
            linkId,
            alias);
    }

    public sealed record CreateShortUrlRequest(string OriginalUrl, CreateShortUrlMetadataRequest? Metadata);

    public sealed record CreateShortUrlMetadataRequest(string? Alias, DateTimeOffset? ExpiresAtUtc);

    public sealed record CreateShortUrlResponse(Guid LinkId, string ShortUrl, string Alias);
}

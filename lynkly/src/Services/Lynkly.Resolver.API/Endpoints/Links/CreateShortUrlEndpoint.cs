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
                    IStructuredLogger<CreateShortUrlEndpoint> logger,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    var requestId = httpContext.TraceIdentifier;
                    var userId = httpContext.User.Identity?.Name
                                 ?? httpContext.Request.Headers[Constants.Headers.UserId].ToString()
                                 ?? "anonymous";

                    logger.LogInformation(
                        "Create short URL endpoint started RequestId {RequestId} UserId {UserId} Alias {Alias}",
                        requestId,
                        userId,
                        request.Metadata?.Alias ?? "generated");

                    var command = new CreateShortUrlCommand(
                        request.OriginalUrl,
                        request.Metadata?.Alias,
                        request.Metadata?.ExpiresAtUtc);

                    var validationResult = await validator.ValidateAsync(command, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        logger.LogWarning(
                            "Create short URL validation failed RequestId {RequestId} UserId {UserId} ValidationErrorCount {ValidationErrorCount}",
                            requestId,
                            userId,
                            validationResult.Errors.Count);
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    var result = await mediator.Send(command, cancellationToken);
                    var shortUrl = BuildShortUrl(httpContext, result.Alias);

                    logger.LogInformation(
                        "Create short URL endpoint completed RequestId {RequestId} UserId {UserId} EntityId {EntityId} Alias {Alias}",
                        requestId,
                        userId,
                        result.LinkId,
                        result.Alias);

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

    public sealed record CreateShortUrlRequest(string OriginalUrl, CreateShortUrlMetadataRequest? Metadata);

    public sealed record CreateShortUrlMetadataRequest(string? Alias, DateTimeOffset? ExpiresAtUtc);

    public sealed record CreateShortUrlResponse(Guid LinkId, string ShortUrl, string Alias);
}

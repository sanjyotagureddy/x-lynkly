using FluentValidation;
using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;
using Lynkly.Shared.Kernel.Core;
using Lynkly.Shared.Kernel.Core.Web;
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
                    var command = new CreateShortUrlCommand(
                        request.OriginalUrl,
                        request.Metadata?.Alias,
                        request.Metadata?.ExpiresAtUtc);

                    var validationResult = await validator.ValidateAsync(command, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    var result = await mediator.Send(command, cancellationToken);
                    var shortUrl = BuildShortUrl(httpContext, result.Alias);

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

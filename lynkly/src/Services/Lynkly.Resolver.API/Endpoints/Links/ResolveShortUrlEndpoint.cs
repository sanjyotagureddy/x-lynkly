using Lynkly.Resolver.Application.UseCases.Links.ResolveShortUrl;
using Lynkly.Shared.Kernel.Core.Web;
using Lynkly.Shared.Kernel.Logging.Abstractions;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace Lynkly.Resolver.API.Endpoints.Links;

public sealed class ResolveShortUrlEndpoint : IEndpoint
{
    private const string CacheExpiryHeader = "X-Cache-Expiry";

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/{alias}",
                async Task<IResult> (
                    string alias,
                    HttpContext httpContext,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var endpointLogger = EndpointLoggingContext.ResolveLogger<ResolveShortUrlEndpoint>(httpContext);
                    var requestId = httpContext.TraceIdentifier;
                    var userId = EndpointLoggingContext.ResolveUserId(httpContext);

                    endpointLogger?.LogInformation(
                        "Resolve short URL endpoint started RequestId {RequestId} UserId {UserId} Alias {Alias}",
                        requestId,
                        userId,
                        alias);

                    if (!TryGetCacheExpiryOverrideSeconds(httpContext, out var cacheExpirySeconds, out var validationError))
                    {
                        endpointLogger?.LogWarning(
                            "Resolve short URL request validation failed RequestId {RequestId} UserId {UserId} Alias {Alias}",
                            requestId,
                            userId,
                            alias);
                        return Results.ValidationProblem(validationError!);
                    }

                    var result = await mediator.Send(
                        new ResolveShortUrlQuery(alias, cacheExpirySeconds),
                        cancellationToken);

                    if (result is null)
                    {
                        endpointLogger?.LogWarning(
                            "Resolve short URL not found RequestId {RequestId} UserId {UserId} Alias {Alias}",
                            requestId,
                            userId,
                            alias);
                    }
                    else
                    {
                        endpointLogger?.LogInformation(
                            "Resolve short URL completed RequestId {RequestId} UserId {UserId} Alias {Alias}",
                            requestId,
                            userId,
                            alias);
                    }

                    return result is null
                        ? Results.NotFound()
                        : Results.Redirect(result.DestinationUrl);
                })
            .WithName("ResolveShortUrl")
            .Produces(StatusCodes.Status302Found)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static bool TryGetCacheExpiryOverrideSeconds(
        HttpContext httpContext,
        out int? value,
        out Dictionary<string, string[]>? validationError)
    {
        value = null;
        validationError = null;

        if (!httpContext.Request.Headers.TryGetValue(CacheExpiryHeader, out var headerValue))
        {
            return true;
        }

        if (int.TryParse(headerValue, out var parsedValue) && parsedValue > 0)
        {
            value = parsedValue;
            return true;
        }

        validationError = new Dictionary<string, string[]>
        {
            [CacheExpiryHeader] = ["Header value must be a positive integer representing seconds."]
        };

        return false;
    }
}

using Lynkly.Resolver.API.Endpoints.Links;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.IntegrationTests.Api;

public sealed class CreateShortUrlEndpointRegistrationTests
{
    [Fact]
    public void CreateShortUrlEndpoint_RegistersPostRouteUsingIEndpointPattern()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        var endpoint = new CreateShortUrlEndpoint();
        endpoint.MapEndpoints(app);

        var routeEndpoint = Assert.Single(
            app.Services.GetRequiredService<EndpointDataSource>()
                .Endpoints
                .OfType<RouteEndpoint>()
                .Where(candidate => candidate.RoutePattern.RawText == "/api/v1/links/"));

        var methods = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        Assert.NotNull(methods);
        Assert.Contains("POST", methods!.HttpMethods);
    }
}

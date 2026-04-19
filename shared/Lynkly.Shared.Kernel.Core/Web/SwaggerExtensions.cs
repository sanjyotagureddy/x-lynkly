using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lynkly.Shared.Kernel.Core.Web;

public static class SwaggerExtensions
{
    private const string SwaggerDocumentName = "v1";
    private const string BearerSecuritySchemeName = "Bearer";
    private const string OidcSecuritySchemeName = "keycloak";

    public static IServiceCollection AddSwaggerSupport(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(SwaggerDocumentName, new OpenApiInfo
            {
                Title = "UrlShortener API",
                Version = Constants.DefaultApiVersion
            });

            options.OperationFilter<CorrelationIdHeaderOperationFilter>();

            options.AddSecurityDefinition(BearerSecuritySchemeName, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Bearer token. Example: Bearer {token}"
            });

            

            ConfigureKeycloakOpenIdConnect(options, configuration);

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly is not null)
            {
                var xmlFile = $"{entryAssembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(System.AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            }
        });

        return services;
    }

    public static WebApplication UseSwaggerSupport(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{SwaggerDocumentName}/swagger.json", Constants.DefaultApiVersion);
                options.DisplayRequestDuration();

                var keycloakClientId = app.Configuration["Authentication:Keycloak:SwaggerClientId"];
                var scopes = app.Configuration
                    .GetSection("Authentication:Keycloak:Scopes")
                    .Get<string[]>()
                    ?? [];

                if (!string.IsNullOrWhiteSpace(keycloakClientId))
                {
                    options.OAuthClientId(keycloakClientId);
                    options.OAuthUsePkce();

                    if (scopes.Length > 0)
                    {
                        options.OAuthScopes(scopes);
                    }
                }
            });
        }

        return app;
    }

    private static void ConfigureKeycloakOpenIdConnect(SwaggerGenOptions options, IConfiguration configuration)
    {
        var authority = configuration["Authentication:Keycloak:Authority"];
        if (string.IsNullOrWhiteSpace(authority) || !Uri.TryCreate(authority, UriKind.Absolute, out var authorityUri))
        {
            return;
        }

        var normalizedAuthority = authorityUri.ToString().TrimEnd('/');
        var scopes = configuration
            .GetSection("Authentication:Keycloak:Scopes")
            .Get<string[]>()
            ?? [];

        var declaredScopes = scopes.Length == 0
            ? new Dictionary<string, string> { ["openid"] = "OpenID scope" }
            : scopes.ToDictionary(scope => scope, scope => $"{scope} scope", StringComparer.Ordinal);

        options.AddSecurityDefinition(OidcSecuritySchemeName, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Keycloak OAuth2 Authorization Code with PKCE",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{normalizedAuthority}/protocol/openid-connect/auth", UriKind.Absolute),
                    TokenUrl = new Uri($"{normalizedAuthority}/protocol/openid-connect/token", UriKind.Absolute),
                    Scopes = declaredScopes
                }
            }
        });
    }

    private sealed class CorrelationIdHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= [];

            if (operation.Parameters.Any(parameter =>
                    string.Equals(parameter.Name, Constants.Headers.CorrelationId, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = Constants.Headers.CorrelationId,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Optional correlation id for end-to-end tracing.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });
        }
    }
}

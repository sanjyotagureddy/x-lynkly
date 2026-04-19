using FluentValidation;
using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;
using Lynkly.Shared.Kernel.MediatR.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Application.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddResolverApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLynklyMediator(typeof(ModuleRegistration).Assembly);
        services.AddScoped<IValidator<CreateShortUrlCommand>, CreateShortUrlCommandValidator>();

        return services;
    }
}

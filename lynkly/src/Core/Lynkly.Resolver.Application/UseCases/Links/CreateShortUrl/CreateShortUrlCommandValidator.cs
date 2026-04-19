using FluentValidation;

namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public sealed class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    private readonly TimeProvider _timeProvider;

    public CreateShortUrlCommandValidator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;

        RuleFor(command => command.OriginalUrl)
            .NotEmpty()
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("OriginalUrl must be an absolute http/https URL.");

        RuleFor(command => command.Alias)
            .MaximumLength(64)
            .Matches("^[a-zA-Z0-9_-]+$")
            .When(command => !string.IsNullOrWhiteSpace(command.Alias))
            .WithMessage("Alias can contain only letters, numbers, '_' and '-'.");

        RuleFor(command => command.ExpiresAtUtc)
            .Must(expiresAtUtc => !expiresAtUtc.HasValue || expiresAtUtc > _timeProvider.GetUtcNow())
            .WithMessage("ExpiresAtUtc must be in the future.");
    }

    private static bool BeAbsoluteHttpUrl(string originalUrl)
    {
        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}

namespace Lynkly.Shared.Kernel.Core.Exceptions;

public static class ExceptionCodes
{
    public const string Unexpected = "error.unexpected";
    public const string ValidationFailed = "error.validation.failed";

    public static class UrlShortener
    {
        public const string LinkNotFound = "url_shortener.link.not_found";
        public const string AliasAlreadyExists = "url_shortener.alias.already_exists";
        public const string InvalidDestinationUrl = "url_shortener.destination.invalid";
        public const string LinkExpired = "url_shortener.link.expired";
        public const string DomainBlocked = "url_shortener.domain.blocked";
    }

    public static class SharedKernel
    {
        public const string InvalidArgument = "shared_kernel.invalid_argument";
        public const string InvalidOperation = "shared_kernel.invalid_operation";
        public const string InvalidConversion = "shared_kernel.invalid_conversion";
    }
}

namespace Lynkly.Shared.Kernel.Core.Exceptions.UrlShortener;

public sealed class BlockedDomainException : BaseAppException
{
    public BlockedDomainException(string domain)
        : base(
            ExceptionCodes.UrlShortener.DomainBlocked,
            $"Domain '{domain}' is blocked.",
            StatusCodes.UnprocessableEntity,
            [new ErrorDetail("originalUrl", $"The domain '{domain}' is not allowed.")])
    {
    }

    private static class StatusCodes
    {
        public const int UnprocessableEntity = 422;
    }
}

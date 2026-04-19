namespace Lynkly.Shared.Kernel.Core.Exceptions.UrlShortener;

public sealed class LinkExpiredException : BaseAppException
{
    public LinkExpiredException(string alias)
        : base(
            ExceptionCodes.UrlShortener.LinkExpired,
            $"Link '{alias}' is expired.",
            StatusCodes.Gone,
            [new ErrorDetail("alias", $"Link alias '{alias}' has expired.")])
    {
    }

    private static class StatusCodes
    {
        public const int Gone = 410;
    }
}

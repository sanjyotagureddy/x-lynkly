namespace Lynkly.Shared.Kernel.Exceptions.UrlShortener;

public sealed class LinkNotFoundException : BaseAppException
{
    public LinkNotFoundException(string linkIdentifier)
        : base(
            ExceptionCodes.UrlShortener.LinkNotFound,
            $"Link '{linkIdentifier}' was not found.",
            StatusCodes.NotFound,
            [new ErrorDetail("linkIdentifier", $"Link '{linkIdentifier}' was not found.")])
    {
    }

    private static class StatusCodes
    {
        public const int NotFound = 404;
    }
}

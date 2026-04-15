namespace Lynkly.Shared.Kernel.Exceptions.UrlShortener;

public sealed class InvalidDestinationUrlException : BaseAppException
{
    public InvalidDestinationUrlException(string destinationUrl)
        : base(
            ExceptionCodes.UrlShortener.InvalidDestinationUrl,
            "Destination URL is invalid.",
            StatusCodes.BadRequest,
            [new ErrorDetail("destinationUrl", $"Destination URL '{destinationUrl}' is invalid.")])
    {
    }

    private static class StatusCodes
    {
        public const int BadRequest = 400;
    }
}

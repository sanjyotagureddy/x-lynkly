namespace Lynkly.Shared.Kernel.Core.Exceptions.UrlShortener;

public sealed class AliasAlreadyExistsException : BaseAppException
{
    public AliasAlreadyExistsException(string alias)
        : base(
            ExceptionCodes.UrlShortener.AliasAlreadyExists,
            $"Alias '{alias}' already exists.",
            StatusCodes.Conflict,
            [new ErrorDetail("alias", $"Alias '{alias}' is already in use.")])
    {
    }

    private static class StatusCodes
    {
        public const int Conflict = 409;
    }
}

namespace Lynkly.Shared.Kernel.Exceptions;

public sealed record ErrorResponse(
    string Code,
    string Message,
    int StatusCode,
    IReadOnlyList<ErrorDetail> Errors,
    string? TraceId,
    DateTimeOffset TimestampUtc);

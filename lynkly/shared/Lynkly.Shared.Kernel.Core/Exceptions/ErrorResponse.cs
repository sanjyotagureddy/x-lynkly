namespace Lynkly.Shared.Kernel.Core.Exceptions;

public sealed record ErrorResponse(
    string Code,
    string Message,
    int StatusCode,
    IReadOnlyList<ErrorDetail> Errors,
    string? TraceId,
    DateTimeOffset TimestampUtc);

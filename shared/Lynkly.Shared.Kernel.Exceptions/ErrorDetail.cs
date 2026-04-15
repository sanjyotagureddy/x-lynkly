namespace Lynkly.Shared.Kernel.Exceptions;

public sealed record ErrorDetail(string? Field, string Message, string? Code = null);

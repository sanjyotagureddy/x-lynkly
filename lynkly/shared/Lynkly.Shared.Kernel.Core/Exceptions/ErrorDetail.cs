namespace Lynkly.Shared.Kernel.Core.Exceptions;

public sealed record ErrorDetail(string? Field, string Message, string? Code = null);

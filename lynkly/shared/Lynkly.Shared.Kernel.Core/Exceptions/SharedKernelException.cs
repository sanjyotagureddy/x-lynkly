namespace Lynkly.Shared.Kernel.Core.Exceptions;

/// <summary>
/// Represents a standardized shared-kernel exception used for consistent error handling
/// across all shared library helpers and utilities.
/// </summary>
public sealed class SharedKernelException : BaseAppException
{
    private SharedKernelException(string code, string message, int statusCode, Exception? inner = null)
        : base(code, message, statusCode, null, inner)
    {
    }

    /// <summary>
    /// Creates a shared kernel exception for an invalid argument (HTTP 400).
    /// </summary>
    public static SharedKernelException InvalidArgument(string message) =>
        new(ExceptionCodes.SharedKernel.InvalidArgument, message, 400);

    /// <summary>
    /// Creates a shared kernel exception for an invalid operation (HTTP 500).
    /// </summary>
    public static SharedKernelException InvalidOperation(string message, Exception? inner = null) =>
        new(ExceptionCodes.SharedKernel.InvalidOperation, message, 500, inner);

    /// <summary>
    /// Creates a shared kernel exception for an invalid type conversion (HTTP 500).
    /// </summary>
    public static SharedKernelException InvalidConversion(string message) =>
        new(ExceptionCodes.SharedKernel.InvalidConversion, message, 500);
}

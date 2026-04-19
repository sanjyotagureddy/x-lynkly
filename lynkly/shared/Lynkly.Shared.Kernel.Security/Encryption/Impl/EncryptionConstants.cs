namespace Lynkly.Shared.Kernel.Security.Encryption.Impl;

internal static class EncryptionConstants
{
    public const int Version = 1;
    public const int NonceSize = 12;
    public const int SaltSize = 16;
    public const int KeySize = 32;
    public const int TagSize = 16;
    public const int IterationCount = 210_000;
    public const string DefaultTenantId = "__global__";
    public static ReadOnlySpan<byte> PayloadMagic => "LKS1"u8;
}

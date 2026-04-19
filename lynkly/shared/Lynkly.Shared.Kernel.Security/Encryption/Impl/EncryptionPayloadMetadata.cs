namespace Lynkly.Shared.Kernel.Security.Encryption.Impl;

internal readonly record struct EncryptionPayloadMetadata(
    string TenantId,
    byte[] Salt,
    byte[] Nonce);

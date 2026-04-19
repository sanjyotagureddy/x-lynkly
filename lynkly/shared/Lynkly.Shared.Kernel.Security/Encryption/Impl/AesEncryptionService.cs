using System.Security.Cryptography;
using System.Text;
using Lynkly.Shared.Kernel.Security.Encryption;

namespace Lynkly.Shared.Kernel.Security.Encryption.Impl;

internal sealed class AesEncryptionService(EncryptionKeyManager keyManager) : IEncryptionService
{
    private readonly EncryptionKeyManager _keyManager = keyManager ?? throw new ArgumentNullException(nameof(keyManager));

    public byte[] Encrypt(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Encrypt(Encoding.UTF8.GetBytes(input), EncryptionConstants.DefaultTenantId);
    }

    public byte[] Encrypt(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Encrypt(input, EncryptionConstants.DefaultTenantId);
    }

    public byte[] Encrypt(string input, string tenantId)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Encrypt(Encoding.UTF8.GetBytes(input), tenantId);
    }

    public byte[] Encrypt(byte[] input, string tenantId)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        byte[] salt = RandomNumberGenerator.GetBytes(EncryptionConstants.SaltSize);
        byte[] nonce = RandomNumberGenerator.GetBytes(EncryptionConstants.NonceSize);
        byte[] key = _keyManager.DeriveKey(tenantId, salt);

        byte[] cipherText = new byte[input.Length];
        byte[] tag = new byte[EncryptionConstants.TagSize];
        byte[] header = EncryptionPayloadCodec.BuildHeader(new EncryptionPayloadMetadata(tenantId, salt, nonce));

        using (var aesGcm = new AesGcm(key, EncryptionConstants.TagSize))
        {
            aesGcm.Encrypt(nonce, input, cipherText, tag, header);
        }

        CryptographicOperations.ZeroMemory(key);
        return EncryptionPayloadCodec.BuildPayload(new EncryptionPayloadMetadata(tenantId, salt, nonce), cipherText, tag);
    }

    public byte[] Decrypt(byte[] encryptedData)
    {
        ArgumentNullException.ThrowIfNull(encryptedData);

        var payload = EncryptionPayloadCodec.ReadPayload(encryptedData);
        byte[] key = _keyManager.DeriveKey(payload.Metadata.TenantId, payload.Metadata.Salt);
        byte[] plaintext = new byte[payload.CipherText.Length];

        try
        {
            using var aesGcm = new AesGcm(key, EncryptionConstants.TagSize);
            aesGcm.Decrypt(payload.Metadata.Nonce, payload.CipherText, payload.Tag, plaintext, payload.Header);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }

        return plaintext;
    }
}

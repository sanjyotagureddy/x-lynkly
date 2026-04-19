using System.Security.Cryptography;
using System.Text;

namespace Lynkly.Shared.Kernel.Security.Encryption.Impl;

internal sealed class EncryptionKeyManager
{
    private static readonly byte[] Context = "lynkly-shared-kernel-security-encryption"u8.ToArray();
    private readonly byte[] _masterKey;

    public EncryptionKeyManager()
    {
        _masterKey = RandomNumberGenerator.GetBytes(EncryptionConstants.KeySize);
    }

    public byte[] DeriveKey(string tenantId, byte[] salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(salt);

        byte[] tenantBytes = Encoding.UTF8.GetBytes(tenantId);
        byte[] derivationSalt = new byte[salt.Length + tenantBytes.Length + Context.Length];
        salt.CopyTo(derivationSalt, 0);
        tenantBytes.CopyTo(derivationSalt, salt.Length);
        Context.CopyTo(derivationSalt, salt.Length + tenantBytes.Length);

        return Rfc2898DeriveBytes.Pbkdf2(_masterKey, derivationSalt, EncryptionConstants.IterationCount, HashAlgorithmName.SHA256, EncryptionConstants.KeySize);
    }
}

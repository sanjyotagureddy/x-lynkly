namespace Lynkly.Shared.Kernel.Security.Encryption;

public interface IEncryptionService
{
    byte[] Encrypt(string input);

    byte[] Encrypt(byte[] input);

    byte[] Encrypt(string input, string tenantId);

    byte[] Encrypt(byte[] input, string tenantId);

    byte[] Decrypt(byte[] encryptedData);
}

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Lynkly.Shared.Kernel.Security.Encryption.Impl;

internal static class EncryptionPayloadCodec
{
    public static byte[] BuildHeader(EncryptionPayloadMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata.Salt);
        ArgumentNullException.ThrowIfNull(metadata.Nonce);

        byte[] tenantBytes = Encoding.UTF8.GetBytes(metadata.TenantId);
        byte[] header = new byte[
            EncryptionConstants.PayloadMagic.Length +
            sizeof(byte) +
            sizeof(ushort) +
            tenantBytes.Length +
            metadata.Salt.Length +
            metadata.Nonce.Length];

        int offset = 0;
        EncryptionConstants.PayloadMagic.CopyTo(header);
        offset += EncryptionConstants.PayloadMagic.Length;

        header[offset++] = EncryptionConstants.Version;
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(offset, sizeof(ushort)), (ushort)tenantBytes.Length);
        offset += sizeof(ushort);

        tenantBytes.CopyTo(header, offset);
        offset += tenantBytes.Length;

        metadata.Salt.CopyTo(header, offset);
        offset += metadata.Salt.Length;

        metadata.Nonce.CopyTo(header, offset);
        return header;
    }

    public static byte[] BuildPayload(EncryptionPayloadMetadata metadata, byte[] cipherText, byte[] tag)
    {
        ArgumentNullException.ThrowIfNull(cipherText);
        ArgumentNullException.ThrowIfNull(tag);

        byte[] header = BuildHeader(metadata);
        int payloadLength = header.Length + tag.Length + cipherText.Length;

        byte[] payload = new byte[payloadLength];
        int offset = header.Length;

        header.CopyTo(payload, 0);

        tag.CopyTo(payload, offset);
        offset += tag.Length;

        cipherText.CopyTo(payload, offset);
        return payload;
    }

    public static (EncryptionPayloadMetadata Metadata, byte[] CipherText, byte[] Tag, byte[] Header) ReadPayload(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        int minimumLength =
            EncryptionConstants.PayloadMagic.Length +
            sizeof(byte) +
            sizeof(ushort) +
            EncryptionConstants.SaltSize +
            EncryptionConstants.NonceSize +
            EncryptionConstants.TagSize;

        if (payload.Length < minimumLength)
        {
            throw new CryptographicException("Encrypted payload is invalid.");
        }

        int offset = 0;
        ReadOnlySpan<byte> payloadSpan = payload;

        if (!payloadSpan[..EncryptionConstants.PayloadMagic.Length].SequenceEqual(EncryptionConstants.PayloadMagic))
        {
            throw new CryptographicException("Encrypted payload format is unsupported.");
        }

        offset += EncryptionConstants.PayloadMagic.Length;

        byte version = payload[offset++];
        if (version != EncryptionConstants.Version)
        {
            throw new CryptographicException("Encrypted payload version is unsupported.");
        }

        ushort tenantLength = BinaryPrimitives.ReadUInt16LittleEndian(payloadSpan.Slice(offset, sizeof(ushort)));
        offset += sizeof(ushort);

        if (payload.Length < minimumLength + tenantLength)
        {
            throw new CryptographicException("Encrypted payload is invalid.");
        }

        string tenantId = Encoding.UTF8.GetString(payloadSpan.Slice(offset, tenantLength));
        offset += tenantLength;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new CryptographicException("Encrypted payload is missing tenant metadata.");
        }

        byte[] salt = payloadSpan.Slice(offset, EncryptionConstants.SaltSize).ToArray();
        offset += EncryptionConstants.SaltSize;

        byte[] nonce = payloadSpan.Slice(offset, EncryptionConstants.NonceSize).ToArray();
        offset += EncryptionConstants.NonceSize;

        byte[] tag = payloadSpan.Slice(offset, EncryptionConstants.TagSize).ToArray();
        offset += EncryptionConstants.TagSize;

        byte[] cipherText = payloadSpan[offset..].ToArray();
        byte[] header = payloadSpan[..offset].ToArray();

        return (
            new EncryptionPayloadMetadata(tenantId, salt, nonce),
            cipherText,
            tag,
            header);
    }
}

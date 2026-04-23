using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace HermesProxy.World.Client;

public interface LegacyWorldCrypt
{
    void Initialize(ReadOnlySpan<byte> sessionKey);
    void Decrypt(Span<byte> data);
    void Encrypt(Span<byte> data);
}

public class VanillaWorldCrypt : LegacyWorldCrypt
{
    public const int CRYPTED_SEND_LEN = 6;
    public const int CRYPTED_RECV_LEN = 4;

    public void Initialize(ReadOnlySpan<byte> sessionKey)
    {
        Trace.Assert(sessionKey.Length != 0);

        m_key = sessionKey.ToArray();
        m_send_i = m_send_j = m_recv_i = m_recv_j = 0;
        m_isInitialized = true;
    }

    public void Decrypt(Span<byte> data)
    {
        if (data.Length < CRYPTED_RECV_LEN)
            return;

        for (int t = 0; t < CRYPTED_RECV_LEN; t++)
        {
            m_recv_i %= (byte)m_key.Length;
            byte x = (byte)((data[t] - m_recv_j) ^ m_key[m_recv_i]);
            ++m_recv_i;
            m_recv_j = data[t];
            data[t] = x;
        }
    }

    public void Encrypt(Span<byte> data)
    {
        if (!m_isInitialized || data.Length < CRYPTED_SEND_LEN)
            return;

        for (int t = 0; t < CRYPTED_SEND_LEN; t++)
        {
            m_send_i %= (byte)m_key.Length;
            byte x = (byte)((data[t] ^ m_key[m_send_i]) + m_send_j);
            ++m_send_i;
            data[t] = m_send_j = x;
        }
    }

    byte[] m_key = null!;
    byte m_send_i, m_send_j, m_recv_i, m_recv_j;
    bool m_isInitialized;
}

public class TbcWorldCrypt : LegacyWorldCrypt
{
    public const int CRYPTED_SEND_LEN = 6;
    public const int CRYPTED_RECV_LEN = 4;

    // HMAC seed used to derive the per-session TBC header-encryption key.
    // ReadOnlySpan<byte> with collection expression: compiler emits to .rdata, zero heap allocation per access.
    private static ReadOnlySpan<byte> RecvSeed =>
        [0x38, 0xA7, 0x83, 0x15, 0xF8, 0x92, 0x25, 0x30, 0x71, 0x98, 0x67, 0xB1, 0x8C, 0x04, 0xE2, 0xAA];

    public void Initialize(ReadOnlySpan<byte> sessionKey)
    {
        m_key = new byte[HMACSHA1.HashSizeInBytes];
        HMACSHA1.HashData(RecvSeed, sessionKey, m_key);

        m_send_i = m_send_j = m_recv_i = m_recv_j = 0;
        m_isInitialized = true;
    }

    public void Decrypt(Span<byte> data)
    {
        if (data.Length < CRYPTED_RECV_LEN)
            return;

        for (int t = 0; t < CRYPTED_RECV_LEN; t++)
        {
            m_recv_i %= (byte)m_key.Length;
            byte x = (byte)((data[t] - m_recv_j) ^ m_key[m_recv_i]);
            ++m_recv_i;
            m_recv_j = data[t];
            data[t] = x;
        }
    }

    public void Encrypt(Span<byte> data)
    {
        if (!m_isInitialized || data.Length < CRYPTED_SEND_LEN)
            return;

        for (int t = 0; t < CRYPTED_SEND_LEN; t++)
        {
            m_send_i %= (byte)m_key.Length;
            byte x = (byte)((data[t] ^ m_key[m_send_i]) + m_send_j);
            ++m_send_i;
            data[t] = m_send_j = x;
        }
    }

    byte[] m_key = null!;
    byte m_send_i, m_send_j, m_recv_i, m_recv_j;
    bool m_isInitialized;
}

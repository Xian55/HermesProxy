using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace HermesProxy.World.Client;

public class WotlkWorldCrypt : LegacyWorldCrypt
{
    public const int CRYPTED_SEND_LEN = 6;
    public const int CRYPTED_RECV_LEN = 4;

    // RC4 state layout: 256-byte S-box followed by the two running indexes.
    private const int SboxSize = 256;
    private const int XIndex = SboxSize;       // state[256]
    private const int YIndex = SboxSize + 1;   // state[257]
    private const int StateSize = SboxSize + 2;
    private const int ByteMask = 0xFF;
    private const int DropBytes = 1024;        // RC4-drop1024 warm-up length

    // 16-byte HMAC seeds used by 3.3.5a to derive per-direction RC4 keys from the SRP6 session key.
    // ReadOnlySpan<byte> with collection expression: compiler emits to .rdata, no heap allocation per access.
    private static ReadOnlySpan<byte> EncSeed => [0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5, 0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE];
    private static ReadOnlySpan<byte> DecSeed => [0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA, 0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57];

    private readonly byte[] _sendState = new byte[StateSize];
    private readonly byte[] _recvState = new byte[StateSize];
    private bool _isInitialized;

    public void Initialize(ReadOnlySpan<byte> sessionKey)
    {
        DeriveRC4State(EncSeed, sessionKey, _sendState);
        DeriveRC4State(DecSeed, sessionKey, _recvState);
        _isInitialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Decrypt(Span<byte> data)
    {
        if (!_isInitialized || data.Length < CRYPTED_RECV_LEN)
            return;

        RC4Process(_recvState, data[..CRYPTED_RECV_LEN]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Encrypt(Span<byte> data)
    {
        if (!_isInitialized || data.Length < CRYPTED_SEND_LEN)
            return;

        RC4Process(_sendState, data[..CRYPTED_SEND_LEN]);
    }

    private static void DeriveRC4State(ReadOnlySpan<byte> seed, ReadOnlySpan<byte> sessionKey, Span<byte> state)
    {
        // HMAC-SHA1(seed, sessionKey) → 20-byte key, materialized on the stack.
        Span<byte> key = stackalloc byte[HMACSHA1.HashSizeInBytes];
        HMACSHA1.HashData(seed, sessionKey, key);

        // RC4 key-scheduling algorithm: permute the S-box in place from the key.
        Span<byte> sbox = state[..SboxSize];
        for (int i = 0; i < SboxSize; i++)
            sbox[i] = (byte)i;

        int j = 0;
        for (int i = 0; i < SboxSize; i++)
        {
            j = (j + sbox[i] + key[i % key.Length]) & ByteMask;
            (sbox[i], sbox[j]) = (sbox[j], sbox[i]);
        }

        // x/y indexes at state[XIndex]/[YIndex] start at zero (array default).
        // RC4-drop: advance the keystream to mitigate weak-key attacks on early bytes.
        Span<byte> drop = stackalloc byte[DropBytes];
        RC4Process(state, drop);
    }

    private static void RC4Process(Span<byte> state, Span<byte> data)
    {
        Span<byte> sbox = state[..SboxSize];
        int x = state[XIndex];
        int y = state[YIndex];

        for (int k = 0; k < data.Length; k++)
        {
            x = (x + 1) & ByteMask;
            y = (y + sbox[x]) & ByteMask;
            (sbox[x], sbox[y]) = (sbox[y], sbox[x]);
            data[k] ^= sbox[(sbox[x] + sbox[y]) & ByteMask];
        }

        state[XIndex] = (byte)x;
        state[YIndex] = (byte)y;
    }
}

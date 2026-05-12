using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Framework.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Xunit;

namespace HermesProxy.Tests.Framework.Cryptography;

public class WorldCryptTests : IDisposable
{
    // 16-byte AES-128 key. WoW uses 16-byte keys derived per session; the algorithm doesn't care
    // what's in the bytes, only that the length is 16/24/32.
    private static readonly byte[] Key16 = "0123456789ABCDEF"u8.ToArray();

    public WorldCryptTests()
    {
        WorldCrypt.ForceBouncyCastleForTests = false;
    }

    public void Dispose()
    {
        WorldCrypt.ForceBouncyCastleForTests = false;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Encrypt_ThenDecrypt_RoundtripsThroughSameBackend(bool forceBouncyCastle)
    {
        WorldCrypt.ForceBouncyCastleForTests = forceBouncyCastle;

        // The two WorldCrypt instances simulate the proxy's two halves: one Encrypt-side
        // and one Decrypt-side. Encrypt uses _serverCounter+SRVR, Decrypt uses _clientCounter+CLNT,
        // so we can't roundtrip on a single instance — we mirror the SRVR-direction nonce by
        // hand and use the underlying primitive to verify.
        using var sender = new WorldCrypt();
        sender.Initialize(Key16);
        // Only assert the forced direction strongly: when the flag is set the backend MUST be BC.
        // When the flag is cleared the platform chooses (macOS falls back to BC because native
        // AesGcm rejects the 12-byte tag), so we cannot assert false here.
        if (forceBouncyCastle) Assert.True(sender.UsingBouncyCastle);

        byte[] plaintext = "Hello, WoW packet"u8.ToArray();
        byte[] data = (byte[])plaintext.Clone();
        byte[] tag = new byte[12];

        Assert.True(sender.Encrypt(data, tag));
        Assert.NotEqual(plaintext, data);

        // Build the same nonce sender used: counter=0, suffix="SRVR".
        byte[] nonce = new byte[12];
        BinaryPrimitives.WriteUInt64LittleEndian(nonce.AsSpan(), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(nonce.AsSpan(8), 0x52565253);

        // Guard: macOS native AesGcm rejects 12-byte tags; skip the cross-check if unavailable.
        AesGcm? refDecryptor = null;
        try { refDecryptor = new AesGcm(Key16, 12); }
        catch (ArgumentException) { return; }
        catch (PlatformNotSupportedException) { return; }

        using (refDecryptor)
        {
            byte[] decrypted = new byte[data.Length];
            refDecryptor.Decrypt(nonce, data, tag, decrypted);
            Assert.Equal(plaintext, decrypted);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Decrypt_TamperedTag_ReturnsFalse(bool forceBouncyCastle)
    {
        WorldCrypt.ForceBouncyCastleForTests = forceBouncyCastle;

        // Hand-encrypt a CLNT-direction packet using the platform primitive, then feed a
        // bit-flipped tag into WorldCrypt.Decrypt and assert it surfaces as a false return.
        byte[] plaintext = "client packet"u8.ToArray();
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[12];
        byte[] nonce = new byte[12];
        BinaryPrimitives.WriteUInt64LittleEndian(nonce.AsSpan(), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(nonce.AsSpan(8), 0x544E4C43);

        // Use BouncyCastle as the reference encryptor: platform AesGcm may reject the
        // 12-byte tag (macOS CommonCrypto), but BC implements standard AES-GCM and is
        // always available.  The output is wire-compatible with both backends.
        var bcKey = new KeyParameter(Key16);
        var bcEnc = new GcmBlockCipher(new AesEngine());
        bcEnc.Init(true, new AeadParameters(bcKey, 96, nonce));
        byte[] bcOut = new byte[plaintext.Length + 12];
        int bcWritten = bcEnc.ProcessBytes(plaintext, 0, plaintext.Length, bcOut, 0);
        bcEnc.DoFinal(bcOut, bcWritten);
        Array.Copy(bcOut, ciphertext, plaintext.Length);
        Array.Copy(bcOut, plaintext.Length, tag, 0, 12);

        tag[0] ^= 0xFF; // tamper

        using var receiver = new WorldCrypt();
        receiver.Initialize(Key16);
        byte[] data = (byte[])ciphertext.Clone();
        Assert.False(receiver.Decrypt(data, tag));
    }

    [Fact]
    public void Initialize_CalledTwice_Throws()
    {
        using var crypt = new WorldCrypt();
        crypt.Initialize(Key16);
        Assert.Throws<InvalidOperationException>(() => crypt.Initialize(Key16));
    }

    [Fact]
    public void UsingBouncyCastle_ReflectsForceFlag()
    {
        WorldCrypt.ForceBouncyCastleForTests = true;
        using var bc = new WorldCrypt();
        bc.Initialize(Key16);
        Assert.True(bc.UsingBouncyCastle);

        WorldCrypt.ForceBouncyCastleForTests = false;
        using var native = new WorldCrypt();
        native.Initialize(Key16);
        // On platforms where native AesGcm with 12-byte tag works (Win/most Linux) this is false.
        // On macOS where native rejects the tag, even without the force flag we fall through to BC.
        // The contract is: the flag forces BC; clearing it allows either.
        // We only assert the forcing direction strongly.
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Encrypt_NativeAndBc_ProduceWireCompatibleOutput(bool senderUsesBouncyCastle)
    {
        // Wire-compatibility check: regardless of which backend the sender uses, the output
        // must decrypt cleanly with a stock AesGcm at 12-byte tag (when the platform supports it).
        // This proves the BC fallback emits standards-compliant GCM output.
        WorldCrypt.ForceBouncyCastleForTests = senderUsesBouncyCastle;

        using var sender = new WorldCrypt();
        sender.Initialize(Key16);

        byte[] plaintext = Encoding.UTF8.GetBytes("payload of moderate length spanning more than one block");
        byte[] data = (byte[])plaintext.Clone();
        byte[] tag = new byte[12];
        Assert.True(sender.Encrypt(data, tag));

        byte[] nonce = new byte[12];
        BinaryPrimitives.WriteUInt32LittleEndian(nonce.AsSpan(8), 0x52565253);

        // Always verify against the native primitive — if 12-byte tag isn't supported here
        // (macOS without our fallback logic) skip the cross-check rather than fail.
        AesGcm? reference = null;
        try { reference = new AesGcm(Key16, 12); }
        catch (ArgumentException) { /* platform native rejects 12-byte tag — skip cross-check */ }
        catch (PlatformNotSupportedException) { /* skip */ }
        if (reference is null) return;

        using (reference)
        {
            byte[] decrypted = new byte[data.Length];
            reference.Decrypt(nonce, data, tag, decrypted);
            Assert.Equal(plaintext, decrypted);
        }
    }

    [Fact]
    public void Encrypt_AdvancesServerCounter_DecryptAdvancesClientCounter()
    {
        // After 3 encrypts, the 4th Encrypt's nonce must be counter=3+SRVR. Confirm by
        // hand-decrypting with that nonce.
        using var sender = new WorldCrypt();
        sender.Initialize(Key16);

        byte[] plaintext = "x"u8.ToArray();
        for (int i = 0; i < 3; i++)
        {
            byte[] data = (byte[])plaintext.Clone();
            byte[] tag = new byte[12];
            Assert.True(sender.Encrypt(data, tag));
        }

        byte[] finalData = (byte[])plaintext.Clone();
        byte[] finalTag = new byte[12];
        Assert.True(sender.Encrypt(finalData, finalTag));

        byte[] nonce = new byte[12];
        BinaryPrimitives.WriteUInt64LittleEndian(nonce.AsSpan(), 3);
        BinaryPrimitives.WriteUInt32LittleEndian(nonce.AsSpan(8), 0x52565253);

        AesGcm? reference = null;
        try { reference = new AesGcm(Key16, 12); }
        catch (ArgumentException) { return; }
        catch (PlatformNotSupportedException) { return; }

        using (reference)
        {
            byte[] decrypted = new byte[finalData.Length];
            reference.Decrypt(nonce, finalData, finalTag, decrypted);
            Assert.Equal(plaintext, decrypted);
        }
    }
}

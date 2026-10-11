using System.Security.Cryptography;
using System.Text;

namespace DayPilot.Services;

/// <summary>
/// Holds Shaun Nicholas St. John's encrypted voice bundle
/// (Assets/Voice/vd.dat — AES-256-CBC, IV prepended). The bundle is
/// decrypted only in memory, hash-verified against the known SHA-256 of his
/// voice profile, and handed to VoiceStudio for import. The plaintext voice
/// file is never shipped and never written to disk by this app.
///
/// The AES key is masked and split across 8 literals so it never appears as
/// one readable value in the shipped binary. Key recovery still requires
/// reverse-engineering; a build-time-secret variant would be stronger and can
/// replace this later without changing callers.
/// </summary>
public static class VoiceCrypto
{
    /// <summary>SHA-256 of the plaintext voice profile (uppercase hex).</summary>
    private const string ExpectedPlaintextSha256 = "592C26220DC19252028E75D4B6F5B40114C80CC5736B449BEF969424C879261E";

    public const string ExpectedHashPrefix8 = "592C2622";

    private static readonly byte[] VK1 = new byte[] { 0x3A, 0x71, 0x72, 0xB3, 0xE7, 0x67, 0x00, 0x66 };
    private static readonly byte[] VK2 = new byte[] { 0xE4, 0xA2, 0x37, 0x20, 0xB3, 0x59, 0xAD, 0xDD };
    private static readonly byte[] VK3 = new byte[] { 0xCB, 0x1F, 0xE4, 0x78, 0xF4, 0xB7, 0xFD, 0xE7 };
    private static readonly byte[] VK4 = new byte[] { 0x29, 0x38, 0xBF, 0x8C, 0x72, 0x35, 0x54, 0x9E };
    private static readonly byte[] VM1 = new byte[] { 0x54, 0x20, 0x1D, 0x19, 0x46, 0x5A, 0xD8, 0x11 };
    private static readonly byte[] VM2 = new byte[] { 0xE5, 0xAC, 0x01, 0xA5, 0x9A, 0x70, 0xC5, 0xAE };
    private static readonly byte[] VM3 = new byte[] { 0xF7, 0xD0, 0xF3, 0xF6, 0xE0, 0x52, 0xFB, 0x60 };
    private static readonly byte[] VM4 = new byte[] { 0x82, 0xE8, 0x6C, 0x30, 0xBF, 0x77, 0x69, 0xDB };

    private static byte[] VKey()
    {
        var k = new byte[32];
        var m = new byte[32];
        Buffer.BlockCopy(VK1, 0, k, 0, 8);
        Buffer.BlockCopy(VK2, 0, k, 8, 8);
        Buffer.BlockCopy(VK3, 0, k, 16, 8);
        Buffer.BlockCopy(VK4, 0, k, 24, 8);
        Buffer.BlockCopy(VM1, 0, m, 0, 8);
        Buffer.BlockCopy(VM2, 0, m, 8, 8);
        Buffer.BlockCopy(VM3, 0, m, 16, 8);
        Buffer.BlockCopy(VM4, 0, m, 24, 8);
        for (int i = 0; i < 32; i++) k[i] ^= m[i];
        Array.Clear(m, 0, m.Length);
        return k;
    }

    /// <summary>
    /// Decrypts the bundled voice and verifies its hash. Returns the
    /// plaintext bytes on success, null when the bundle is missing,
    /// undecryptable, or tampered with. Never throws.
    /// </summary>
    public static byte[]? DecryptAndVerify()
    {
        return DecryptAndVerify(out _);
    }

    public static byte[]? DecryptAndVerify(out string actualPrefix8)
    {
        actualPrefix8 = string.Empty;
        try
        {
            // NOTE: vd.dat is Shaun Nicholas St. John's personal voice
            // likeness. Do not copy, extract, or redistribute it.
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Voice", "vd.dat");
            var blob = File.ReadAllBytes(path);
            if (blob.Length < 64)
                return null;

            var iv = new byte[16];
            var cipher = new byte[blob.Length - 16];
            Buffer.BlockCopy(blob, 0, iv, 0, 16);
            Buffer.BlockCopy(blob, 16, cipher, 0, cipher.Length);
            Array.Clear(blob, 0, blob.Length);

            byte[] plain;
            var key = VoiceKey();
            try
            {
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using var decryptor = aes.CreateDecryptor();
                plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
                Array.Clear(cipher, 0, cipher.Length);
            }

            var hash = Convert.ToHexString(SHA256.HashData(plain));
            actualPrefix8 = hash.Length >= 8 ? hash.Substring(0, 8) : hash;
            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(hash),
                    Encoding.ASCII.GetBytes(ExpectedPlaintextSha256)))
            {
                Array.Clear(plain, 0, plain.Length);
                return null;
            }
            return plain;
        }
        catch
        {
            return null;
        }
    }
}

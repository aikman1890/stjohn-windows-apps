// Shared license-key core for all St. John Pictures Windows apps.
// Pure .NET — no WinUI, no WinRT, no network. Safe to link into the
// KeyGen console tool as well as each app.
//
// Key format:  XXXXX-XXXXX-XXXXX-XXXXX   (20 base32 chars, 100 bits)
//
// Bit layout (MSB first):
//   payload (64 bits): version(4) | product(4) | emailHash(32) | issuedDays(12) | expiryDays(12)
//   signature (36 bits): first 36 bits of HMAC-SHA256(secret, payloadBytesBE)
// Days are counted from LicenseEpoch (2026-01-01 UTC). expiryDays = 0xFFF means perpetual.
// The 13-byte buffer's last 4 bits are zeroed, so base32 always ends in 'A',
// which is stripped for display (20 chars) and re-appended when parsing.

using System.Security.Cryptography;
using System.Text;

namespace StJohn.Licensing;

/// <summary>Licensed product identifiers. Stored in every key (4 bits).</summary>
public enum ProductId : byte
{
    DayPilot = 1,
    BizPilot = 2,
    Wuse = 3,
    ClaimPilot = 4,
    ClaimPilotPlus = 5,
    LegalMatters = 6,
    WuseBusiness = 7,
}

/// <summary>Decoded contents of a valid license key.</summary>
public sealed class LicenseInfo
{
    public ProductId Product { get; init; }
    public uint EmailHash { get; init; }
    public DateTime IssuedUtc { get; init; }
    public DateTime? ExpiryUtc { get; init; }
    public bool IsPerpetual => ExpiryUtc is null;
}

public static class LicenseCore
{
    /// <summary>Day zero for the 12-bit day counters (supports ~11.2 years).</summary>
    public static readonly DateTime LicenseEpoch = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private const int KeyVersion = 1;
    private const int PerpetualMarker = 0xFFF;
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    // ------------------------------------------------------------------
    // Key generation (used by KeyGen; also usable in-app for tests only)
    // ------------------------------------------------------------------

    public static string GenerateKey(byte[] secret, ProductId product, string email, DateTime issuedUtc, DateTime? expiryUtc)
    {
        if (secret is null || secret.Length < 16)
            throw new ArgumentException("Secret must be at least 16 bytes.", nameof(secret));
        var emailHash = EmailHashOf(email);
        var issuedDays = DaysSinceEpoch(issuedUtc);
        var expiryDays = expiryUtc is null ? PerpetualMarker : DaysSinceEpoch(expiryUtc.Value);
        if ((byte)product > 0x0F)
            throw new ArgumentException("Product id does not fit in 4 bits.", nameof(product));

        ulong payload = 0;
        payload |= ((ulong)KeyVersion & 0xF) << 60;
        payload |= ((ulong)(byte)product & 0xF) << 56;
        payload |= ((ulong)emailHash & 0xFFFFFFFF) << 24;
        payload |= ((ulong)(issuedDays & 0xFFF)) << 12;
        payload |= (ulong)(expiryDays & 0xFFF);

        var payloadBytes = new byte[8];
        for (var i = 0; i < 8; i++)
            payloadBytes[i] = (byte)(payload >> (56 - 8 * i));

        var mac = HMACSHA256.HashData(secret, payloadBytes);

        // 13-byte buffer: 8 payload + 4.5 signature bytes (last nibble zeroed).
        var buf = new byte[13];
        Buffer.BlockCopy(payloadBytes, 0, buf, 0, 8);
        Buffer.BlockCopy(mac, 0, buf, 8, 4);
        buf[12] = (byte)(mac[4] & 0xF0);

        var b32 = Base32Encode(buf); // 21 chars, last is always 'A'
        var raw = b32.Substring(0, 20);
        return $"{raw.Substring(0, 5)}-{raw.Substring(5, 5)}-{raw.Substring(10, 5)}-{raw.Substring(15, 5)}";
    }

    // ------------------------------------------------------------------
    // Key verification (offline; used by every app on launch)
    // ------------------------------------------------------------------

    public static bool TryParse(string? keyText, byte[] secret, ProductId expectedProduct,
        out LicenseInfo? info, out string error)
    {
        info = null;
        error = string.Empty;

        var raw = (keyText ?? string.Empty).Trim().ToUpperInvariant()
            .Replace("-", string.Empty).Replace(" ", string.Empty);
        if (raw.Length != 20 || !raw.All(Base32Alphabet.Contains))
        {
            error = "That doesn't look like a license key. It should be 20 characters in 4 groups of 5.";
            return false;
        }

        byte[] buf;
        try
        {
            buf = Base32Decode(raw + "A"); // re-append the stripped zero-nibble char
        }
        catch
        {
            error = "That license key is malformed.";
            return false;
        }
        if (buf.Length != 13)
        {
            error = "That license key is malformed.";
            return false;
        }

        var payloadBytes = new byte[8];
        Buffer.BlockCopy(buf, 0, payloadBytes, 0, 8);

        var mac = HMACSHA256.HashData(secret, payloadBytes);
        if (!SigMatches(buf, mac))
        {
            error = "That license key is invalid (signature check failed). Check for typos, or request a new key.";
            return false;
        }

        ulong payload = 0;
        for (var i = 0; i < 8; i++)
            payload = (payload << 8) | payloadBytes[i];

        var version = (int)((payload >> 60) & 0xF);
        if (version != KeyVersion)
        {
            error = "That license key uses an unsupported format version.";
            return false;
        }

        var product = (ProductId)((payload >> 56) & 0xF);
        if (product != expectedProduct)
        {
            error = "That key is for a different product.";
            return false;
        }

        var emailHash = (uint)((payload >> 24) & 0xFFFFFFFF);
        var issuedDays = (int)((payload >> 12) & 0xFFF);
        var expiryDays = (int)(payload & 0xFFF);

        info = new LicenseInfo
        {
            Product = product,
            EmailHash = emailHash,
            IssuedUtc = LicenseEpoch.AddDays(issuedDays),
            ExpiryUtc = expiryDays == PerpetualMarker ? null : LicenseEpoch.AddDays(expiryDays),
        };
        return true;
    }

    /// <summary>True when the key's email binding matches the given address.</summary>
    public static bool EmailMatches(LicenseInfo info, string email) =>
        info.EmailHash == EmailHashOf(email);

    /// <summary>True when a dated (non-perpetual) key is past its expiry day (UTC).</summary>
    public static bool IsExpired(LicenseInfo info) =>
        info.ExpiryUtc is DateTime exp && DateTime.UtcNow.Date > exp.Date;

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    public static uint EmailHashOf(string email)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes((email ?? string.Empty).Trim().ToLowerInvariant()));
        return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
    }

    public static string EmailHashHex(uint hash) => hash.ToString("X8");

    private static int DaysSinceEpoch(DateTime utc)
    {
        var days = (int)(utc.ToUniversalTime().Date - LicenseEpoch).TotalDays;
        if (days < 0 || days > 0xFFE)
            throw new ArgumentOutOfRangeException(nameof(utc), "Date is outside the encodable range (2026-01-01 .. ~2037).");
        return days;
    }

    private static bool SigMatches(byte[] buf, byte[] mac)
    {
        // Compare the 36 signature bits in constant time.
        var diff = 0;
        for (var i = 0; i < 4; i++)
            diff |= buf[8 + i] ^ mac[i];
        diff |= (buf[12] ^ mac[4]) & 0xF0;
        return diff == 0;
    }

    internal static string Base32Encode(byte[] data)
    {
        var sb = new StringBuilder((data.Length * 8 + 4) / 5);
        var buffer = 0;
        var bitsLeft = 0;
        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                bitsLeft -= 5;
                sb.Append(Base32Alphabet[(buffer >> bitsLeft) & 31]);
            }
        }
        if (bitsLeft > 0)
            sb.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 31]);
        return sb.ToString();
    }

    internal static byte[] Base32Decode(string text)
    {
        var outBytes = new List<byte>(text.Length * 5 / 8);
        var buffer = 0;
        var bitsLeft = 0;
        foreach (var c in text)
        {
            var v = Base32Alphabet.IndexOf(c);
            if (v < 0)
                throw new FormatException("Invalid base32 character.");
            buffer = (buffer << 5) | v;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bitsLeft -= 8;
                outBytes.Add((byte)(buffer >> bitsLeft));
            }
        }
        return outBytes.ToArray();
    }

    // ------------------------------------------------------------------
    // Light obfuscation for values stored in LocalSettings.
    // This is NOT encryption — it only keeps casual snoopers from reading
    // dates/keys at a glance. (For real encryption use DPAPI via
    // System.Security.Cryptography.ProtectedData, which needs an extra
    // NuGet package not referenced here.)
    // ------------------------------------------------------------------

    private static readonly byte[] XorPad = Encoding.UTF8.GetBytes("StJohnPictures-LocalVault-v1");

    public static string Obfuscate(string plain)
    {
        var bytes = Encoding.UTF8.GetBytes(plain);
        for (var i = 0; i < bytes.Length; i++)
            bytes[i] ^= XorPad[i % XorPad.Length];
        return Convert.ToBase64String(bytes);
    }

    public static string Deobfuscate(string? cipher)
    {
        if (string.IsNullOrEmpty(cipher))
            return string.Empty;
        var bytes = Convert.FromBase64String(cipher);
        for (var i = 0; i < bytes.Length; i++)
            bytes[i] ^= XorPad[i % XorPad.Length];
        return Encoding.UTF8.GetString(bytes);
    }
}

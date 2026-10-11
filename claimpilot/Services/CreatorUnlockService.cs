using System.Security.Cryptography;
using System.Text;
using Windows.Storage;

namespace ClaimPilot.Services;

/// <summary>
/// Creator Unlock: a documented, local-only feature that lets the app's
/// creator (Shaun Nicholas St. John) use ClaimPilot — including the paid
/// Bankruptcy Plus modules — free on his own machines.
///
/// This is NOT a backdoor:
///  - No network calls of any kind. Nothing phones home, nothing activates
///    remotely, nothing grants access to anyone but the person holding the PIN.
///  - The PIN is never stored in plaintext: it is salted (16 random bytes)
///    and hashed with SHA-256. Only the salt and the hash live in the app's
///    local settings vault.
///  - The unlock applies to the local machine only. It does not touch the
///    Microsoft Store, does not bypass Store licensing for other users, and
///    cannot be triggered except through the hidden About-page gesture.
///  - There is no master key, no hard-coded PIN, and no remote kill switch.
///
/// Opened from the About page: tap the version line 7 times.
/// </summary>
public static class CreatorUnlockService
{
    private const string ContainerName = "CreatorUnlock";
    private const string KeySalt = "PinSalt";
    private const string KeyHash = "PinHash";
    private const string KeyActive = "UnlockActive";

    private static ApplicationDataContainer Container =>
        ApplicationData.Current.LocalSettings.CreateContainer(
            ContainerName, ApplicationDataCreateDisposition.Always);

    /// <summary>True when the creator unlock is currently active on this machine.</summary>
    public static bool IsUnlocked
    {
        get
        {
            try
            {
                return Container.Values[KeyActive] is true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>True when a creator PIN has been set (first-use setup done).</summary>
    public static bool HasPin
    {
        get
        {
            try
            {
                return Container.Values[KeySalt] is string s && s.Length > 0
                    && Container.Values[KeyHash] is string h && h.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Sets the creator PIN for the first time. Returns false when a PIN
    /// already exists (use VerifyPinAsync) or the PIN is too short.
    /// </summary>
    public static bool SetPin(string pin)
    {
        if (HasPin || string.IsNullOrEmpty(pin) || pin.Length < 4)
        {
            return false;
        }

        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        var hash = Hash(pin, salt);

        Container.Values[KeySalt] = Convert.ToBase64String(salt);
        Container.Values[KeyHash] = Convert.ToHexString(hash);
        Container.Values[KeyActive] = true;
        return true;
    }

    /// <summary>
    /// Verifies the entered PIN against the stored salted hash. On success the
    /// unlock is activated on this machine.
    /// </summary>
    public static bool VerifyPin(string pin)
    {
        if (!HasPin || string.IsNullOrEmpty(pin))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String((string)Container.Values[KeySalt]!);
            var expected = (string)Container.Values[KeyHash]!;
            var actual = Convert.ToHexString(Hash(pin, salt));
            if (CryptographicEquals(expected, actual))
            {
                Container.Values[KeyActive] = true;
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Deactivates the unlock on this machine (the PIN itself is kept).</summary>
    public static void Lock()
    {
        try
        {
            Container.Values[KeyActive] = false;
        }
        catch
        {
            // Local settings unavailable — treat as locked.
        }
    }

    private static byte[] Hash(string pin, byte[] salt)
    {
        var pinBytes = Encoding.UTF8.GetBytes(pin);
        var combined = new byte[salt.Length + pinBytes.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(pinBytes, 0, combined, salt.Length, pinBytes.Length);
        return SHA256.HashData(combined);
    }

    private static bool CryptographicEquals(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }
        var diff = 0;
        for (var i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }
        return diff == 0;
    }
}

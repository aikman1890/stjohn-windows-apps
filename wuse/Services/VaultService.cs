using Windows.Security.Credentials;

namespace Wuse.Services;

/// <summary>
/// A saved login entry. The password is only ever materialized when the user
/// explicitly asks to copy it — it's never listed, logged, or spoken.
/// </summary>
public sealed class VaultEntry
{
    public string Name { get; set; } = string.Empty;      // "Netflix", "Gmail"…
    public string Username { get; set; } = string.Empty;
}

/// <summary>
/// Secure login vault backed by the Windows Credential Locker
/// (Windows.Security.Credentials.PasswordVault): OS-encrypted, tied to the
/// Windows user account. Passwords are NEVER written to files, JSON, logs,
/// chat history, or AI prompts — the vault is the only storage mechanism.
/// wuse never auto-fills logins into sites (phishing risk); it only stores
/// and copies on explicit request.
/// </summary>
public sealed class VaultService
{
    /// <summary>Credential Locker resource name for user-saved logins.</summary>
    public const string VaultResource = "wuse-vault";

    /// <summary>Reserved entry name for the Yahoo IMAP app password.</summary>
    public const string YahooCredentialName = "yahoo-imap-app-password";

    private readonly PasswordVault _vault = new();

    public IReadOnlyList<VaultEntry> GetAll()
    {
        var list = new List<VaultEntry>();
        try
        {
            foreach (var cred in _vault.RetrieveAll())
            {
                if (!string.Equals(cred.Resource, VaultResource, StringComparison.Ordinal))
                    continue;
                if (string.Equals(cred.UserName, YahooCredentialName, StringComparison.Ordinal))
                    continue; // service credential, not a user login
                // The real username travels inside the encrypted payload
                // ("username\npassword"); only the username half is surfaced.
                string username = string.Empty;
                try
                {
                    cred.RetrievePassword();
                    var payload = cred.Password ?? string.Empty;
                    var idx = payload.IndexOf('\n');
                    username = idx < 0 ? string.Empty : payload.Substring(0, idx);
                }
                catch
                {
                    // Unreadable entry: still list it by site name.
                }
                list.Add(new VaultEntry { Name = cred.UserName, Username = username });
            }
        }
        catch
        {
            // Empty vault throws; treat as no entries.
        }
        // NOTE: the locker UserName holds the *site name*; the real username
        // is inside the encrypted payload. Only the username half is ever
        // surfaced — the password half stays in the locker.
        return list.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Saves a login. The locker entry is keyed by site name; the real
    /// username travels inside the encrypted payload as "username\npassword".
    /// </summary>
    public void Save(string siteName, string username, string password)
    {
        var site = (siteName ?? string.Empty).Trim();
        var user = (username ?? string.Empty).Trim();
        if (site.Length == 0 || password.Length == 0)
            throw new ArgumentException("Site name and password are required.");

        Delete(site); // replace existing
        var payload = user + "\n" + password;
        _vault.Add(new PasswordCredential(VaultResource, site, payload));
    }

    /// <summary>
    /// Returns the (username, password) for a site, or null if not found.
    /// Callers must never display or speak the password.
    /// </summary>
    public (string Username, string Password)? Retrieve(string siteName)
    {
        var site = (siteName ?? string.Empty).Trim();
        if (site.Length == 0)
            return null;
        try
        {
            var cred = _vault.Retrieve(VaultResource, site);
            cred.RetrievePassword();
            var payload = cred.Password ?? string.Empty;
            var idx = payload.IndexOf('\n');
            if (idx < 0)
                return (string.Empty, payload);
            return (payload.Substring(0, idx), payload.Substring(idx + 1));
        }
        catch
        {
            return null;
        }
    }

    public bool Delete(string siteName)
    {
        var site = (siteName ?? string.Empty).Trim();
        if (site.Length == 0)
            return false;
        try
        {
            var cred = _vault.Retrieve(VaultResource, site);
            _vault.Remove(cred);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public VaultEntry? FindBestMatch(string query)
    {
        var q = (query ?? string.Empty).Trim().ToLowerInvariant();
        if (q.Length == 0)
            return null;
        var all = GetAll();
        return all
            .Select(e => (Entry: e, Score: MatchScore(e.Name.ToLowerInvariant(), q)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Entry)
            .FirstOrDefault();
    }

    private static int MatchScore(string name, string query)
    {
        if (name.Equals(query, StringComparison.Ordinal))
            return 100;
        if (name.StartsWith(query, StringComparison.Ordinal))
            return 75;
        if (name.Contains(query, StringComparison.Ordinal))
            return 50;
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int hits = words.Count(w => w.Length > 2 && name.Contains(w, StringComparison.Ordinal));
        return hits == words.Length && hits > 0 ? 25 : 0;
    }

    // ---------- Service credentials (Yahoo app password) ----------

    /// <summary>
    /// Stores a service credential (e.g. the Yahoo IMAP app password) under a
    /// reserved name, separate from user logins. Never displayed in the UI list.
    /// </summary>
    public void SaveServiceCredential(string name, string username, string secret)
    {
        try { _vault.Remove(_vault.Retrieve(VaultResource, name)); } catch { }
        _vault.Add(new PasswordCredential(VaultResource, name, (username ?? "") + "\n" + (secret ?? "")));
    }

    public (string Username, string Secret)? GetServiceCredential(string name)
    {
        try
        {
            var cred = _vault.Retrieve(VaultResource, name);
            cred.RetrievePassword();
            var payload = cred.Password ?? string.Empty;
            var idx = payload.IndexOf('\n');
            if (idx < 0)
                return (string.Empty, payload);
            return (payload.Substring(0, idx), payload.Substring(idx + 1));
        }
        catch
        {
            return null;
        }
    }

    public void DeleteServiceCredential(string name)
    {
        try { _vault.Remove(_vault.Retrieve(VaultResource, name)); } catch { }
    }
}

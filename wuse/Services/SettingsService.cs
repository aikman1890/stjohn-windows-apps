using System.Text.Json;
using Windows.Storage;
using Wuse.Models;

namespace Wuse.Services;

/// <summary>
/// App preferences persisted in ApplicationData.Current.LocalSettings.
/// </summary>
public sealed class SettingsService
{
    private ApplicationDataContainer Container => ApplicationData.Current.LocalSettings;

    public string AssistantName
    {
        get => GetString("AssistantName", "wuse");
        set => Container.Values["AssistantName"] = value;
    }

    public string ThemeName
    {
        get => GetString("ThemeName", "System");
        set => Container.Values["ThemeName"] = value;
    }

    public bool RememberAboutMe
    {
        get => GetBoolean("RememberAboutMe", true);
        set => Container.Values["RememberAboutMe"] = value;
    }

    public bool HasRun
    {
        get => GetBoolean("HasRun", false);
        set => Container.Values["HasRun"] = value;
    }

    /// <summary>
    /// When true, wuse speaks its replies aloud (in addition to showing them).
    /// </summary>
    public bool VoiceReplies
    {
        get => GetBoolean("VoiceReplies", true);
        set => Container.Values["VoiceReplies"] = value;
    }

    /// <summary>
    /// When true, wuse runs in Family mode: chat talks only. No command
    /// executes (no vault, email, calendar, files, or account actions),
    /// no conversation is saved, and private topics get one boundary
    /// reply. Shaun turns this on when handing the PC to family. It is a
    /// household-trust boundary, not a lock: anyone who opens Preferences
    /// can switch it back off.
    /// </summary>
    public bool FamilyMode
    {
        get => GetBoolean("FamilyMode", false);
        set => Container.Values["FamilyMode"] = value;
    }

    /// <summary>LocalSettings key for the VoiceStudio voice profile name.</summary>
    public const string VoiceStudioProfileKey = "VoiceStudioProfile";

    /// <summary>
    /// Name of the VoiceStudio voice profile wuse speaks through (Shaun's
    /// cloned voice). Defaults to "shaun2". Empty disables the VoiceStudio
    /// voice and wuse uses the Windows system voice instead. VoiceStudio
    /// must be running (wuse tries to start it automatically).
    /// </summary>
    public string VoiceStudioProfile
    {
        get
        {
            try
            {
                var values = Container.Values;
                if (!values.ContainsKey(VoiceStudioProfileKey))
                    return "shaun2";
                return (values[VoiceStudioProfileKey] as string ?? string.Empty).Trim();
            }
            catch { return "shaun2"; }
        }
        set
        {
            try { Container.Values[VoiceStudioProfileKey] = (value ?? string.Empty).Trim(); }
            catch { }
        }
    }

    /// <summary>
    /// When true, wuse draws on Shaun's family memory book (FamilyMemories.json
    /// in LocalFolder) when the family asks about memories. Only meaningful
    /// when a memory book is actually installed.
    /// </summary>
    public bool MemoryBookEnabled
    {
        get => GetBoolean("MemoryBookEnabled", true);
        set => Container.Values["MemoryBookEnabled"] = value;
    }

    public UserFacts GetUserFacts()
    {
        var facts = new UserFacts { AssistantName = AssistantName };
        try
        {
            if (Container.Values["UserFacts"] is string json && !string.IsNullOrWhiteSpace(json))
            {
                var stored = JsonSerializer.Deserialize<UserFacts>(json);
                if (stored is not null)
                {
                    if (!string.IsNullOrWhiteSpace(stored.AssistantName))
                        facts.AssistantName = stored.AssistantName;
                    facts.Facts = stored.Facts ?? new List<string>();
                }
            }
        }
        catch
        {
            // Corrupt JSON just means we start fresh.
        }
        return facts;
    }

    public void SaveUserFacts(UserFacts facts)
    {
        try
        {
            AssistantName = facts.AssistantName;
            Container.Values["UserFacts"] = JsonSerializer.Serialize(facts);
        }
        catch
        {
            // Persistence is best-effort; the in-memory values still work.
        }
    }

    private string GetString(string key, string fallback)
    {
        try
        {
            if (Container.Values[key] is string value && !string.IsNullOrWhiteSpace(value))
                return value;
        }
        catch { }
        return fallback;
    }

    private bool GetBoolean(string key, bool fallback)
    {
        try
        {
            if (Container.Values[key] is bool value)
                return value;
        }
        catch { }
        return fallback;
    }
}

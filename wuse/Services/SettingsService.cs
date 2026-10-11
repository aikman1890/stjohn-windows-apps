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

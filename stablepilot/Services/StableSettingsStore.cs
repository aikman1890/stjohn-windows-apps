using System.Text.Json;
using StablePilot.Models;
using Windows.Storage;

namespace StablePilot.Services;

public sealed class StableSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<StableSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<StableSettings>(json, Options) ?? new StableSettings();
            }
        }
        catch { /* blank settings */ }
        return new StableSettings();
    }

    public async Task SaveAsync(StableSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

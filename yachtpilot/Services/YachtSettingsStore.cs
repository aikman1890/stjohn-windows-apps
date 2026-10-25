using System.Text.Json;
using YachtPilot.Models;
using Windows.Storage;

namespace YachtPilot.Services;

public sealed class YachtSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<YachtSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<YachtSettings>(json, Options) ?? new YachtSettings();
            }
        }
        catch { /* blank settings */ }
        return new YachtSettings();
    }

    public async Task SaveAsync(YachtSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

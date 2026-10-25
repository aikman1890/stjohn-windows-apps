using System.Text.Json;
using KennelPilot.Models;
using Windows.Storage;

namespace KennelPilot.Services;

public sealed class KennelSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<KennelSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<KennelSettings>(json, Options) ?? new KennelSettings();
            }
        }
        catch { /* blank settings */ }
        return new KennelSettings();
    }

    public async Task SaveAsync(KennelSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

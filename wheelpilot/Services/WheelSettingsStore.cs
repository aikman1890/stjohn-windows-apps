using System.Text.Json;
using WheelPilot.Models;
using Windows.Storage;

namespace WheelPilot.Services;

public sealed class WheelSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<WheelSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<WheelSettings>(json, Options) ?? new WheelSettings();
            }
        }
        catch { /* blank settings */ }
        return new WheelSettings();
    }

    public async Task SaveAsync(WheelSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

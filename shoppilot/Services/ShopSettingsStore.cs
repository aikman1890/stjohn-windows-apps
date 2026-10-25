using System.Text.Json;
using ShopPilot.Models;
using Windows.Storage;

namespace ShopPilot.Services;

public sealed class ShopSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<ShopSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<ShopSettings>(json, Options) ?? new ShopSettings();
            }
        }
        catch { /* blank settings */ }
        return new ShopSettings();
    }

    public async Task SaveAsync(ShopSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

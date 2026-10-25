using System.Text.Json;
using FarewellPilot.Models;
using Windows.Storage;

namespace FarewellPilot.Services;

public sealed class FarewellSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<FarewellSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<FarewellSettings>(json, Options) ?? new FarewellSettings();
            }
        }
        catch { /* blank settings */ }
        return new FarewellSettings();
    }

    public async Task SaveAsync(FarewellSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

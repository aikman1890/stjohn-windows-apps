using System.Text.Json;
using PlatePilot.Models;
using Windows.Storage;

namespace PlatePilot.Services;

public sealed class PlateSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<PlateSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<PlateSettings>(json, Options) ?? new PlateSettings();
            }
        }
        catch { /* blank settings */ }
        return new PlateSettings();
    }

    public async Task SaveAsync(PlateSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

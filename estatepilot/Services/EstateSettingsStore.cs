using System.Text.Json;
using EstatePilot.Models;
using Windows.Storage;

namespace EstatePilot.Services;

public sealed class EstateSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<EstateSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<EstateSettings>(json, Options) ?? new EstateSettings();
            }
        }
        catch { /* blank settings */ }
        return new EstateSettings();
    }

    public async Task SaveAsync(EstateSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

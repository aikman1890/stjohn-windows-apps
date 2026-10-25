using System.Text.Json;
using LienPilot.Models;
using Windows.Storage;

namespace LienPilot.Services;

public sealed class FacilitySettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<FacilitySettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<FacilitySettings>(json, Options) ?? new FacilitySettings();
            }
        }
        catch { /* blank settings */ }
        return new FacilitySettings();
    }

    public async Task SaveAsync(FacilitySettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

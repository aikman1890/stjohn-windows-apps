using System.Text.Json;
using VenuePilot.Models;
using Windows.Storage;

namespace VenuePilot.Services;

public sealed class VenueSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<VenueSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<VenueSettings>(json, Options) ?? new VenueSettings();
            }
        }
        catch { /* blank settings */ }
        return new VenueSettings();
    }

    public async Task SaveAsync(VenueSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

using System.Text.Json;
using GalleryPilot.Models;
using Windows.Storage;

namespace GalleryPilot.Services;

public sealed class GallerySettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<GallerySettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<GallerySettings>(json, Options) ?? new GallerySettings();
            }
        }
        catch { /* blank settings */ }
        return new GallerySettings();
    }

    public async Task SaveAsync(GallerySettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

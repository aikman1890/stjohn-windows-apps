using System.Text.Json;
using EntityPilot.Models;
using Windows.Storage;

namespace EntityPilot.Services;

public sealed class EntitySettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<EntitySettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<EntitySettings>(json, Options) ?? new EntitySettings();
            }
        }
        catch { /* blank settings */ }
        return new EntitySettings();
    }

    public async Task SaveAsync(EntitySettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

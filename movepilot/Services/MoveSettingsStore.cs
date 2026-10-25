using System.Text.Json;
using MovePilot.Models;
using Windows.Storage;

namespace MovePilot.Services;

public sealed class MoveSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<MoveSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<MoveSettings>(json, Options) ?? new MoveSettings();
            }
        }
        catch { /* blank settings */ }
        return new MoveSettings();
    }

    public async Task SaveAsync(MoveSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

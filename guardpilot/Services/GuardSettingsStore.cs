using System.Text.Json;
using GuardPilot.Models;
using Windows.Storage;

namespace GuardPilot.Services;

public sealed class GuardSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<GuardSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<GuardSettings>(json, Options) ?? new GuardSettings();
            }
        }
        catch { /* blank settings */ }
        return new GuardSettings();
    }

    public async Task SaveAsync(GuardSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

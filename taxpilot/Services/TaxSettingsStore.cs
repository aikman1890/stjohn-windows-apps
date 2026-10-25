using System.Text.Json;
using TaxPilot.Models;
using Windows.Storage;

namespace TaxPilot.Services;

public sealed class TaxSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<TaxSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<TaxSettings>(json, Options) ?? new TaxSettings();
            }
        }
        catch { /* blank settings */ }
        return new TaxSettings();
    }

    public async Task SaveAsync(TaxSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}

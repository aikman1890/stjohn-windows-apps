using System.Text.Json;
using AgreementPilot.Models;
using Windows.Storage;

namespace AgreementPilot.Services;

public sealed class TradeSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<TradeSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<TradeSettings>(json, Options) ?? new TradeSettings();
            }
        }
        catch { /* blank settings */ }
        return new TradeSettings();
    }

    public async Task SaveAsync(TradeSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}
